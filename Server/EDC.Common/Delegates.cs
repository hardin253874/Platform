// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;

// ReSharper disable CheckNamespace

namespace EDC.Common
{
	/// <summary>
	///     Collection of assorted delegate helpers.
	/// </summary>
	public static class Delegates
	{
		/// <summary>
		///     Adds a list of values to an existing collection.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="target">The collection to receive the values.</param>
		/// <param name="values">The list of values to add.</param>
		public static void AddRange<T>( this ICollection<T> target, IEnumerable<T> values )
		{
			var list = target as List<T>;

			if ( list != null )
			{
				list.AddRange( values );
			}
			else
			{
				foreach ( T value in values )
				{
					target.Add( value );
				}
			}
		}

		/// <summary>
		///     Converts the specified enumeration as a list.
		///     A non-null list is always returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public static List<T> AsList<T>( this IEnumerable<T> source )
		{
			var result = new List<T>( );

			if ( source != null )
			{
				result.AddRange( source );
			}

			return result;
		}

		/// <summary>
		///     Copies the specified list.
		/// </summary>
		/// <typeparam name="T">The type of the list.</typeparam>
		/// <param name="list">The list.</param>
		/// <returns>The copied list.</returns>
		public static List<T> Copy<T>( this List<T> list )
		{
			// Yes, this works. However ToList does not intuitively convey an intention to copy.
			return list.ToList( );
		}

		/// <summary>
		///     Copies the specified list.
		/// </summary>
		/// <typeparam name="TList"> </typeparam>
		/// <typeparam name="TItem"> </typeparam>
		/// <returns>The copied list.</returns>
		public static TList Copy<TList, TItem>( this IEnumerable<TItem> source ) where TList : List<TItem>, new( )
		{
			var result = new TList( );
			var asCollection = source as ICollection<TItem>;

			if ( asCollection != null )
			{
				// Pre-allocate sufficient capacity in target list (with a bit of extra wiggle room)
				result.Capacity = asCollection.Count + 16;
			}

			// Copy items
			result.AddRange( source );

			return result;
		}

		/// <summary>
		///     Given a selector function, creates a comparer that can compare some type by selecting data out of it and selecting that type.
		///     Example: CreateComparer(res => res.Name) to create a resource comparer that compares by name.
		/// </summary>
		/// <typeparam name="TSelector">Type of data being compared.</typeparam>
		/// <typeparam name="TData">Type of data accessed by the selector.</typeparam>
		/// <param name="selector">The selector function.</param>
		/// <returns>A comparer</returns>
		public static IComparer<TSelector> CreateComparer<TSelector, TData>( Func<TSelector, TData> selector )
		{
			return new GenericComparer<TSelector, TData>
				{
					Selector = selector
				};
		}

		/// <summary>
		///     Converts a null list to an empty list.
		/// </summary>
		public static List<T> EmptyIfNull<T>( this List<T> list )
		{
			if ( list == null )
			{
				return new List<T>( );
			}
			return list;
		}

		/// <summary>
		///     Locates the position of the first element in a collection that matches a predicate, or returns -1 if not found.
		/// </summary>
		/// <typeparam name="T">Type of collection.</typeparam>
		/// <param name="collection">Collection to search.</param>
		/// <param name="test">Test to perform.</param>
		/// <returns>Zero based index of first match, or -1 if not found.</returns>
		public static int IndexOf<T>( this IEnumerable<T> collection, Predicate<T> test )
		{
			if ( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}
			if ( test == null )
			{
				throw new ArgumentNullException( "test" );
			}

			int i = 0;

			foreach ( T value in collection )
			{
				if ( test( value ) )
				{
					return i;
				}
				i++;
			}
			return -1;
		}

		/// <summary>
		///     Takes a single item and returns a list of that item type, containing the item.
		/// </summary>
		/// <typeparam name="T">Type of the list.</typeparam>
		/// <param name="value">The item to place in the list.</param>
		/// <returns></returns>
		public static List<T> ListOfOne<T>( T value )
		{
			var list = new List<T>
				{
					value
				};

			return list;
        }

        /// <summary>
        ///     Force evaluation of an enumerable, and discard it.
        /// </summary>
        /// <typeparam name="T">Type of item being enumerated.</typeparam>
        /// <returns></returns>
        public static void VisitAll<T>( this IEnumerable<T> value )
        {
            if ( value == null )
                return;

            IEnumerator<T> enumerator = value.GetEnumerator( );
            while ( enumerator.MoveNext( ) )
            {
                // This space is intentionally left blank.
            }
        }

        /// <summary>
        ///     Converts an empty list to null.
        /// </summary>
        public static List<T> NullIfEmpty<T>( this List<T> list )
		{
			if ( list == null || list.Count == 0 )
			{
				return null;
			}

			return list;
		}

