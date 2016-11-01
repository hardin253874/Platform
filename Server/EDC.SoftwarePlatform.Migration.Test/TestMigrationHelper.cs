// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Test
{
	/// <summary>
	///     Test Migration Helper class.
	/// </summary>
	public static class TestMigrationHelper
	{
		/// <summary>
		///     Gets or sets the tenant upgrade identifier to entity identifier map.
		/// </summary>
		/// <value>
		///     The tenant upgrade identifier to entity identifier map.
		/// </value>
		private static Dictionary<long, Dictionary<Guid, long>> TenantUpgradeIdToEntityIdMap
		{
			get;
			set;
		}

		/// <summary>
		///     Converts the cardinality enumeration to upgrade identifier.
		/// </summary>
		/// <param name="cardinality">The cardinality.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Invalid cardinality value</exception>
		public static Guid ConvertCardinalityEnumToUpgradeId( CardinalityEnum_Enumeration cardinality )
		{
			switch ( cardinality )
			{
				case CardinalityEnum_Enumeration.OneToOne:
					return AppDetails.OneToOneUid;
				case CardinalityEnum_Enumeration.OneToMany:
					return AppDetails.OneToManyUid;
				case CardinalityEnum_Enumeration.ManyToOne:
					return AppDetails.ManyToOneUid;
				case CardinalityEnum_Enumeration.ManyToMany:
					return AppDetails.ManyToManyUid;
				default:
					throw new ArgumentException( "Invalid cardinality value" );
			}
		}

		/// <summary>
		///     Creates the application.
		/// </summary>
		/// <returns></returns>
		public static AppDetails CreateAppLibraryApplication( )
		{
			return new AppDetails( GetEmptyDataSet( ) );
		}

		/// <summary>
		/// Gets the empty data set.
		/// </summary>
		/// <returns></returns>
		public static DataSet GetEmptyDataSet( )
		{
			var dataSet = new DataSet( );

			Assembly assembly = Assembly.GetExecutingAssembly( );

			using ( Stream unitTestApplicationSchema = assembly.GetManifestResourceStream( "EDC.SoftwarePlatform.Migration.Test.UnitTestApplicationSchema.xsd" ) )
			{
				dataSet.ReadXmlSchema( unitTestApplicationSchema );
			}

			return dataSet;
		}

		/// <summary>
		///     Gets the tenant entity identifier from upgrade identifier.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <returns></returns>
		public static long GetTenantEntityIdFromUpgradeId( long tenantId, Guid upgradeId )
		{
			if ( TenantUpgradeIdToEntityIdMap == null )
			{
				TenantUpgradeIdToEntityIdMap = new Dictionary<long, Dictionary<Guid, long>>( );
			}

			Dictionary<Guid, long> tenantMap;

			if ( !TenantUpgradeIdToEntityIdMap.TryGetValue( tenantId, out tenantMap ) )
			{
				tenantMap = new Dictionary<Guid, long>( );
				TenantUpgradeIdToEntityIdMap[ tenantId ] = tenantMap;
			}

			long entityId;

			if ( !tenantMap.TryGetValue( upgradeId, out entityId ) )
			{
				using ( DatabaseContext context = DatabaseContext.GetContext( ) )
				{
					var command = context.CreateCommand( string.Format( @"SELECT Id FROM Entity WHERE TenantId = {0} AND UpgradeId = '{1}'", tenantId, upgradeId ) );

					var id = command.ExecuteScalar( );

					if ( id == null || id == DBNull.Value )
					{
						throw new ArgumentException( "Unable to find specified upgrade id." );
					}

					entityId = ( long ) id;

					tenantMap[ upgradeId ] = entityId;
				}
			}

			return entityId;
		}

		/// <summary>
		///     Saves the application library application.
		/// </summary>
		/// <param name="details">The details.</param>
		public static void SaveAppLibraryApplication( AppDetails details )
		{
			if ( details == null )
			{
				throw new ArgumentNullException( "details" );
			}

			using ( DatabaseContext context = DatabaseContext.GetContext( ) )
			{
				if ( !context.TransactionActive )
				{
					throw new InvalidOperationException( "Transaction required." );
				}

				var connection = context.GetUnderlyingConnection( ) as SqlConnection;

				if ( connection == null )
				{
					throw new InvalidOperationException( "Invalid Database connection" );
				}

				foreach ( DataTable table in details.Data.Tables )
				{
					if ( details.Saved && table.TableName == TableNames.Entity )
					{
						details.FlushTenantData( );
					}

					if ( table.Rows.Count > 0 )
					{
						using ( var bulkCopy = new SqlBulkCopy( connection ) )
						{
							bulkCopy.BulkCopyTimeout = 600;

							bulkCopy.DestinationTableName = table.TableName;
							bulkCopy.WriteToServer( table );
						}
					}

					if ( table.TableName == TableNames.Entity )
					{
						details.PopulateTenantData( );
					}
				}
			}

			details.Saved = true;
		}
	}
}