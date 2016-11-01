// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Cache
{
	/// <summary>
	///     Keyed cache event arguments.
	/// </summary>
	public class KeyCacheEventArgs<TKey> : CacheEventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="KeyCacheEventArgs{TKey}" /> class.
		/// </summary>
		/// <param name="key">The key.</param>
		public KeyCacheEventArgs( TKey key )
		{
			Key = key;
		}

		/// <summary>
		///     Gets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		public TKey Key
		{
			get;
			private set;
		}
	}
}