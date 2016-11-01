// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using ReadiNow.Expressions.Compiler;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Represents a function that is available in the language.
    /// One instance per function signature.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// The type of result produced by this function.
        /// </summary>
        public LanguageExprType ResultType { get; set; }


        /// <summary>
        /// The types of the parameters that this function accepts.
        /// </summary>
        [DefaultValue(null)]
        //[XmlArrayItem( typeof(LanguageExprType), ElementName = "ExprType" )]
        public LanguageExprType[] ParameterTypes { get; set; }


        /// <summary>
        /// The name of the class that provides the implementation of this function.
        /// </summary>
        public string Class { get; set; }


        /// <summary>
        /// The language keyword or operator text that this function matches. E.g. "abs" or "+"
        /// </summary>
        public string Token { get; set; }


        /// <summary>
        /// If true, then decimals and percents carry into the result type if one is present in any input types.
        /// </summary>
        [DefaultValue(false)]
        public bool PreserveNumericType { get; set; }

        
        /// <summary>
        /// Additional information so that we can re-use the comparison operator classes.
        /// </summary>
        [DefaultValue(null)]
        public LanguageExprType InputType { get; set; }


        /// <summary>
        /// The CalculationExpression type that corresponds to this function. 
        /// </summary>
        [DefaultValue(null)]
        public CalculationOperator SqlOperator { get; set; }


        /// <summary>
        /// Indicates the script host that may use this function. 
        /// </summary>
        [DefaultValue(ScriptHostType.Any)]
        public ScriptHostType Availability { get; set; }


        /// <summary>
        /// Indicates that the script host must be called from an internal API.
        /// </summary>
        [DefaultValue(false)]
        public bool ApiUseOnly { get; set; }


        /// <summary>
        /// Type type that provides the implementation of this function.
        /// </summary>
        [XmlIgnore]
        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    _type = Assembly.GetExecutingAssembly().GetType("ReadiNow.Expressions.Tree.Nodes." + Class);
                }
                return _type;
            }
        }
        Type _type;


        /// <summary>
        /// Deep clone.
        /// </summary>
        public virtual Function Clone()
        {
            var result = (Function)MemberwiseClone();
            result.ResultType = ResultType.Clone();
            result.ParameterTypes = ParameterTypes.Select(p => p.Clone()).ToArray();
            if (InputType != null)
                result.InputType = InputType.Clone();
            return result;
        }

    }

}
