// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Interface for performing double check locking.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public interface IDoubleCheckLock<TKey, TValue>
	{
		/// <summary>
		///     Doubles the check lock get.
		/// </summary>
		/// <param name="check">The check.</param>
		/// <param name="checkFail">The check fail.</param>
		/// <returns></returns>
		TValue Get( TKey check, Func<TValue> checkFail );
	}
}