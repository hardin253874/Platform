// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.Collections.Generic;
using EDC.Common;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Core;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Pre-fills the entity cache based on a query.
	/// </summary>
	public static class BulkPreloader
	{
		[ThreadStatic]
		private static bool _tenantWarmupRunning;

        private static bool _tenantWarmupDone;

		public static bool TenantWarmupRunning
		{
			get
			{
				return _tenantWarmupRunning;
			}
		}

		/// <summary>
		///     Fill the EntityFieldCache with preloaded data.
		/// </summary>
		/// <param name="entityData">The entity data.</param>
		/// <param name="isOfTypeId">The is of type identifier.</param>
		private static void AddEntityToCache( EntityData entityData, long isOfTypeId )
		{
			long entityId = entityData.Id.Id;

			// Check if already in cache
			if ( EntityCache.Instance.ContainsKey( entityId ) )
				return;

			foreach ( RelationshipData relType in entityData.Relationships )
			{
				// Find the isOfType relationship
				if ( relType.RelationshipTypeId.Id != isOfTypeId || relType.IsReverseActual )
					continue;

				// Get the types
				List<long> typeIds = relType.Instances.Select( inst => inst.Entity.Id.Id ).ToList( );

				// Activate the instance, which will fill the cache
				var activationData = new ActivationData( entityId, RequestContext.TenantId, typeIds );
				Entity.ActivateInstance( ref activationData );
				return;
			}

			EventLog.Application.WriteTrace( "No isOfType info found for preloaded entity " + entityId );
		}

		/// <summary>
		///     Fill the EntityFieldCache with preloaded data.
		/// </summary>
		/// <param name="entityData"></param>
		private static void AddFieldsToCache( EntityData entityData )
		{
			long entityId = entityData.Id.Id;

			// Get field cache for entity
            IEntityFieldValues cachedFieldValues = EntityFieldCache.Instance.GetOrCreate( entityId );

			// Fields
			foreach ( FieldData fieldEntry in entityData.Fields )
			{
				long fieldId = fieldEntry.FieldId.Id;				

                // Only update field cache if the field is not present
                // This is to avoid populating the cache with stale data
			    if (!cachedFieldValues.ContainsField(fieldId))
			    {
                    TypedValue typedValue = fieldEntry.Value;
                    object fieldValue = typedValue?.Value;

                    cachedFieldValues[fieldId] = fieldValue;
                }
			}
		}

		/// <summary>
		///     Fill the EntityRelationshipCache with preloaded data.
		/// </summary>
		/// <param name="entityData"></param>
		private static void AddRelationshipsToCache( EntityData entityData )
		{
			long entityId = entityData.Id.Id;

			// Relationships
			IEnumerable<IGrouping<bool, RelationshipData>> relsByEntityAndDir = entityData.Relationships
				.GroupBy( rel => rel.IsReverseActual );
			foreach ( var entry in relsByEntityAndDir )
			{
				Direction direction = entry.Key ? Direction.Reverse : Direction.Forward;

				var entityAndDirKey = new EntityRelationshipCacheKey( entityId, direction );

				// Get relationship cache for entity
				IDictionary<long, ISet<long>> cachedRelationships = new ConcurrentDictionary<long, ISet<long>>( );

				// Group by relationship type
				foreach ( RelationshipData relTypeEntry in entry )
				{
					long relTypeId = relTypeEntry.RelationshipTypeId.Id;
					List<RelationshipInstanceData> values = relTypeEntry.Instances;
                    var relSet = new HashSet<long>( values.Select( value => value.Entity.Id.Id ) );
					cachedRelationships[ relTypeId ] = relSet;
				}

				EntityRelationshipCache.Instance.Merge( entityAndDirKey, cachedRelationships );
			}
		}

		/// <summary>
		///     Pre-compiles an entity query so it will run more quickly when first encountered.
		/// </summary>
		public static void PrecompileEntityQuery( string requestString, string hint = null )
		{
			var rq = new EntityRequest( -1, requestString, hint ?? "PrecompileEntityQuery" );
			BulkSqlQueryCache.GetBulkSqlQuery( rq );
		}

		/// <summary>
		///     Pre-load data into the entity model cache according to the specified query.
		///     Note: subsequent calls to preload take almost no time, unless the data queried has changed.
		///     Caution: this mutates the entity request.
		/// </summary>
		/// <param name="request">Entity info request to preload.</param>
		public static void Preload( EntityRequest request )
		{
			using ( Profiler.Measure( request.Hint ?? "Preload" ) )
			using ( new SecurityBypassContext( ) )
			{
				// Check cache and/or get unsecured result
				// Note: the results don't actually get secured at this point (we don't want to for the purpose of preload. So we're in a SecurityBypassContext)
				// The reason we convert to entity data is because it's smart enough to fill in the empty fields/relationships for which there is no data.
				// So we can cache the fact that those fields/relationships are empty. (BulkRequestResult alone doesn't give us this info).

				request.DontProcessResultIfFromCache = true;

				IEnumerable<EntityData> rootEntities;
				if ( request.QueryType == QueryType.ExactInstances || request.QueryType == QueryType.Instances )
				{
					rootEntities = BulkRequestRunner.GetEntitiesByType( request );
				}
				else
				{
					rootEntities = BulkRequestRunner.GetEntitiesData( request );
				}

				if ( !request.ResultFromCache )
				{
                    long isOfTypeId = WellKnownAliases.CurrentTenant.IsOfType;

					// Flatten the graph
					IEnumerable<EntityData> allEntityData = Delegates.WalkGraph( rootEntities, entityData => entityData.Relationships.SelectMany( relType => relType.Entities ) );

					using ( Entity.DistributedMemoryManager.Suppress( ) )
					{
						// Extract field/relationship data and shove into cache
						foreach ( EntityData entityData in allEntityData )
						{
							AddEntityToCache( entityData, isOfTypeId );

							AddFieldsToCache( entityData );

							AddRelationshipsToCache( entityData );
						}
					}
				}
			}
		}

		/// <summary>
		///     Run various queries to pre-fill the entity cache for the current tenant.
		/// </summary>
		public static void TenantWarmup( )
		{
            using ( new DeferredChannelMessageContext() )
            using ( EntryPointContext.SetEntryPoint( "Warmup TenantId=" + RequestContext.TenantId ) )
            using ( Profiler.Measure( "BulkPreloader.TenantWarmup TenantId=" + RequestContext.TenantId ) )
			{
                ProcessMonitorWriter.Instance.Write("Warmup TenantId=" + RequestContext.TenantId);

                _tenantWarmupRunning = true;
				try
				{
					CacheRelationships( );

					// Fetch all aliases in the tenant
					EntityIdentificationCache.PreloadAliases( );

                    // Prewarm type hierarchy
                    PerTenantEntityTypeCache.Instance.Prewarm( );

					// Includes relationships, as they inherit type.
                    const string typePreloaderQuery = "alias, name, isOfType.{id, isMetadata}, inherits.id, {k:defaultEditForm, defaultPickerReport, defaultDisplayReport}.isOfType.id, allowEveryoneRead, isAbstract, relationships.{id, isOfType.id}, reverseRelationships.{id, isOfType.id}, instanceFlags.id";
					Preload( new EntityRequest( "type", typePreloaderQuery, QueryType.Instances, "Preload types" ) );

					const string derivedTypePreloaderQuery = "derivedTypes.id";
					Preload( new EntityRequest( "type", derivedTypePreloaderQuery, QueryType.Instances, "Preload types" ) );

					// Includes enum definitions
					const string relationshipPreloaderQuery = "reverseAlias, cardinality.id, fromType.id, toType.id, fromName, toName, securesTo, securesFrom, securesToReadOnly, securesFromReadOnly, flags.id, relationshipInKey.id";
					Preload( new EntityRequest( "relationship", relationshipPreloaderQuery, QueryType.Instances, "Preload relationships" ) );

                    const string resourceKeyPreloaderQuery = "resourceKeyRelationships.id, keyFields.id";
                    Preload(new EntityRequest("resourceKey", resourceKeyPreloaderQuery, QueryType.ExactInstances, "Preload resourcekeys"));

                    const string fieldTypesPreloaderQuery = "alias, isOfType.id, dbFieldTable, dbType, dbTypeFull, readiNowType, { k:renderingControl, k:defaultRenderingControls }.{ isOfType.id, k:context.isOfType.id }";
					Preload( new EntityRequest( "fieldType", fieldTypesPreloaderQuery, QueryType.ExactInstances, "Preload field types" ) );

					const string fieldPreloaderQuery = @"alias, name, isOfType.id, description, isFieldReadOnly, defaultValue, isRequired, isFieldWriteOnly, allowMultiLines,
                    pattern.{isOfType.id, regex, regexDescription},{flags, fieldRepresents}.{ isOfType.id, alias }, 
                    minLength, maxLength, minInt, maxInt, minDecimal, maxDecimal, minDate, maxDate, minTime, maxTime, minDateTime, maxDateTime, decimalPlaces, autoNumberDisplayPattern, isCalculatedField, fieldCalculation";
					Preload( new EntityRequest( "field", fieldPreloaderQuery, QueryType.Instances, "Preload field definitions" ) );

					const string tenantSettingsPreloaderQuery = "alias, isOfType.id, finYearStartMonth.isOfType.id";
					Preload( new EntityRequest( "tenantGeneralSettings", tenantSettingsPreloaderQuery, QueryType.ExactInstances, "Preload tenant settings" ) );

                    const string enumValuesPreloaderQuery = "alias, name, enumOrder, isOfType.instancesOfType.id";
					Preload( new EntityRequest( "enumValue", enumValuesPreloaderQuery, QueryType.Instances, "Preload enum values" ) );

                    const string accessRulesPreloaderQuery = "alias, isOfType.id, allowAccess.{ isOfType.id, accessRuleReport.isOfType.id, controlAccess.isOfType.id, permissionAccess.isOfType.id, accessRuleEnabled, accessRuleHidden, accessRuleIgnoreForReports }";
					Preload( new EntityRequest( "subject", accessRulesPreloaderQuery, QueryType.Instances, "Preload access rules" ) );

					const string userPreloaderQuery = "alias, name, isOfType.id, userHasRole.{id, isOfType.id}";
					Preload( new EntityRequest( "userAccount", userPreloaderQuery, QueryType.Instances, "Preload users" ) );

					const string rolesPreloaderQuery = "alias, name, isOfType.id, includesRoles.{id, isOfType.id}, includedByRoles.{id, isOfType.id}, roleMembers.{id, isOfType.id}";
					Preload( new EntityRequest( "role", rolesPreloaderQuery, QueryType.Instances, "Preload roles" ) );

					// edit form controls
					const string controlsTypePreloaderQueryCommon = @"
                    name, alias, isOfType.id, 
                    instancesOfType.{
                        name, 
                        k:requiresContext, k:isHiddenFromFormDesigner, k:designControl, k:control, k:viewControl, 
                        k:context.{alias, isOfType.id}
                    }";

					const string controlsTypePreloaderQuery = controlsTypePreloaderQueryCommon + @",
                    instancesOfType.{
                        k:wbcHideSuccessConfirmation, 
                        {
                            k:wbcWorkflowToRun, k:wbcResourceInputParameter, 
                            k:wbcDisableControlBasedOnRelationshipControl, k:wbcDisableControlBasedOnResources, 
                            k:reportToRender, k:chartToRender, k:formToRender
                        }.{alias, isOfType.id}
                    }";

					Preload( new EntityRequest( "k:renderControlType", controlsTypePreloaderQuery, QueryType.Instances, "Preload editForm controls" ) );

					const string fieldControlsTypePreloaderQuery = controlsTypePreloaderQueryCommon + @",
                    instancesOfType.{ 
                        {k:fieldTypeToRender, k:defaultFieldTypesToRender, k:fieldInstanceToRender}.{alias, isOfType.id},
                        k:textControlWidth, k:textControlHeight
                    }";

					Preload( new EntityRequest( "k:fieldRenderControlType", fieldControlsTypePreloaderQuery, QueryType.Instances, "Preload editForm field controls" ) );

					const string relControlsTypePreloaderQuery = controlsTypePreloaderQueryCommon + @",
                    instancesOfType.{ 
                        {k:thumbnailScalingSetting, k:thumbnailSizeSetting}.{alias, id}
                    }";
					Preload( new EntityRequest( "k:relationshipRenderControlType", relControlsTypePreloaderQuery, QueryType.Instances, "Preload editForm rel controls" ) );
					Preload( new EntityRequest( "k:structureRenderControlType", controlsTypePreloaderQuery, QueryType.Instances, "Preload editForm struct controls" ) );


                    PrecompileEntityQuery( ReportHelpers.ReportPreloaderQuery, "Precompile report query" );
                    PrecompileEntityQuery( ReportHelpers.QueryPreloaderQuery, "Precompile report-query query" );
					PrecompileEntityQuery( ActionServiceHelpers.ReportRequest, "Precompile report actions query" );
                    PrecompileEntityQuery( ActionServiceHelpers.ResourceRequest, "Precompile resource actions query" );
                    PrecompileEntityQuery( ActionServiceHelpers.ResourceViewerRequest, "Precompile resource viewer actions query" );
                    PrecompileEntityQuery( ActionServiceHelpers.WorkflowRequest, "Precompile workflow actions query" );

					PrecompileEntityQuery( CustomEditFormHelper.GetHtmlFormQuery( ), "Precompile form view query" );
					PrecompileEntityQuery( CustomEditFormHelper.GetHtmlFormQuery( true ), "Precompile form design query" );
					PrecompileEntityQuery( DefaultLayoutGenerator.FormGenerationPreloaderQuery, "Precompile default form generator query" );

                    Factory.ExpressionCompiler.Prewarm( );
                    TimeZoneHelper.Prewarm( );

                    PreloadAccessRuleReports( );

                    _tenantWarmupDone = true;
				}
				finally
				{
					_tenantWarmupRunning = false;
                }
			}

		}

		/// <summary>
		///		Obtains the forward relationship query.
		/// </summary>
		/// <returns>
		/// 	The forward relationship query.
		/// </returns>
		private static string ForwardRelationshipQuery( )
		{
			return @"
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @coreSolution BIGINT = dbo.fnAliasNsId( 'coreSolution', 'core', @tenantId )
DECLARE @consoleSolution BIGINT = dbo.fnAliasNsId( 'consoleSolution', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @createdBy BIGINT = dbo.fnAliasNsId( 'createdBy', 'core', @tenantId )
DECLARE @securityOwner BIGINT = dbo.fnAliasNsId( 'securityOwner', 'core', @tenantId )
DECLARE @resourceKeyDataHashAppliesToResourceKey BIGINT = dbo.fnAliasNsId( 'resourceKeyDataHashAppliesToResourceKey', 'core', @tenantId )

SELECT
	r.TypeId
	,r.FromId
	,r.ToId
FROM
	Relationship r
JOIN
	Relationship etype
ON
	r.FromId = etype.FromId
	AND etype.TenantId = @tenantId
	AND etype.TypeId = @isOfType
JOIN
	Relationship tsol
ON
	etype.ToId = tsol.FromId
	AND tsol.TenantId = @tenantId
	AND tsol.TypeId = @inSolution
	AND tsol.ToId IN ( @coreSolution, @consoleSolution )
WHERE
	r.TenantId = @tenantId
   AND r.TypeId NOT IN ( @inSolution, @securityOwner, @createdBy )
ORDER BY
	r.FromId
	,r.TypeId
";
		}

		/// <summary>
		///		Obtains the reverse relationship query.
		/// </summary>
		/// <returns>
		/// 	The reverse relationship query.
		/// </returns>
		private static string ReverseRelationshipQuery( )
		{
			return @"
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @coreSolution BIGINT = dbo.fnAliasNsId( 'coreSolution', 'core', @tenantId )
DECLARE @consoleSolution BIGINT = dbo.fnAliasNsId( 'consoleSolution', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @createdBy BIGINT = dbo.fnAliasNsId( 'createdBy', 'core', @tenantId )
DECLARE @securityOwner BIGINT = dbo.fnAliasNsId( 'securityOwner', 'core', @tenantId )
DECLARE @resourceKeyDataHashAppliesToResourceKey BIGINT = dbo.fnAliasNsId( 'resourceKeyDataHashAppliesToResourceKey', 'core', @tenantId )

SELECT
	r.TypeId
	,r.ToId
	,r.FromId
FROM
	Relationship r
JOIN
	Relationship etype
ON
	r.ToId = etype.FromId
	AND etype.TenantId = @tenantId
	AND etype.TypeId = @isOfType
JOIN
	Relationship tsol
ON
	etype.ToId = tsol.FromId
	AND tsol.TenantId = @tenantId
	AND tsol.TypeId = @inSolution
	AND tsol.ToId IN ( @coreSolution, @consoleSolution )
WHERE
	r.TenantId = @tenantId
    AND r.TypeId NOT IN ( @inSolution, @securityOwner, @createdBy, @isOfType, @resourceKeyDataHashAppliesToResourceKey )
ORDER BY
	r.ToId
	,r.TypeId
";
		}

		/// <summary>
		///		Caches the relationships (both forward and reverse).
		/// </summary>
		private static void CacheRelationships( )
		{
			CacheRelationshipTable( ForwardRelationshipQuery, Direction.Forward );
			CacheRelationshipTable( ReverseRelationshipQuery, Direction.Reverse );
		}

		/// <summary>
		///		Caches the relationship table.
		/// </summary>
		/// <param name="getQuery">Function that returns the query to execute.</param>
		/// <param name="direction">The direction.</param>
		private static void CacheRelationshipTable( Func<string> getQuery, Direction direction )
		{
			using ( var context = DatabaseContext.GetContext( ) )
			{
				using ( var command = context.CreateCommand( getQuery( ) ) )
				{
					context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					var values = new Dictionary<EntityRelationshipCacheKey, IDictionary<long, ISet<long>>>( );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						long currentTypeId = -1;
						long currentFromId = -1;

						EntityRelationshipCacheKey key = null;
						ConcurrentDictionary<long, ISet<long>> dic = null;
						ISet<long> set = null;
						
						while ( reader.Read( ) )
						{
							long typeId = reader.GetInt64( 0 );
							long fromId = reader.GetInt64( 1 );
							long toId = reader.GetInt64( 2 );

							if ( fromId != currentFromId )
							{
								if ( dic != null )
								{
									values.Add( key, dic );
								}

								key = new EntityRelationshipCacheKey( fromId, direction );
								dic = new ConcurrentDictionary<long, ISet<long>>( );
								currentFromId = fromId;
								currentTypeId = -1;
							}

							if ( typeId != currentTypeId )
							{
								set = new HashSet<long>( );

								if ( dic != null )
								{
									dic[ typeId ] = set;
								}

								currentTypeId = typeId;
							}

							if ( set != null )
							{
								set.Add( toId );
							}
						}

						if ( dic != null )
						{
							values.Add( key, dic );
						}

						EntityRelationshipCache.Instance.Preload( values );
					}
				}
			}
		}

		/// <summary>
        /// Preload reports that are used in access rules.
        /// </summary>
        private static void PreloadAccessRuleReports( )
        {
            var authReports = Entity.GetInstancesOfType<AccessRule>( true, "accessRuleReport.isOfType.id" )
                .Select( auth => auth.AccessRuleReport )
                .Where( r => r != null )
                .Select( r => r.Id )
                .Distinct( );

            foreach ( long reportId in authReports )
            {
                try
                {
                    ReportHelpers.PreloadQuery( new EntityRef( reportId ) );
                }
                catch ( Exception ex )
                {
                    EventLog.Application.WriteError( "Access rule report {0} failed to preload\n{1}", reportId, ex );
                }
            }
        }

        /// <summary>
        /// Run the tenant warmup - only if it has not already been run.
        /// </summary>
        public static void TenantWarmupIfNotWarm()
        {
            if (!_tenantWarmupDone)
                TenantWarmup();
        }
	}
}