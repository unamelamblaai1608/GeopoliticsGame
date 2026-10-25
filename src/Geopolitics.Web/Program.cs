using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Geopolitics.Core.Data;
using Geopolitics.Core.Engines;
using Geopolitics.Core.Interfaces;
using Geopolitics.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// ----- Services -----
builder.Services.AddControllers();

// Database — EF Core + SQLite (replaces flat-file JSON storage)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=data/geopolitics.db";

// Ensure data directory exists for the SQLite file
var dbPath = connectionString.Replace("Data Source=", "");
var dbDir = Path.GetDirectoryName(Path.GetFullPath(dbPath));
if (!string.IsNullOrEmpty(dbDir))
    Directory.CreateDirectory(dbDir);

builder.Services.AddDbContextFactory<GeopoliticsDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IDatabaseService, EfDatabaseService>();
builder.Services.AddScoped<AuthService>();

// CORS (allow any origin for development)
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// JWT authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "geopolitics-super-secret-key-2026-csharp-version";
var key = Encoding.UTF8.GetBytes(jwtSecret.PadRight(32, 'x'));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = System.TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// Gemini AI configuration — exposed via DI for CommentaryEngine
var geminiKey = builder.Configuration["Gemini:ApiKey"]
    ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
    ?? string.Empty;

// Engine services — instance classes implementing OOP interfaces (DI pattern)
builder.Services.AddSingleton<IGameEngine, GameEngineService>();
builder.Services.AddSingleton<INpcEngine, NpcEngineService>();
builder.Services.AddSingleton<ICommentaryEngine>(sp =>
    new CommentaryEngineService(geminiKey));
builder.Services.AddScoped<IAnalyticsEngine, AnalyticsEngineService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); // keep existing OpenAPI support

var app = builder.Build();

// ----- Middleware -----

// Global exception handler — returns consistent JSON errors
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
    var msg = feature?.Error?.Message ?? "An unexpected error occurred.";
    await ctx.Response.WriteAsJsonAsync(new { error = msg });
}));

// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles(); // Serve default files like index.html
app.UseStaticFiles(); // Serve files from wwwroot (client SPA)
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Map attribute-routed controllers

// Fallback to SPA index.html for any non-API route
app.MapFallbackToFile("index.html");

app.Run();
