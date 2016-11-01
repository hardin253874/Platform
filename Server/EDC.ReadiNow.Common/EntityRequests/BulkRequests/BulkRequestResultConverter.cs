// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Model.PartialClasses;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.Expressions.CalculatedFields;

namespace EDC.ReadiNow.EntityRequests.BulkRequests
{
    /// <summary>
    /// Converts an unsecured BulkRequestResult to a secured EntityData structure.
    /// Also applies any calculated fields.
    /// </summary>
    static class BulkRequestResultConverter
    {
        private static readonly IEntityAccessControlService AccessControlService = Factory.EntityAccessControlService;

        private static readonly ICalculatedFieldProvider CalculatedFieldProvider = Factory.CalculatedFieldProvider;

        /// <summary>
        /// Converts an unsecured BulkRequestResult to a secured EntityData structure.
        /// </summary>
        public static IEnumerable<EntityData> BuildAndSecureResults(BulkRequestResult data, IEnumerable<long> entities, SecurityOption securityOption)
        {            
            // Get readability (security) for all entities in the unsecured result set
            var readability = BulkRequestResultSecurityHelper.GetEntityReadability( Factory.EntityAccessControlService, data);

            // Prepare for recursive walk
            var context = new Context
            {
                Readability = readability,
                RawData = data,
                BulkSqlQuery = data.BulkSqlQuery
            };            

            if (securityOption == SecurityOption.DemandAll)
            {
                if (context.RawData.RootEntitiesList.Any(id => !context.Readability(id)))
                {
                    throw new PlatformSecurityException(
                        RequestContext.GetContext().Identity.Name,
                        new [] { Permissions.Read },
                        context.RawData.RootEntitiesList
                            .Where(id => !context.Readability(id))
                            .Select(id => new EntityRef(id)));
                }
            }
            else if (securityOption != SecurityOption.SkipDenied)
            {
                throw new ArgumentException(
                    string.Format("Unknown security option {0}", securityOption),
                    "securityOption");
            }

            IEnumerable<EntityData> result = GetRootEntities(context, entities);
            return result;
        }


        /// <summary>
        /// Load and return the entites that are identified by the root of the query.
        /// Note: this algo has been designed to allow multiple roots starting at different node tags.
        /// Though for now it will just be entities at nodetag=0
        /// </summary>
        private static IEnumerable<EntityData> GetRootEntities(Context context, IEnumerable<long> entityIDs)
        {
            // Note: context.RawData.RootEntities contains the set of entities that we need to return. (ie the ones that could be found)
            // but it's a requirement to return them in the order requested, so use the order of entityIDs.

            IEnumerable<long> filteredIDs = entityIDs.Where(id => context.RawData.RootEntities.Contains(id));
            IEnumerable<EntityData> entities = GetEntities(context, filteredIDs);
            return entities;
        }

        /// <summary>
        /// Load and return the entites that are identified by the root of the query.
        /// Note: this algo has been designed to allow multiple roots starting at different node tags.
        /// Though for now it will just be entities at nodetag=0
        /// </summary>
        private static IEnumerable<EntityData> GetEntities(Context context, IEnumerable<long> entityIDs)
        {
            var builder = new EntityDataBuilder<long>();
            builder.GetIsLookup = (relId, dir) =>
            {
                long key = dir == Direction.Forward ? relId.Id : -relId.Id;
                RelationshipInfo relInfo;
                if (context.BulkSqlQuery.Relationships.TryGetValue(key, out relInfo))
                    return relInfo.IsLookup;
                return false; // log error for invalid query? assert false?
            };
            builder.GetId = x => x;
            builder.CanAccess = e => context.Readability(e);
	        builder.IsFieldApplicable = IsFieldApplicableToEntity;
			builder.GetFieldValue = (entityId, fieldId) => GetFieldValueImpl(context, fieldId, entityId);
            builder.GetRelationships = (entityId, relTypeId, direction) => GetRelationshipsImpl(context, relTypeId, direction, entityId);

            // Run the packager
            var result = builder.PackageEntities(context.BulkSqlQuery.Request, entityIDs);
            return result;
        }

		/// <summary>
		///		Determines whether the specified field is applicable to the specified entity.
		/// </summary>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <returns></returns>
		private static bool IsFieldApplicableToEntity( IEntityRef fieldId, long entityId )
		{
			Field field = fieldId.Entity.As<Field>( );

			/////
			// Note* If this becomes a performance bottleneck, having Entity.SetField not throw
			// when calling with non-applicable fields would achieve the same result.
			/////
			bool isApplicable = field.IsApplicableToEntity( new EntityRef( entityId ).Entity );

			return isApplicable;
		}

