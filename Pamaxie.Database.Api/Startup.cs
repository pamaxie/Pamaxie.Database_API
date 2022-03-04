using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Pamaxie.Authentication;
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
            System.Environment.Exit(-501);
        }

        AppConfigManagement.LoadConfiguration();
        services.AddControllers();
        
        services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; })
            .AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.RefreshInterval = new TimeSpan(0, AppConfigManagement.JwtSettings.ExpiresInMinutes, 0);
                x.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppConfigManagement.JwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "Pamaxie",
                    ValidAudience = "Pamaxie-API"}; 
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
        dbDriver.Configuration.LoadConfig(AppConfigManagement.DbSettings.Settings);
        services.AddSingleton(dbDriver);
        services.AddTransient<JwtTokenGenerator>();
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