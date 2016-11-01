// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Security
{
    [DataContract]
    public class TypeAccessReason
    {
        /// <summary>
        ///     The role or user by which access is granted.
        /// </summary>
        [DataMember( Name = "subname", IsRequired = true )]
        public string SubjectName
        {
            get;
            set;
        }

        /// <summary>
        ///     The scope of records that can be accessed
        /// </summary>
        [DataMember( Name = "scope", IsRequired = true )]
        public string Scope
        {
            get;
            set;
        }

        /// <summary>
        ///     The name of the type that can be accessed.
        /// </summary>
        [DataMember( Name = "typename", IsRequired = true )]
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        ///     The permissions being granted.
        /// </summary>
        [DataMember( Name = "perms", IsRequired = true )]
        public string Permissions
        {
            get;
            set;
        }

        /// <summary>
        ///     The reason for access.
        /// </summary>
        [DataMember( Name = "reason", IsRequired = true )]
        public string Reason
        {
            get;
            set;
        }
    }
}