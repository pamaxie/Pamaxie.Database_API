// This project is mostly used for testing functions of the native database to see if everything works as intended

using System.Diagnostics;
using Pamaxie.Database.Native;
using Pamaxie.Database.Native.Sql;


var myWriter = new TextWriterTraceListener(System.Console.Out);
Environment.SetEnvironmentVariable("PamRedisDbVar", "localhost:6379");
Environment.SetEnvironmentVariable("PamPgSqlDbVar", "Host=localhost;Username=postgres;Database=pamaxie");
Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "true");
var driver = new PamaxieDatabaseDriver();
driver.Configuration.LoadConfig(driver.Configuration.GenerateConfig());
driver.Service.ConnectToDatabase();
driver.Service.ValidateDatabase();

driver.Service.Users.Create(new User()
{
    Email = "TestUser@Test.de", FirstName = "Emilie", Flags = 0,
    LastName = "Espiada"
});

driver.Service.Users.Get(1);