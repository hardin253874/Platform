// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Common.ConfigParser.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Templates
{
    /// <summary>
    /// Holds information about a parsed relationship, in the context of some particular entity type that returned it.
    /// </summary>
    public class FieldData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal FieldData(Entity field, Entity declaringType, bool isInherited)
        {
            FieldEntity = field;
            DeclaringType = declaringType;
            IsInherited = isInherited;
        }

        /// <summary>
        /// The parsed entity that represents this relationship.
        /// </summary>
        public Entity FieldEntity { get; set; }

        /// <summary>
        /// The type that declares the field.
        /// </summary>
        public Entity DeclaringType { get; set; }

        /// <summary>
        /// True if it was inherited by the type that returned it.
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name
        {
            get
            {
                return Model.GetTypeDisplayName(FieldEntity);
            }
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string FieldType
        {
            get
            {
                return Model.GetTypeGenericTypeDisplayName(Model.ResolveType(FieldEntity));
            }
        }

        /// <summary>
        /// Text description of the inheritance.
        /// </summary>
        public string InheritedText
        {
            get
            {
                return IsInherited ? "inherited " : "";
            }
        }

        /// <summary>
        /// The namespace:alias for the field.
        /// </summary>
        public string NsAlias
        {
            get
            {
                return FieldEntity.Alias.NsAlias;
            }
        }
    }
}
