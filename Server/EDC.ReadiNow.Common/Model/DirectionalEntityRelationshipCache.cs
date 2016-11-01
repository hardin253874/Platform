// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Directional entity relationship cache.
	/// </summary>
	public class DirectionalEntityRelationshipCache
	{
		/// <summary>
		///     The cache
		/// </summary>
		private readonly ConcurrentDictionary<long, IDictionary<long, ISet<long>>> _cache = new ConcurrentDictionary<long, IDictionary<long, ISet<long>>>( );

		/// <summary>
		///     The reverse cache
		/// </summary>
		private readonly ConcurrentDictionary<long, IDictionary<long, ISet<long>>> _reverseCache = new ConcurrentDictionary<long, IDictionary<long, ISet<long>>>( );

		/// <summary>
		///     The sync root
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     The type cache.
		/// </summary>
		private readonly ConcurrentDictionary<long, ISet<long>> _typeCache = new ConcurrentDictionary<long, ISet<long>>( );

		/// <summary>
		///     Gets the count.
		/// </summary>
		/// <value>
		///     The count.
		/// </value>
		public int Count
		{
			get
			{
				return _cache.Count;
			}
		}

		/// <summary>
		///     Gets or sets the
		///     <see>
		///         <cref>IDictionary{System.Int64, ISet{System.Int64}}</cref>
		///     </see>
		///     with the specified key.
		/// </summary>
		/// <value>
		///     The
		///     <see>
		///         <cref>IDictionary{System.Int64, ISet{System.Int64}}</cref>
		///     </see>
		///     .
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public IDictionary<long, ISet<long>> this[ long key ]
		{
			get
			{
				IDictionary<long, ISet<long>> val;

				_cache.TryGetValue( key, out val );

				return val;
			}
			set
			{
				SetValue( key, value );
			}
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		internal bool SetValue( long key, IDictionary<long, ISet<long>> value )
		{
			bool modified = false;

			if ( value == null )
			{
				IDictionary<long, ISet<long>> output;
				return Remove( key, out output );
			}

			lock ( _syncRoot )
			{
				IDictionary<long, ISet<long>> targetTypedCacheValues;
				if ( !_cache.TryGetValue( key, out targetTypedCacheValues ) )
				{
					modified = true;
					targetTypedCacheValues = new ConcurrentDictionary<long, ISet<long>>( );
					_cache [ key ] = targetTypedCacheValues;
				}

				foreach ( KeyValuePair<long, ISet<long>> sourceTypedCacheValue in value.ToArray( ) )
				{
					var newTargetIds = new HashSet<long>( sourceTypedCacheValue.Value );
					ISet<long> existingTargetIds;
					if ( targetTypedCacheValues.TryGetValue( sourceTypedCacheValue.Key, out existingTargetIds ) )
					{
						if ( newTargetIds.Count != existingTargetIds.Count || existingTargetIds.Except( newTargetIds ).Any( ) )
						{
							modified = true;
							targetTypedCacheValues[ sourceTypedCacheValue.Key ] = newTargetIds;
						}
					}
					else
					{
						modified = true;
						targetTypedCacheValues [ sourceTypedCacheValue.Key ] = newTargetIds;
					}

					ISet<long> typedValues;
					if ( !_typeCache.TryGetValue( sourceTypedCacheValue.Key, out typedValues ) )
					{
						modified = true;
						typedValues = new HashSet<long>( );
						_typeCache [ sourceTypedCacheValue.Key ] = typedValues;
					}

					if ( !typedValues.Contains( key ) )
					{
						modified = true;
						typedValues.Add( key );
					}

					foreach ( long sourceValue in sourceTypedCacheValue.Value )
					{
						IDictionary<long, ISet<long>> reverseTypedValues;
						if ( !_reverseCache.TryGetValue( sourceValue, out reverseTypedValues ) )
						{
							modified = true;
							reverseTypedValues = new ConcurrentDictionary<long, ISet<long>>( );
							_reverseCache [ sourceValue ] = reverseTypedValues;
						}

						ISet<long> reverseValues;
						if ( !reverseTypedValues.TryGetValue( sourceTypedCacheValue.Key, out reverseValues ) )
						{
							modified = true;
							reverseValues = new HashSet<long>( );
							reverseTypedValues [ sourceTypedCacheValue.Key ] = reverseValues;
						}

						if ( !reverseValues.Contains( key ) )
						{
							modified = true;
							reverseValues.Add( key );
						}
					}
				}
			}

			return modified;
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public void Clear( )
		{
			lock ( _syncRoot )
			{
				_cache.Clear( );
				_typeCache.Clear( );
				_reverseCache.Clear( );
			}
		}

		/// <summary>
		///     Determines whether the specified key contains key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool ContainsKey( long key )
		{
			return _cache.ContainsKey( key );
		}

		/// <summary>
		///     Debugs the specified cache.
		/// </summary>
		/// <param name="cache">The cache.</param>
		/// <param name="typeCache">The type cache.</param>
		/// <param name="reverseCache">The reverse cache.</param>
		internal void Debug( out IDictionary<long, IDictionary<long, ISet<long>>> cache, out IDictionary<long, ISet<long>> typeCache, out IDictionary<long, IDictionary<long, ISet<long>>> reverseCache )
		{
			cache = _cache;
			typeCache = _typeCache;
			reverseCache = _reverseCache;
		}

		/// <summary>
		///     Dumps this instance.
		/// </summary>
		internal void Dump( )
		{
			System.Console.WriteLine( @"_cache" );
			System.Console.WriteLine( @"------" );

			foreach ( KeyValuePair<long, IDictionary<long, ISet<long>>> entry in _cache )
			{
				foreach ( var subEntry in entry.Value )
				{
					if ( subEntry.Value == null || subEntry.Value.Count == 0 )
					{
						System.Console.WriteLine( @"{0} -> {1} -> {2}", entry.Key, subEntry.Key, @"Empty" );
					}
					else
					{
						foreach ( long subSubEntry in subEntry.Value )
						{
							System.Console.WriteLine( @"{0} -> {1} -> {2}", entry.Key, subEntry.Key, subSubEntry );
						}
					}
				}
			}

			System.Console.WriteLine( @"_typeCache" );
			System.Console.WriteLine( @"----------" );

			foreach ( KeyValuePair<long, ISet<long>> entry in _typeCache )
			{
				foreach ( long subEntry in entry.Value )
				{
					System.Console.WriteLine( @"{0} -> {1}", entry.Key, subEntry );
				}
			}

			System.Console.WriteLine( @"_reverseCache" );
			System.Console.WriteLine( @"-------------" );

			foreach ( KeyValuePair<long, IDictionary<long, ISet<long>>> entry in _reverseCache )
			{
				foreach ( var subEntry in entry.Value )
				{
					if ( subEntry.Value == null || subEntry.Value.Count == 0 )
					{
						System.Console.WriteLine( @"{0} -> {1} -> {2}", entry.Key, subEntry.Key, @"Empty" );
					}
					else
					{
						foreach ( long subSubEntry in subEntry.Value )
						{
							System.Console.WriteLine( @"{0} -> {1} -> {2}", entry.Key, subEntry.Key, subSubEntry );
						}
					}
				}
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="typedCacheValues">The typed cache values.</param>
		/// <returns></returns>
		public bool Remove( long key, out IDictionary<long, ISet<long>> typedCacheValues )
		{
			bool removed = false;

			lock ( _syncRoot )
			{
				if ( _cache.TryGetValue( key, out typedCacheValues ) )
				{
					foreach ( KeyValuePair<long, ISet<long>> typedCacheValue in typedCacheValues.ToArray( ) )
					{
						if ( RemoveEntry( key, typedCacheValue.Key ) )
						{
							removed = true;
						}
					}
				}

				IDictionary<long, ISet<long>> reverseTypedValues;
				if ( _reverseCache.TryGetValue( key, out reverseTypedValues ) )
				{
					foreach ( var reverseTypedValue in reverseTypedValues.ToArray( ) )
					{
						foreach ( long reverseValue in reverseTypedValue.Value.ToArray( ) )
						{
							if ( RemoveEntry( reverseValue, reverseTypedValue.Key ) )
							{
								removed = true;
							}
						}
					}
				}

				ISet<long> keyTypes;
				if ( _typeCache.TryRemove( key, out keyTypes ) )
				{
					removed = true;

					foreach ( long keyType in keyTypes )
					{
						IDictionary<long, ISet<long>> removedKeyTypes;
						Remove( keyType, key, out removedKeyTypes );
					}
				}
			}

			return removed;
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="type">The type.</param>
		/// <param name="typedCacheValues">The typed cache values.</param>
		/// <returns></returns>
		public bool Remove( long key, long type, out IDictionary<long, ISet<long>> typedCacheValues )
		{
			lock ( _syncRoot )
			{
				_cache.TryGetValue( key, out typedCacheValues );

				RemoveEntry( key, type );

				IDictionary<long, ISet<long>> reverseTypedValues;
				if ( _reverseCache.TryGetValue( key, out reverseTypedValues ) )
				{
					ISet<long> reverseValues;
					if ( reverseTypedValues.TryGetValue( type, out reverseValues ) )
					{
						foreach ( long reverseValue in reverseValues.ToArray( ) )
						{
							RemoveEntry( reverseValue, type );
						}
					}
				}

				ISet<long> keyTypes;
				if ( _typeCache.TryRemove( key, out keyTypes ) )
				{
					foreach ( long keyType in keyTypes )
					{
						IDictionary<long, ISet<long>> removedKeyTypes;
						Remove( keyType, key, out removedKeyTypes );
					}
				}
			}

			return true;
		}
        
		/// <summary>
		///     Removes the entry.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="type">The type.</param>
		private bool RemoveEntry( long key, long type )
		{
			bool removed = false;

			lock ( _syncRoot )
			{
				IDictionary<long, ISet<long>> forwardValues;
				if ( _cache.TryGetValue( key, out forwardValues ) )
				{
					ISet<long> forwardTypeValues;
					if ( forwardValues.TryGetValue( type, out forwardTypeValues ) )
					{
						foreach ( long forwardTypedValue in forwardTypeValues )
						{
							IDictionary<long, ISet<long>> reverseTypedValues;
							if ( _reverseCache.TryGetValue( forwardTypedValue, out reverseTypedValues ) )
							{
								ISet<long> reverseValues;
								if ( reverseTypedValues.TryGetValue( type, out reverseValues ) )
								{
									removed = true;
									reverseValues.Remove( key );

									if ( reverseValues.Count == 0 )
									{
										reverseTypedValues.Remove( type );
									}
								}

								if ( reverseTypedValues.Count <= 0 )
								{
									if ( _reverseCache.TryRemove( forwardTypedValue, out reverseTypedValues ) )
									{
										removed = true;
									}
								}
							}
						}

						if ( forwardValues.Remove( type ) )
						{
							removed = true;
						}

						if ( forwardValues.Count <= 0 )
						{
							if ( _cache.TryRemove( key, out forwardValues ) )
							{
								removed = true;
							}
						}
					}

					if ( _typeCache.TryGetValue( type, out forwardTypeValues ) )
					{
						if ( forwardTypeValues.Remove( key ) )
						{
							removed = true;
						}

						if ( forwardTypeValues.Count == 0 )
						{
							if ( _typeCache.TryRemove( type, out forwardTypeValues ) )
							{
								removed = true;
							}
						}
					}
				}
			}

			return removed;
		}

		/// <summary>
		///     Tries the get value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetValue( long key, out IDictionary<long, ISet<long>> value )
		{
			return _cache.TryGetValue( key, out value );
		}

		/// <summary>
		///     Validates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		internal void Validate( long key, IDictionary<long, ISet<long>> values )
		{
			if ( values == null )
			{
				return;
			}

			lock ( _syncRoot )
			{
				IDictionary<long, ISet<long>> reverseValues;
				if ( _reverseCache.TryGetValue( key, out reverseValues ) )
				{
					foreach ( KeyValuePair<long, ISet<long>> value in values )
					{
						ISet<long> targetValues;
						if ( reverseValues.TryGetValue( value.Key, out targetValues ) )
						{
							foreach ( long targetValue in targetValues.ToArray( ) )
							{
								if ( !value.Value.Contains( targetValue ) )
								{
									RemoveEntry( targetValue, value.Key );
								}
							}
						}
					}
				}

				foreach ( KeyValuePair<long, ISet<long>> typedValue in values )
				{
					foreach ( long value in typedValue.Value )
					{
						IDictionary<long, ISet<long>> forwardTypedValues;
						if ( _cache.TryGetValue( value, out forwardTypedValues ) )
						{
							ISet<long> forwardValues;
							if ( forwardTypedValues.TryGetValue( typedValue.Key, out forwardValues ) )
							{
								if ( forwardValues.Count == 0 || !forwardValues.Contains( key ) )
								{
									RemoveEntry( value, typedValue.Key );
								}
							}
						}
					}
				}

				ISet<long> typeValues;
				if ( _typeCache.TryGetValue( key, out typeValues ) )
				{
					foreach ( long typeValue in typeValues.ToArray( ) )
					{
						RemoveEntry( typeValue, key );
					}
				}
			}
		}
	}
}