// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Helper methods for bulk request processing.
    /// </summary>
    public static class BulkRequestHelper
    {
        /// <summary>
        /// Determine if we can process this request using the BulkRequest mechanism.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>True if the BulkRequestRunner can process it, otherwise false.</returns>
        public static bool IsValidForBulkRequest(EntityMemberRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            // BulkRequests currently cannot support
            // - Load AllFields (must specify exact fields)
            // - Loading fields on relationships

            var nodes = request.WalkNodes();
            foreach (var node in nodes)
            {
                if (node.AllFields)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determine if the field is special virtual Access Control field.
        /// </summary>
        /// <param name="field">The field</param>
        public static bool IsVirtualAccessControlField(EntityRef field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            if (field.Namespace == "core"
                && (field.Alias == "canCreateType" || field.Alias == "canModify" || field.Alias == "canDelete"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determine if the field is a virtual field.
        /// </summary>
        /// <param name="field">The field</param>
        public static bool IsVirtualField(IEntityRef field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            var fieldEntity = Entity.Get<Field>(field.Id);
            if (fieldEntity == null)
                throw new ArgumentException("Invalid field Id");

            return fieldEntity.IsFieldVirtual == true;
        }

		/// <summary>
		/// Determines whether the fields are virtual.
		/// </summary>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">fields</exception>
		public static IDictionary<IEntityRef, bool> IsVirtualField( IEnumerable<IEntityRef> fields )
		{
			if ( fields == null )
				throw new ArgumentNullException( "fields" );

			return Entity.GetField<bool>( fields, new EntityRef( "core", "isFieldVirtual" ) );
		}

    }
}
