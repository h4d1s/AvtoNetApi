using AvtoNet.API.Data;
using AvtoNet.API.DTOs;
using AvtoNet.API.Models;
using AvtoNet.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AvtoNet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly ApplicationDbContext _context;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers(int page = 1, int pageSize = 10)
        {
            if (_context.ApplicationUsers == null)
            {
                return NotFound();
            }

            IQueryable<ApplicationUser> query = _context.ApplicationUsers;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null && !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("User not found.");
            }

            var currentUserId = userIdClaim.Value;

            query = query.Where(u => u.Id != currentUserId);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginationMetadata = new
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            return await query
                  .Skip((page - 1) * pageSize)
                  .Take(pageSize)
                  .ToListAsync();
        }

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDTO dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null && !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("User not found.");
            }

            var currentUserId = userIdClaim.Value;

            if (currentUserId.ToString() != id)
            { 
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id);

            if(user == null)
            {
                return NotFound("User not found.");
            }

            if (!dto.PhoneNumber.IsNullOrEmpty()) {
                user.PhoneNumber = dto.PhoneNumber;
            }
            if (!dto.City.IsNullOrEmpty()) {
                user.City = dto.City;
            }
            if (!dto.FirstName.IsNullOrEmpty()) {
                user.FirstName = dto.FirstName;
            }
            if (!dto.LastName.IsNullOrEmpty()) {
                user.LastName = dto.LastName;
            }
            if (!dto.Street.IsNullOrEmpty()) {
                user.Street = dto.Street;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return StatusCode(500, $"Failed to update user {user.UserName}. Err: {string.Join(", ", result.Errors)}");
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                BadRequest("User not found.");
            }

            await _userManager.DeleteAsync(user);

            return NoContent();
        }

        [HttpGet("currentUserId")]
        public IActionResult GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Ok(userId);
            }

            return BadRequest("User Id not found in token.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return BadRequest("Invalid credentials");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                // Generate JWT token
                var token = _jwtService.GenerateJwtToken(user.Id, userRoles);

                return Ok(new { User = user, Token = token, UserRoles = userRoles });
            }

            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var newUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Street = model.Street,
                City = model.City,
                PhoneNumber = model.PhoneNumber
            };

            // Check if passwords are same
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Password does not match!");
            }

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                await _userManager.AddToRoleAsync(user, "User");
                var userRoles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateJwtToken(user.Id, userRoles);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new { User = user, Token = token, UserRoles = roles });
            }

            // Failure
            return BadRequest(new { Errors = result.Errors });
        }
    }
}
