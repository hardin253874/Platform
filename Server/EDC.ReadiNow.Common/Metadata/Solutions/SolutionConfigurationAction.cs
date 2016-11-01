// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;

namespace EDC.ReadiNow.Metadata.Solutions
{
    /// <summary>
    /// Defines the solution configuration actions.
    /// </summary>
    [Serializable]
    public enum SolutionConfigurationAction
    {
        /// <summary>
        /// Specifies the solution configuration action is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Specifies that the solution is being configured to activate a new tenant.
        /// </summary>
        ActivateTenant,

        /// <summary>
        /// Specifies that the solution is being configured to deactivate an existing tenant.
        /// </summary>
        DeactivateTenant
    }
}
