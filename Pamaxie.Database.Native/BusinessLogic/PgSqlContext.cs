using System;
using Microsoft.EntityFrameworkCore;

namespace Pamaxie.Database.Native;

public class PgSqlContext : DbContext
{
    
    
    
    
    /// <summary>
    /// Connection string used for the database
    /// </summary>
    internal static string SqlConnectionString;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (string.IsNullOrWhiteSpace(SqlConnectionString))
        {
            SqlConnectionString = "Host=localhost;Username=postgres;Database=pamaxie";
        }

        optionsBuilder.UseNpgsql(SqlConnectionString).UseSnakeCaseNamingConvention();
    }
}