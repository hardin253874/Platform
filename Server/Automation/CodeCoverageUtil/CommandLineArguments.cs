// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

namespace CodeCoverageUtil
{    
    /// <summary>
    /// 
    /// </summary>
    internal class CommandLineArguments
    {
        #region Constants        
        private const string ConfigFileOption = "configfile:";
        private const string HelpOptionLong = "help";
        private const string HelpOptionShort = "?";
        private const string TestsOption = "tests:";
        private const string TestAssembliesOption = "testassemblies:";
        private const string CategoryOption = "category:";
        private const string NoResultsOption = "noresults";
        private const string DetailOption = "detail:";
        private const string CoverageToolOption = "coveragetool:";

        // These options are to support visual studio integration
        private const string ExploreToResultsFolderOption = "exploretoresultsfolder";
        private const string OpenCoverageHtmlReportOption = "opencoveragehtmlreport";
        private const string TestSourceFileLineOption = "testsourcefileline:";
        private const string TestSourceFilePathOption = "testsourcefilepath:";
        private const string TestAssemblyFilePathOption = "testassemblyfilepath:";
        #endregion


        #region Properties
        /// <summary>
        /// The error message if the command line arguments are invalid.
        /// </summary>
        public string ErrorMessage { get; private set; }


        /// <summary>
        /// The path to the config file.
        /// </summary>
        public string ConfigFilePath { get; private set; }


        /// <summary>
        /// True if the command line arguments are valid, false otherwise.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; private set; }


        /// <summary>
        /// True to show help.
        /// </summary>
        /// <value>
        ///   <c>true</c> to show help; otherwise, <c>false</c>.
        /// </value>
        public bool ShowHelp { get; private set; }


        /// <summary>
        /// The list of tests to run.
        /// </summary>
        public ReadOnlyCollection<String> TestsToRun 
        {
            get
            {
                return testsToRun.AsReadOnly();
            }
        }
        private List<string> testsToRun = new List<string>();


        /// <summary>
        /// The test filter category.
        /// </summary>
        public string Category { get; private set; }


        /// <summary>
        /// True to not generate results.
        /// </summary>
        public bool? NoResults 
        { 
            get
            {
                return noResults;
            }
        }
        private bool? noResults = null;


        /// <summary>
        /// Gets the details.
        /// </summary>
        public ReadOnlyCollection<String> Details
        {
            get
            {
                return details.AsReadOnly();
            }
        }
        private List<string> details = new List<string>();


        /// <summary>
        /// The list of test assemblies to run tests on.
        /// </summary>
        public ReadOnlyCollection<String> TestAssemblies
        {
            get
            {
                return testAssemblies.AsReadOnly();
            }
        }
        private List<string> testAssemblies = new List<string>();


        /// <summary>
        /// True to open an explorer window to the results folder
        /// when completed successfully, false otherwise.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to explore to the results folder; otherwise, <c>false</c>.
        /// </value>
        public bool ExploreToResultsFolder { get; private set; }


        /// <summary>
        /// Gets a value indicating whether to open the coverage HTML report.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to open the coverage HTML report; otherwise, <c>false</c>.
        /// </value>
        public bool OpenCoverageHtmlReport { get; private set; }


        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber { get; private set; }


        /// <summary>
        /// Gets the test source file path.
        /// </summary>
        public string TestSourceFilePath { get; private set; }


        /// <summary>
        /// Gets the test assembly file path.
        /// </summary>
        public string TestAssemblyFilePath { get; private set; }


        /// <summary>
        /// Gets the coverage tool.
        /// </summary>
        public CoverageTool CoverageTool { get; private set; }
        #endregion        
        

