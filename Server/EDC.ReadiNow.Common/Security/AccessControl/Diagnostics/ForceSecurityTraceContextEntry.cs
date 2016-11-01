// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl.Diagnostics
{
    /// <summary>
    /// An entry in the context for <see cref="ForceSecurityTraceContext"/>.
    /// </summary>
    internal class ForceSecurityTraceContextEntry
    {
        /// <summary>
        /// Create a new <see cref="ForceSecurityTraceContextEntry"/>.
        /// </summary>
        /// <param name="entitiesToTrace">
        /// The entities to trace. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entitiesToTrace"/> cannot be null.
        /// </exception>
        public ForceSecurityTraceContextEntry(IEnumerable<long> entitiesToTrace)
        {
            if (entitiesToTrace == null)
            {
                throw new ArgumentNullException("entitiesToTrace");
            }

            EntitiesToTrace = new HashSet<long>();
            EntitiesToTrace.UnionWith(entitiesToTrace);
        }

        /// <summary>
        /// The entities to trace.
        /// </summary>
        public ISet<long> EntitiesToTrace { get; private set; }
    }
}
