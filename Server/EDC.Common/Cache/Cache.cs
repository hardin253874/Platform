// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Cache.Locators;

namespace EDC.Cache
{
	/// <summary>
	///     Cache base class that uses Dependency Injection to obtain the cache provider.
	/// </summary>
	public abstract class Cache<TKey, TValue> : IDisposable
	{
		/// <summary>
		///     Cache provider.
		/// </summary>
        private readonly ICache<TKey, TValue> _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cache" /> class.
        /// </summary>
        /// <param name="cacheName">The name of the cache - used to resolve the provider using the default locator.</param>
        protected Cache(string cacheName)
        {
            if (cacheName == null)
            {
                throw new ArgumentNullException("cacheName");
            }

            _provider = Default.GetProvider<TKey, TValue>(cacheName);
        }

		/// <summary>
		///     Initializes a new instance of the <see cref="Cache" /> class.
		/// </summary>
		/// <param name="cacheProvider">The cache provider.</param>
		protected Cache( ICache<TKey, TValue> cacheProvider )
		{
			if ( cacheProvider == null )
			{
				throw new ArgumentNullException( "cacheProvider" );
			}

			_provider = cacheProvider;
		}

		/// <summary>
		///     Gets or sets the <see cref="System.Object" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="System.Object" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public virtual TValue this[ TKey key ]
		{
			get
			{
				return Get( key );
			}
			set
			{
				Provider[key] = value;

				OnRaiseAdd( key, value );
			}
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		public int Count
		{
			get
			{
				return Provider.Count;
			}
		}

		/// <summary>
		///     Gets or sets the provider.
		/// </summary>
		/// <value>
		///     The provider.
		/// </value>
        private ICache<TKey, TValue> Provider
		{
			get
			{
				return _provider;
			}
		}

		/// <summary>
		///     Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public virtual void Add( TKey key, TValue value )
		{
			ProviderAdd( key, value );

			OnRaiseAdd( key, value );
		}

		/// <summary>
		///     Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		protected virtual void ProviderAdd( TKey key, TValue value )
		{
			Provider.Add( key, value );
		}

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public virtual void Clear( )
		{
			ProviderClear( );

			OnRaiseCleared( );
		}

		/// <summary>
		/// Providers the clear.
		/// </summary>
		protected virtual void ProviderClear( )
		{
			Provider.Clear( );
		}

		/// <summary>
		///     Determines whether the specified key contains key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
		/// </returns>
		public virtual bool ContainsKey( TKey key )
		{
			return Provider.ContainsKey( key );
		}

	    /// <summary>
	    ///     Gets the specified key.
	    /// </summary>
	    /// <param name="key">The key.</param>
	    /// <returns></returns>
	    public virtual TValue Get(TKey key)
	    {
	        TValue result = Provider[key];

	        return result;
	    }

	    /// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public virtual bool Remove( TKey key )
	    {
		    bool result = ProviderRemove( key );

			OnRaiseRemoved( key );

		    return result;
		}

		/// <summary>
		/// Providers the remove.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected virtual bool ProviderRemove( TKey key )
		{
			return Provider.Remove( key );
		}

		/// <summary>
		///     Tries the get value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public virtual bool TryGetValue( TKey key, out TValue value )
		{
			bool result = Provider.TryGetValue( key, out value );

			return result;
		}

		/// <summary>
		///     Called when an element is added.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		protected virtual void OnRaiseAdd( TKey key, TValue value )
		{
			EventHandler<KeyValueCacheEventArgs<TKey, TValue>> handler = Added;

			if ( handler != null )
			{
				handler( this, new KeyValueCacheEventArgs<TKey, TValue>( key, value ) );
			}
		}

		/// <summary>
		///     Called when the cache is cleared.
		/// </summary>
		protected virtual void OnRaiseCleared( )
		{
			EventHandler<CacheEventArgs> handler = Cleared;

			if ( handler != null )
			{
				handler( this, new CacheEventArgs( ) );
			}
		}

		/// <summary>
		///     Called when an element is removed.
		/// </summary>
		/// <param name="key">The key.</param>
		protected virtual void OnRaiseRemoved( TKey key )
		{
			EventHandler<KeyCacheEventArgs<TKey>> handler = Removed;

			if ( handler != null )
			{
				handler( this, new KeyCacheEventArgs<TKey>( key ) );
			}
		}

		/// <summary>
		///     Occurs when an element is added.
		/// </summary>
		public event EventHandler<KeyValueCacheEventArgs<TKey, TValue>> Added;

		/// <summary>
		///     Occurs when the cache is cleared.
		/// </summary>
		public event EventHandler<CacheEventArgs> Cleared;

		/// <summary>
		///     Occurs when an element is removed.
		/// </summary>
		public event EventHandler<KeyCacheEventArgs<TKey>> Removed;

		/// <summary>
		///		Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///		Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose( bool disposing )
		{

		}
	}
}