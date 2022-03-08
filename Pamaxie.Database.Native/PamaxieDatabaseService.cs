using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Pamaxie.Database.Extensions;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Spectre.Console;
using StackExchange.Redis;

namespace Pamaxie.Database.Native;

/// <inheritdoc cref="IPamaxieDatabaseService"/>
public sealed class PamaxieDatabaseService : IPamaxieDatabaseService
{
    private readonly PamaxieDatabaseDriver _owner;
    internal PamaxieDatabaseService(PamaxieDatabaseDriver owner)
    {
        this._owner = owner;
        UserSingleton = new PamUserInteraction();
        ProjectSingleton = new PamProjectInteraction();
    }

    public PamaxieDatabaseService(IPamUserInteraction userInteraction, IPamProjectInteraction projectInteraction, IPamScanInteraction scanInteraction, IPamScanMachineInteraction scanMachineInteraction)
    {
        UserSingleton = userInteraction;
        ProjectSingleton = projectInteraction;
        ScanSingleton = scanInteraction;
        ScanMachinesSingleton = scanMachineInteraction;
    }
    
    /// <inheritdoc cref="IPamaxieDatabaseService.IsDbConnected"/>
    public bool IsDbConnected { get; private set; }

    /// <inheritdoc cref="IPamaxieDatabaseService.DbConnectionHost1"/>
    public object DbConnectionHost1 { get; private set; }
        
    /// Not needed since we work over DbContext
    public object DbConnectionHost2 { get; }

    /// <inheritdoc cref="IPamaxieDatabaseService.Projects"/>
    public IPamProjectInteraction Projects => ProjectSingleton;
    
    internal static IPamProjectInteraction ProjectSingleton { get; set; }

    /// <inheritdoc cref="IPamaxieDatabaseService.Users"/>
    public IPamUserInteraction Users => UserSingleton;

    internal static IPamUserInteraction UserSingleton { get; set; }

    /// <inheritdoc cref="IPamaxieDatabaseService.Scans"/>
    public IPamScanInteraction Scans => ScanSingleton;

    internal static IPamScanInteraction ScanSingleton { get; set; }

    /// <inheritdoc cref="IPamaxieDatabaseService.ScanMachines"/>
    public IPamScanMachineInteraction ScanMachines => ScanMachinesSingleton;
    
    internal static IPamScanMachineInteraction ScanMachinesSingleton { get; set; }

    /// <inheritdoc cref="IPamaxieDatabaseService.ValidateConfiguration"/>
    public bool ValidateConfiguration(IPamaxieDatabaseConfiguration connectionParams)
    {
        if (connectionParams == null) throw new ArgumentNullException(nameof(connectionParams));
            
        using var conn = ConnectionMultiplexer.Connect(connectionParams.ToString());
        return conn.IsConnected;
    }

    /// <inheritdoc cref="IPamaxieDatabaseService.ConnectToDatabase"/>
    public void ConnectToDatabase(IPamaxieDatabaseConfiguration connectionParams = null)
    {
        if (connectionParams != null || string.IsNullOrWhiteSpace(_owner.Configuration.Db1Config))
        {
            throw new ArgumentException($"You need to either set/create {nameof(connectionParams)} or make sure that" +
                                        $"{nameof(_owner.Configuration.Db1Config)} is loaded and not null or whitespace", nameof(connectionParams));
        }
            
        if (!string.IsNullOrWhiteSpace(_owner.Configuration.Db1Config))
        {
            connectionParams = _owner.Configuration;
        }

        if (connectionParams == null)
        {
            throw new ArgumentNullException(nameof(connectionParams), "Suffered a critical error while trying to use the Database configuration");
        }
        
        CleanRedisConnection();

        try
        {
            PgSqlContext.SqlConnectionString = connectionParams.Db1Config;
            DbConnectionHost1 = ConnectionMultiplexer.Connect(connectionParams.Db2Config);
            ScanSingleton = new PamScanInteraction(this);
            ScanMachinesSingleton = new PamScanMachineInteraction(this);
        }
        catch (RedisConnectionException)
        {
            AnsiConsole.MarkupLine("[red]Unable to connect to Redis database. Please validate that it's connection parameters are correct.[/]");
        }

        using (var dbContext = new PgSqlContext())
        {
            if (!dbContext.Database.CanConnect())
            {
                AnsiConsole.MarkupLine("[red]Unable to connect to the Postgres database. Please validate that it's connection parameters are correct.[/]");
            }
        }

        //Stating our driver is in a functional state again.
        IsDbConnected = true;
    }

