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
    internal class AssemblyUnderTestConfig
    {
        #region Properties
        /// <summary>
        /// The path to the assembly under test.
        /// </summary>
        public string AssemblyPath { get; private set; }


        /// <summary>
        /// The directory where the assembly should be published.
        /// </summary>
        public string PublishPath { get; private set; }


        /// <summary>
        /// True if this assembly is strong named, false otherwise.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is strong named; otherwise, <c>false</c>.
        /// </value>
        public bool IsStrongNamed { get; set; }
        #endregion        


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyUnderTestConfig"/> class.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="publishPath">The publish path.</param>
        /// <param name="isStrongNamed">if set to <c>true</c> the assembly is strong named.</param>
        public AssemblyUnderTestConfig(string assemblyPath, string publishPath, bool isStrongNamed)
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
            IsStrongNamed = isStrongNamed;
        }
        #endregion                
    }
}
