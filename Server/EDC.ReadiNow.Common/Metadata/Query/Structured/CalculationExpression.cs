// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization ;
using System.Xml.Serialization ;
using EDC.Core;
using EDC.Database;
namespace EDC.ReadiNow.Metadata.Query.Structured
{

    /// <summary>
    /// 
    /// </summary>
    // internal only; not in entity model
    [Serializable]
    [DataContract(Namespace = Constants.DataContractNamespace)] 
    public enum CalculationOperator
    {
        //Mathes
        [EnumMember]
        Add,

        [EnumMember]
        Subtract,

        [EnumMember]
        Multiply,

        [EnumMember]
        Divide,

        [EnumMember]
        Modulo,

        [EnumMember]
        Square,

        [EnumMember]
        Sqrt,

        [EnumMember]
        Ceiling,

        [EnumMember]
        Floor,

        [EnumMember]
        Log,

        [EnumMember]
        Log10,

        [EnumMember]
        Sign,

        [EnumMember]
        Abs,

        [EnumMember]
        Power,

        [EnumMember]
        Exp,
        //string functions

        [EnumMember]
        Concatenate,

        [EnumMember]
        Replace ,

        [EnumMember]
        StringLength,

        [EnumMember]
        ToLower,

        [EnumMember]
        ToUpper,

        [EnumMember]
        Left,

        [EnumMember]
        Right,

        [EnumMember]
        Substring,

        [EnumMember]
        Charindex,

        //Data & time
        [EnumMember]
        DateDescription,

        [EnumMember]
        TodayDate,

        [EnumMember]
        TodayDateTime,

        [EnumMember]
        Time,

        [EnumMember]
        Year,

        [EnumMember]
        Month,

        [EnumMember]
        Day,

        [EnumMember]
        Hour,

        [EnumMember]
        Minute,

        [EnumMember]
        Second,

        [EnumMember]
        Week,

        [EnumMember]
        WeekDay,

        [EnumMember]
        DayOfYear,

        [EnumMember]
        Quarter,

        [EnumMember]
        DateAdd,

        [EnumMember]
        DateDiff,

        [EnumMember]
        DateName,

        [EnumMember]
        DateTimeFromParts,

        [EnumMember]
        DateFromParts,

        [EnumMember]
        TimeFromParts,

        //extra
        //Sin,
        //Cos,
        //Cot,
        //ACos,
        //ASin,
        //ATin,
        //ATin2,
        //Degrees,
        //PI,
        //Radians,
        //Rand,
        [EnumMember]
        Round,
        //Tin,
        //T-SQL
        [EnumMember]
        IsNull,

        [EnumMember]
        Null,
        //IIF,
        //NULLIF
        [EnumMember]
        Cast,

        /// <summary>
        /// Just like concatenate, but preserves the natural ordering of its inputs.
        /// </summary>
        [EnumMember]
        SmartOrderConcatenate,

        [EnumMember]
        Negate,
    }

    /// <summary>
    /// Arguments for the DateAdd, DateName and DateDiff functions.
    /// </summary>
    // internal only; not in entity model
    [Serializable]
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum DateTimeParts
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Year,
        [EnumMember]
        Quarter,
        [EnumMember]
        Month,
        [EnumMember]
        DayOfYear,
        [EnumMember]
        Day,
        [EnumMember]
        Week,
        [EnumMember]
        Weekday,
        [EnumMember]
        Hour,
        [EnumMember]
        Minute,
        [EnumMember]
        Second
    }

    // internal only; not in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class CalculationExpression : ScalarExpression, ICompoundExpression
    {
        public CalculationExpression() 
        {
            Expressions = new List<ScalarExpression>();
        }


        [DataMember(Order = 1)]
        public CalculationOperator Operator
        {
            get;
            set;
        }

        [DataMember(Order = 2)]
        public DatabaseType DisplayType
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        [DefaultValue( DataType.None )]
        public DataType InputType
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to cast type
        /// </summary>
        [DataMember(Order = 4)]
        public DatabaseType CastType
        {
            get;
            set;
        }

        /// <summary>
        /// The date-time part to use. Only applicable to some date-time operations.
        /// </summary>
        [DataMember(Order = 5)]
        [DefaultValue(DateTimeParts.None)]
        public DateTimeParts DateTimePart
        {
            get;
            set;
        }


        [DataMember(Order = 6)]
        [XmlArray("Expressions")]
        [XmlArrayItem("Expression")]
        public List<ScalarExpression> Expressions
        {
            get;
            set;
        }

        public IEnumerable<ScalarExpression> Children
        {
            get { return Expressions; }
        }
    }
}
