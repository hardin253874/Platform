// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Keeps track of parameter names that were passed in during compilation.
    /// </summary>
    /// <remarks>
    /// The names are split so that we can parse parameter names with dots.
    /// See #18298 - Calculation language should accept dotted parameter names without needing to escape the entire sequence.
    /// </remarks>
    public class ParameterBag
    {
        private readonly HashSet<string> _fullNames = new HashSet<string>();

        private readonly HashSet<string> _partialNames = new HashSet<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public ParameterBag(BuilderSettings settings)
        {
            if (settings == null || settings.ParameterNames == null)
                return;

            foreach (string name in settings.ParameterNames)
            {
                _fullNames.Add(name);
                
                string[] parts = name.Split('.');
                if (parts.Length > 1)
                {
                    string partial = "";
                    for (int i=0; i<parts.Length-1; i++)    // don't add last part
                    {
                        if (i != 0)
                            partial += ".";
                        partial += parts[i];
                        _partialNames.Add(partial);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the text is a known parameter.
        /// </summary>
        public bool HasParameter( string name )
        {
            return _fullNames.Contains(name);
        }

        /// <summary>
        /// Returns true if the text is a left-hand substring of a known parameter.
        /// </summary>
        public bool HasPartial( string partialName )
        {
            return _partialNames.Contains(partialName);
        }


    }
}

