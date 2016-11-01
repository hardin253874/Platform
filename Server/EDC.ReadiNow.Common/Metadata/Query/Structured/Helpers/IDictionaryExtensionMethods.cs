// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Metadata.Query.Structured.Helpers
{
	/// <summary>
	///     Dictionary extension methods.
	/// </summary>
	public static class DictionaryExtensionMethods
	{
		/// <summary>
		///     Unions the target dictionary with the source dictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="target">The target.</param>
		/// <exception cref="System.NullReferenceException"></exception>
		/// <exception cref="System.InvalidOperationException">Found parameter with mismatched name.</exception>
		public static void UnionWith<TKey, TValue>( this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target )
		{
			if ( source == null )
			{
				throw new NullReferenceException( );
			}

			if ( target == null )
			{
				return;
			}

			foreach ( KeyValuePair<TKey, TValue> pair in target )
			{
				TValue value;

				if ( source.TryGetValue( pair.Key, out value ) )
				{
					if ( ! Equals( value, pair.Value ) )
					{
						throw new InvalidOperationException( "Found parameter with mismatched name." );
					}
				}
				else
				{
					source[ pair.Key ] = pair.Value;
				}
			}
		}
	}
}