    public async Task ConnectToDatabaseAsync(IPamaxieDatabaseConfiguration connectionParams = null)
    { 
        if (connectionParams != null || string.IsNullOrWhiteSpace(_owner.Configuration.Db1Config))
        {
            throw new ArgumentException($"You need to either set/create {nameof(connectionParams)} or make sure that" +
                                        $"{nameof(_owner.Configuration.Db1Config)} is loaded and not null or whitespace", nameof(connectionParams));
        }
            
        if (!string.IsNullOrWhiteSpace(_owner.Configuration.Db1Config))
        {
            connectionParams = _owner.Configuration;
        }

        if (connectionParams == null)
        {
            throw new ArgumentNullException(nameof(connectionParams), "Suffered a critical error while trying to use the Database configuration");
        }
        
        CleanRedisConnection();
        DbConnectionHost1 = await ConnectionMultiplexer.ConnectAsync(connectionParams.Db1Config);
        ScanSingleton = new PamScanInteraction(this);
        ScanMachinesSingleton = new PamScanMachineInteraction(this);
        PgSqlContext.SqlConnectionString = connectionParams.Db2Config;
        


        //Stating our driver is in a functional state again.
        IsDbConnected = true;
    }
    
    /// <inheritdoc cref="IPamaxieDatabaseService.ValidateDatabase"/>
    public void ValidateDatabase()
    {
        using var dbContext = new PgSqlContext();
        AnsiConsole.MarkupLine("[yellow]Ensuring the database can be connected to[/]");

        if (!IsDbConnected)
        {
            throw new InvalidOperationException(
                "Please validate the database connection can be established before validating the database.");
        }
        
        if (!dbContext.Database.IsNpgsql())
        {
            throw new InvalidOperationException("The underlying database is not postgres");
        }
        
        AnsiConsole.MarkupLine("[yellow]Checking if database has any pending migrations[/]");
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        
        if (pendingMigrations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]Migrations pending... applying them now! " +
                                   "Also automatically creating your database if it doesn't exist yet[/]");
            dbContext.Database.Migrate();
        }

        if (!dbContext.Database.CanConnect())
        {
            throw new InvalidOperationException(
                "Cannot connect to postgres database. Please validate that the connection parameters are correct");
        }
    }
    
    /// <inheritdoc cref="IPamaxieDatabaseService.ValidateDatabaseAsync"/>
    public async Task ValidateDatabaseAsync()
    {
        await using var dbContext = new PgSqlContext();
        AnsiConsole.MarkupLine("[yellow]Ensuring the database can be connected to[/]");
        
        if (!dbContext.Database.IsNpgsql())
        {
            throw new InvalidOperationException("The underlying database is not postgres");
        }

        AnsiConsole.MarkupLine("[yellow]Checking if database has any pending migrations[/]");
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            AnsiConsole.MarkupLine("[yellow]Migrations pending... applying them now! " +
                                   "Also automatically creating your database if it doesn't exist yet[/]");
            await dbContext.Database.MigrateAsync();
        }
        
        if (!await dbContext.Database.CanConnectAsync())
        {
            throw new InvalidOperationException("A connection to our database could not be established. " +
                                                "Please validate the connection string is correct.");
        }
    }

    /// <summary>
    /// Cleans up connections to the database
    /// </summary>
    private void CleanRedisConnection()
    {
        //Setting this to false in case commands fly in while we clean everything up
        IsDbConnected = false;

        if (DbConnectionHost1 is not ConnectionMultiplexer redisDbConnection)
        {
            if (DbConnectionHost1 is IDisposable disposable)
            {
                disposable.Dispose();
            }
                
            DbConnectionHost1 = null;
        }
    }
}