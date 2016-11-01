// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using EDC.Database;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// How a cast may be used.
    /// </summary>
    public enum CastType
    {
        None,
        Implicit,
        Explicit
    }

    /// <summary>
    /// Represents an allowable type of cast in the language database.
    /// </summary>
    [XmlRoot("Cast")]
    public class CastInfo
    {
        /// <summary>
        /// Source data.
        /// </summary>
        [XmlAttribute("from")]
        public DataType FromType { get; set; }


        /// <summary>
        /// Target data.
        /// </summary>
        [XmlAttribute("to")]
        public DataType ToType { get; set; }


        /// <summary>
        /// Implicit or explicit cast.
        /// </summary>
        [XmlAttribute("cast")]
        public CastType CastType { get; set; }


        /// <summary>
        /// Lower number = higher priority
        /// </summary>
        [XmlAttribute("priority")]
        public int Priority { get; set; }


        /// <summary>
        /// The class name of the type that represents this cast in the expression tree.
        /// </summary>
        [XmlAttribute("class")]
        public string Class { get; set; }


        /// <summary>
        /// The token that is used to perform an explicit cast.
        /// </summary>
        [XmlAttribute("token")]
        [DefaultValue(null)]
        public string Token { get; set; }


        /// <summary>
        /// The type that represents this cast in the expression tree.
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
    }

}
