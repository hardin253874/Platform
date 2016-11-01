// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Class representing the SqlServerNameResolver type.
	/// </summary>
	/// <seealso cref="INameResolver" />
	public class SqlServerNameResolver : INameResolver
	{
		/// <summary>
		///     The cache
		/// </summary>
		private readonly Dictionary<Guid, EntityAlias> _cache = new Dictionary<Guid, EntityAlias>( );

		/// <summary>
		///     The _error count
		/// </summary>
		private int _errorCount;

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
				if ( _errorCount <= 5 )
				{
					try
					{
						using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
						{
							using ( var command = ctx.CreateCommand( "spResolveAppAlias", CommandType.StoredProcedure ) )
							{
								ctx.AddParameter( command, "@upgradeId", DbType.Guid, upgradeId );

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
					catch ( Exception )
					{
						_errorCount++;
					}
				}
			}

			return alias;
		}
	}
}