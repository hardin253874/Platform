// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System.Diagnostics;

namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// Represents a request to load related entities. Part of the <see cref="EntityMemberRequest"/> request contract.
    /// </summary>
    [DebuggerDisplay( "RelType {RelationshipTypeId}, Rev {IsReverse}" )]
    public class RelationshipRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RelationshipRequest()
        {
            RequestedMembers = new EntityMemberRequest();
        }


        /// <summary>
        /// The relationship type (definition) to follow.
        /// </summary>
        public IEntityRef RelationshipTypeId { get; set; }
        

        /// <summary>
        /// True if the relationship is being followed in the forward direction, otherwise false.
        /// </summary>
        /// <remarks>
        /// If the relationship is already a reverse alias, then this will reverse it again. Hopefully.
        /// </remarks>
        public bool IsReverse { get; set; }

        
        /// <summary>
        /// If set, then do not specify RequestedMembers, as the parent set will be reused.
        /// </summary>
        public bool IsRecursive { get; set; }


        /// <summary>
        /// Represents the list of fields (and subsequent relationships) to load for the related entities.
        /// Do not set if 'IsRecursive' is specified.
        /// </summary>
        public EntityMemberRequest RequestedMembers { get; set; }


        /// <summary>
        /// If true, get the metadata for the relationship, but don't load instances.
        /// </summary>
        public bool MetadataOnly { get; set; }
    }
}
