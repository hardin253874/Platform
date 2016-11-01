// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Extensions to the System.Object class.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		///     Gets all fields.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <returns></returns>
		public static IEnumerable<Field> GetAllFields( this IEnumerable<EntityType> types )
		{
			if ( types == null )
			{
				throw new NullReferenceException( );
			}

			IEnumerable<Field> fields = Enumerable.Empty<Field>( );

			return types.Aggregate( fields, ( current, type ) => current.Union( type.GetAllFields( ) ) );
		}

		/// <summary>
		///     Locates the index of the specified element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     The index of the specified element if found, -1 otherwise.
		/// </returns>
		[DebuggerStepThrough]
		public static int IndexOf<T>( this IEnumerable<T> obj, T value )
		{
			return obj.IndexOf( value, null );
		}

		/// <summary>
		///     Locates the index of the specified element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>
		///     The index of the specified element if found, -1 otherwise.
		/// </returns>
		[DebuggerStepThrough]
		public static int IndexOf<T>( this IEnumerable<T> obj, T value, IEqualityComparer<T> comparer )
		{
			comparer = comparer ?? EqualityComparer<T>.Default;

			var found = obj.Select( ( element, index ) => new
				{
					element,
					index
				} ).FirstOrDefault( x => comparer.Equals( x.element, value ) );

			return found == null ? -1 : found.index;
		}

		/// <summary>
		///     Determines whether the specified obj is enumerable of the specified type.
		/// </summary>
		/// <typeparam name="T">Enumerable type to check for.</typeparam>
		/// <param name="value">The object to check.</param>
		/// <returns>
		///     <c>true</c> if the specified obj is enumerable; otherwise, <c>false</c>.
		/// </returns>
		[DebuggerStepThrough]
		[SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter" )]
		public static bool IsEnumerable<T>( this object value )
		{
			Type[] interfaces = value.GetType( ).GetInterfaces( );

			return interfaces.Any( iFace =>
				{
					if ( iFace.IsGenericType && iFace.GetGenericTypeDefinition( ) == typeof ( IEnumerable<> ) )
					{
						Type[] genericArguments = iFace.GetGenericArguments( );

						if ( genericArguments.Length > 0 )
						{
							return typeof ( T ).IsAssignableFrom( genericArguments[ 0 ] );
						}
					}

					return false;
				} );
		}

		/// <summary>
		///     Determines whether the specified obj is enumerable.
		/// </summary>
		/// <param name="value">The obj.</param>
		/// <returns>
		///     <c>true</c> if the specified obj is enumerable; otherwise, <c>false</c>.
		/// </returns>
		[DebuggerStepThrough]
		public static bool IsEnumerable( this object value )
		{
			Type[] interfaces = value.GetType( ).GetInterfaces( );

			return interfaces.Any( iFace => iFace == typeof ( IEnumerable ) );
		}

		/// <summary>
		///     Converts a single instance of type T into an enumerable set of T.
		/// </summary>
		/// <typeparam name="T">The type of the enumeration to return. This can be inferred from the objects type.</typeparam>
		/// <param name="obj">The object to be converted into an enumerable set.</param>
		/// <returns>
		///     An enumeration containing the specified object.
		/// </returns>
		[DebuggerStepThrough]
		public static IEnumerable<T> ToEnumerable<T>( this T obj )
		{
			return Enumerable.Repeat( obj, 1 );
		}

		/// <summary>
		///     Creates a grouped dictionary based off the input IEnumerable using the delegate functions.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>
		///     A grouped dictionary.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public static IDictionary<TKey, ISet<TSource>> ToGroupedDictionary<TSource, TKey>( this IEnumerable<TSource> source, Func<TSource, TKey> keySelector )
		{
			var dictionary = new Dictionary<TKey, ISet<TSource>>( );

			foreach ( TSource element in source )
			{
				/////
				// ReSharper disable EmptyGeneralCatchClause
				/////
				try
				{
					TKey key = keySelector( element );

					ISet<TSource> existingValues;

					if ( !dictionary.TryGetValue( key, out existingValues ) )
					{
						existingValues = new HashSet<TSource>( );
						dictionary[ key ] = existingValues;
					}

					existingValues.Add( element );
				}
				catch
				{
					/////
					// Ignore any exceptions here.
					/////
				}

				/////
				// ReSharper restore EmptyGeneralCatchClause
				/////
			}

			return dictionary;
		}

		/// <summary>
		///     Creates a grouped dictionary based off the input IEnumerable using the delegate functions.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TElement">The type of the element.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="valueSelector">The value selector.</param>
		/// <returns>
		///     A grouped dictionary.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public static IDictionary<TKey, ISet<TElement>> ToGroupedDictionary<TSource, TKey, TElement>( this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> valueSelector )
		{
			var dictionary = new Dictionary<TKey, ISet<TElement>>( );

			foreach ( TSource element in source )
			{
				/////
				// ReSharper disable EmptyGeneralCatchClause
				/////
				try
				{
					TKey key = keySelector( element );

					ISet<TElement> existingValues;

					if ( !dictionary.TryGetValue( key, out existingValues ) )
					{
						existingValues = new HashSet<TElement>( );
						dictionary[ key ] = existingValues;
					}

					existingValues.Add( valueSelector( element ) );
				}
				catch
				{
					/////
					// Ignore any exceptions here.
					/////
				}
				/////
				// ReSharper restore EmptyGeneralCatchClause
				/////
			}

			return dictionary;
		}

		/// <summary>
		///     Creates a grouped dictionary (using an enumerable key) based off the input IEnumerable using the delegate functions.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>
		///     A grouped dictionary.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public static IDictionary<TKey, ISet<TSource>> ToManyGroupedDictionary<TSource, TKey>( this IEnumerable<TSource> source, Func<TSource, IEnumerable<TKey>> keySelector )
		{
			var dictionary = new Dictionary<TKey, ISet<TSource>>( );

			foreach ( TSource element in source )
			{
				/////
				// ReSharper disable EmptyGeneralCatchClause
				/////
				try
				{
					IEnumerable<TKey> keys = keySelector( element );

					foreach ( TKey key in keys )
					{
						ISet<TSource> existingValues;

						if ( !dictionary.TryGetValue( key, out existingValues ) )
						{
							existingValues = new HashSet<TSource>( );
							dictionary[ key ] = existingValues;
						}

						existingValues.Add( element );
					}
				}
				catch
				{
					/////
					// Ignore any exceptions here.
					/////
				}
				/////
				// ReSharper restore EmptyGeneralCatchClause
				/////
			}

			return dictionary;
		}

		/// <summary>
		/// Creates a grouped dictionary based off the input IEnumerable using the delegate functions.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TElement">The type of the element.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="valueSelector">The value selector.</param>
		/// <param name="comparer">Optional key equality comparer.</param>
		/// <returns>
		/// A grouped dictionary.
		/// </returns>
		[SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public static IDictionary<TKey, ISet<TElement>> ToManyGroupedDictionary<TSource, TKey, TElement>( this IEnumerable<TSource> source, Func<TSource, IEnumerable<TKey>> keySelector, Func<TSource, TElement> valueSelector, IEqualityComparer<TKey> comparer = null )
		{
			Dictionary<TKey, ISet<TElement>> dictionary = comparer == null ? new Dictionary<TKey, ISet<TElement>>( ) : new Dictionary<TKey, ISet<TElement>>( comparer );

			foreach ( TSource element in source )
			{
				/////
				// ReSharper disable EmptyGeneralCatchClause
				/////
				try
				{
					IEnumerable<TKey> keys = keySelector( element );

					foreach ( TKey key in keys )
					{
						ISet<TElement> existingValues;

						if ( !dictionary.TryGetValue( key, out existingValues ) )
						{
							existingValues = new HashSet<TElement>( );
							dictionary[ key ] = existingValues;
						}

						existingValues.Add( valueSelector( element ) );
					}
				}
				catch
				{
					/////
					// Ignore any exceptions here.
					/////
				}
				/////
				// ReSharper restore EmptyGeneralCatchClause
				/////
			}

			return dictionary;
		}
	}
}