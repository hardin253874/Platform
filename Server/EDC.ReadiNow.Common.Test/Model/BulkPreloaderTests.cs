// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.EntityRequests.BulkRequests;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class BulkPreloaderTests
    {
        [Test]
        public void ChoiceField_Inherits_IsCached( )
        {
            EntityType entityType = Entity.Get<EntityType>("test:weekdayEnum");

            // Includes relationships, as they inherit type.
            const string typePreloaderQuery = "alias, name, isOfType.id, inherits.id, {k:defaultEditForm, defaultPickerReport, defaultDisplayReport}.isOfType.id, allowEveryoneRead, isAbstract, relationships.{id, isOfType.id}, reverseRelationships.{id, isOfType.id}, instanceFlags.id";
            BulkPreloader.Preload( new EntityRequest( "type", typePreloaderQuery, QueryType.Instances, "Preload types" ) );

            using ( CacheManager.ExpectCacheHits( ) )
            {
                entityType.Inherits.ToList();
            }
        }

        [Test]
        public void TestFieldAccess( )
        {
            using ( new SecurityBypassContext( ) )
            {
                EntityType entityType = Entity.Get<EntityType>( "test:weekdayEnum" );
                bool? res = entityType.IsAbstract;
            }            
        }

        [Test]
        public void TestRelationshipAccess( )
        {
            using ( new SecurityBypassContext( ) )
            {
                EntityType entityType = Entity.Get<EntityType>( "test:weekdayEnum" );
                var res = entityType.Inherits;
                res.ToList( );
            }
        }

        [Test]
        [Explicit]
        public void CleanPreload( )
        {
            BulkResultCache.Clear( );
            BulkSqlQueryCache.Clear( );

            using ( new SecurityBypassContext( ) )
            {
                BulkPreloader.TenantWarmup( );
            }
        }

		/// <summary>
		/// Ensure preloaded stuff is actually in the cache.
		/// </summary>
		[Test]
		public void Reverse_Relationship_IsCached( )
		{
			// columnRollup is a reverse relationship.
			long reportId = CodeNameResolver.GetInstance( "AA_Manager", Report.Report_Type ).Single( ).Id;

			string query = @"
                isOfType.id,
                reportColumns.isOfType.id,
                reportColumns.columnRollup.isOfType.id";

			var rq = new EntityRequest( reportId, query );
			BulkPreloader.Preload( rq );

			using ( CacheManager.ExpectCacheHits( ) )
			{
				var report = EDC.ReadiNow.Model.Entity.Get<EDC.ReadiNow.Model.Report>( reportId );
				report.ReportColumns.Any( rc => rc.ColumnRollup.Any( ) );
			}
		}

    }
}
