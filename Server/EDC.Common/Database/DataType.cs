// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDC.Database
{
    /// <summary>
    /// A type of data.
    /// </summary>
    [DataContract]
    public enum DataType
    {
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Entity")]
        Entity = 1,

        [EnumMember(Value = "String")]
        String = 2,

        [EnumMember(Value = "Int32")]
        Int32 = 3,

        [EnumMember(Value = "Decimal")]
        Decimal = 4,

        [EnumMember(Value = "Currency")]
        Currency = 6,

        [EnumMember(Value = "Date")]
        Date = 7,

        [EnumMember(Value = "Time")]
        Time = 8,

        [EnumMember(Value = "DateTime")]
        DateTime = 9,

        [EnumMember(Value = "Bool")]
        Bool = 10,

        [EnumMember(Value = "Guid")]
        Guid = 11,

        [EnumMember(Value = "Binary")]
        Binary = 12,

        [EnumMember(Value = "Xml")]
        Xml = 13
    }
}
