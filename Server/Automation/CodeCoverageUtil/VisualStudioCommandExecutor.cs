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
    internal class VisualStudioCommandExecutor : CommandExecutor
    {
        private const string TestResultsFileName = "{0}_TestResults.trx";


        /// <summary>
        /// Initializes a new instance of the <see cref="VisualStudioCommandExecutor"/> class.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        public VisualStudioCommandExecutor(SessionInfo sessionInfo)
            :base(sessionInfo)
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
            // Instrument and publish assemblies
            commandsToExecute.AddRange(CreateInstrumentAssembliesCommands(sessionInfo));
            commandsToExecute.AddRange(CreatePublishInstrumentedAssembliesCommands(sessionInfo));
            // Start the vsperfmon
            commandsToExecute.Add(CreateStartVsPerfMonCommand(sessionInfo.CoverageResultsFile));
            // Run any startup commands
            commandsToExecute.AddRange(sessionInfo.Configuration.StartUpCommands);
            // Run MsTest
            commandsToExecute.AddRange(CreateRunMsTestCommands(sessionInfo));
            // Shutdown the performance monitor
            commandsToExecute.Add(CreateShutDownVsPerfMonCommand());
            // Export the coverage file to xml
            commandsToExecute.Add(CreateExportCoverageFileToXmlCommand(sessionInfo));
            // Run any shutdown commands
            commandsToExecute.AddRange(sessionInfo.Configuration.ShutdownCommands);
            // Restore any pre existing published assemblies
            commandsToExecute.AddRange(CreateRestoreInstrumentedAssembliesCommands(sessionInfo));
            // Run any startup commands
            commandsToExecute.AddRange(sessionInfo.Configuration.StartUpCommands);

            return commandsToExecute;
        }


        /// <summary>
        /// Create commands to instrument the assemblies for code coverage
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static List<ICommand> CreateInstrumentAssembliesCommands(SessionInfo sessionInfo)
        {
            List<ICommand> commands = new List<ICommand>();

            // Instrument the code coverage assemblies
            foreach (AssemblyUnderTestConfig assemblyUnderTest in sessionInfo.Configuration.AssembliesUnderTestConfig)
            {
                VsInstrCommand vsInstrCommand = new VsInstrCommand(string.Format("\"{0}\" /outputpath:\"{1}\" /coverage", assemblyUnderTest.AssemblyPath, sessionInfo.AssembliesDirectory), false);
                vsInstrCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);

                commands.Add(vsInstrCommand);
            }

            return commands;
        }


        /// <summary>
        /// Create commands to publish the instrumented assemblies.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static List<ICommand> CreatePublishInstrumentedAssembliesCommands(SessionInfo sessionInfo)
        {
            List<ICommand> commands = new List<ICommand>();

            // Publish the code coverage assemblies
            foreach (AssemblyUnderTestConfig assemblyUnderTest in sessionInfo.Configuration.AssembliesUnderTestConfig)
            {
                string assemblyPath = Path.Combine(sessionInfo.AssembliesDirectory, Path.GetFileName(assemblyUnderTest.AssemblyPath));

                if (assemblyUnderTest.IsStrongNamed)
                {
                    SnCommand snCommand = new SnCommand(string.Format("-Vr \"{0}\"", assemblyUnderTest.AssemblyPath), false);
                    snCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                    commands.Add(snCommand);
                }

                PublishAssemblyCommand publishAssemblyCommand = new PublishAssemblyCommand(assemblyPath, assemblyUnderTest.PublishPath, sessionInfo.AssemblyBackupDirectory, false);
                publishAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(publishAssemblyCommand);
            }

            // Publish the test assemblies
            foreach (TestAssemblyConfig testAssemblyConfig in sessionInfo.Configuration.TestAssembliesConfig)
            {
                if (!string.IsNullOrEmpty(testAssemblyConfig.PublishPath))
                {
                    PublishAssemblyCommand publishAssemblyCommand = new PublishAssemblyCommand(testAssemblyConfig.AssemblyPath, testAssemblyConfig.PublishPath, sessionInfo.AssemblyBackupDirectory, false);
                    publishAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                    commands.Add(publishAssemblyCommand);
                }
            }

            // Publish the referenced assemblies
            foreach (ReferencedAssemblyConfig referenecedAssemblyConfig in sessionInfo.Configuration.ReferencedAssembliesConfig)
            {
                PublishAssemblyCommand publishAssemblyCommand = new PublishAssemblyCommand(referenecedAssemblyConfig.AssemblyPath, referenecedAssemblyConfig.PublishPath, sessionInfo.AssemblyBackupDirectory, false);
                publishAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(publishAssemblyCommand);
            }

            return commands;
        }


        /// <summary>
        /// Create a command to start the VS performance monitor for code coverage.
        /// </summary>
        /// <param name="coverageResultsFile">The coverage results file.</param>
        /// <returns></returns>
        private static ICommand CreateStartVsPerfMonCommand(string coverageResultsFile)
        {
            VsPerfMonCommand vsPerfMonCommand = new VsPerfMonCommand(string.Format("/coverage /output:\"{0}\"", coverageResultsFile), false);
            vsPerfMonCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
            return vsPerfMonCommand;
        }


        /// <summary>
        /// Create commands to run MsTest.exe
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static List<ICommand> CreateRunMsTestCommands(SessionInfo sessionInfo)
        {
            List<ICommand> testCommands = new List<ICommand>();

            StringBuilder invariantArgsBuilder = new StringBuilder();

            invariantArgsBuilder.AppendFormat("/testsettings:\"{0}\" ", sessionInfo.Configuration.TestSettingsPath);
            foreach (string testToRun in sessionInfo.CommandLineArguments.TestsToRun)
            {
                invariantArgsBuilder.AppendFormat("/test:\"{0}\" ", testToRun);
            }

            if (sessionInfo.TestCodeContext != null)
            {
                if (sessionInfo.TestCodeContext.IsValid)
                {
                    invariantArgsBuilder.AppendFormat("/test:\"{0}\" ", sessionInfo.TestCodeContext.ToString());
                }
                else
                {
                    throw new InvalidOperationException("The test context is invalid.");
                }
            }

            if (!string.IsNullOrEmpty(sessionInfo.CommandLineArguments.Category))
            {
                invariantArgsBuilder.AppendFormat("/category:\"{0}\" ", sessionInfo.CommandLineArguments.Category);
            }

            if (sessionInfo.NoResults)
            {
                invariantArgsBuilder.AppendFormat("/noresults ");
            }

            foreach (string detail in sessionInfo.Details)
            {
                invariantArgsBuilder.AppendFormat("/detail:\"{0}\" ", detail);
            }

            foreach (TestAssemblyConfig testAssemblyConfig in sessionInfo.Configuration.TestAssembliesConfig)
            {
                if (sessionInfo.CommandLineArguments.TestAssemblies.Count > 0)
                {
                    string fileName = Path.GetFileName(testAssemblyConfig.AssemblyPath).ToUpper();

                    int count = (from fn in sessionInfo.CommandLineArguments.TestAssemblies
                                 where Path.GetFileName(fn).ToUpper() == fileName
                                 select fn).Count();
                    if (count == 0)
                    {
                        continue;
                    }
                }

                StringBuilder argsBuilder = new StringBuilder();

                string testResultsFile = string.Empty;

                string testAssemblyPath = string.Empty;

                if (!string.IsNullOrEmpty(testAssemblyConfig.PublishPath))
                {
                    testAssemblyPath = Path.Combine(testAssemblyConfig.PublishPath, Path.GetFileName(testAssemblyConfig.AssemblyPath));
                }
                else
                {
                    testAssemblyPath = testAssemblyConfig.AssemblyPath;
                }

                argsBuilder.AppendFormat("/testcontainer:\"{0}\" ", testAssemblyPath);
                if (!sessionInfo.NoResults)
                {
                    testResultsFile = Path.Combine(sessionInfo.ResultsDirectory, string.Format(TestResultsFileName, Path.GetFileNameWithoutExtension(testAssemblyConfig.AssemblyPath)));
                    argsBuilder.AppendFormat("/resultsfile:\"{0}\" ", testResultsFile);
                }
                argsBuilder.Append(invariantArgsBuilder);

                MSTestCommand msTestCommand = new MSTestCommand(argsBuilder.ToString(), testResultsFile, false);
                msTestCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);

                testCommands.Add(msTestCommand);
            }

            return testCommands;
        }


        /// <summary>
        /// Create a command to shutdown the VS performance monitor
        /// </summary>
        /// <returns></returns>
        private static ICommand CreateShutDownVsPerfMonCommand()
        {
            VsPerfCmdCommand vsPerfCmdCommand = new VsPerfCmdCommand("/shutdown", true);
            vsPerfCmdCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
            return vsPerfCmdCommand;
        }


        /// <summary>
        /// Create command to export the coverage file to xml.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static ICommand CreateExportCoverageFileToXmlCommand(SessionInfo sessionInfo)
        {
            IEnumerable<string> assemblyPaths = from c in sessionInfo.Configuration.AssembliesUnderTestConfig
                                                select c.AssemblyPath;

            ExportCoverageFileCommand command = new ExportCoverageFileCommand(sessionInfo.CoverageResultsFile, sessionInfo.AssembliesDirectory, assemblyPaths, false);
            command.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
            return command;
        }


        /// <summary>
        /// Create commands to restore any pre-existing published assemblies
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static List<ICommand> CreateRestoreInstrumentedAssembliesCommands(SessionInfo sessionInfo)
        {
            List<ICommand> commands = new List<ICommand>();

            foreach (AssemblyUnderTestConfig assemblyUnderTest in sessionInfo.Configuration.AssembliesUnderTestConfig)
            {
                RestoreAssemblyCommand restoreAssemblyCommand = new RestoreAssemblyCommand(Path.GetFileName(assemblyUnderTest.AssemblyPath), sessionInfo.AssemblyBackupDirectory, true);
                restoreAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(restoreAssemblyCommand);

                if (assemblyUnderTest.IsStrongNamed)
                {
                    SnCommand snCommand = new SnCommand(string.Format("-Vu \"{0}\"", assemblyUnderTest.AssemblyPath), true);
                    snCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                    commands.Add(snCommand);
                }
            }

            // Restore the test assemblies
            foreach (TestAssemblyConfig testAssemblyConfig in sessionInfo.Configuration.TestAssembliesConfig)
            {
                RestoreAssemblyCommand restoreAssemblyCommand = new RestoreAssemblyCommand(Path.GetFileName(testAssemblyConfig.AssemblyPath), sessionInfo.AssemblyBackupDirectory, true);
                restoreAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(restoreAssemblyCommand);
            }

            foreach (ReferencedAssemblyConfig referencedAssemblyConfig in sessionInfo.Configuration.ReferencedAssembliesConfig)
            {
                RestoreAssemblyCommand restoreAssemblyCommand = new RestoreAssemblyCommand(Path.GetFileName(referencedAssemblyConfig.AssemblyPath), sessionInfo.AssemblyBackupDirectory, true);
                restoreAssemblyCommand.OutputDataReceived += new EventHandler<OutputDataEventArgs>(WriteCommandOutputToConsole);
                commands.Add(restoreAssemblyCommand);
            }

            return commands;
        }
    }
}
