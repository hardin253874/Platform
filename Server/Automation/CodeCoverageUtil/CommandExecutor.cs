// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeCoverageUtil.Commands;

namespace CodeCoverageUtil
{
    /// <summary>
    /// 
    /// </summary>
    internal class CommandExecutor : ICommandExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutor"/> class.
        /// </summary>
        /// <param name="sessionInfo">The session info.</param>
        public CommandExecutor(SessionInfo sessionInfo)
        {
            this.sessionInfo = sessionInfo;
        }


        /// <summary>
        /// The session info.
        /// </summary>
        public SessionInfo SessionInfo
        {
            get
            {
                return sessionInfo;
            }
        }
        protected SessionInfo sessionInfo;


        /// <summary>
        /// Execute the commands
        /// </summary>
        /// <returns></returns>
        public List<Tuple<ICommand, string>> ExecuteCommands()
        {
            List<ICommand> commandsToExecute = OnCreateCommands();

            List<Tuple<ICommand, string>> errors = new List<Tuple<ICommand, string>>();

            foreach (ICommand command in commandsToExecute)
            {
                try
                {
                    Console.WriteLine();
                    ConsoleHelper.ConsoleWriteLine(ConsoleColor.White, "**** Running command ****");
                    ConsoleHelper.ConsoleWriteLine(ConsoleColor.White, "  * {0}", command.Name);

                    if (errors.Count > 0 && !command.Mandatory)
                    {
                        // Skip running non-mandatory command
                        // because a previous command failed.
                        ConsoleHelper.ConsoleWriteLine(ConsoleColor.Magenta, "  * Skipping command {0} due to previous error(s).", command.Name);
                        Console.WriteLine();
                        continue;
                    }

                    command.Execute();

                    if (!command.Succeeded)
                    {
                        ConsoleHelper.ConsoleWriteLine(ConsoleColor.Red, "  * An error occured running command {0}. Error {1}.", command.Name, command.ErrorMessage);
                        errors.Add(new Tuple<ICommand, string>(command, command.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.ConsoleWriteLine(ConsoleColor.Red, "  * An error occured running command {0}. Error {1}.", command.Name, ex.ToString());
                    errors.Add(new Tuple<ICommand, string>(command, ex.ToString()));
                }
            }

            return errors;
        }


        /// <summary>
        /// Called to create commands.
        /// </summary>
        /// <returns>The list of commands</returns>
        protected virtual List<ICommand> OnCreateCommands()
        {
            return new List<ICommand>();
        }


        /// <summary>
        /// Write any command output to the console
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="CodeCoverageUtil.Commands.OutputDataEventArgs"/> instance containing the event data.</param>
        public static void WriteCommandOutputToConsole(object sender, OutputDataEventArgs args)
        {
            ConsoleHelper.ConsoleWriteLine(ConsoleColor.Gray, args.Data);
        }
    }
}
