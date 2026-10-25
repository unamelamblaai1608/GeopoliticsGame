using System.Text.Json;
using Geopolitics.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Geopolitics.Core.Data;

/// <summary>
/// Entity Framework Core DbContext for the Geopolitics application.
/// Uses SQLite with complex game state stored as JSON columns.
/// </summary>
public class GeopoliticsDbContext : DbContext
{
    public GeopoliticsDbContext(DbContextOptions<GeopoliticsDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<GameEntity> Games => Set<GameEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users table — indexed for fast login lookup
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(30);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        // Games table — complex nested objects stored as JSON text columns
        modelBuilder.Entity<GameEntity>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.OwnerUserId).IsRequired();
            e.Property(g => g.Status).IsRequired().HasDefaultValue("active");
            e.HasIndex(g => g.OwnerUserId);
            e.HasIndex(g => g.Status);
        });
    }
}

/// <summary>
/// Relational entity for Users — maps to the 'Users' table in SQLite.
/// Stats are stored as a serialized JSON string for flexibility.
/// </summary>
public class UserEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string StatsJson { get; set; } = "{}";
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}

/// <summary>
/// Relational entity for Games — stores full game state as a JSON column.
/// Top-level fields (OwnerUserId, Status, Title) are indexed for querying.
/// </summary>
public class GameEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OwnerUserId { get; set; } = string.Empty;
    public string OwnerUsername { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public string GameJson { get; set; } = "{}";
}

