// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Diff class.
	/// </summary>
	internal static class Diff
	{
		/// <summary>
		/// Detects the changes.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TData">The type of the data.</typeparam>
		/// <param name="oldApp">The old app.</param>
		/// <param name="newApp">The new app.</param>
		/// <param name="missingApp">The missing application.</param>
		/// <param name="addedAction">The added action.</param>
		/// <param name="removedAction">The removed action.</param>
		/// <param name="changedAction">The changed action.</param>
		/// <param name="unchangedAction">The unchanged action.</param>
		/// <param name="added">The added.</param>
		/// <param name="removed">The removed.</param>
		/// <param name="changed">The changed.</param>
		/// <param name="unchanged">The unchanged.</param>
		/// <param name="debugCallback">The debug callback.</param>
		public static void DetectChanges<TKey, TData>( IDictionary<TKey, TData> oldApp, IDictionary<TKey, TData> newApp, IDictionary<TKey, TData> missingApp, Func<TData, bool> addedAction, Func<TData, bool> removedAction, Func<TData, TData, bool> changedAction, Func<TData, bool> unchangedAction, out List<TData> added, out List<TData> removed, out List<TData> changed, out List<TData> unchanged, Action<TKey, TData> debugCallback = null )
			where TData : class, IEntry<TKey>
		{
			added = new List<TData>( );
			changed = new List<TData>( );
			unchanged = new List<TData>( );

			/////
			// Find added & changed entries
			/////
			foreach ( var pair in newApp )
			{
				debugCallback?.Invoke( pair.Key, pair.Value );

				TData newValue = pair.Value;
				TData oldValue;
				TData missingValue;

				if ( oldApp.TryGetValue( pair.Key, out oldValue ) && ( missingApp == null || !missingApp.TryGetValue( pair.Value.GetKey( ), out missingValue ) ) )
				{
					/////
					// Exists in both
					/////
					if ( !newValue.IsSameData( oldValue ) )
					{
						if ( changedAction != null )
						{
							if ( changedAction( oldValue, newValue ) )
							{
								changed.Add( newValue );
							}
						}
						else
						{
							changed.Add( newValue );
						}
					}
					else
					{
						if ( unchangedAction != null )
						{
							if ( unchangedAction( newValue ) )
							{
								unchanged.Add( newValue );
							}
						}
						else
						{
							unchanged.Add( newValue );
						}
					}
				}
				else
				{
					if ( addedAction != null )
					{
						if ( addedAction( newValue ) )
						{
							added.Add( newValue );
						}
					}
					else
					{
						added.Add( newValue );
					}
				}
			}

			/////
			// Find removed entries
			/////
			List<TData> removedEntries = oldApp
				.Where( entry => !newApp.ContainsKey( entry.Key ) )
				.Select( entry => entry.Value )
				.ToList( );

			if ( removedAction == null )
			{
				removed = removedEntries;
			}
			else
			{
				removed = new List<TData>( );

				foreach ( TData removedValue in removedEntries )
				{
					if ( removedAction( removedValue ) )
					{
						removed.Add( removedValue );
					}
				}
			}
		}
	}
}