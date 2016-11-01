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
    interface ICommandExecutor
    {
        /// <summary>
        /// Executes the commands.
        /// </summary>
        /// <returns></returns>
        List<Tuple<ICommand, string>> ExecuteCommands();
    }
}
