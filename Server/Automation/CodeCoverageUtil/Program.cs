// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Coverage.Analysis;
using System.Xml;
using CodeCoverageUtil.Commands;
using CodeCoverageUtil.Configuration;
using System.Threading;
using System.Text.RegularExpressions;

namespace CodeCoverageUtil
{
    class Program
    {
        #region Constants
        private const string DefaultConfigFileName = "Default.xml";        
        #endregion        
                

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main(string[] args)
        {
            Mutex mutex = null;

            try
            {
                Console.WriteLine("**** Starting ****");
                Console.WriteLine("  * Command line {0}", Environment.CommandLine);

                bool createdNew = false;
                mutex = new Mutex(false, "CodeCoverageUtil_Mutex", out createdNew);
                try
                {
                    Console.WriteLine("  * Waiting for any running CodeCoverageUtil instances to complete.");
                    mutex.WaitOne();
                    Console.WriteLine("  * No instances detected, starting now.");
                }
                catch(AbandonedMutexException)
                {
                }

                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string defaultConfigFilePath = Path.Combine(currentDirectory, DefaultConfigFileName);

                #region Process Command Line Arguments
                CommandLineArguments commandLineArgs = CommandLineArguments.Parse(args);
                if (commandLineArgs.ShowHelp)
                {
                    Console.WriteLine(CommandLineArguments.GetHelpMessage());
                    return;
                }

                if (!commandLineArgs.IsValid &&
                    !File.Exists(defaultConfigFilePath))
                {
                    ConsoleHelper.ConsoleWriteLine(ConsoleColor.Red, commandLineArgs.ErrorMessage);
                    Console.WriteLine();
                    Console.WriteLine(CommandLineArguments.GetHelpMessage());
                    return;
                }

                string configFile = commandLineArgs.ConfigFilePath;
                if (string.IsNullOrEmpty(configFile) &&
                    File.Exists(defaultConfigFilePath))
                {
                    configFile = defaultConfigFilePath;
                }
                #endregion

                #region Load Configuration
                CodeCoverageUtilConfiguration config = new CodeCoverageUtilConfiguration(configFile);                
                SessionInfo sessionInfo = new SessionInfo(commandLineArgs, config);

                if (sessionInfo.CommandLineArguments.TestAssemblies.Count > 0)
                {
                    bool isValidTestAssembly = false;
                    foreach (TestAssemblyConfig testAssemblyConfig in sessionInfo.Configuration.TestAssembliesConfig)
                    {                        
                        string fileName = Path.GetFileName(testAssemblyConfig.AssemblyPath).ToUpper();

                        int count = (from fn in sessionInfo.CommandLineArguments.TestAssemblies
                                     where Path.GetFileName(fn).ToUpper() == fileName
                                     select fn).Count();
                        if (count != 0)
                        {
                            isValidTestAssembly = true;
                            break;
                        }                        
                    }
                    if (!isValidTestAssembly)
                    {
                        Console.WriteLine();
                        ConsoleHelper.ConsoleWriteLine(ConsoleColor.Red, "None of the specified assemblies are valid test assemblies, exiting.");
                        return;
                    }
                }

                if (sessionInfo.TestCodeContext != null &&
                    !sessionInfo.TestCodeContext.IsValid)
                {                    
                    Console.WriteLine();
                    ConsoleHelper.ConsoleWriteLine(ConsoleColor.Red, "The selected source line is invalid. Select either a namespace, class or method definition, exiting.");
                    return;                    
                }
                #endregion                

                #region Create and Run Commands
                if (Directory.Exists(sessionInfo.ResultsDirectory))
                {
                    Directory.Delete(sessionInfo.ResultsDirectory, true);
                }

                ICommandExecutor commandExecutor = CreateCommandExecutor(sessionInfo);                
                List<Tuple<ICommand, string>> errors = commandExecutor.ExecuteCommands();
                bool succeeded = (errors.Count == 0);
                #endregion

                #region Write Summary Information
                ConsoleColor summaryColor = ConsoleColor.Green;
                if (!succeeded)
                {
                    summaryColor = ConsoleColor.Red;
                }
                Console.WriteLine();
                ConsoleHelper.ConsoleWriteLine(summaryColor, "**** Summary ****");
                if (!succeeded)
                {
                    ConsoleHelper.ConsoleWriteLine(summaryColor, " * Failed. The following errors occurred:");
                    foreach (Tuple<ICommand, string> error in errors)
                    {
                        ConsoleHelper.ConsoleWriteLine(summaryColor, "   * Command:{0}", error.Item1.Name);
                        ConsoleHelper.ConsoleWriteLine(summaryColor, "   * Error:{0}", error.Item2);
                        Console.WriteLine();
                    }
                }
                else
                {
                    if (config.KeepHistory)
                    {
                        string savedResultsDirectory = Path.Combine(config.ResultsPath, string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss")));
                        CopyDirectory(sessionInfo.ResultsDirectory, savedResultsDirectory);
                    }

                    ConsoleHelper.ConsoleWriteLine(summaryColor, " * Succeeded. Results written to directory: {0}", sessionInfo.ResultsDirectory);                    
                }

                if (commandLineArgs.ExploreToResultsFolder)
                {
                    if (File.Exists(sessionInfo.CoverageResultsFile))
                    {
                        Process.Start("explorer.exe", string.Format("/select,\"{0}\"", sessionInfo.CoverageResultsFile));
                    }
                    else
                    {
                        Process.Start("explorer.exe", string.Format("/root,\"{0}\"", sessionInfo.ResultsDirectory));
                    }
                }

                if (commandLineArgs.OpenCoverageHtmlReport)
                {
                    if (Directory.Exists(sessionInfo.CoverageReportDirectory))
                    {
                        Process.Start(Path.Combine(sessionInfo.CoverageReportDirectory,"index.htm"));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ConsoleHelper.ConsoleWriteLine(ConsoleColor.Red, "An unexpected exception occurred. Error {0}", ex.ToString());
            }            
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Dispose();
                }
            }
        }


        /// <summary>
        /// Copies the directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="destinationDirectory">The destination directory.</param>
        private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                return;
            }

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            string [] sourceFiles = Directory.GetFiles(sourceDirectory);

            foreach (string sourceFile in sourceFiles)
            {
                string fileName = Path.GetFileName(sourceFile);
                string destinationFile = Path.Combine(destinationDirectory, fileName);

                File.Copy(sourceFile, destinationFile);
            }

            string[] childDirectories = Directory.GetDirectories(sourceDirectory);
            foreach (string childDirectory in childDirectories)
            {
                string directoryName = Path.GetFileName(childDirectory);
                string destinationChildDir = Path.Combine(destinationDirectory, directoryName);
                CopyDirectory(childDirectory, destinationChildDir);
            }
        }


        /// <summary>
        /// Create the commands to run
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        /// <returns></returns>
        private static ICommandExecutor CreateCommandExecutor(SessionInfo sessionInfo)
        {
            ICommandExecutor commandExecutor = null;

            switch (sessionInfo.CommandLineArguments.CoverageTool)
            {
                case CoverageTool.OpenCover:
                    commandExecutor = new OpenCoverCommandExecutor(sessionInfo);
                    break;
                case CoverageTool.VS:
                    commandExecutor = new VisualStudioCommandExecutor(sessionInfo);                    
                    break;
            }

            return commandExecutor;
        }                
    }                
}
