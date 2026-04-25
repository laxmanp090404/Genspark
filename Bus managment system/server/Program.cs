using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using server.Auth;
using server.Data;
using server.Repositories;
using server.Repositories.Interfaces;
using server.Services;
using server.Services.Interfaces;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "ClientCors";
const string AccessTokenCookieName = "access_token";

// Controllers
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddResponseCaching();
builder.Services.AddSignalR();

// CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5500", "http://localhost:8000", "http://127.0.0.1:5500", "http://127.0.0.1:8000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bus Management API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// JWT Config
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing JWT configuration.");

var envJwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if (!string.IsNullOrWhiteSpace(envJwtKey))
{
    jwtOptions.Key = envJwtKey;
}

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Contains("replace-this", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JWT key is missing. Set JWT_KEY environment variable.");
}

// if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
// {
//     throw new InvalidOperationException("JWT key must be at least 32 bytes.");
// }

var envConnection = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = string.IsNullOrWhiteSpace(envConnection)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : envConnection;

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is missing. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");
}

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Auth
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) &&
                    context.Request.Cookies.TryGetValue(AccessTokenCookieName, out var tokenFromCookie))
                {
                    context.Token = tokenFromCookie;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// DB migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasMigrations = db.Database.GetMigrations().Any();
    if (hasMigrations)
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    await DbSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<server.Hubs.SeatHub>("/hubs/seats");

app.Run();