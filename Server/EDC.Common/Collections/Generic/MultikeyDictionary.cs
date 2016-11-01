// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     Multikey dictionary.
	/// </summary>
	/// <typeparam name="TKey1">The type of the key1.</typeparam>
	/// <typeparam name="TKey2">The type of the key2.</typeparam>
	/// <typeparam name="TValue">The type of the value stored.</typeparam>
	/// <remarks>
	///     Implements a dictionary that allows multiple keys to be assigned to the one value.
	///     When looking up via the primary key, retrieval time is still O(1).
	///     Looking up via any of the related keys is still O(1).
	///     To relate a child key with the primary key, call the 'Relate' method.
	///     To remove a relation, call the the 'Unrelate' method.
	/// </remarks>
	[SuppressMessage( "Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes" )]
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	[SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
	public class MultikeyDictionary<TKey1, TKey2, TValue> : IEnumerable<KeyValuePair<TKey1, TValue>>
	{
		/// <summary>
		///     Related key to primary key lookup.
		/// </summary>
		private readonly Dictionary<TKey2, TKey1> _nonPrimaryKeyLookup = new Dictionary<TKey2, TKey1>( );

		/// <summary>
		///     Primary key to related key lookup.
		/// </summary>
		private readonly Dictionary<TKey1, TKey2> _primaryKeyLookup = new Dictionary<TKey1, TKey2>( );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		protected readonly object SyncRoot = new object( );

		/// <summary>
		///     Primary key to value lookup.
		/// </summary>
		private readonly Dictionary<TKey1, TValue> _valueLookup = new Dictionary<TKey1, TValue>( );

		/// <summary>
		///     Gets the count.
		/// </summary>
		public int Count
		{
			get
			{
				lock ( SyncRoot )
				{
					return _valueLookup.Count;
				}
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="TValue" /> with the specified primary key.
		/// </summary>
		public TValue this[ TKey1 key ]
		{
			get
			{
				lock ( SyncRoot )
				{
					return _valueLookup[ key ];
				}
			}
			set
			{
				lock ( SyncRoot )
				{
					_valueLookup[ key ] = value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="TValue" /> with the specified key.
		/// </summary>
		public TValue this[ TKey2 key ]
		{
			get
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						return _valueLookup[ primaryKey ];
					}
				}

				throw new KeyNotFoundException( );
			}
			set
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						_valueLookup[ primaryKey ] = value;
						return;
					}
				}

				throw new KeyNotFoundException( );
			}
		}

		/// <summary>
		///     Gets the collection of primary keys.
		/// </summary>
		public ICollection<TKey1> Keys
		{
			get
			{
				lock ( SyncRoot )
				{
					return _primaryKeyLookup.Keys;
				}
			}
		}

		/// <summary>
		///     Gets the values.
		/// </summary>
		public ICollection<TValue> Values
		{
			get
			{
				lock ( SyncRoot )
				{
					return _valueLookup.Values;
				}
			}
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey1, TValue>> GetEnumerator( )
		{
			lock ( SyncRoot )
			{
				return _valueLookup.GetEnumerator( );
			}
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			lock ( SyncRoot )
			{
				return ( ( IEnumerable ) _valueLookup ).GetEnumerator( );
			}
		}

		/// <summary>
		///     Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Add( TKey1 key, TValue value )
		{
			lock ( SyncRoot )
			{
				_valueLookup.Add( key, value );
			}
		}

		/// <summary>
		///     Clears this dictionary.
		/// </summary>
		public virtual void Clear( )
		{
			lock ( SyncRoot )
			{
				_valueLookup.Clear( );
				_primaryKeyLookup.Clear( );
				_nonPrimaryKeyLookup.Clear( );
			}
		}

		/// <summary>
		///     Determines whether the Multikey dictionary contains the specified primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the Multikey dictionary contains the specified primary key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey( TKey1 key )
		{
			lock ( SyncRoot )
			{
				return _valueLookup.ContainsKey( key );
			}
		}

		/// <summary>
		///     Determines whether the Multikey dictionary contains the specified related key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the Multikey dictionary contains the specified related key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey( TKey2 key )
		{
			lock ( SyncRoot )
			{
				return _nonPrimaryKeyLookup.ContainsKey( key );
			}
		}

		/// <summary>
		///     Relates the specified key to an existing primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="primaryKey">The primary key.</param>
		public void Relate( TKey2 key, TKey1 primaryKey )
		{
			lock ( SyncRoot )
			{
				if ( ContainsKey( primaryKey ) )
				{
					_nonPrimaryKeyLookup[ key ] = primaryKey;
					_primaryKeyLookup[ primaryKey ] = key;
				}
				else
				{
					throw new KeyNotFoundException( );
				}
			}
		}

		/// <summary>
		///     Removes the specified primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public virtual bool Remove( TKey1 key )
		{
			lock ( SyncRoot )
			{
				TKey2 nonPrimaryKey;

				if ( _primaryKeyLookup.TryGetValue( key, out nonPrimaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( nonPrimaryKey );
					_primaryKeyLookup.Remove( key );
				}

				return _valueLookup.Remove( key );
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public virtual bool Remove( TKey2 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( key );
					_primaryKeyLookup.Remove( primaryKey );
				}

				return _valueLookup.Remove( primaryKey );
			}
		}

		/// <summary>
		///     Tries the get value using the specified primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the value is retrieved; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetValue( TKey1 key, out TValue value )
		{
			lock ( SyncRoot )
			{
				return _valueLookup.TryGetValue( key, out value );
			}
		}

		/// <summary>
		///     Tries the get value using the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the value is retrieved; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetValue( TKey2 key, out TValue value )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					return _valueLookup.TryGetValue( primaryKey, out value );
				}
			}

			value = default( TValue );
			return false;
		}

		/// <summary>
		///     Unrelates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the related key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Unrelate( TKey2 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_primaryKeyLookup.Remove( primaryKey );
				}

				return _nonPrimaryKeyLookup.Remove( key );
			}
		}
	}

	/// <summary>
	///     Multikey dictionary.
	/// </summary>
	/// <typeparam name="TKey1">The type of the key1.</typeparam>
	/// <typeparam name="TKey2">The type of the key2.</typeparam>
	/// <typeparam name="TKey3">The type of the key3.</typeparam>
	/// <typeparam name="TValue">The type of the value stored.</typeparam>
	/// <remarks>
	///     Implements a dictionary that allows multiple keys to be assigned to the one value.
	///     When looking up via the primary key, retrieval time is still O(1).
	///     Looking up via any of the related keys is still O(1).
	///     To relate a child key with the primary key, call the 'Relate' method.
	///     To remove a relation, call the the 'Unrelate' method.
	/// </remarks>
	/// \
	[SuppressMessage( "Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes" )]
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	[SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
	public class MultikeyDictionary<TKey1, TKey2, TKey3, TValue> : MultikeyDictionary<TKey1, TKey2, TValue>
	{
		/// <summary>
		///     Related key to primary key lookup.
		/// </summary>
		private readonly Dictionary<TKey3, TKey1> _nonPrimaryKeyLookup = new Dictionary<TKey3, TKey1>( );

		/// <summary>
		///     Primary key to related key lookup.
		/// </summary>
		private readonly Dictionary<TKey1, TKey3> _primaryKeyLookup = new Dictionary<TKey1, TKey3>( );

		/// <summary>
		///     Gets or sets the <see cref="TValue" /> with the specified key.
		/// </summary>
		public TValue this[ TKey3 key ]
		{
			get
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						return base[ primaryKey ];
					}
				}

				throw new KeyNotFoundException( );
			}
			set
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						base[ primaryKey ] = value;
						return;
					}
				}

				throw new KeyNotFoundException( );
			}
		}

		/// <summary>
		///     Clears this dictionary.
		/// </summary>
		public override void Clear( )
		{
			lock ( SyncRoot )
			{
				_primaryKeyLookup.Clear( );
				_nonPrimaryKeyLookup.Clear( );

				base.Clear( );
			}
		}

		/// <summary>
		///     Determines whether the Multikey dictionary contains the specified related key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the Multikey dictionary contains the specified related key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey( TKey3 key )
		{
			lock ( SyncRoot )
			{
				return _nonPrimaryKeyLookup.ContainsKey( key );
			}
		}

		/// <summary>
		///     Relates the specified key to an existing primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="primaryKey">The primary key.</param>
		public void Relate( TKey3 key, TKey1 primaryKey )
		{
			lock ( SyncRoot )
			{
				if ( ContainsKey( primaryKey ) )
				{
					_nonPrimaryKeyLookup[ key ] = primaryKey;
					_primaryKeyLookup[ primaryKey ] = key;
				}
				else
				{
					throw new KeyNotFoundException( );
				}
			}
		}

		/// <summary>
		///     Removes the specified primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public override bool Remove( TKey1 key )
		{
			lock ( SyncRoot )
			{
				TKey3 nonPrimaryKey;

				if ( _primaryKeyLookup.TryGetValue( key, out nonPrimaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( nonPrimaryKey );
					_primaryKeyLookup.Remove( key );
				}

				return base.Remove( key );
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Remove( TKey3 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( key );
					_primaryKeyLookup.Remove( primaryKey );
				}

				return base.Remove( primaryKey );
			}
		}

		/// <summary>
		///     Tries the get value using the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the value is retrieved; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetValue( TKey3 key, out TValue value )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					return TryGetValue( primaryKey, out value );
				}
			}

			value = default( TValue );
			return false;
		}

		/// <summary>
		///     Unrelates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the related key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Unrelate( TKey3 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_primaryKeyLookup.Remove( primaryKey );
				}

				return _nonPrimaryKeyLookup.Remove( key );
			}
		}
	}

	/// <summary>
	///     Multikey dictionary.
	/// </summary>
	/// <typeparam name="TKey1">The type of the key1.</typeparam>
	/// <typeparam name="TKey2">The type of the key2.</typeparam>
	/// <typeparam name="TKey3">The type of the key3.</typeparam>
	/// <typeparam name="TKey4">The type of the key4.</typeparam>
	/// <typeparam name="TValue">The type of the value stored.</typeparam>
	/// <remarks>
	///     Implements a dictionary that allows multiple keys to be assigned to the one value.
	///     When looking up via the primary key, retrieval time is still O(1).
	///     Looking up via any of the related keys is still O(1).
	///     To relate a child key with the primary key, call the 'Relate' method.
	///     To remove a relation, call the the 'Unrelate' method.
	/// </remarks>
	[SuppressMessage( "Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes" )]
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	[SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
	public class MultikeyDictionary<TKey1, TKey2, TKey3, TKey4, TValue> : MultikeyDictionary<TKey1, TKey2, TKey3, TValue>
	{
		/// <summary>
		///     Related key to primary key lookup.
		/// </summary>
		private readonly Dictionary<TKey4, TKey1> _nonPrimaryKeyLookup = new Dictionary<TKey4, TKey1>( );

		/// <summary>
		///     Primary key to related key lookup.
		/// </summary>
		private readonly Dictionary<TKey1, TKey4> _primaryKeyLookup = new Dictionary<TKey1, TKey4>( );

		/// <summary>
		///     Gets or sets the <see cref="TValue" /> with the specified key.
		/// </summary>
		public TValue this[ TKey4 key ]
		{
			get
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						return base[ primaryKey ];
					}
				}

				throw new KeyNotFoundException( );
			}
			set
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						base[ primaryKey ] = value;
						return;
					}
				}

				throw new KeyNotFoundException( );
			}
		}

		/// <summary>
		///     Clears this dictionary.
		/// </summary>
		public override void Clear( )
		{
			lock ( SyncRoot )
			{
				_primaryKeyLookup.Clear( );
				_nonPrimaryKeyLookup.Clear( );

				base.Clear( );
			}
		}

		/// <summary>
		///     Determines whether the Multikey dictionary contains the specified related key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the Multikey dictionary contains the specified related key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey( TKey4 key )
		{
			lock ( SyncRoot )
			{
				return _nonPrimaryKeyLookup.ContainsKey( key );
			}
		}

		/// <summary>
		///     Relates the specified key to an existing primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="primaryKey">The primary key.</param>
		public void Relate( TKey4 key, TKey1 primaryKey )
		{
			lock ( SyncRoot )
			{
				if ( ContainsKey( primaryKey ) )
				{
					_nonPrimaryKeyLookup[ key ] = primaryKey;
					_primaryKeyLookup[ primaryKey ] = key;
				}
				else
				{
					throw new KeyNotFoundException( );
				}
			}
		}

		/// <summary>
		///     Removes the specified primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public override bool Remove( TKey1 key )
		{
			lock ( SyncRoot )
			{
				TKey4 nonPrimaryKey;

				if ( _primaryKeyLookup.TryGetValue( key, out nonPrimaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( nonPrimaryKey );
					_primaryKeyLookup.Remove( key );
				}

				return base.Remove( key );
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Remove( TKey4 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( key );
					_primaryKeyLookup.Remove( primaryKey );
				}

				return base.Remove( primaryKey );
			}
		}

		/// <summary>
		///     Tries the get value using the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the value is retrieved; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetValue( TKey4 key, out TValue value )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					return TryGetValue( primaryKey, out value );
				}
			}

			value = default( TValue );
			return false;
		}

		/// <summary>
		///     Unrelates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the related key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Unrelate( TKey4 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_primaryKeyLookup.Remove( primaryKey );
				}

				return _nonPrimaryKeyLookup.Remove( key );
			}
		}
	}

	/// <summary>
	///     Multikey dictionary.
	/// </summary>
	/// <typeparam name="TKey1">The type of the key1.</typeparam>
	/// <typeparam name="TKey2">The type of the key2.</typeparam>
	/// <typeparam name="TKey3">The type of the key3.</typeparam>
	/// <typeparam name="TKey4">The type of the key4.</typeparam>
	/// <typeparam name="TKey5">The type of the key5.</typeparam>
	/// <typeparam name="TValue">The type of the value stored.</typeparam>
	/// <remarks>
	///     Implements a dictionary that allows multiple keys to be assigned to the one value.
	///     When looking up via the primary key, retrieval time is still O(1).
	///     Looking up via any of the related keys is still O(1).
	///     To relate a child key with the primary key, call the 'Relate' method.
	///     To remove a relation, call the the 'Unrelate' method.
	/// </remarks>
	[SuppressMessage( "Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes" )]
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	[SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
	public class MultikeyDictionary<TKey1, TKey2, TKey3, TKey4, TKey5, TValue> : MultikeyDictionary<TKey1, TKey2, TKey3, TKey4, TValue>
	{
		/// <summary>
		///     Related key to primary key lookup.
		/// </summary>
		private readonly Dictionary<TKey5, TKey1> _nonPrimaryKeyLookup = new Dictionary<TKey5, TKey1>( );

		/// <summary>
		///     Primary key to related key lookup.
		/// </summary>
		private readonly Dictionary<TKey1, TKey5> _primaryKeyLookup = new Dictionary<TKey1, TKey5>( );

		/// <summary>
		///     Gets or sets the <see cref="TValue" /> with the specified key.
		/// </summary>
		public TValue this[ TKey5 key ]
		{
			get
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						return base[ primaryKey ];
					}
				}

				throw new KeyNotFoundException( );
			}
			set
			{
				lock ( SyncRoot )
				{
					TKey1 primaryKey;

					if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
					{
						base[ primaryKey ] = value;
						return;
					}
				}

				throw new KeyNotFoundException( );
			}
		}

		/// <summary>
		///     Clears this dictionary.
		/// </summary>
		public override void Clear( )
		{
			lock ( SyncRoot )
			{
				_primaryKeyLookup.Clear( );
				_nonPrimaryKeyLookup.Clear( );

				base.Clear( );
			}
		}

		/// <summary>
		///     Determines whether the Multikey dictionary contains the specified related key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the Multikey dictionary contains the specified related key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey( TKey5 key )
		{
			lock ( SyncRoot )
			{
				return _nonPrimaryKeyLookup.ContainsKey( key );
			}
		}

		/// <summary>
		///     Relates the specified key to an existing primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="primaryKey">The primary key.</param>
		public void Relate( TKey5 key, TKey1 primaryKey )
		{
			lock ( SyncRoot )
			{
				if ( ContainsKey( primaryKey ) )
				{
					_nonPrimaryKeyLookup[ key ] = primaryKey;
					_primaryKeyLookup[ primaryKey ] = key;
				}
				else
				{
					throw new KeyNotFoundException( );
				}
			}
		}

		/// <summary>
		///     Removes the specified primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public override bool Remove( TKey1 key )
		{
			lock ( SyncRoot )
			{
				TKey5 nonPrimaryKey;

				if ( _primaryKeyLookup.TryGetValue( key, out nonPrimaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( nonPrimaryKey );
					_primaryKeyLookup.Remove( key );
				}

				return base.Remove( key );
			}
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Remove( TKey5 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_nonPrimaryKeyLookup.Remove( key );
					_primaryKeyLookup.Remove( primaryKey );
				}

				return base.Remove( primaryKey );
			}
		}

		/// <summary>
		///     Tries the get value using the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the value is retrieved; otherwise, <c>false</c>.
		/// </returns>
		public bool TryGetValue( TKey5 key, out TValue value )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					return TryGetValue( primaryKey, out value );
				}
			}

			value = default( TValue );
			return false;
		}

		/// <summary>
		///     Unrelates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the related key is removed; otherwise, <c>false</c>.
		/// </returns>
		public bool Unrelate( TKey5 key )
		{
			lock ( SyncRoot )
			{
				TKey1 primaryKey;

				if ( _nonPrimaryKeyLookup.TryGetValue( key, out primaryKey ) )
				{
					_primaryKeyLookup.Remove( primaryKey );
				}

				return _nonPrimaryKeyLookup.Remove( key );
			}
		}
	}
}