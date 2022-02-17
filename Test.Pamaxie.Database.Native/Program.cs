// This project is mostly used for testing functions of the native database to see if everything works as intended

using System.Diagnostics;
using System.Runtime.CompilerServices;
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

PamUser user = new PamUser
{
    Email = "Test@Test.de",
    UserName = "Lia",
    LastName = "Duerr",
    FirstName = "Lia",
    PasswordHash = "hashedPw",
    Flags = UserFlags.None,
    TTL = DateTime.Now.AddMonths(6)
};

driver.Service.Users.Create(user);

var foundUser = (IPamUser) driver.Service.Users.Get(943656985650270208);

var project = new PamProject
{
    Name = "Test",
    Owner = new LazyObject<(IPamUser User, long UserId)>() {IsLoaded = false, Data = (null, foundUser.Id)},
    CreationDate = DateTime.Now,
    Flags = ProjectFlags.None,
    TTL = DateTime.Now.AddMonths(6)
};

driver.Service.Projects.Create(project);

var apiToken = driver.Service.Projects.AddToken(project.Id);


Console.WriteLine("Don");

