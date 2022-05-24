using System;

namespace Pamaxie.Database.Api;

/// <summary>
/// Struct for storing all Environment Variables that the docker container offers
/// </summary>
public struct DockerEnvVars
{
    internal static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.ToLower() == "true";

    /// <summary>
    /// Settings for database 1
    /// </summary>
    internal const string Db1Settings = "Db1Settings";
    
    /// <summary>
    /// Settings for database 2
    /// </summary>
    internal const string Db2Settings = "Db2Settings";

    /// <summary>
    /// Settings for the database driver guid
    /// </summary>
    internal const string DatabaseDriverGuid = "DbDriverGuid";

    /// <summary>
    /// Settings that hold the configuration for the jwt settings
    /// </summary>
    internal const string JwtSecret = "JwtSecret";
    
    /// <summary>
    /// Timeout Settings Env Var
    /// </summary>
    internal const string TokenTimeoutSettingsEnvVar = "JwtTimeout";
        
    /// <summary>
    /// Long Lived timeout setting
    /// </summary>
    internal const string LongLivedTokenTimeoutSettingEnvVar = "JwtLLTimeout";

    /// <summary>
    /// Default port the application uses
    /// </summary>
    internal const string HostStringEnvVar = "Hosts";
    
    /// <summary>
    /// Token for Twilio sendgrid, used for Email verification
    /// </summary>
    internal const string SendGridEnvVar = "SendGridToken";

    /// <summary>
    /// Host url so we can make sure we offer the right url while we send certain emails.
    /// </summary>
    internal const string HostUrl = "EndpointUrl";

    public static void LogEnvVars()
    {
        Console.WriteLine("Outputting Currently set environment variables");
        Console.WriteLine($"Detected Docker: {InDocker}");
        Console.WriteLine($"Db1 Settings: {Environment.GetEnvironmentVariable(Db1Settings)}");
        Console.WriteLine($"Db2 Settings: {Environment.GetEnvironmentVariable(Db2Settings)}");
        Console.WriteLine($"Database Driver Guid: {Environment.GetEnvironmentVariable(DatabaseDriverGuid)}");
        Console.WriteLine($"Jwt Secret: {Environment.GetEnvironmentVariable(JwtSecret)}");
        Console.WriteLine($"Token Timeout: {Environment.GetEnvironmentVariable(TokenTimeoutSettingsEnvVar)}");
        Console.WriteLine($"Long-Lived Token Timeout: {Environment.GetEnvironmentVariable(LongLivedTokenTimeoutSettingEnvVar)}");
        Console.WriteLine($"Accepted Hosts: {Environment.GetEnvironmentVariable(HostStringEnvVar)}");
        Console.WriteLine($"Send Grid Token: {Environment.GetEnvironmentVariable(SendGridEnvVar)}");
        Console.WriteLine($"Host Url: {Environment.GetEnvironmentVariable(HostUrl)}");
    }
}