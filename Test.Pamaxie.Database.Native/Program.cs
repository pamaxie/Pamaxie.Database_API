// This project is mostly used for testing functions of the native database to see if everything works as intended

using System.Diagnostics;
using Pamaxie.Data;
using Pamaxie.Database.Native;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.Sql;


var myWriter = new TextWriterTraceListener(System.Console.Out);
Environment.SetEnvironmentVariable("PamRedisDbVar", "localhost:6379");
Environment.SetEnvironmentVariable("PamPgSqlDbVar", "Host=localhost;Username=postgres;Database=pamaxie");
Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "true");
var driver = new PamaxieDatabaseDriver();
driver.Configuration.LoadConfig(driver.Configuration.GenerateConfig());
driver.Service.ConnectToDatabase();
driver.Service.ValidateDatabase();


var foundUser = (IPamUser) driver.Service.Users.Get(943111882401648640);

Console.WriteLine("Don");

