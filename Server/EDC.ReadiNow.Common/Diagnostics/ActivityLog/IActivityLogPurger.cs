// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Diagnostics.ActivityLog
{
    /// <summary>
    /// Interface for things that delete values from the activity log, usually purging.
    /// </summary>
    public interface IActivityLogPurger
    {
        /// <summary>
        /// Purge any excess entries.
        /// </summary>
        void Purge();
    }
}
