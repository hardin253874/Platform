// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration
{
	/// <summary>
	///     Class representing the SqlServerTenantNameResolver type.
	/// </summary>
	/// <seealso cref="EDC.SoftwarePlatform.Migration.Contract.INameResolver" />
	public class SqlServerTenantNameResolver : INameResolver
	{
		/// <summary>
		///     The cache
		/// </summary>
		private readonly Dictionary<Guid, EntityAlias> _cache = new Dictionary<Guid, EntityAlias>( );

		/// <summary>
		///     The tenant identifier
		/// </summary>
		private readonly long _tenantId;

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlServerTenantNameResolver" /> class.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <exception cref="System.ArgumentException">Invalid tenantId</exception>
		public SqlServerTenantNameResolver( long tenantId )
		{
			if ( tenantId < 0 )
			{
				throw new ArgumentException( "Invalid tenantId" );
			}

			_tenantId = tenantId;
		}

		/// <summary>
		///     Resolves the specified upgrade identifier.
		/// </summary>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <returns></returns>
		public EntityAlias Resolve( Guid upgradeId )
		{
			EntityAlias alias;

			if ( !_cache.TryGetValue( upgradeId, out alias ) )
			{
				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( var command = ctx.CreateCommand( "spResolveTenantAppAlias", CommandType.StoredProcedure ) )
					{
						ctx.AddParameter( command, "@upgradeId", DbType.Guid, upgradeId );
						ctx.AddParameter( command, "@tenantId", DbType.Int64, _tenantId );

						var result = command.ExecuteScalar( );

						if ( result != null && result != DBNull.Value )
						{
							alias = new EntityAlias( result.ToString( ) );

							_cache[ upgradeId ] = alias;
						}
						else
						{
							_cache[ upgradeId ] = null;
						}
					}
				}
			}

			return alias;
		}
	}
}