		/// <summary>
		///     Removes all items in an IList that match a predicate.
		/// </summary>
		/// <typeparam name="T">The type of the list.</typeparam>
		/// <param name="list">The list.</param>
		/// <param name="match">The match.</param>
		public static void RemoveAll<T>( this IList<T> list, Predicate<T> match )
		{
			var lst = list as List<T>;

			if ( lst != null )
			{
				lst.RemoveAll( match );
			}
			else
			{
				for ( int i = list.Count - 1; i != -1; --i )
				{
					if ( match( list[ i ] ) )
					{
						list.RemoveAt( i );
					}
				}
			}
		}

		/// <summary>
		///     Creates a single predicate from a list of predicates that requires that every individual predicate evaluate to true.
		/// </summary>
		/// <param name="predicateList">A list of predicates.</param>
		/// <param name="returnNullIfEmpty">If set to true, an empty list will return null. If set to false, an empty list will return an always-true predicate.</param>
		/// <returns>
		///     A single predicate that evaluates to true if every individual predicate does.
		/// </returns>
		public static Predicate<T> RequireAll<T>( IEnumerable<Predicate<T>> predicateList, bool returnNullIfEmpty )
		{
			Predicate<T>[] predicateArray = predicateList.ToArray( );
			int count = predicateArray.Length;

			switch ( count )
			{
				case 0:
					if ( returnNullIfEmpty )
					{
						return null;
					}
					return value => true; // always evaluate true
				case 1:
					return predicateArray[ 0 ]; // optimise special case

				default:
					return value =>
						{
							for ( int i = 0; i < count; i++ )
							{
								if ( !predicateArray[ i ]( value ) )
								{
									return false;
								}
							}
							return true;
						};
			}
		}

		/// <summary>
		///     Creates a single predicate from a list of predicates that requires that any one predicate evaluate to true.
		/// </summary>
		/// <param name="predicateList">A list of predicates.</param>
		/// <param name="returnNullIfEmpty">If set to true, an empty list will return null. If set to false, an empty list will return an always-false predicate.</param>
		/// <returns>
		///     A single predicate that evaluates to true if at least one predicate does.
		/// </returns>
		public static Predicate<T> RequireAny<T>( IEnumerable<Predicate<T>> predicateList, bool returnNullIfEmpty )
		{
			Predicate<T>[] predicateArray = predicateList.ToArray( );
			int count = predicateArray.Length;

			switch ( count )
			{
				case 0:
					if ( returnNullIfEmpty )
					{
						return null;
					}
					return value => false; // always evaluate false
				case 1:
					return predicateArray[ 0 ]; // optimise special case

				default:
					return value =>
						{
							for ( int i = 0; i < count; i++ )
							{
								if ( predicateArray[ i ]( value ) )
								{
									return true;
								}
							}
							return false;
						};
			}
		}

		/// <summary>
		///     Retries the specified action a number of times.
		/// </summary>
		/// <param name="maxRetries">The maximum number of retries.</param>
		/// <param name="millisecondsTimeout">The milliseconds timeout.</param>
		/// <param name="action">The action to perform.</param>
		/// <returns>True if the action succeeded; false otherwise.</returns>
		public static bool Retry( int maxRetries, int millisecondsTimeout, Func<bool> action )
		{
			int counter = 0;

			bool result = action( );

			while ( !result && counter < maxRetries )
			{
				counter++;
				Thread.Sleep( millisecondsTimeout );
				result = action( );
			}

			return result;
		}

		/// <summary>
		///     Copies the specified list.
		/// </summary>
		/// <typeparam name="TList">The type of the list.</typeparam>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="source">The source.</param>
		/// <returns>The copied list.</returns>
		/// <remarks></remarks>
		public static TList To<TList, TItem>( this IEnumerable<TItem> source ) where TList : List<TItem>, new( )
		{
			if ( source == null )
			{
				return null;
			}
			if ( source is TList )
			{
				return ( TList ) source;
			}
			return source.Copy<TList, TItem>( );
		}

		/// <summary>
		///     Copies a collection into a hash set.
		/// </summary>
		/// <typeparam name="TItem">The type of the item.</typeparam>
		/// <param name="source">The source.</param>
		/// <returns>The hash set.</returns>
		/// <remarks>This function just exists to make code look a bit more tidy.</remarks>
		public static ISet<TItem> ToSet<TItem>( this IEnumerable<TItem> source )
		{
			if ( source == null )
			{
				return null;
			}
			return new HashSet<TItem>( source );
		}

        /// <summary>
        ///     Converts an enumeration to a dictionary, but ignores duplicates.
        /// </summary>
        public static Dictionary<TKey, TSource> ToDictionarySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            var dict = new Dictionary<TKey, TSource>();
            foreach (TSource value in source)            
            {
                TKey key = keySelector(value);
                dict[key] = value;
            }
            return dict;
        }

