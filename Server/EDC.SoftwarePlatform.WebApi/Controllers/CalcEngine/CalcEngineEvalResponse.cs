// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEngine
{
    /// <summary>
    /// Calc engine response.
    /// </summary>
    [DataContract]
    public class CalcEngineEvalResponse
    {
        /// <summary>
        ///     Gets or sets the results.
        /// </summary>
        /// <value>
        ///     The calculations.
        /// </value>
        [DataMember(Name = "results", EmitDefaultValue = true, IsRequired = true)]
        public Dictionary<string, CalcEngineEvalResult> Results { get; set; }

        /// <summary>
        ///     Should the results be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeResults()
        {
            return Results != null;
        }
    }
}