// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity;
using EDC.ReadiNow.Security.AccessControl;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Entity Nugget represents an encoded entity that is being passed from the client back to the server as part of a
	///     custom service request.
	///     For example, the reports system needs to pass unsaved entity structures back for ad-hoc viewing of unsaved reports
	///     in the report builder.
	/// </summary>
	[DataContract]
	public class EntityNugget
	{
		/// <summary>
		///     The data v1
		/// </summary>
		[DataMember( Name = "v1" )]
		public JsonEntityQueryResult DataV1;

		/// <summary>
		///     Decodes the entity.
		/// </summary>
		/// <param name="entityNugget">The entity nugget.</param>
        /// <param name="persistChanges">If true, the nugget is intended to be persisted.</param>
		/// <returns></returns>
		public static IEntity DecodeEntity( EntityNugget entityNugget, bool persistChanges = false )
		{
			if ( entityNugget == null )
				return null;
			if ( entityNugget.DataV1 == null )
				return null;

			// Recover EntityData
			JsonEntityQueryResult jeqr = entityNugget.DataV1;
			jeqr.ResolveIds( );
			long id = jeqr.Ids.FirstOrDefault( );
			EntityData entityData = jeqr.GetEntityData( id );

            // Bulk-check security (to fill security cache)
            var entitiesToCheck = jeqr.Entities.Where( je => je.Id > 0 ).Select( je => new EntityRef( je.Id ) ).ToList( );
            Factory.EntityAccessControlService.Check(entitiesToCheck, new[] { Permissions.Read });

			// Load as a modified entity structure
			var svc = new EntityInfoService( );
            IEntity entity = svc.DecodeEntity(entityData, persistChanges);
			return entity;
		}
	}
}