        /// <summary>
        ///     Converts an enumeration to a dictionary, but ignores duplicates.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionarySafe<TSource, TKey, TValue>( this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );
            if ( keySelector == null )
                throw new ArgumentNullException( "keySelector" );
            if ( valueSelector == null )
                throw new ArgumentNullException( "valueSelector" );

            var dict = new Dictionary<TKey, TValue>( );
            foreach ( TSource sourceItem in source )
            {
                TKey key = keySelector( sourceItem );
                dict [ key ] = valueSelector( sourceItem );
            }
            return dict;
        }

		/// <summary>
		///     Modifies an action delegate so that any exceptions thrown are silently caught.
		/// </summary>
		/// <typeparam name="T">Type of data that the action delegate accepts.</typeparam>
		/// <param name="innerAction">The action delegate to be wrapped.</param>
		/// <returns>An exception-wrapped action delegate.</returns>
		public static Action<T> TryCatch<T>( this Action<T> innerAction )
		{
			return delegate( T value )
				{
					// ReSharper disable EmptyGeneralCatchClause
					try
					{
						innerAction( value );
					}
					catch
					{
						// TODO FIX ME - SHOULD NOT SWALLOW ERRORS
					}
					// ReSharper restore EmptyGeneralCatchClause
				};
		}


        /// <summary>
        ///     Walks a graph of nodes, visiting each one once.
        /// </summary>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <param name="startNode">The node to start with. This gets returned.</param>
        /// <param name="getRelated">A delegate for finding other nodes related to a node. Delegate may return null to indicate no results.</param>
        /// <param name="followRelationship">Optional delegate to control whether a specific relationship should be followed. Accept parent then child, return true to follow.</param>
        /// <param name="nodeEqualityComparer"></param>
        /// <returns>IEnumerable that returns all nodes once.</returns>
        public static IEnumerable<T> WalkGraph<T>( T startNode, Func<T, IEnumerable<T>> getRelated, Func<T, T, bool> followRelationship = null, IEqualityComparer<T> nodeEqualityComparer = null )
		{
			return WalkGraph( Enumerable.Repeat( startNode, 1 ), getRelated, followRelationship, nodeEqualityComparer );
		}


		/// <summary>
		///     Walks a graph of nodes, visiting each one once.
		/// </summary>
		/// <typeparam name="T">Type of the node</typeparam>
		/// <param name="startNodes">A collection of nodes to start with. This gets returned.</param>
		/// <param name="getRelated">A delegate for finding other nodes related to a node. Delegate may return null to indicate no results.</param>
		/// <param name="followRelationship">Optional delegate to control whether a specific relationship should be followed. Accept parent then child, return true to follow.</param>
		/// <param name="nodeEqualityComparer"></param>
		/// <returns>IEnumerable that returns all nodes once.</returns>
		public static IEnumerable<T> WalkGraph<T>( IEnumerable<T> startNodes, Func<T, IEnumerable<T>> getRelated, Func<T, T, bool> followRelationship = null, IEqualityComparer<T> nodeEqualityComparer = null )
		{
			if ( startNodes == null )
			{
				throw new ArgumentNullException( "startNodes" );
			}
			if ( getRelated == null )
			{
				throw new ArgumentNullException( "getRelated" );
			}

			var nodesProcessed = new HashSet<T>( nodeEqualityComparer ?? EqualityComparer<T>.Default );
			var nodesToProcess = new Queue<T>( startNodes );

			while ( nodesToProcess.Count != 0 )
			{
				// Fetch a node
				T node = nodesToProcess.Dequeue( );

				// Have we already visited it?
				if ( nodesProcessed.Contains( node ) )
				{
					continue;
				}
				nodesProcessed.Add( node );

				// Return the node
				yield return node;

				// Find its friends
				IEnumerable<T> relatedNodes = getRelated( node );
				if ( relatedNodes != null )
				{
					foreach ( T relatedNode in relatedNodes )
					{
						if ( followRelationship == null || followRelationship( node, relatedNode ) )
						{
							nodesToProcess.Enqueue( relatedNode );
						}
					}
				}
			}
        }


        /// <summary>
        ///     Walks a graph of nodes, visiting each one once.
        ///     Wraps all delegates and data to include a layer that tracks how we arrived at each node.
        /// </summary>
        /// <remarks>
        ///     If there are multiple ways to reach each node, then only the first is recorded.
        /// </remarks>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <param name="startNodes">A collection of nodes to start with. This gets returned.</param>
        /// <param name="getRelated">A delegate for finding other nodes related to a node. Delegate may return null to indicate no results.</param>
        /// <param name="nodeEqualityComparer"></param>
        /// <returns>IEnumerable that returns all nodes once.</returns>
        public static IEnumerable<WalkStep<T>> WalkGraphWithSteps<T>( IEnumerable<T> startNodes, Func<T, IEnumerable<T>> getRelated, IEqualityComparer<T> nodeEqualityComparer = null )
        {
            IEnumerable<WalkStep<T>> stepStartNodes;
            Func<WalkStep<T>, IEnumerable<WalkStep<T>>> stepGetRelated;
            IEqualityComparer<WalkStep<T>> stepComparer;

            // Wrap each start node with a step that has no previous step
            stepStartNodes = startNodes.Select( node => new WalkStep<T>( ) { Node = node } );

            // Wrap relationship callback to include previous step data
            stepGetRelated = walkStep =>
                getRelated( walkStep.Node ).Select( data => new WalkStep<T>
                {
                    PreviousStep = walkStep,
                    Node = data
                } );

            // Wrap comparer to unpackage the raw data
            stepComparer = new CastingComparer<WalkStep<T>, T>( walkStep => walkStep.Node, nodeEqualityComparer );

            return WalkGraph( stepStartNodes, stepGetRelated, null, stepComparer );
        }

        /// <summary>
        ///     Private class to support the CreateComparer method.
        /// </summary>
        /// <typeparam name="TSelector"></typeparam>
        /// <typeparam name="TData"></typeparam>
        private class GenericComparer<TSelector, TData> : IComparer<TSelector>
		{
			private readonly Comparer<TData> _realComparer = Comparer<TData>.Default;

			public Func<TSelector, TData> Selector
			{
				private get;
				set;
			}

			public int Compare( TSelector x, TSelector y )
			{
				// ReSharper disable CompareNonConstrainedGenericWithNull
				TData ux = x == null ? default( TData ) : Selector( x );
				TData uy = y == null ? default( TData ) : Selector( y );
				// ReSharper restore CompareNonConstrainedGenericWithNull
				return _realComparer.Compare( ux, uy );
			}
		}


		/// <summary>
		/// Takes a collection, breaks it into batches, runs a process, and accumulates the results.
		/// </summary>
		/// <typeparam name="T">Type of the node</typeparam>
		/// <param name="source">The source.</param>
		/// <returns>
		/// IEnumerable that returns all nodes once.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">source</exception>
        public static IEnumerable<T> WhereType<T>(this IEnumerable<object> source) where T : class
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );
            return source.Select( o => o as T ).Where( o => o != null );
        }


        /// <summary>
        ///     Takes a collection, and returns non-null entries.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>( this IEnumerable<T> source ) where T : class
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );
            return source.Where( o => o != null );
        }


        /// <summary>
        ///     Takes a collection, breaks it into batches, runs a process, and accumulates the results.
        /// </summary>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <typeparam name="TRes">Type of the node</typeparam>
        /// <returns>IEnumerable that returns all nodes once.</returns>
        public static List<TRes> Batch<T, TRes>(IEnumerable<T> inputs, int batchSize, Func<IEnumerable<T>, IEnumerable<TRes>> function)
        {
            var inputBatch = new List<T>(batchSize);
            var results = new List<TRes>();

            Action<List<T>> runBatch = list =>
                {
                    var res = function(list);
                    results.AddRange(res);
                };

            foreach (T item in inputs)
            {
                inputBatch.Add(item);
                if (inputBatch.Count == batchSize)
                {
                    runBatch(inputBatch);
                    inputBatch.Clear();
                }
            }
            if (inputBatch.Count > 0)
            {
                runBatch(inputBatch);
            }
            return results;
        }


        /// <summary>
        ///     Takes a collection, breaks it into batches, runs a process, and accumulates the results.
        /// </summary>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <returns>IEnumerable that returns all nodes once.</returns>
        public static void Batch<T>(IEnumerable<T> inputs, int batchSize, Action<IEnumerable<T>> action)
        {
            Func<IEnumerable<T>, IEnumerable<object>> func = item =>
                {
                    action(item);
                    return Enumerable.Empty<object>();
                };

            Batch(inputs, batchSize, func);
        }


        /// <summary>
        ///     An implementation of string.Join that inserts into an existing string builder, to avoid the additional copy.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="values">The values to join.</param>
        public static void Join(this StringBuilder sb, string separator, IEnumerable<object> values)
        {
            bool first = true;
            foreach (object o in values)
            {
                if (first)
                    first = false;
                else
                    sb.Append(separator);
                if (o != null)
                    sb.Append(o.ToString());
            }
        }

        /// <summary>
        /// Converts a NameValueCollection to a Dictionary.
        /// </summary>
        public static Dictionary<string, string> ToDictionary( this NameValueCollection collection )
        {
            if ( collection == null )
                throw new ArgumentNullException( "collection" );
            return collection.AllKeys.ToDictionary( k => k, k => collection [ k ] );
        }
    }
}

// ReSharper restore CheckNamespace