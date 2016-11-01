// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeCoverageUtil.Commands;
using CodeCoverageUtil.Configuration;
using System.IO;

namespace CodeCoverageUtil
{
    /// <summary>
    /// 
    /// </summary>
    internal class OpenCoverCommandExecutor : CommandExecutor
    {
        private const string OpenCoverMsTestResultsFileName = "{0}_TestResults.trx";
        private const string OpenCoverNunitTestResultsFileName = "{0}_TestResults.xml";
        private const string OpenCoverCoverageResultsFileName = "{0}_CoverageResults.xml";


        /// <summary>
        /// Initializes a new instance of the <see cref="OpenCoverCommandExecutor"/> class.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        public OpenCoverCommandExecutor(SessionInfo sessionInfo)
            : base(sessionInfo)
        {
        }


        /// <summary>
        /// Called to create commands.
        /// </summary>
        /// <returns>
        /// The list of commands
        /// </returns>
        protected override List<ICommand> OnCreateCommands()
        {
            List<ICommand> commandsToExecute = new List<ICommand>();

            // Add all the shutdown commands
            commandsToExecute.AddRange(sessionInfo.Configuration.ShutdownCommands);            
            // Copy the assembly pdbs to the pdb folder
            commandsToExecute.AddRange(CreateCopyAssembliesCommands(sessionInfo));
            // Run any startup commands
            commandsToExecute.AddRange(sessionInfo.Configuration.StartUpCommands);
            // Run open cover which will run mstest
            commandsToExecute.AddRange(CreateRunOpenCoverConsoleCommands(sessionInfo));            
            // Run the report generator to generate a report
            ICommand command = CreateRunReportGeneratorCommand(sessionInfo);
            if (command != null)
            {
                commandsToExecute.Add(command);
            }

            return commandsToExecute;
        }
        

        /// <summary>
        /// Create commands to publish the assemblies under test
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static List<ICommand> CreateCopyAssembliesCommands(SessionInfo sessionInfo)
        {
            List<ICommand> commands = new List<ICommand>();

            // Publish the code coverage assemblies
            foreach (AssemblyUnderTestConfig assemblyUnderTest in sessionInfo.Configuration.AssembliesUnderTestConfig)
            {
                PublishAssemblyCommand publishAssemblyCommand = new PublishAssemblyCommand(assemblyUnderTest.AssemblyPath, sessionInfo.AssembliesDirectory, null, false);
                publishAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(publishAssemblyCommand);

                string pdbFileName = Path.GetFileNameWithoutExtension(assemblyUnderTest.AssemblyPath) + ".pdb";
                string pdbFilePath = Path.Combine(Path.GetDirectoryName(assemblyUnderTest.AssemblyPath), pdbFileName);
                PublishAssemblyCommand publishPdbCommand = new PublishAssemblyCommand(pdbFilePath, sessionInfo.AssembliesDirectory, null, false);
                publishPdbCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(publishPdbCommand);
            }

            return commands;
        }


