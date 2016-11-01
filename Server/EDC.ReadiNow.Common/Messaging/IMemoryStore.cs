// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Messaging.Redis;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Distributed Memory Manager store
	/// </summary>
	public interface IMemoryStore
	{
		/// <summary>
		///     Flushes this instance.
		/// </summary>
		void Flush( );

		/// <summary>
		///     Deletes the specified hash value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool HashDelete( RedisHashKey key, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Deletes the specified hash values.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool[ ] HashDelete( RedisHashKey[ ] values, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Determines if the specified hash field exists in redis.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		bool HashKeyExists( RedisHashKey key );

		/// <summary>
		///     Determines if the specified hash fields exists in redis.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns></returns>
		bool[ ] HashKeyExists( RedisHashKey[ ] keys );

		/// <summary>
		///     Hashes the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool HashSet( RedisHashKey key, RedisValue value, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Sets the specified values in Redis.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool[ ] HashSet( KeyValuePair<RedisHashKey, RedisValue>[ ] values, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Deletes the specified key from the memory store.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool KeyDelete( RedisKey key, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Deletes the specified keys from the memory store.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool[ ] KeyDelete( RedisKey[ ] keys, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Deletes all keys with the specified prefix.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool KeyDeletePrefix( RedisKey key, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Determines if the specified key exists in redis.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		bool KeyExists( RedisKey key );

		/// <summary>
		///     Determines if the specified keys exist in redis.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <returns></returns>
		bool[ ] KeyExists( RedisKey[ ] keys );

		/// <summary>
		///     Sets the specified keys to expiry when the specified timespan is up.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="flags">The flags.</param>
		bool KeyExpire( RedisKey[ ] keys, TimeSpan? timeSpan, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Sets the specified key to expire when the specified timespan is up.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="flags">The flags.</param>
		bool KeyExpire( RedisKey key, TimeSpan? timeSpan, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Scans the key space for the specified pattern.
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <returns></returns>
		RedisKey[ ] Scan( RedisValue pattern, int pageSize = 100 );

		/// <summary>
		///     Adds the specified value to the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool SetAdd( RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Adds the specified values to the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool SetAdd( RedisKey key, RedisValue[ ] values, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Adds the members to the specified sets.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool[ ] SetAdd( KeyValuePair<RedisKey, RedisValue[ ]>[ ] values, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Removes the specified value from the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool SetRemove( RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Removes the specified values from the set.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool SetRemove( RedisKey key, RedisValue[ ] values, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Stores the specified value against the specified key in the memory store.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="expiry">The expiry.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool StringSet( RedisKey key, RedisValue value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Stores the specified values against the specified keys in the memory store.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <param name="flags">The flags.</param>
		/// <returns></returns>
		bool[ ] StringSet( KeyValuePair<RedisKey, RedisValue>[ ] values, CommandFlags flags = CommandFlags.FireAndForget );

		/// <summary>
		///     Attempts to retrieve the value stored against the specified key and hash field.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		bool TryGetHash( RedisHashKey key, out RedisValue value );

		/// <summary>
		///     Attempts to retrieve the values stored against the specified key and hash fields.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="hashFields">The hash fields.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		bool[ ] TryGetHash( RedisKey key, RedisValue[ ] hashFields, out RedisValue[ ] values );

		/// <summary>
		///     Attempts to retrieve the values stored against the specified key and hash fields.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		bool[ ] TryGetHash( RedisHashKey[ ] keys, out RedisValue[ ] values );

		/// <summary>
		///     Attempts to retrieve the values associated with the specified key from redis.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		bool TryGetSet( RedisKey key, out RedisValue[ ] values );

		/// <summary>
		///     Attempts to retrieve the values associated with the specified keys from redis.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		bool[ ] TryGetSet( RedisKey[ ] keys, out RedisValue[ ][ ] values );

		/// <summary>
		///     Attempts to retrieve the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>True if the value is found; false otherwise.</returns>
		bool TryGetString( RedisKey key, out RedisValue value );

		/// <summary>
		///     Attempts to retrieve the values associated with the specified keys.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		bool[ ] TryGetString( RedisKey[ ] keys, out RedisValue[ ] values );
	}
}