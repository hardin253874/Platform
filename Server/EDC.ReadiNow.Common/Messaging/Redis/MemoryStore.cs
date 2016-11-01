// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Memory store.
	/// </summary>
	public class MemoryStore : IMemoryStore
	{
		/// <summary>
		///     The underlying database.
		/// </summary>
		private readonly IDatabase _database;

		/// <summary>
		///     The delete key prefix script.
		/// </summary>
		private readonly LoadedLuaScript _deleteKeyPrefixScript;

		/// <summary>
		///     Initializes a new instance of the <see cref="MemoryStore" /> class.
		/// </summary>
		/// <param name="database">The database.</param>
		/// <param name="deleteKeyPrefixScript">The delete key prefix script.</param>
		/// <exception cref="System.ArgumentNullException">database</exception>
		public MemoryStore( IDatabase database, LoadedLuaScript deleteKeyPrefixScript )
		{
			if ( database == null )
			{
				throw new ArgumentNullException( "database" );
			}

			_database = database;
			_deleteKeyPrefixScript = deleteKeyPrefixScript;
		}


		/// <summary>
		///     Deletes the specified key from the memory store.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public bool KeyDelete( RedisKey key, CommandFlags flags = CommandFlags.FireAndForget )
		{
			_database.KeyDelete( key, flags );

			return true;
		}

		/// <summary>
		///     Deletes all keys with the specified prefix.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public bool KeyDeletePrefix( RedisKey key, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( _deleteKeyPrefixScript == null )
			{
				throw new InvalidOperationException( "Redis script has not been prepared." );
			}

			_database.ScriptEvaluate( _deleteKeyPrefixScript.Hash, null, new RedisValue[ ]
			{
				key.ToString( ) + ":*"
			}, flags );

			return true;
		}

		/// <summary>
		///     Retrieves the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetString( RedisKey key, out RedisValue value )
		{
			var val = _database.StringGet( key );

			if ( val.HasValue )
			{
				value = val;
				return true;
			}

			value = RedisValue.Null;
			return false;
		}

		/// <summary>
		///     Stores the specified value against the specified key in the memory store.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="expiry"></param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public bool StringSet( RedisKey key, RedisValue value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.FireAndForget )
		{
			return _database.StringSet( key, value, expiry, When.Always, flags );
		}

		/// <summary>
		///     Stores the specified values against the specified keys in the memory store.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">values</exception>
		public bool[ ] StringSet( KeyValuePair<RedisKey, RedisValue>[ ] values, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( values == null )
			{
				throw new ArgumentNullException( "values" );
			}

			KeyValuePair<RedisKey, RedisValue>[ ] redisValues = values.Select( value => new KeyValuePair<RedisKey, RedisValue>( value.Key, value.Value ) ).ToArray( );

			_database.StringSet( redisValues, When.Always, flags );

			var result = new bool[values.Length];

			for ( int i = 0; i < values.Length; i++ )
			{
				result[ i ] = true;
			}

			return result;
		}

		/// <summary>
		///     Deletes the specified hash field from the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public bool HashDelete( RedisHashKey key, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			return _database.HashDelete( key.Key, key.HashField, flags );
		}

		/// <summary>
		///     Stores the specified value against the specified hash value for the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool HashSet( RedisHashKey key, RedisValue value, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			return _database.HashSet( key.Key, key.HashField, value, When.Always, flags );
		}

		/// <summary>
		///     Attempts to retrieve the value stored against the specified key and hash field.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool TryGetHash( RedisHashKey key, out RedisValue value )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			var val = _database.HashGet( key.Key, key.HashField );

			if ( val.HasValue )
			{
				value = val;
				return true;
			}

			value = RedisValue.Null;
			return false;
		}

		/// <summary>
		///     Attempts to retrieve the values associated with the specified keys.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		public bool[ ] TryGetString( RedisKey[ ] keys, out RedisValue[ ] values )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			values = _database.StringGet( keys );

			if ( values == null || keys.Length != values.Length )
			{
				throw new InvalidOperationException( "Failed to retrieve redis values." );
			}

			var result = new bool[keys.Length];

			for ( int i = 0; i < keys.Length; i++ )
			{
				result[ i ] = values[ i ].HasValue;
			}

			return result;
		}


		/// <summary>
		///     Attempts to retrieve the values stored against the specified key and hash fields.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">keys</exception>
		public bool[ ] TryGetHash( RedisHashKey[ ] keys, out RedisValue[ ] values )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			/////
			// Group the requested key/hashField values by key.
			/////
			List<IGrouping<RedisKey, RedisHashKey>> groups = keys.GroupBy( key => key.Key ).ToList( );

			var tasks = new Task[groups.Count];
			int counter = 0;

			foreach ( IGrouping<RedisKey, RedisHashKey> group in groups )
			{
				RedisValue[ ] redisFields = group.Select( field => field.HashField ).ToArray( );

				Task<RedisValue[ ]> task = _database.HashGetAsync( group.Key, redisFields );

				tasks[ counter++ ] = task;
			}

			/////
			// Wait for all asynchronous tasks to return.
			/////
			_database.WaitAll( tasks );

			/////
			// Generate a lookup
			/////
			Dictionary<RedisKey, Dictionary<RedisValue, RedisValue>> lookup = tasks.Select( ( task, index ) => new
			{
				Task = ( Task<RedisValue[ ]> ) task,
				Index = index
			} ).ToDictionary( anonTypeA => groups[ anonTypeA.Index ].Key, anonTypeA => anonTypeA.Task.Result.Select( ( redisValue, index ) => new
			{
				RedisValue = redisValue,
				Index = index
			} ).ToDictionary( anonTypeB => groups[ anonTypeA.Index ].ElementAt( anonTypeB.Index ).HashField, anonTypeB => anonTypeA.Task.Result[ anonTypeB.Index ] ) );


			var result = keys.Select( key => lookup[ key.Key ][ key.HashField ].HasValue ).ToArray( );

			values = keys.Select( key => lookup[ key.Key ][ key.HashField ] ).ToArray( );

			return result;
		}


		/// <summary>
		///     Attempts to retrieve the values stored against the specified key and hash fields.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="hashFields">The hash fields.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     key
		///     or
		///     hashFields
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Failed to retrieve redis hash values.</exception>
		public bool[ ] TryGetHash( RedisKey key, RedisValue[ ] hashFields, out RedisValue[ ] values )
		{
			if ( hashFields == null )
			{
				throw new ArgumentNullException( "hashFields" );
			}

			values = _database.HashGet( key, hashFields );

			if ( values == null || hashFields.Length != values.Length )
			{
				throw new InvalidOperationException( "Failed to retrieve redis hash values." );
			}

			var result = new bool[hashFields.Length];

			for ( int i = 0; i < hashFields.Length; i++ )
			{
				result[ i ] = values[ i ].HasValue;
			}

			return result;
		}

		/// <summary>
		///     Flushes this instance.
		/// </summary>
		public void Flush( )
		{
			EndPoint[ ] endPoints = _database.Multiplexer.GetEndPoints( );

			foreach ( EndPoint endPoint in endPoints )
			{
				IServer server = _database.Multiplexer.GetServer( endPoint );

				if ( server != null )
				{
					server.FlushDatabase( );
				}
			}
		}

		/// <summary>
		///     Deletes the specified keys from the memory store.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">keys</exception>
		public bool[ ] KeyDelete( RedisKey[ ] keys, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			_database.KeyDelete( keys, flags );

			var result = new bool[keys.Length];

			for ( int i = 0; i < keys.Length; i++ )
			{
				result[ i ] = true;
			}

			return result;
		}


		/// <summary>
		///     Deletes the specified hash values.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">values</exception>
		public bool[ ] HashDelete( RedisHashKey[ ] values, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( values == null )
			{
				throw new ArgumentNullException( "values" );
			}

			List<IGrouping<RedisKey, RedisValue>> redisValues = values.GroupBy( pair => pair.Key, pair => pair.HashField ).ToList( );

			var tasks = new Task[redisValues.Count];

			for ( int i = 0; i < redisValues.Count; i++ )
			{
				tasks[ i ] = _database.HashDeleteAsync( redisValues[ i ].Key, redisValues[ i ].Select( value => value ).ToArray( ), flags );
			}

			_database.WaitAll( tasks );

			var result = new bool[values.Length];

			for ( int i = 0; i < values.Length; i++ )
			{
				result[ i ] = true;
			}

			return result;
		}


		/// <summary>
		///     Sets the specified values in Redis.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">values</exception>
		public bool[ ] HashSet( KeyValuePair<RedisHashKey, RedisValue>[ ] values, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( values == null )
			{
				throw new ArgumentNullException( "values" );
			}

			var tasks = new Task[values.Length];

			for ( int i = 0; i < values.Length; i++ )
			{
				KeyValuePair<RedisHashKey, RedisValue> pair = values[ i ];
				tasks[ i ] = _database.HashSetAsync( pair.Key.Key, pair.Key.HashField, pair.Value, When.Always, flags );
			}

			_database.WaitAll( tasks );

			var result = new bool[values.Length];

			for ( int i = 0; i < values.Length; i++ )
			{
				result[ i ] = true;
			}

			return result;
		}

		/// <summary>
		///     Adds the specified value to the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool SetAdd( RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.FireAndForget )
		{
			_database.SetAdd( key, value, flags );

			return true;
		}

		/// <summary>
		///     Adds the specified values to the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool SetAdd( RedisKey key, RedisValue[ ] values, CommandFlags flags = CommandFlags.FireAndForget )
		{
			_database.SetAdd( key, values.ToArray( ), flags );

			return true;
		}

		/// <summary>
		///     Adds the members to the specified sets.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">values</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		public bool[ ] SetAdd( KeyValuePair<RedisKey, RedisValue[ ]>[ ] values, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( values == null )
			{
				throw new ArgumentNullException( "values" );
			}

			var tasks = new Task[values.Length];
			int counter = 0;

			foreach ( KeyValuePair<RedisKey, RedisValue[ ]> value in values )
			{
				Task<long> task = _database.SetAddAsync( value.Key, value.Value.ToArray( ), flags );

				tasks[ counter++ ] = task;
			}

			/////
			// Wait for all asynchronous tasks to return.
			/////
			_database.WaitAll( tasks );

			var result = new bool[values.Length];

			for ( int i = 0; i < tasks.Length; i++ )
			{
				var task = tasks[ i ] as Task<long>;

				if ( task == null )
				{
					throw new InvalidCastException( );
				}

				result[ i ] = task.Status == TaskStatus.RanToCompletion;
			}

			return result;
		}

		/// <summary>
		///     Removes the specified value from the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool SetRemove( RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.FireAndForget )
		{
			_database.SetRemove( key, value, flags );

			return true;
		}

		/// <summary>
		///     Removes the specified values from the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool SetRemove( RedisKey key, RedisValue[ ] values, CommandFlags flags = CommandFlags.FireAndForget )
		{
			_database.SetRemove( key, values.ToArray( ), flags );

			return true;
		}

		/// <summary>
		///     Attempts to retrieve the values associated with the specified key from redis.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public bool TryGetSet( RedisKey key, out RedisValue[ ] values )
		{
			values = _database.SetMembers( key ).ToArray( );

			return values.Length > 0;
		}

		/// <summary>
		///     Attempts to retrieve the values associated with the specified keys from redis.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">keys</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		public bool[ ] TryGetSet( RedisKey[ ] keys, out RedisValue[ ][ ] values )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}


			var tasks = new Task[keys.Length];
			int counter = 0;

			foreach ( RedisKey key in keys )
			{
				Task<RedisValue[ ]> task = _database.SetMembersAsync( key );

				tasks[ counter++ ] = task;
			}

			/////
			// Wait for all asynchronous tasks to return.
			/////
			_database.WaitAll( tasks );

			var result = new bool[keys.Length];
			values = new RedisValue[keys.Length][ ];

			for ( int i = 0; i < tasks.Length; i++ )
			{
				var task = tasks[ i ] as Task<RedisValue[ ]>;

				if ( task == null )
				{
					throw new InvalidCastException( );
				}

				result[ i ] = task.Result.Length > 0;

				values[ i ] = task.Result.ToArray( );
			}

			return result;
		}


		/// <summary>
		///     Determines if the specified hash field exists in redis.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool HashKeyExists( RedisHashKey key )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			return _database.HashExists( key.Key, key.HashField );
		}

		/// <summary>
		///     Determines if the specified hash fields exists in redis.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns></returns>
		public bool[ ] HashKeyExists( RedisHashKey[ ] keys )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}


			var tasks = new Task[keys.Length];
			int counter = 0;

			foreach ( RedisHashKey key in keys )
			{
				Task<bool> task = _database.HashExistsAsync( key.Key, key.HashField );

				tasks[ counter++ ] = task;
			}

			/////
			// Wait for all asynchronous tasks to return.
			/////
			_database.WaitAll( tasks );

			var result = new bool[keys.Length];

			for ( int i = 0; i < tasks.Length; i++ )
			{
				var task = tasks[ i ] as Task<bool>;

				if ( task == null )
				{
					throw new InvalidCastException( );
				}

				result[ i ] = task.Result;
			}

			return result;
		}

		/// <summary>
		///     Determines if the specified key exists in redis.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool KeyExists( RedisKey key )
		{
			return _database.KeyExists( key );
		}

		/// <summary>
		///     Determines if the specified keys exist in redis.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns></returns>
		public bool[ ] KeyExists( RedisKey[ ] keys )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			var tasks = new Task[keys.Length];
			int counter = 0;

			foreach ( RedisKey key in keys )
			{
				Task<bool> task = _database.KeyExistsAsync( key );

				tasks[ counter++ ] = task;
			}

			/////
			// Wait for all asynchronous tasks to return.
			/////
			_database.WaitAll( tasks );

			var result = new bool[keys.Length];

			for ( int i = 0; i < tasks.Length; i++ )
			{
				var task = tasks[ i ] as Task<bool>;

				if ( task == null )
				{
					throw new InvalidCastException( );
				}

				result[ i ] = task.Result;
			}

			return result;
		}

		/// <summary>
		///     Keys the expire.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public bool KeyExpire( RedisKey[ ] keys, TimeSpan? timeSpan, CommandFlags flags = CommandFlags.FireAndForget )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			foreach ( RedisKey key in keys )
			{
				_database.KeyExpire( key, timeSpan, flags );
			}

			return true;
		}

		/// <summary>
		///     Keys the expire.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		public bool KeyExpire( RedisKey key, TimeSpan? timeSpan, CommandFlags flags = CommandFlags.FireAndForget )
		{
			_database.KeyExpire( key, timeSpan, flags );

			return true;
		}

		/// <summary>
		///     Scans the specified pattern.
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <returns></returns>
		public RedisKey[ ] Scan( RedisValue pattern, int pageSize = 100 )
		{
			var endPoints = _database.Multiplexer.GetEndPoints( );

			if ( endPoints == null || endPoints.Length <= 0 )
			{
				return new RedisKey[0];
			}

			EndPoint endPoint = endPoints[ 0 ];

			if ( endPoint == null )
			{
				return new RedisKey[0];
			}

			IServer server = _database.Multiplexer.GetServer( endPoint );

			if ( server == null )
			{
				return new RedisKey[0];
			}

			return server.Keys( _database.Database, pattern, pageSize, CommandFlags.None ).ToArray( );
		}
	}
}