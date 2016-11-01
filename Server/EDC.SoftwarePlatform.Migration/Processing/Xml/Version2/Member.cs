// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    /// <summary>
    ///     Class representing a partially deserialized field or relationship.
    /// </summary>
    class Member
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MemberContainer" /> class.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="memberId">The field/relationship GUID or alias.</param>
        /// <param name="memberType">The type of member, if known.</param>
        /// <param name="data">The relationship target, or alias, or field data.</param>
        public Member( string entityId, string memberId, string memberType, string data = null )
        {
            EntityId = entityId;
            MemberId = memberId;
            MemberType = memberType;
            Data = data;
        }

        /// <summary>
        ///     Gets or sets the entity identifier.
        /// </summary>
        public string EntityId
        {
            get;
        }

        /// <summary>
        ///     Gets the field identifier.
        /// </summary>
        public string MemberId
        {
            get;
        }

        /// <summary>
        ///     Gets the field type.
        /// </summary>
        public string MemberType
        {
            get;
        }

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public string Data
        {
            get;
        }
    }
}