        /// <summary>
        /// Gets the relationship values for a single relationship on a single entity.
        /// </summary>
        private static IEnumerable<long> GetRelationshipsImpl(Context context, IEntityRef relTypeId, Direction direction, long entityId)
        {
            long relTypeIdWithDir = relTypeId.Id;
            if (direction == Direction.Reverse)
                relTypeIdWithDir = -relTypeIdWithDir;

            RelationshipKey key = new RelationshipKey(entityId, relTypeIdWithDir);
            List<long> relList;
            if (!context.RawData.Relationships.TryGetValue(key, out relList))
                return Enumerable.Empty<long>();

            return relList;
        }

        /// <summary>
        /// Gets the value for a single field on a single entity.
        /// </summary>
        private static TypedValue GetFieldValueImpl(Context context, IEntityRef fieldId, long entityId)
        {
            FieldInfo fieldInfo;
            if (!context.BulkSqlQuery.FieldTypes.TryGetValue(fieldId.Id, out fieldInfo))
                throw new InvalidOperationException("Assert false: attempted to load type info for field that was not in query.");
            
            FieldKey key = new FieldKey(entityId, fieldId.Id);
            DatabaseType type = fieldInfo.DatabaseType;

            // Get value
            FieldValue fieldValue;
            TypedValue typedValue;
            if (fieldInfo.IsVirtualAccessControlField)
            {
                return TryGetAccessControlField(entityId, fieldId);
            }
            else if (fieldInfo.IsWriteOnly)
            {
                typedValue = new WriteOnlyFieldReadValueGenerator().GenerateValue(fieldId.Id, type);
            }
            else if (fieldInfo.IsCalculated)
            {
                object calcValue = CalculatedFieldProvider.GetCalculatedFieldValue(fieldId.Id, entityId, CalculatedFieldSettings.Default);
                typedValue = new TypedValue(DateTimeKind.Utc);
                typedValue.Type = type;
                typedValue.Value = calcValue;
            }
            else if (context.RawData.FieldValues.TryGetValue(key, out fieldValue))
            {
                // 'normal' scenario
                typedValue = fieldValue.TypedValue;
            }
            else
            {
                // No value, so create a null value with type information.
                typedValue = new TypedValue();
                typedValue.Type = type;
            }

            return typedValue;
        }

        /// <summary>
        /// Get the value of an access control field.
        /// </summary>
        /// <param name="entityId">The entityId</param>
        /// <param name="fieldId">The fieldId</param>
        /// <returns>A A typed value.</returns>
        internal static TypedValue TryGetAccessControlField(long entityId, IEntityRef fieldId)
        {
            var value = new TypedValue { Type = DatabaseType.BoolType };

            if (entityId < 1)
                throw new ArgumentNullException("entityId");

            if (fieldId == null || fieldId.Namespace != "core")
                throw new ArgumentException("fieldId: invalid namespace");

            switch (fieldId.Alias)
            {
                case "canCreateType":
                    {
                        var entityType = Entity.Get<EntityType>(entityId);

                        if (entityType == null)
                        {
                            throw new InvalidOperationException("Assert false: attempted to load an entity as an EntityType which is not a type.");
                        }

                        SecurityBypassContext.RunAsUser(() => {
                            value.Value = AccessControlService.CanCreate(entityType);
                        });                        
                        break;
                    }
                case "canModify":
                    {                        
                        SecurityBypassContext.RunAsUser(() => {
                            value.Value = AccessControlService.Check(entityId, new[] { Permissions.Modify });
                        });
                        break;

                    }
                case "canDelete":
                    {
                        SecurityBypassContext.RunAsUser(() => {
                            value.Value = AccessControlService.Check(entityId, new[] { Permissions.Delete });
                        });                        
                        break;
                    }
                default:
                    {
                        // return null value
                        break;
                    }
            }
            return value;
        }

       
        /// <summary>
        /// Private data that is passed around as the results are recursively constructed.
        /// </summary>
        private class Context
        {
            public Context()
            {
                Entities = new Dictionary<long, EntityData>();
            }

            /// <summary>
            /// Security data. Dictionary mapping entities to their readability. True=readable.
            /// </summary>
            public Predicate<long> Readability { get; set; }

            /// <summary>
            /// Visited entities, so we can join them together.
            /// </summary>
            public Dictionary<long, EntityData> Entities { get; private set; }

            /// <summary>
            /// Unsecured data
            /// </summary>
            public BulkRequestResult RawData { get; set; }

            /// <summary>
            /// The processed query
            /// </summary>
            public BulkSqlQuery BulkSqlQuery { get; set; }            
        }
    }
}
