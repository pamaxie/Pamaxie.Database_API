using System;
using System.Reflection;

namespace Pamaxie.Helpers
{
    public static class UnitTest
    {
        /// <summary>
        /// <see cref="bool"/> for checking if the environment is running through unit testing
        /// </summary>
        public static bool IsRunningFromUnitTests { get; }

        static UnitTest()
        {
            for (int i = 0; i < AppDomain.CurrentDomain.GetAssemblies().Length; i++)
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()[i];
                if (assembly.FullName != null && assembly.FullName.ToLowerInvariant().StartsWith("xunit"))
                {
                    IsRunningFromUnitTests = true;
                    break;
                }
            }
        }
    }
}