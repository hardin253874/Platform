// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using EDC.Database;
using EDC.ReadiNow.Expressions;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor
{
    /// <summary>
    ///     JSON contract for ExprType.
    /// </summary>
    [DataContract]
    public class ExpressionType
    {
        /// <summary>
        ///     Gets or sets the type of the result.
        /// </summary>
        /// <value>
        ///     The type of the result.
        /// </value>
        [IgnoreDataMember]
        public DataType DataType
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is list.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is list; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "isList", EmitDefaultValue = true, IsRequired = false)]
        public bool IsList
        {
            get;
            set;
        }

        /// <summary>
        ///     Explicitly indicates that a list type is not permitted in this context.
        /// </summary>
        /// <value>
        ///     <c>true</c> if list types are not allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "disallowList", EmitDefaultValue = true, IsRequired = false)]
        public bool DisallowList
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        ///     The entity type identifier.
        /// </value>
        [DataMember(Name = "entityTypeId", EmitDefaultValue = true, IsRequired = false)]
        public long EntityTypeId
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets the result type string.
        /// </summary>
        /// <value>
        ///     The result type string.
        /// </value>
        [DataMember(Name = "dataType", EmitDefaultValue = true, IsRequired = false)]
        public string DataTypeString
        {
            get
            {
                return DataType.ToString();
            }
            set
            {
                DataType tmp;
                if (Enum.TryParse(value, out tmp))
                {
                    DataType = tmp;
                }
            }
        }

        /// <summary>
        /// Map to an expression type.
        /// </summary>
        /// <returns></returns>
        public ExprType ToExprType()
        {
            var res = new ExprType(DataType);
            res.IsList = IsList;
            res.DisallowList = DisallowList;
            if (EntityTypeId > 0)
                res.EntityType = new ReadiNow.Model.EntityRef(EntityTypeId);
            return res;
        }
    }
}