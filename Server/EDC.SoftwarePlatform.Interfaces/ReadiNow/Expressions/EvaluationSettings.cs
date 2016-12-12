// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Settings passed by the consumer into the Expression Engine when evaluating an expression.
    /// This gets stored in an EvaluationContext, which gets passed around.
    /// </summary>
    public class EvaluationSettings
    {
        /// <summary>
        /// The entity on which this is operating, if any.
        /// </summary>
        public IEntity ContextEntity { get; set; }


        /// <summary>
        /// Name of the time zone that should be used whenever date-times need to be converted to local time.
        /// </summary>
        public string TimeZoneName { get; set; }


        /// <summary>
        /// Set by the host to allow the resolution of named parameters.
        /// </summary>
        public Func<string, object> ParameterResolver { get; set; }
    }
}
