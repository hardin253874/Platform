// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Settings for evaluating calculated fields.
    /// </summary>
    public class CalculatedFieldSettings
    {
        /// <summary>
        /// Default settings.
        /// </summary>
        public static readonly CalculatedFieldSettings Default = new CalculatedFieldSettings();

        /// <summary>
        /// Time zone to use when performing calculations.
        /// </summary>
        public string TimeZone { get; set; }
    }
}