        /// <summary>
        /// Create a command to start the VS performance monitor for code coverage.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static List<ICommand> CreateRunOpenCoverConsoleCommands(SessionInfo sessionInfo)
        {
            List<ICommand> commands = new List<ICommand>();

            foreach (TestAssemblyConfig testAssemblyConfig in sessionInfo.Configuration.TestAssembliesConfig)
            {
                if (!IncludeTestAssembly(sessionInfo, testAssemblyConfig))
                {
                    continue;
                }

                string nunitTestArgs = GetNUnitTestArgs(sessionInfo, testAssemblyConfig);                               

                string opencoverResultsFile = Path.Combine(sessionInfo.ResultsDirectory, string.Format(OpenCoverCoverageResultsFileName, Path.GetFileNameWithoutExtension(testAssemblyConfig.AssemblyPath)));
                string nunitResultsFile = Path.Combine(sessionInfo.ResultsDirectory, string.Format(OpenCoverNunitTestResultsFileName, Path.GetFileNameWithoutExtension(testAssemblyConfig.AssemblyPath)));
                
                StringBuilder filtersArgsBuilder = new StringBuilder();

                // Include assemblies under test
                foreach (AssemblyUnderTestConfig assemblyUnderTestConfig in sessionInfo.Configuration.AssembliesUnderTestConfig)
                {
                    filtersArgsBuilder.AppendFormat("+[{0}]* ", Path.GetFileNameWithoutExtension(assemblyUnderTestConfig.AssemblyPath));
                }

                StringBuilder openCoverArgsBuilder = new StringBuilder();
                openCoverArgsBuilder.Append("-register:user ");
                openCoverArgsBuilder.AppendFormat("-targetdir:\"{0}\" ", sessionInfo.AssembliesDirectory);
                openCoverArgsBuilder.AppendFormat("-target:\"{0}\" ", Settings.Default.NunitExePath);
                openCoverArgsBuilder.AppendFormat("-targetargs:\"{0}\" ", nunitTestArgs);
                openCoverArgsBuilder.AppendFormat("-filter:\"{0}\" ", filtersArgsBuilder.ToString());
                openCoverArgsBuilder.AppendFormat("-output:\"{0}\" ", opencoverResultsFile);
                openCoverArgsBuilder.Append("-mergebyhash ");

                ICommand opencoverCommand = new OpenCoverConsoleCommand(openCoverArgsBuilder.ToString(), nunitResultsFile, false);
                opencoverCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);

                commands.Add(opencoverCommand);
            }                      

            return commands;
        }


        /// <summary>
        /// Creates the run report generator command.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static ICommand CreateRunReportGeneratorCommand(SessionInfo sessionInfo)
        {
            StringBuilder resultFiles = new StringBuilder();

            foreach (TestAssemblyConfig testAssemblyConfig in sessionInfo.Configuration.TestAssembliesConfig)
            {
                if (!IncludeTestAssembly(sessionInfo, testAssemblyConfig))
                {
                    continue;
                }

                string opencoverResultsFile = Path.Combine(sessionInfo.ResultsDirectory, string.Format(OpenCoverCoverageResultsFileName, Path.GetFileNameWithoutExtension(testAssemblyConfig.AssemblyPath)));

                if (resultFiles.Length > 0)
                {
                    resultFiles.Append(";");   
                }

                resultFiles.AppendFormat("{0}", opencoverResultsFile);                
            }            

            ICommand reportGenCommand = null;
            
            if (resultFiles.Length > 0)
            {                
                if (Directory.Exists(sessionInfo.CoverageReportDirectory))
                {
                    Directory.Delete(sessionInfo.CoverageReportDirectory, true);
                }

                StringBuilder reportArgsBuilder = new StringBuilder();

                reportArgsBuilder.AppendFormat("\"{0}\" ", resultFiles.ToString());
                reportArgsBuilder.AppendFormat("\"{0}\"", sessionInfo.CoverageReportDirectory);                

                reportGenCommand = new ReportGeneratorCommand(reportArgsBuilder.ToString(), false);
                reportGenCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
            }

            return reportGenCommand;
        }


        /// <summary>
        /// Includes the test assembly.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <param name="testAssemblyConfig">The test assembly config.</param>
        /// <returns></returns>
        private static bool IncludeTestAssembly(SessionInfo sessionInfo, TestAssemblyConfig testAssemblyConfig)
        {
            bool includeTestAssembly = true;

            if (sessionInfo.CommandLineArguments.TestAssemblies.Count > 0)
            {
                string fileName = Path.GetFileName(testAssemblyConfig.AssemblyPath).ToUpper();

                int count = (from fn in sessionInfo.CommandLineArguments.TestAssemblies
                             where Path.GetFileName(fn).ToUpper() == fileName
                             select fn).Count();
                if (count == 0)
                {
                    includeTestAssembly = false;
                }
            }

            return includeTestAssembly;
        }


