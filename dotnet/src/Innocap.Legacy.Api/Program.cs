using System;
using System.Text;
using Innocap.Legacy.Infrastructure;
using Innocap.Legacy.Infrastructure.Repositories;
using Innocap.Legacy.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Startup for Innocap Investor Portal v1 — Carlos M., 2019
// Last touched by Aarav (intern) 2023-07 to add the mega-service registration

var builder = WebApplication.CreateBuilder(args);

// Serilog configured from appsettings.json — reads the hardcoded connection string section too
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// CORS — allow all origins, all methods, all headers
// "Temporary until we scope it to the fund admin IPs" — Carlos, 2021
// TODO: lock down before production (2021) — still wide open
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// EF Core — SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<InnocapDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repos — PositionRepository and NavRepository take a raw connection string
// because they use Dapper/ADO.NET, not EF. Mixing patterns: smell #8 of infrastructure.
builder.Services.AddScoped<InvestorRepository>();
builder.Services.AddScoped<PositionRepository>(_ => new PositionRepository(connectionString));
builder.Services.AddScoped<NavRepository>(_ => new NavRepository(connectionString));
builder.Services.AddScoped<FeeCalculator>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<PositionNavStatementService>();

// JWT authentication — key from config, not rotated since 2019
var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Using Newtonsoft for JSON serialisation in controllers — mixed with STJ elsewhere (smell #12)
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling     = Newtonsoft.Json.NullValueHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS middleware — using the all-permissive policy
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Innocap Investor Portal v1");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
