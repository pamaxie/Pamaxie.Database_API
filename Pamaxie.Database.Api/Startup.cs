using System;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Extensions;
using Spectre.Console;

namespace Pamaxie.Database.Api;

/// <summary>
/// Startup class, usually gets called by <see cref="Program"/>
/// </summary>
public sealed class Startup
{
    /// <summary>
    /// Initializer
    /// </summary>
    /// <param name="configuration">Configuration to use</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    private IConfiguration Configuration { get; }

    /// <summary>
    /// Gets called by the runtime to add Services to the container
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add</param>
    public void ConfigureServices(IServiceCollection services)
    {
        if (!AppConfigManagement.ValidateConfiguration(out var issue))
        {
            AnsiConsole.MarkupLine("The applications configuration could not be read. The detailed problem was: \n" + 
                                   issue);
            AnsiConsole.MarkupLine("These errors need to be corrected before starting the application. Exiting the application now...");
            System.Environment.Exit(-501);
        }

        AppConfigManagement.LoadConfiguration();
        services.AddControllers();
        
        byte[] key = Encoding.ASCII.GetBytes(AppConfigManagement.JwtSettings.Secret);
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = "api.pamaxie.com"
                };
            });

        services.AddSwaggerGen(o => {
            o.SwaggerDoc(
                "v1",
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Pamaxie Database API",
                    Version = "v1"
                });
        });


        var dbDriver = DbDriverManager.LoadDatabaseDriver(AppConfigManagement.DbSettings.DatabaseDriverGuid);

        if (dbDriver == null)
        {
            AnsiConsole.MarkupLine("[red]Could not find the specified database driver from the configuration file. " +
                                  "Please ensure it exists. If you want to reconfigure your database to use a new driver, " +
                                  "please delete your existing config.[/]");
            
            Environment.Exit(-1);
        }
        
        dbDriver.Configuration.LoadConfig(AppConfigManagement.DbSettings.Db1Settings, AppConfigManagement.DbSettings.Db2Settings);
        services.AddSingleton(dbDriver);
        services.AddTransient<JwtTokenGenerator>();
        dbDriver.Service.ConnectToDatabase();

        if (!dbDriver.Service.IsDbConnected)
        {
            AnsiConsole.MarkupLine("[red]Could not connect to one or both databases. Please make sure their configurations are correct[/]");
            Environment.Exit(-502);
        }
        
        dbDriver.Service.ValidateDatabase();
    }

    /// <summary>
    /// Gets called by the runtime to configure HTTP Services
    /// </summary>
    /// <param name="app"><see cref="IApplicationBuilder"/> Builder</param>
    /// <param name="env">Hosting Environment</param>
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}