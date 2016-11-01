// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using EDC.Annotations;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Migration.Test.Sql
{
	/// <summary>
	///     The SqlHelper class.
	/// </summary>
	public static class SqlHelper
	{
		/// <summary>
		///     Inserts the bit data.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">if set to <c>true</c> [data].</param>
		[UsedImplicitly]
		public static void InsertBitData( long entityId, long fieldId, bool data )
		{
			InsertData( entityId, fieldId, data, "Bit", DbType.Boolean );
		}

		/// <summary>
		///     Inserts the date time data.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		[UsedImplicitly]
		public static void InsertDateTimeData( long entityId, long fieldId, DateTime data )
		{
			InsertData( entityId, fieldId, data, "DateTime", DbType.DateTime );
		}

		/// <summary>
		///     Inserts the entity.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		public static long InsertEntity( )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( "DECLARE @output TABLE ( Id BIGINT ) INSERT INTO Entity ( TenantId, UpgradeId ) OUTPUT INSERTED.Id INTO @output VALUES ( @tenantId, @upgradeId ) SELECT Id FROM @output" ) )
				{
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@upgradeId", DbType.Guid, Guid.NewGuid( ) );

					return ( long ) command.ExecuteScalar( );
				}
			}
		}

		/// <summary>
		///     Inserts the unique identifier data.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		[UsedImplicitly]
		public static void InsertGuidData( long entityId, long fieldId, Guid data )
		{
			InsertData( entityId, fieldId, data, "Guid", DbType.Guid );
		}

		/// <summary>
		///     Inserts the int data.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		[UsedImplicitly]
		public static void InsertIntData( long entityId, long fieldId, int data )
		{
			InsertData( entityId, fieldId, data, "Int", DbType.Int32 );
		}

		/// <summary>
		///     Inserts the n variable character data.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		[UsedImplicitly]
		public static void InsertNVarCharData( long entityId, long fieldId, string data )
		{
			InsertData( entityId, fieldId, data, "NVarChar", DbType.String );
		}

		/// <summary>
		///     Inserts the relationship.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		/// <returns></returns>
		[UsedImplicitly]
		public static void InsertRelationship( long typeId, long fromId, long toId )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( "INSERT INTO Relationship ( TenantId, TypeId, FromId, ToId ) VALUES ( @tenantId, @typeId, @fromId, @toId )" ) )
				{
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@typeId", DbType.Int64, typeId );
					ctx.AddParameter( command, "@fromId", DbType.Int64, fromId );
					ctx.AddParameter( command, "@toId", DbType.Int64, toId );

					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		///     Inserts the data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="data">The data.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="databaseType">Type of the database.</param>
		private static void InsertData<T>( long entityId, long fieldId, T data, string tableName, DbType databaseType )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( string.Format( "INSERT INTO Data_{0} ( EntityId, TenantId, FieldId, Data ) VALUES ( @entityId, @tenantId, @fieldId, @data )", tableName ) ) )
				{
					ctx.AddParameter( command, "@entityId", DbType.Int64, entityId );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );
					ctx.AddParameter( command, "@fieldId", DbType.Int64, fieldId );
					ctx.AddParameter( command, "@data", databaseType, data );

					command.ExecuteNonQuery( );
				}
			}
		}
	}
}