// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCoverageUtil.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    internal class TestAssemblyConfig
    {
        #region Properties
        /// <summary>
        /// The path to the test assembly.
        /// </summary>
        public string AssemblyPath { get; private set; }


        /// <summary>
        /// Gets the publish path.
        /// </summary>
        public string PublishPath { get; private set; }       
        #endregion        


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyConfig"/> class.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="publishPath">The publish path.</param>
        public TestAssemblyConfig(string assemblyPath, string publishPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new ArgumentNullException("assemblyPath");
            }

            if (string.IsNullOrEmpty(publishPath))
            {
                throw new ArgumentNullException("publishPath");
            }

            AssemblyPath = assemblyPath;
            PublishPath = publishPath;           
        }
        #endregion        
    }
}
