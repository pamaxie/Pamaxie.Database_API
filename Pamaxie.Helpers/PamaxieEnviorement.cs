using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Helpers
{
    /// <summary>
    /// This specifies some options relevant for working with our enviorement (like finding settings files and other things depending on operating system)
    /// </summary>
    public class PamaxieEnviorement
    {
        private static string _pamaxieConfigurationDirectory;
        private static string _processPath;


        /// <summary>
        /// Gets our directory where we store enviorement relevant information
        /// </summary>
        /// <returns></returns>
        public static string GetPamaxieConfigurationDirectory()
        {
            if (string.IsNullOrWhiteSpace(_pamaxieConfigurationDirectory))
            {
                return _pamaxieConfigurationDirectory;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _pamaxieConfigurationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Pamaxie/";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _pamaxieConfigurationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Pamaxie/";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                _pamaxieConfigurationDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Pamaxie/";
            }

            return _pamaxieConfigurationDirectory;
        }

        /// <summary>
        /// Gets the directory of the current file
        /// </summary>
        /// <returns></returns>
        public static string GetExecutingAssemblyDirectory()
        {
            if (!string.IsNullOrWhiteSpace(_processPath))
            {
                return _processPath;
            }

            string codeBase = Assembly.GetExecutingAssembly().Location;
            FileInfo dir = new FileInfo(codeBase);
            _processPath = dir.DirectoryName;
            return _processPath;
        }
    }
}
