// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Basic id information about an entity.
    /// </summary>
    [DataContract]
    public class ActionEntityReference
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        [DataMember(Name = "id")]
        public long Id { get; set; }

        /// <summary>
        /// The type identifier.
        /// </summary>
        [DataMember(Name = "typeId")]
        public long TypeId { get; set; }
    }
}