        #region Methods
        /// <summary>
        /// Parse the specified command line arguments.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static CommandLineArguments Parse(string[] args)
        {
            CommandLineArguments commandLineArgs = new CommandLineArguments();
            commandLineArgs.CoverageTool = CoverageTool.OpenCover;

            for(int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                string option = string.Empty;

                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    option = arg.Substring(1);

                    if (option.StartsWith(ConfigFileOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.ConfigFilePath = option.Substring(ConfigFileOption.Length);
                    }
                    else if (option.StartsWith(HelpOptionLong, StringComparison.OrdinalIgnoreCase) ||
                             option.StartsWith(HelpOptionShort, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.ShowHelp = true;
                    }
                    else if (option.StartsWith(TestsOption, StringComparison.OrdinalIgnoreCase))
                    {                        
                        string cmdLineArg = option.Substring(TestsOption.Length);
                        if (!string.IsNullOrEmpty(cmdLineArg))
                        {
                            string[] tests = cmdLineArg.Split(',');
                            IEnumerable<string> trimmed = from t in tests
                                                          where !string.IsNullOrEmpty(t.Trim())
                                                          select t.Trim();
                            commandLineArgs.testsToRun.AddRange(trimmed);
                        }                                                         
                    }                    
                    else if (option.StartsWith(CategoryOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.Category = option.Substring(CategoryOption.Length);
                    }
                    else if (option.StartsWith(NoResultsOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.noResults = true;
                    }
                    else if (option.StartsWith(DetailOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.details.Add(option.Substring(DetailOption.Length));
                    }
                    else if (option.StartsWith(TestAssembliesOption, StringComparison.OrdinalIgnoreCase))
                    {
                        string cmdLineArg = option.Substring(TestAssembliesOption.Length);
                        if (!string.IsNullOrEmpty(cmdLineArg))
                        {
                            string [] testAssemblies = cmdLineArg.Split(',');
                            IEnumerable<string> trimmed = from ta in testAssemblies
                                                          where !string.IsNullOrEmpty(ta.Trim())
                                                          select ta.Trim();
                            commandLineArgs.testAssemblies.AddRange(trimmed);
                        }                                                                                
                    }
                    else if (option.StartsWith(ExploreToResultsFolderOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.ExploreToResultsFolder = true;
                    }
                    else if (option.StartsWith(TestSourceFileLineOption, StringComparison.OrdinalIgnoreCase))
                    {
                        int lineNumber = 0;
                        if (int.TryParse(option.Substring(TestSourceFileLineOption.Length), out lineNumber))
                        {
                            commandLineArgs.LineNumber = lineNumber;
                        }
                        else
                        {
                            commandLineArgs.LineNumber = -1;
                        }
                    }
                    else if (option.StartsWith(TestSourceFilePathOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.TestSourceFilePath = option.Substring(TestSourceFilePathOption.Length);
                    }
                    else if (option.StartsWith(TestAssemblyFilePathOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.TestAssemblyFilePath = option.Substring(TestAssemblyFilePathOption.Length);
                    }
                    else if (option.StartsWith(CoverageToolOption, StringComparison.OrdinalIgnoreCase))
                    {                        
                        CoverageTool coverageTool = CoverageTool.OpenCover;
                        if (Enum.TryParse<CoverageTool>(option.Substring(CoverageToolOption.Length), out coverageTool))
                        {
                            commandLineArgs.CoverageTool = coverageTool;
                        }                        
                    }
                    else if (option.StartsWith(OpenCoverageHtmlReportOption, StringComparison.OrdinalIgnoreCase))
                    {
                        commandLineArgs.OpenCoverageHtmlReport = true;
                    }
                }
            }

            ValidateCommandLineArgs(commandLineArgs);

            return commandLineArgs;
        }


        /// <summary>
        /// Validate the command line arguments.
        /// </summary>
        /// <param name="commandLineArgs">The command line args.</param>
        private static void ValidateCommandLineArgs(CommandLineArguments commandLineArgs)
        {
            commandLineArgs.IsValid = true;

            if (string.IsNullOrEmpty(commandLineArgs.ConfigFilePath))
            {
                commandLineArgs.IsValid = false;
                commandLineArgs.ErrorMessage = "The config file path must be specified";
            }
            else if (!string.IsNullOrEmpty(commandLineArgs.ConfigFilePath))
            {
                if (!File.Exists(commandLineArgs.ConfigFilePath))
                {
                    commandLineArgs.IsValid = false;
                    commandLineArgs.ErrorMessage = string.Format("The specified config file path {0} does not exist", commandLineArgs.ConfigFilePath);
                }
            }      
            else if (commandLineArgs.LineNumber < 0)
            {
                commandLineArgs.IsValid = false;
                commandLineArgs.ErrorMessage = "The linenumber must be a valid integer greater than 0.";
            }
        }


        /// <summary>
        /// Display the help message.
        /// </summary>
        /// <returns></returns>
        public static string GetHelpMessage()
        {
            return
@"Usage: CodeCoverageUtil.exe [options] 
Options are listed below:

/configFile:[config file]       - Specify the config file to use. 
                                  If no config file is specified
                                  a config file called Default.xml
                                  in the current folder will
                                  be used.

/tests:[test name]              - Comma separated list of tests to run.
                                  Can be a namespace, class or method name.                             

/testassemblies:[assembly name] - Comma separated list of test assemblies to run.                                  

/category:[filter]              - Use the specified filter to select tests.
                                  See mstest.exe /category option for more info.                                  

/detail:[property id]           - The name of the property to show values for.
                                  This option can be specified
                                  more than once for different properties.
                                  e.g. /detail:duration
                                  See mstest.exe /detail option for more info.                                  

/noresults                      - Do save the results in a TRX file.
                                  This option improves speed.
                                  See mstest.exe /noresults for more info.

/exploretoresultsfolder         - Open an explorer window to the results
                                  folder when completed successfully.

/testsourcefileline             - Used for Visual Studio integration.
                                  The line number of the selected
                                  namespace, class or method definition to test.

/testsourcefilepath             - Used for Visual Studio integration.
                                  The path to the source file containing
                                  the namespace, class or method definition to test.

/testassemblyfilepath           - Used for Visual Studio integration.
                                  The path to the test assembly containing
                                  the namespace, class or method definition to test.

/coveragetool:[opencover|vs]    - Used to select the coverage tool. Default is opencover.

/help                           - Show this help message.";        
        }
        #endregion
    }
}
