using Pamaxie.Database.Design;
using Pamaxie.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Extensions
{
    public class DbDriverManager
    {
        public static IEnumerable<IPamaxieDatabaseDriver> LoadDatabaseDrivers()
        {
            var callingProcessPath = PamaxieEnviorement.GetExecutingAssemblyDirectory();
            var files = Directory.GetFiles(callingProcessPath, "*.dll");

            var foundtypes = new List<IPamaxieDatabaseDriver>();

            foreach (var file in files)
            {
                Type[] types;
                try
                {
                    types = Assembly.LoadFrom(file).GetTypes();
                }
                catch
                {
                    continue;
                }

                var knownTypes = types.Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IPamaxieDatabaseDriver))).Select(x => (IPamaxieDatabaseDriver)Activator.CreateInstance(x));
                foundtypes.AddRange(knownTypes);
            }

            return foundtypes;
        }

        public static IPamaxieDatabaseDriver LoadDatabaseDriver(Guid driverGuid)
        {
            var callingProcessPath = new FileInfo(Environment.ProcessPath).DirectoryName;
            var files = Directory.GetFiles(callingProcessPath, "*.dll");

            foreach (var file in files)
            {
                Type[] types;
                try
                {
                    types = Assembly.LoadFrom(file).GetTypes();
                }
                catch
                {
                    continue;
                }

                var knownTypes = types.Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IPamaxieDatabaseDriver))).Select(x => (IPamaxieDatabaseDriver)Activator.CreateInstance(x));
                var foundType = knownTypes.FirstOrDefault(x => x.DatabaseTypeGuid == driverGuid);
                if (foundType != null)
                {
                    return foundType;
                }
            }

            return null;
        }
    }
}
