using AvtoNet.API.Data;
using AvtoNet.API.Models;
using AvtoNet.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddSingleton<JwtService>();

// Entity Framework
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ApplicationDbContextProd") ??
            throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found."))
    );
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("ApplicationDbContext") ??
                throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found."))
        );
}
builder.Services.AddScoped<DbInitializer>();

builder.Services.BuildServiceProvider().GetService<ApplicationDbContext>().Database.Migrate();

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Authorization
builder.Services.AddAuthorization();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Cors
builder.Services.AddCors(options =>
    options.AddPolicy(name: "AvtoNetOrigins", builder =>
        builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("WWW-Authenticate")
            .WithExposedHeaders("X-Pagination"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AvtoNetOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

var services = app.Services;    

app.MapControllers();

app.Run();
