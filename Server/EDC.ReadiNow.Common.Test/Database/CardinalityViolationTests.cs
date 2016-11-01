// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.ReadiNow.Database;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Database
{
	/// <summary>
	///     Cardinality Violation tests.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class CardinalityViolationTests
	{
		/// <summary>
		///     Detects the tenant cardinality violations in the forward direction.
		/// </summary>
		private void DetectTenantCardinalityViolations_Forward( )
		{
			const string query = @"
DECLARE @tenant BIGINT
SELECT @tenant = Id FROM _vTenant WHERE LOWER(name) = LOWER(@tenantName)
DECLARE @cardinality BIGINT = dbo.fnAliasNsId('cardinality', 'core', @tenant)
DECLARE @oneToOne BIGINT = dbo.fnAliasNsId('oneToOne', 'core', @tenant)
DECLARE @oneToMany BIGINT = dbo.fnAliasNsId('oneToMany', 'core', @tenant)
DECLARE @manyToOne BIGINT = dbo.fnAliasNsId('manyToOne', 'core', @tenant)

SELECT r.*
FROM dbgRelationship r
JOIN (
    SELECT TypeId, FromId
    FROM Relationship
    WHERE TenantId = @tenant AND TypeId IN
    (
        SELECT FromId
        FROM Relationship
        WHERE TenantId = @tenant AND TypeId = @cardinality AND (ToId = @oneToOne OR ToId = @manyToOne)
    )
    GROUP BY TypeId, FromId
    HAVING COUNT(FromId) > 1
) d ON r.TypeId = d.TypeId AND r.FromId = d.FromId AND r.TenantId = @tenant
ORDER BY r.TenantId, r.TypeId, r.FromId, r.ToId";

			DetectCardinalityViolations( query, string.Format( "Found one or more cardinality violations in the forward direction for tenant '{0}'.", RunAsDefaultTenant.DefaultTenantName ), ( context, command ) => context.AddParameter( command, "@tenantName", DbType.String, RunAsDefaultTenant.DefaultTenantName ) );
		}

		/// <summary>
		///     Detects the tenant cardinality violations in the reverse direction.
		/// </summary>
		private void DetectTenantCardinalityViolations_Reverse( )
		{
			const string query = @"
DECLARE @tenant BIGINT
SELECT @tenant = Id FROM _vTenant WHERE LOWER(name) = LOWER(@tenantName)
DECLARE @cardinality BIGINT = dbo.fnAliasNsId('cardinality', 'core', @tenant)
DECLARE @oneToOne BIGINT = dbo.fnAliasNsId('oneToOne', 'core', @tenant)
DECLARE @oneToMany BIGINT = dbo.fnAliasNsId('oneToMany', 'core', @tenant)
DECLARE @manyToOne BIGINT = dbo.fnAliasNsId('manyToOne', 'core', @tenant)

SELECT r.*
FROM dbgRelationship r
JOIN (
    SELECT TypeId, ToId
    FROM Relationship
    WHERE TypeId IN
    (
        SELECT FromId
        FROM Relationship
        WHERE TenantId = @tenant AND  TypeId = @cardinality AND (ToId = @oneToOne OR ToId = @oneToMany)
    )
    GROUP BY TypeId, ToId
    HAVING COUNT(ToId) > 1
) d ON r.TypeId = d.TypeId AND r.ToId = d.ToId AND r.TenantId = @tenant
ORDER BY r.TenantId, r.TypeId, r.ToId, r.FromId";

			DetectCardinalityViolations( query, string.Format( "Found one or more cardinality violations in the reverse direction for tenant '{0}'.", RunAsDefaultTenant.DefaultTenantName ), ( context, command ) => context.AddParameter( command, "@tenantName", DbType.String, RunAsDefaultTenant.DefaultTenantName ) );
		}

		/// <summary>
		///     Detects the application library cardinality violations in the forward direction.
		/// </summary>
		private void DetectApplicationLibraryCardinalityViolations_Forward( )
		{
			const string query = @"
DECLARE @cardinality UNIQUEIDENTIFIER
DECLARE @oneToOne UNIQUEIDENTIFIER
DECLARE @oneToMany UNIQUEIDENTIFIER
DECLARE @manyToOne UNIQUEIDENTIFIER
SELECT @cardinality = EntityUid FROM AppData_Alias WHERE Data = 'cardinality' AND Namespace = 'core'
SELECT @oneToOne = EntityUid FROM AppData_Alias WHERE Data = 'oneToOne' AND Namespace = 'core'
SELECT @oneToMany = EntityUid FROM AppData_Alias WHERE Data = 'oneToMany' AND Namespace = 'core'
SELECT @manyToOne = EntityUid FROM AppData_Alias WHERE Data = 'manyToOne' AND Namespace = 'core'

SELECT r.*
FROM dbgAppRelationship r
JOIN (
    SELECT TypeUid, FromUid
    FROM AppRelationship
    WHERE TypeUid IN
    (
        SELECT FromUid
        FROM AppRelationship
        WHERE TypeUid = @cardinality AND (ToUid = @oneToOne OR ToUid = @manyToOne)
    )
    GROUP BY AppVerUid, TypeUid, FromUid
    HAVING COUNT(FromUid) > 1
) d ON r.TypeUid = d.TypeUid AND r.FromUid = d.FromUid
ORDER BY r.AppVerUid, r.TypeUid, r.FromUid, r.ToUid";

			DetectCardinalityViolations( query, "Found one or more forward cardinality violations in the application library" );
		}

		/// <summary>
		///     Detects the application library cardinality violations in the reverse direction.
		/// </summary>
		private void DetectApplicationLibraryCardinalityViolations_Reverse( )
		{
			const string query = @"
DECLARE @cardinality UNIQUEIDENTIFIER
DECLARE @oneToOne UNIQUEIDENTIFIER
DECLARE @oneToMany UNIQUEIDENTIFIER
DECLARE @manyToOne UNIQUEIDENTIFIER
SELECT @cardinality = EntityUid FROM AppData_Alias WHERE Data = 'cardinality' AND Namespace = 'core'
SELECT @oneToOne = EntityUid FROM AppData_Alias WHERE Data = 'oneToOne' AND Namespace = 'core'
SELECT @oneToMany = EntityUid FROM AppData_Alias WHERE Data = 'oneToMany' AND Namespace = 'core'
SELECT @manyToOne = EntityUid FROM AppData_Alias WHERE Data = 'manyToOne' AND Namespace = 'core'

SELECT r.*
FROM dbgAppRelationship r
JOIN (
    SELECT TypeUid, ToUid
    FROM AppRelationship
    WHERE TypeUid IN
    (
        SELECT FromUid
        FROM AppRelationship
        WHERE TypeUid = @cardinality AND (ToUid = @oneToOne OR ToUid = @oneToMany)
    )
    GROUP BY AppVerUid, TypeUid, ToUid
    HAVING COUNT(ToUid) > 1
) d ON r.TypeUid = d.TypeUid AND r.ToUid = d.ToUid
ORDER BY r.AppVerUid, r.TypeUid, r.ToUid, r.FromUid";

			DetectCardinalityViolations( query, "Found one or more reverse cardinality violations in the application library" );
		}

		/// <summary>
		///     Detects tenant cardinality violations.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="errorMessage">The error message.</param>
		/// <param name="commandSetup">The command setup.</param>
		/// <exception cref="System.ArgumentNullException">
		///     query
		///     or
		///     errorMessage
		/// </exception>
		private void DetectCardinalityViolations( string query, string errorMessage, Action<DatabaseContext, IDbCommand> commandSetup = null )
		{
			if ( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			if ( errorMessage == null )
			{
				throw new ArgumentNullException( "errorMessage" );
			}

			using ( var context = DatabaseContext.GetContext( ) )
			{
				var command = context.CreateCommand( query );

				if ( commandSetup != null )
				{
					commandSetup( context, command );
				}

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
                        Assert.Fail(errorMessage + " rel: " + String.Format("{0}", reader[0]));
					}
				}
			}
		}

		/// <summary>
		///     Detects the application library cardinality violations.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DetectApplicationLibraryCardinalityViolations( )
		{
			DetectApplicationLibraryCardinalityViolations_Forward( );
			DetectApplicationLibraryCardinalityViolations_Reverse( );
		}

		/// <summary>
		///     Detects the tenant cardinality violations.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void DetectTenantCardinalityViolations( )
		{
			DetectTenantCardinalityViolations_Forward( );
			DetectTenantCardinalityViolations_Reverse( );
		}
	}
}