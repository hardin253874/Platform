// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// An extension of ExprType that supports deserialization of the XML language database.
    /// </summary>
    [XmlType("ExprType")]
    public class LanguageExprType : ExprType
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LanguageExprType()
        {
            AnyNumeric = true;
        }


        /// <summary>
        /// Gets/sets a group of types. Just allows for convenient representation in XML.
        /// </summary>
        [XmlAttribute("typeGroup")]
        public TypeGroup TypeGroup { get; set; }


        /// <summary>
        /// Only used when declaring operators. Indicates that any number of parameters may be passed.
        /// </summary>
        [XmlIgnore]
        public bool AnyNumeric { get; set; }


        /// <summary>
        /// If true, indicates that an input will be used for an ordering or comparison operation, and it should first be transformed accordingly.
        /// </summary>
        [XmlAttribute("tranformForOrdering")]
        public bool TranformForOrdering { get; set; }


        /// <summary>
        /// Gets/sets the recognised entity type of the expression.
        /// </summary>
        /// <remarks>
        /// Replace EntityType with a version declared as EntityRef instead of IEntityRef.
        /// IEntityRef cannot be used in serialization, which is required here. However, EntityRef cannot be 
        /// referenced by ExprType in the interfaces dll.
        /// </remarks>
        public new EntityRef EntityType
        {
            get { return base.EntityType as EntityRef; }
            set { base.EntityType = value; }
        }


        /// <summary>
        /// Clone.
        /// </summary>
        public new LanguageExprType Clone()
        {
            return (LanguageExprType)MemberwiseClone();
        }
    }


}
