// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using StackExchange.Redis;

namespace RedisInit
{
	/// <summary>
	///     Redis Server.
	/// </summary>
	public class RedisServer : IDisposable
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisServer" /> class.
		/// </summary>
		/// <param name="serverDetails">The server details.</param>
		public RedisServer( RedisServerDetails serverDetails )
		{
			Details = serverDetails;

			RedisConnection = ConnectionMultiplexer.Connect( string.Format( "{0}:{1},allowAdmin=true", serverDetails.Name, serverDetails.Port ) );

			RedisDatabase = RedisConnection.GetDatabase( );
		}

		/// <summary>
		///     Gets or sets the details.
		/// </summary>
		/// <value>
		///     The details.
		/// </value>
		public RedisServerDetails Details
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the redis connection.
		/// </summary>
		/// <value>
		///     The redis connection.
		/// </value>
		private ConnectionMultiplexer RedisConnection
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the redis database.
		/// </summary>
		/// <value>
		///     The redis database.
		/// </value>
		private IDatabase RedisDatabase
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( RedisConnection != null )
			{
				RedisConnection.Dispose( );
			}
		}

		/// <summary>
		///     Adds the tenant.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		public void AddTenant( EntityInfo tenant )
		{
		}

		/// <summary>
		///     Flushes this instance.
		/// </summary>
		public void Flush( )
		{
			var server = RedisConnection.GetServer( Details.Name, Details.Port );
			server.FlushDatabase( );
		}

		/// <summary>
		///     Writes the entities.
		/// </summary>
		/// <param name="tenant">The tenant.</param>
		/// <param name="entities">The entities.</param>
		public void WriteEntities( TenantInfo tenant, List<EntityInfo> entities )
		{
			foreach ( EntityInfo entity in entities )
			{
				if ( entity.Type == Types.Relationship )
				{
					string forwardKey = string.Format( "{0}:{1}:{2}", entity.TenantId, entity.EntityId, entity.FieldId );
					string reverseKey = string.Format( "{0}:{1}:{2}", entity.TenantId, entity.Value, entity.FieldId );

					RedisDatabase.SetAdd( forwardKey, entity.Value.ToString( ), CommandFlags.FireAndForget );
					RedisDatabase.SetAdd( reverseKey, entity.EntityId, CommandFlags.FireAndForget );
				}
				else
				{
					string key = string.Format( "{0}:{1}:{2}", entity.TenantId, entity.EntityId, entity.Type );

					RedisDatabase.HashSet( key, entity.FieldId, entity.Value.ToString( ), When.Always, CommandFlags.FireAndForget );

					if ( entity.Type == Types.Alias )
					{
						key = string.Format( "{0}:{1}", entity.TenantId, entity.Value );

						RedisDatabase.StringSet( key, entity.EntityId );
					}
				}
			}
		}
	}
}