        /// <summary>
        /// Create commands to run MsTest.exe
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <param name="testAssemblyConfig">The test assembly config.</param>
        /// <returns></returns>
        private static string GetMsTestCommandArgs(SessionInfo sessionInfo, TestAssemblyConfig testAssemblyConfig)
        {
            StringBuilder mstestArgsBuilder = new StringBuilder();                        

            string testAssemblyPath = testAssemblyConfig.AssemblyPath;

            mstestArgsBuilder.AppendFormat("/testcontainer:\"{0}\" ", testAssemblyPath);            
            
            mstestArgsBuilder.AppendFormat("/testsettings:\"{0}\" ", sessionInfo.Configuration.TestSettingsPath);
            foreach (string testToRun in sessionInfo.CommandLineArguments.TestsToRun)
            {
                mstestArgsBuilder.AppendFormat("/test:\"{0}\" ", testToRun);
            }

            if (sessionInfo.TestCodeContext != null)
            {
                if (sessionInfo.TestCodeContext.IsValid)
                {
                    mstestArgsBuilder.AppendFormat("/test:\"{0}\" ", sessionInfo.TestCodeContext.ToString());
                }
                else
                {
                    throw new InvalidOperationException("The test context is invalid.");
                }
            }

            if (!string.IsNullOrEmpty(sessionInfo.CommandLineArguments.Category))
            {
                mstestArgsBuilder.AppendFormat("/category:\"{0}\" ", sessionInfo.CommandLineArguments.Category);
            }

            if (sessionInfo.NoResults)
            {
                mstestArgsBuilder.AppendFormat("/noresults ");
            }
            else
            {
                string mstestResultsFile = Path.Combine(sessionInfo.ResultsDirectory, string.Format(OpenCoverMsTestResultsFileName, Path.GetFileNameWithoutExtension(testAssemblyConfig.AssemblyPath)));
                mstestArgsBuilder.AppendFormat("/resultsfile:\"{0}\" ", mstestResultsFile);
            }

            foreach (string detail in sessionInfo.Details)
            {
                mstestArgsBuilder.AppendFormat("/detail:\"{0}\" ", detail);
            }

            mstestArgsBuilder = mstestArgsBuilder.Replace("\"", "\\\"");            

            return mstestArgsBuilder.ToString();
        }


        /// <summary>
        /// Gets the N unit test args.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <param name="testAssemblyConfig">The test assembly config.</param>
        /// <returns></returns>
        private static string GetNUnitTestArgs(SessionInfo sessionInfo, TestAssemblyConfig testAssemblyConfig)
        {
            StringBuilder nunitArgsBuilder = new StringBuilder();

            string testAssemblyPath = testAssemblyConfig.AssemblyPath;

            nunitArgsBuilder.AppendFormat("\"{0}\" ", testAssemblyPath);

            StringBuilder nunitTestBuilder = new StringBuilder();            

            bool firstTest = true;

            foreach (string testToRun in sessionInfo.CommandLineArguments.TestsToRun)
            {
                if (!firstTest)
                {
                    nunitTestBuilder.Append(",");                
                }
                nunitTestBuilder.AppendFormat("{0}", testToRun);
                firstTest = false;
            }

            if (sessionInfo.TestCodeContext != null)
            {
                if (sessionInfo.TestCodeContext.IsValid)
                {
                    if (!firstTest)
                    {
                        nunitTestBuilder.Append(",");
                    }
                    nunitTestBuilder.AppendFormat("{0}", sessionInfo.TestCodeContext.ToString());
                }
                else
                {
                    throw new InvalidOperationException("The test context is invalid.");
                }
            }

            if (nunitTestBuilder.Length > 0)
            {
                nunitArgsBuilder.AppendFormat("/run:\"{0}\" ", nunitTestBuilder.ToString());                
            }

            if (!string.IsNullOrEmpty(sessionInfo.CommandLineArguments.Category))
            {
                nunitArgsBuilder.AppendFormat("/include:\"{0}\" ", sessionInfo.CommandLineArguments.Category);
            }

            if (sessionInfo.NoResults)
            {
                nunitArgsBuilder.AppendFormat("/noresult ");
            }
            else
            {
                string nunitResultsFile = Path.Combine(sessionInfo.ResultsDirectory, string.Format(OpenCoverNunitTestResultsFileName, Path.GetFileNameWithoutExtension(testAssemblyConfig.AssemblyPath)));
                nunitArgsBuilder.AppendFormat("/result:\"{0}\" ", nunitResultsFile);
            }            

            nunitArgsBuilder = nunitArgsBuilder.Replace("\"", "\\\"");

            return nunitArgsBuilder.ToString();
        }
    }
}
