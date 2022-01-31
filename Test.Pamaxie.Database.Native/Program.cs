// This project is mostly used for testing functions of the native database to see if everything works as intended

using System.Diagnostics;
using Pamaxie.Database.Native;



var myWriter = new TextWriterTraceListener(System.Console.Out);
Environment.SetEnvironmentVariable("PamRedisDbVar", "localhost:6379");
Environment.SetEnvironmentVariable("PamPgSqlDbVar", "Host=localhost;Username=postgres;Database=pamaxie");
var driver = new PamaxieDatabaseDriver();
driver.Configuration.LoadConfig(driver.Configuration.GenerateConfig());
driver.Service.ConnectToDatabase();


driver.Service.ValidateDatabase();