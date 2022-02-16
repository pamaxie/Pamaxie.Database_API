using System;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native;

public class PgSqlContext : DbContext
{
    /// <summary>
    /// Connection string used for the database
    /// </summary>
    internal static string SqlConnectionString;
    
    /// <inheritdoc cref="Projects"/>
    public DbSet<Project> Projects { get; set; }
    
    /// <inheritdoc cref="User"/>
    public DbSet<User> Users { get; set; }
    

    //Relationship Data
    
    /// <inheritdoc cref="ApiKey"/>
    public DbSet<ApiKey> ApiKeys { get; set; }
    
    /// <inheritdoc cref="KnownUserIps"/>
    public DbSet<KnownUserIp> KnownUserIps { get; set; }
    
    /// <inheritdoc cref="ProjectUsers"/>
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    
    /// <inheritdoc cref="TwoFactorUsers"/>
    public DbSet<TwoFactorUser> TwoFactorUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (string.IsNullOrWhiteSpace(SqlConnectionString))
        {
            SqlConnectionString = "Host=localhost;Username=postgres;Database=pamaxie";
        }

        optionsBuilder.UseNpgsql(SqlConnectionString).UseSnakeCaseNamingConvention();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>()
            .HasOne(x => x.Project)
            .WithMany(y => y.ApiKeys);

        modelBuilder.Entity<KnownUserIp>()
            .HasOne(x => x.User)
            .WithMany(y => y.KnownIps);
        
        modelBuilder.Entity<ProjectUser>()
            .HasOne(x => x.Project)
            .WithMany(y => y.Users);
        
        modelBuilder.Entity<ProjectUser>()
            .HasOne(x => x.User)
            .WithMany(y => y.Projects);

        modelBuilder.Entity<TwoFactorUser>()
            .HasOne(x => x.User)
            .WithMany(y => y.TwoFactorAuths);
        
        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.TTL })
            .IsUnique(false);
        
        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.Username })
            .IsUnique(true);

        modelBuilder.Entity<User>()
            .HasKey(x => new {x.Id});

        modelBuilder.Entity<User>()
            .HasIndex(x => new {x.Email})
            .IsUnique(true);

        modelBuilder.Entity<Project>()
            .HasIndex(x => new { x.TTL })
            .IsUnique(false);

        modelBuilder.Entity<Project>()
            .HasKey(x => new {x.Id});
        
        modelBuilder.Entity<Project>()
            .HasIndex(x => new { x.Name })
            .IsUnique(false);
    }
}