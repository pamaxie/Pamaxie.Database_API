using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pamaxie.Database.Extensions;

namespace Pamaxie.Api
{
    /// <summary>
    /// Class for the Main entry point
    /// BUG: This is just here for future proofing. This is a concept idea to have the data flow over this api instead of the ML Apis
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main Function
        /// </summary>
        /// <param name="args">Args to be passed</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Create our Host builder
        /// </summary>
        /// <param name="args">Startup Args</param>
        /// <returns><see cref="IHostBuilder"/> for the API</returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().AddCommandLine(args).Build();
            List<string> hostUrl = new List<string>();
            string nameString = configuration["hosturl"];

            if (string.IsNullOrWhiteSpace(nameString))
            {
                //Default Url if nothing was specified, this is basically the "default" server url
                nameString = "https://0.0.0.0";
            }
            else if (nameString.Contains(","))
            {
                hostUrl = nameString.Split(',').ToList();
            }

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseConfiguration(configuration)
                        .UseKestrel()
                        .UseUrls(hostUrl.Any() ? hostUrl.ToArray() : new[] { nameString })
                        .UseIISIntegration();
                });
        }
    }
}