// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CodeCoverageUtil
{
    /// <summary>
    /// 
    /// </summary>    
    internal class AssemblyLoader : IDisposable
    {
        #region Fields
        /// <summary>
        /// 
        /// </summary>
        private AppDomain appDomain;


        /// <summary>
        /// 
        /// </summary>
        private AssemblyLoaderAppDomainWorker appDomainWorker;


        /// <summary>
        /// 
        /// </summary>
        private bool disposed;
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
        /// </summary>
        public AssemblyLoader()
        {
            appDomain = AppDomain.CreateDomain("CodeCoverageUtil.AssemblyLoaderAppDomainWorker");
            appDomainWorker = (AssemblyLoaderAppDomainWorker)appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AssemblyLoaderAppDomainWorker).FullName);

            appDomainWorker.Initialize();
        }
        #endregion


        #region Methods
        /// <summary>
        /// Gets the specified assembly's location.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public string GetAssemblyLocation(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw new ArgumentNullException("assemblyName");
            }            

            return appDomainWorker.GetAssemblyLocation(assemblyName);
        }


        /// <summary>
        /// True if the assembly is strong named, false otherwise.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        ///   <c>true</c> if the assembly strong named.
        /// </returns>
        public bool IsAssemblyStrongNamed(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw new ArgumentNullException("assemblyName");
            }            

            return appDomainWorker.IsAssemblyStrongNamed(assemblyName);
        }


        /// <summary>
        /// Determines whether [is test context valid] [the specified test context].
        /// </summary>
        /// <param name="testContext">The test context.</param>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>
        ///   <c>true</c> if the test context is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTestContextValid(TestCodeContext testContext, string assemblyPath)
        {
            return appDomainWorker.IsTestContextValid(testContext, assemblyPath);
        }
        #endregion


        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (appDomain != null)
                    {
                        appDomainWorker = null;
                        AppDomain.Unload(appDomain);
                        appDomain = null;
                    }
                }
            }
            disposed = true;
        }
        #endregion        
    }
}
