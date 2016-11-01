// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using EDC.Database;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Represents the data type returned by an expression, or sub expression.
    /// </summary>
    [XmlType(AnonymousType = true)]       // (so LanguageExprType can own "ExprType" without conflicting)
    public class ExprType
    {
        /// <summary>
        /// Default construtor.
        /// </summary>
        public ExprType()
        { }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="type">Type of data represented by this expression type.</param>
        public ExprType(DataType type)
        {
            Type = type;
        }

        #region Convenient Static Properties

        /// <summary>
        /// Scalar string expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType String = new ExprType(DataType.String);

        /// <summary>
        /// Scalar int expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Int32 = new ExprType(DataType.Int32);

        /// <summary>
        /// Scalar decimal expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Decimal = new ExprType(DataType.Decimal);

        /// <summary>
        /// Scalar Guid expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Guid = new ExprType(DataType.Guid);

        /// <summary>
        /// Scalar bool expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Bool = new ExprType(DataType.Bool);

        /// <summary>
        /// Scalar date/time expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType DateTime = new ExprType(DataType.DateTime);

        /// <summary>
        /// Scalar time expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Time = new ExprType(DataType.Time);

        /// <summary>
        /// Scalar date expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Date = new ExprType(DataType.Date);

        /// <summary>
        /// Scalar currency expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Currency = new ExprType(DataType.Currency);

        /// <summary>
        /// Scalar entity expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType Entity = new ExprType(DataType.Entity);

        /// <summary>
        /// Entity list expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType EntityList = new ExprType(DataType.Entity) { IsList = true };

        /// <summary>
        /// Scalar untyped expression type. Use clone if you intend on modifying!
        /// </summary>
        public static readonly ExprType None = new ExprType(DataType.None);
        #endregion

        /// <summary>
        /// Gets/sets the root type of the expression.
        /// </summary>
        [XmlAttribute("type")]
        [DefaultValue(DataType.None)]
        public DataType Type { get; set; }


        /// <summary>
        /// Gets/sets the recognised entity type of the expression.
        /// </summary>
        [XmlIgnore]       // overridden in LanguageExprType, so it can declare as an EntityRef
        public IEntityRef EntityType { get; set; }


        /// <summary>
        /// Gets/sets the recognised entity type of the expression.
        /// </summary>
        [XmlIgnore]
        public long EntityTypeId
        {
            get { return EntityType == null ? 0 : EntityType.Id; }
        }


        /// <summary>
        /// Does this expression evaluate to a list of values.
        /// </summary>
        [XmlAttribute("isList")]
        [DefaultValue(false)]
        public bool IsList { get; set; }


        /// <summary>
        /// Explicitly disallow lists.
        /// </summary>
        [XmlAttribute("disallowList")]
        [DefaultValue(false)]
        public bool DisallowList { get; set; }


        /// <summary>
        /// Can this expression be statically evaluated.
        /// </summary>
        [XmlAttribute("const")]
        [DefaultValue(false)]
        public bool Constant { get; set; }


        /// <summary>
        /// Number of decimal places.
        /// </summary>
        [XmlIgnore]
        [DefaultValue(null)]
        public int? DecimalPlaces { get; set; }

        #region Helper Methods
        /// <summary>
        /// Clones the expression type info.
        /// </summary>
        public ExprType Clone()
        {
            return (ExprType)MemberwiseClone();
        }

        /// <summary>
        /// Override ToString 
        /// </summary>
        public override string ToString()
        {
            string res = Type.ToString();
            if (Type == DataType.Entity && EntityType != null)
            {
                try
                {
                    res += " (" + (string)EntityType.Entity.GetField("core:name") + ")";
                }
                catch { }
            }
            if (Constant)
                res += " (const)";
            if (IsList)
                res = "List of " + res;
            return res;
        }
        #endregion
    }

}
