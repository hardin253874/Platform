// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.Syslog
{
    /// <summary>
    /// Represents a syslog structured data element.
    /// </summary>
    public class SyslogSdElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyslogSdElement" /> class.
        /// </summary>
        /// <param name="sdId">The sd identifier.</param>
        /// <exception cref="System.ArgumentNullException">sdId</exception>
        public SyslogSdElement(string sdId)
        {
            if (string.IsNullOrWhiteSpace(sdId))
            {
                throw new ArgumentNullException( nameof(sdId));
            }

            SdId = sdId;

            Parameters = new List<SyslogSdParameter>();
        }


        /// <summary>
        ///     Gets the sd identifier.
        /// </summary>
        /// <value>
        ///     The sd identifier.
        /// </value>
        public string SdId { get; private set; }
        

        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public IList<SyslogSdParameter> Parameters { get; private set; }
    }
}