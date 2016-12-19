// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.Syslog
{
    /// <summary>
    /// Represents a syslog structured data parameter.
    /// </summary>
    public class SyslogSdParameter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SyslogSdParameter" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public SyslogSdParameter(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof(name));
            }

            Name = name;
            Value = value;
        }


        /// <summary>
        ///     Gets the name of the parameter.
        /// </summary>
        /// <value>
        ///     The name of the parameter.
        /// </value>
        public string Name { get; private set; }


        /// <summary>
        ///     Gets the parameter value.
        /// </summary>
        /// <value>
        ///     The parameter value.
        /// </value>
        public string Value { get; private set; }
    }
}