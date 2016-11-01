// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
    public class TestControllersAssemblyResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            var baseAssemblies = base.GetAssemblies();
            var assemblies = new List<Assembly>(baseAssemblies);
            var testControllersAssembly = GetTestAssembly();
            if (testControllersAssembly != null)
               baseAssemblies.Add(testControllersAssembly);
            return assemblies;
        }

        /// <summary>
        /// Get web api test controller assembly.
        /// </summary>
        /// <returns>Assembly</returns>
        public static Assembly GetTestAssembly()
        {
            string binPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin");
            string assemblyPath = string.Format("{0}\\{1}", binPath, "EDC.SoftwarePlatform.WebApiTestControllers.dll");

            try
            {
                if (File.Exists(assemblyPath))
                {
                     return Assembly.LoadFile(assemblyPath);
                }
                return null;
            }
            catch (System.Exception e)
            {
                EventLog.Application.WriteError(string.Format("Failed to load assembly {0}, exception {1}", assemblyPath, e.Message));
                throw;
            }
            
        }
    }
}