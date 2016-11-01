// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;

namespace EDC.ReadiNow.Metadata.Solutions
{
    /// <summary>
    /// Defines the solution installation actions.
    /// </summary>
    [Serializable]
    public enum SolutionInstallAction
    {
        /// <summary>
        /// Specifies the solution installation action is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Specifies that the solution is being installed.
        /// </summary>
        Install,

        /// <summary>
        /// Specifies that the solution is being upgraded.
        /// </summary>
        Upgrade,

        /// <summary>
        /// Specifies that the solution is being uninstalled.
        /// </summary>
        Uninstall
    }
}
