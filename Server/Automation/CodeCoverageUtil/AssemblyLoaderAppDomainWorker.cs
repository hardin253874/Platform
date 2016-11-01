// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CodeCoverageUtil
{
    internal class AssemblyLoaderAppDomainWorker : MarshalByRefObject
    {
        #region Methods
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {            
        }


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

            string assemblyLocation = string.Empty;

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyName);
                if (assembly != null)
                {
                    byte[] g = assembly.GetName().GetPublicKey();
                    assemblyLocation = assembly.Location;
                }
            }
            catch
            {
            }

            return assemblyLocation;
        }


        /// <summary>
        /// True if the assembly is strong named, false otherwise.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        ///   <c>true</c> if the assembly is strong named; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAssemblyStrongNamed(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw new ArgumentNullException("assemblyName");
            } 

            bool isAssemblyStrongNamed = false;

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyName);
                if (assembly != null)
                {
                    byte[] publicKey = assembly.GetName().GetPublicKey();
                    if (publicKey != null && publicKey.Length > 0)
                    {
                        isAssemblyStrongNamed = true;
                    }
                }
            }
            catch
            {
            }

            return isAssemblyStrongNamed;
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
            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new ArgumentNullException("assemblyPath");
            }

            if (testContext == null)
            {
                return false;
            }

            bool isTestContextValid = true;

            try
            {
                Assembly.LoadFrom(Settings.Default.NunitExeFrameworkPath);
                Assembly assembly = Assembly.LoadFrom(assemblyPath);                

                if (!string.IsNullOrEmpty(testContext.ClassNameQualified) ||
                    !string.IsNullOrEmpty(testContext.MethodNameQualified))
                {
                    bool isTestClassValid = true;
                    bool isTestMethodValid = true;

                    if (!string.IsNullOrEmpty(testContext.ClassNameQualified))
                    {
                        isTestClassValid = false;
                    }
                    if (!string.IsNullOrEmpty(testContext.MethodNameQualified))
                    {
                        isTestMethodValid = false;
                    }

                    Type classType = assembly.GetType(testContext.ClassNameQualified, false);
                    if (classType != null)
                    {
                        isTestClassValid = true;

                        if (!string.IsNullOrEmpty(testContext.MethodNameQualified))
                        {
                            MethodInfo methodInfo = classType.GetMethod(testContext.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                            if (methodInfo != null)
                            {
                                isTestMethodValid = true;
                            }
                        }
                    }

                    isTestContextValid = isTestClassValid && isTestMethodValid;
                }
            }
            catch
            {
                isTestContextValid = false;
            }

            return isTestContextValid;
        }
        #endregion        
    }
}
