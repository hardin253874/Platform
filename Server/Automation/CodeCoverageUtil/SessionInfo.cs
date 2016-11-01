// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CodeCoverageUtil.Configuration;

namespace CodeCoverageUtil
{
    /// <summary>
    /// 
    /// </summary>
    internal class SessionInfo
    {
        #region Constants
        private const string CoverageResultsFileName = "CoverageResults.coverage";
        #endregion


        #region Properties
        /// <summary>
        /// Gets a value indicating whether [no results].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [no results]; otherwise, <c>false</c>.
        /// </value>
        public bool NoResults { get; private set; }


        /// <summary>
        /// The assemblies directory.
        /// </summary>
        public string AssembliesDirectory
        {
            get
            {
                if (!Directory.Exists(assembliesDirectory))
                {
                    Directory.CreateDirectory(assembliesDirectory);
                }
                return assembliesDirectory;
            }
        }
        private string assembliesDirectory;


        /// <summary>
        /// The results directory.
        /// </summary>
        public string ResultsDirectory
        {
            get
            {
                if (!Directory.Exists(resultsDirectory))
                {
                    Directory.CreateDirectory(resultsDirectory);
                }
                return resultsDirectory;
            }
        }
        private string resultsDirectory;

        /// <summary>
        /// The coverage report
        /// </summary>
        public string CoverageReportDirectory
        {
            get
            {
                if (!Directory.Exists(coverageReportDirectory))
                {
                    Directory.CreateDirectory(coverageReportDirectory);
                }
                return coverageReportDirectory;
            }
        }
        private string coverageReportDirectory;


        /// <summary>
        /// The assembly backup directory.
        /// </summary>
        public string AssemblyBackupDirectory
        {
            get
            {
                if (!Directory.Exists(assemblyBackupDirectory))
                {
                    Directory.CreateDirectory(assemblyBackupDirectory);
                }
                return assemblyBackupDirectory;
            }
        }
        private string assemblyBackupDirectory;


        /// <summary>
        /// Gets the coverage results file.
        /// </summary>
        public string CoverageResultsFile { get; private set; }


        /// <summary>
        /// Gets the command line arguments.
        /// </summary>
        public CommandLineArguments CommandLineArguments { get; private set; }


        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public CodeCoverageUtilConfiguration Configuration { get; private set; }


        /// <summary>
        /// Gets the details.
        /// </summary>
        public List<String> Details
        {
            get
            {
                return details;
            }
        }
        private List<string> details = new List<string>();


        /// <summary>
        /// Gets the test code context.
        /// </summary>
        public TestCodeContext TestCodeContext { get; private set; }
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionInfo"/> class.
        /// </summary>
        /// <param name="commandLineArguments">The command line arguments.</param>
        /// <param name="configuration">The configuration.</param>
        public SessionInfo(CommandLineArguments commandLineArguments, CodeCoverageUtilConfiguration configuration)
        {
            assembliesDirectory = Path.Combine(configuration.TempPath, "Assemblies");

            resultsDirectory = Path.Combine(configuration.ResultsPath, "Latest");

            assemblyBackupDirectory = Path.Combine(configuration.TempPath, "Backup");

            coverageReportDirectory = Path.Combine(resultsDirectory, "CoverageReport");

            CoverageResultsFile = Path.Combine(ResultsDirectory, CoverageResultsFileName);

            CommandLineArguments = commandLineArguments;
            Configuration = configuration;

            NoResults = configuration.NoResults;
            if (commandLineArguments.NoResults.HasValue)
            {
                NoResults = commandLineArguments.NoResults.Value;
            }

            details.AddRange(configuration.Details);
            details.AddRange(commandLineArguments.Details);
            details = details.Distinct().ToList<string>();

            if (!string.IsNullOrEmpty(commandLineArguments.TestSourceFilePath) &&
                !string.IsNullOrEmpty(commandLineArguments.TestAssemblyFilePath) &&
                commandLineArguments.LineNumber > 0)
            {
                TestCodeContext = new TestCodeContext(
                    commandLineArguments.TestSourceFilePath,
                    commandLineArguments.LineNumber,
                    commandLineArguments.TestAssemblyFilePath);
            }
        }
        #endregion
    }
}
