// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.Coverage.Analysis;

namespace CodeCoverageUtil.Commands
{
    /// <summary>
    /// This command exports the coverage results file to xml.
    /// </summary>
    internal class ExportCoverageFileCommand : CommandBase
    {
        #region Fields        
        /// <summary>
        /// The path to the coverage file.
        /// </summary>
        public string CoverageFilePath
        {
            get
            {
                return coverageFilePath;
            }
        }
        protected string coverageFilePath;


        /// <summary>
        /// The path to the instrumented assemblies directory.
        /// </summary>
        public string InstrumentedAssembliesDirectory
        {
            get
            {
                return instrumentedAssembliesDirectory;
            }
        }
        protected string instrumentedAssembliesDirectory;


        /// <summary>
        /// The paths to the assemblies under test.
        /// </summary>
        public IEnumerable<string> AssembliesUnderTestPaths
        {
            get
            {
                return assembliesUnderTestPaths;
            }
        }
        protected IEnumerable<string> assembliesUnderTestPaths;
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCoverageFileCommand"/> class.
        /// </summary>
        /// <param name="coverageFilePath">The coverage file path.</param>
        /// <param name="instrumentedAssembliesDirectory">The instrumented assemblies directory.</param>
        /// <param name="assembliesUnderTestPaths">The assemblies under test paths.</param>
        /// <param name="mandatory">if set to <c>true</c> the command is mandatory.</param>
        public ExportCoverageFileCommand(string coverageFilePath, string instrumentedAssembliesDirectory, IEnumerable<string> assembliesUnderTestPaths, bool mandatory)
        {
            if (string.IsNullOrEmpty(coverageFilePath))
            {
                throw new ArgumentNullException("coverageFilePath");
            }

            if (string.IsNullOrEmpty(instrumentedAssembliesDirectory))
            {
                throw new ArgumentNullException("instrumentedAssembliesDirectory");
            }

            if (assembliesUnderTestPaths == null)
            {
                throw new ArgumentNullException("assembliesUnderTestPaths");
            }            

            this.coverageFilePath = coverageFilePath;
            this.instrumentedAssembliesDirectory = instrumentedAssembliesDirectory;
            this.assembliesUnderTestPaths = assembliesUnderTestPaths;
            this.mandatory = mandatory;

            name = string.Format("Export code coverage file {0} to xml", coverageFilePath);
        }
        #endregion


        #region Methods
        /// <summary>
        /// Export the specified coverage results file to xml.
        /// </summary>
        protected override void OnExecute()
        {
            string coverageXmlFile = Path.Combine(Path.GetDirectoryName(coverageFilePath), Path.GetFileNameWithoutExtension(coverageFilePath) + ".xml");
            string coverageXmlFileDirectory = Path.GetDirectoryName(coverageXmlFile);

            List<string> exePaths = new List<string>();
            List<string> symbolPaths = new List<string>();

            // Get the exe and symbol paths for the instrumented assemblies
            foreach (string assemblyPath in assembliesUnderTestPaths)
            {
                string exePath = Path.Combine(instrumentedAssembliesDirectory, Path.GetFileName(assemblyPath));
                string symbolPath = Path.Combine(instrumentedAssembliesDirectory, Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb");

                exePaths.Add(exePath);
                symbolPaths.Add(symbolPath);

                string exePathInResultsDirectory = Path.Combine(coverageXmlFileDirectory, Path.GetFileName(assemblyPath));
                OnOutputDataReceived(string.Format("  * Copying file from {0} to {1}", exePath, exePathInResultsDirectory));
                File.Copy(exePath, exePathInResultsDirectory, true);

                string symbolPathInResultsDirectory = Path.Combine(coverageXmlFileDirectory, Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb");
                OnOutputDataReceived(string.Format("  * Copying file from {0} to {1}", symbolPath, symbolPathInResultsDirectory));
                File.Copy(symbolPath, symbolPathInResultsDirectory, true);
            }

            OnOutputDataReceived(string.Format("  * Exporting code coverage file {0} to xml file {1}", coverageFilePath, coverageXmlFile));

            if (File.Exists(coverageXmlFile))
            {
                OnOutputDataReceived(string.Format("  * The code coverage file {0}, already exists. Deleting it.", coverageXmlFile));
                File.Delete(coverageXmlFile); 
            }
            

            // Export the results
            using (CoverageInfo ci = CoverageInfo.CreateFromFile(coverageFilePath, exePaths, symbolPaths))
            {
                CoverageDS data = ci.BuildDataSet(false);
                data.ExportXml(coverageXmlFile);
            }
        }        
        #endregion        
    }
}
