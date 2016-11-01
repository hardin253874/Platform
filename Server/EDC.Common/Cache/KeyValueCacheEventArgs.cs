// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Cache
{
	/// <summary>
	///     Key Value cached event arguments.
	/// </summary>
	public class KeyValueCacheEventArgs<TKey, TValue> : KeyCacheEventArgs<TKey>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="KeyValueCacheEventArgs{TKey,TValue}" /> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public KeyValueCacheEventArgs( TKey key, TValue value )
			: base( key )
		{
			Value = value;
		}

		/// <summary>
		///     Gets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public TValue Value
		{
			get;
			private set;
		}
	}
}