// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Database;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Settings that get passed into the static builder.
    /// </summary>
    public class BuilderSettings
    {
        public BuilderSettings()
        {
            RootContextType = new ExprType(DataType.None);
            DefaultDecimalPrecision = 3;
            DefaultCurrencyPlaces = 2;
            ScriptHost = ScriptHostType.Any;
        }

        /// <summary>
        /// Called by the StaticBuilder to gain access about the root context type.
        /// </summary>
        public ExprType RootContextType { get; set; }


        /// <summary>
        /// Optional, may be null. A final cast that will get applied to the output expression if necessary.
        /// </summary>
        public ExprType ExpectedResultType { get; set; }


        /// <summary>
        /// Specifies how the script will be used.
        /// </summary>
        public ScriptHostType ScriptHost { get; set; }


        /// <summary>
        /// The script is not user-entered. I.e. it comes from an API use.
        /// (We allow the id() function in this scenario only, as we don't want customers relying on IDs)
        /// </summary>
        public bool ScriptHostIsApi { get; set; }


        /// <summary>
        /// If true, then aliases are extracted for use in expressions.
        /// </summary>
        public bool TestMode { get; set; }


        /// <summary>
        /// Default number of decimal places for decimal types.
        /// </summary>
        public int DefaultDecimalPrecision { get; set; }


        /// <summary>
        /// Default number of decimal places for currency types.
        /// </summary>
        public int DefaultCurrencyPlaces { get; set; }


        /// <summary>
        /// Complete list of parameter names.
        /// </summary>
        public List<string> ParameterNames { get; set; }


        /// <summary>
        /// Set by the host to allow the resolution of named parameters.
        /// </summary>
        /// <remarks>
        /// Accept a parameter name (without the @ symbol), and return the type of that parameter.
        /// If the parameter cannot be resolved, then return null.
        /// </remarks>
        public Func<string, ExprType> StaticParameterResolver { get; set; }

    }
}
