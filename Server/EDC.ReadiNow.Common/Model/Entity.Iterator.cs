// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.Internal;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Base entity class.
	/// </summary>
	public partial class Entity
	{
		#region Iterators

		/// <summary>
		///     Iterator used for walking over entity instances retrieved from the 'Get' method.
		/// </summary>
		private class EntityIterator : Iterator<IEntity>
		{
			#region Fields

			/// <summary>
			///     Activation function
			/// </summary>
			private readonly Func<ActivationData, IEntity> _activationFunction;

			/// <summary>
			///     Bulk field load action.
			/// </summary>
			private readonly Action<IEntity> _bulkFieldLoadAction;

			/// <summary>
			///     Entity identifiers requested.
			/// </summary>
			private readonly IEnumerable<EntityRef> _identifiers;

			/// <summary>
			///     Retrieval function.
			/// </summary>
			private readonly Func<List<ActivationData>> _retrievalFunction;

			/// <summary>
			///     Security Options.
			/// </summary>
			private readonly SecurityOption _securityOption = SecurityOption.SkipDenied;

			/// <summary>
			///     Whether the entities are to be writable or not.
			/// </summary>
			private readonly bool _writable;

			/// <summary>
			/// Whether the source entity is writable.
			/// </summary>
			private readonly bool? _sourceEntityIsWritable;

			/// <summary>
			///     Entity identifiers enumerator.
			/// </summary>
			private IEnumerator<EntityRef> _identifiersEnumerator;

			/// <summary>
			///     Dictionary of activation data instances.
			/// </summary>
			private IDictionary<long, ActivationData> _retrievalResults;

			#endregion Fields

			#region Methods

			/// <summary>
			/// Initializes a new instance of the <see cref="EntityIterator" /> class.
			/// </summary>
			/// <param name="identifiers">The identifiers.</param>
			/// <param name="writable">if set to <c>true</c> [writable].</param>
			/// <param name="securityOption">The security options.</param>
			/// <param name="retrievalFunction">The function to be performed when iteration starts.</param>
			/// <param name="activationFunction">The activation function.</param>
			/// <param name="bulkFieldLoadAction">The bulk field load action.</param>
			/// <param name="sourceEntityIsWritable">The source entity is writable.</param>
			/// <exception cref="System.ArgumentNullException">identifiers</exception>
			public EntityIterator( IEnumerable<EntityRef> identifiers, bool writable, SecurityOption securityOption, Func<List<ActivationData>> retrievalFunction, Func<ActivationData, IEntity> activationFunction, Action<IEntity> bulkFieldLoadAction, bool? sourceEntityIsWritable = null )
			{
				if ( identifiers == null )
				{
					throw new ArgumentNullException( "identifiers" );
				}

				if ( retrievalFunction == null )
				{
					throw new ArgumentNullException( "retrievalFunction" );
				}

				if ( activationFunction == null )
				{
					throw new ArgumentNullException( "activationFunction" );
				}

				if ( bulkFieldLoadAction == null )
				{
					throw new ArgumentNullException( "bulkFieldLoadAction" );
				}

				/////
				// Set constructor references.
				/////
				_identifiers = identifiers;
				_writable = writable;
				_retrievalFunction = retrievalFunction;
				_activationFunction = activationFunction;
				_bulkFieldLoadAction = bulkFieldLoadAction;
				_securityOption = securityOption;
				_sourceEntityIsWritable = sourceEntityIsWritable;
			}

			/// <summary>
			///     Clones this instance.
			/// </summary>
			/// <returns>
			///     A clone of the iterator.
			/// </returns>
			public override Iterator<IEntity> Clone( )
			{
				return new EntityIterator( _identifiers, _writable, _securityOption, _retrievalFunction, _activationFunction, _bulkFieldLoadAction );
			}

			/// <summary>
			///     Moves the next value in the enumeration.
			/// </summary>
			/// <returns>
			///     True if the move succeeds; false otherwise.
			/// </returns>
			public override bool MoveNext( )
			{
				switch ( State )
				{
					case IteratorState.Initialized:

						/////
						// Start enumerating over the identifiers.
						/////
                        // _identifiersEnumerator is set in TraverseIdentifiers()
						State = IteratorState.Active;
						break;

					case IteratorState.Active:
						/////
						// Continue iterating.
						/////
						break;

					default:
						return false;
				}

				return TraverseIdentifiers( );
			}

            /// <summary>
            /// Are the entities returned writeable?
            /// </summary>
            private bool IsWriteable
		    {
                get { return _writable || (_sourceEntityIsWritable != null && _sourceEntityIsWritable.Value); }
		    }

			/// <summary>
			///     Retrieves the entity.
			/// </summary>
			/// <param name="entityRef">The entity ref.</param>
			/// <returns>
			///     The current entity if found; null otherwise.
			/// </returns>
			private IEntity RetrieveEntity( EntityRef entityRef )
			{
				if ( entityRef.HasId )
				{
					Trace.TraceEntityGetById( entityRef.Id );
				}
				else if ( entityRef.HasEntity )
				{
					Trace.TraceEntityGetById( entityRef.Entity.Id );
				}
				else
				{
					Trace.TraceEntityGetByAlias( string.IsNullOrEmpty( entityRef.Namespace ) ? entityRef.Alias : string.Format( "{0}:{1}", entityRef.Namespace, entityRef.Alias ) );
				}

				EntitySnapshotContextData snapshotContext = null;

				if ( !_writable )
				{
					snapshotContext = EntitySnapshotContext.GetContextData( );

					if ( snapshotContext != null )
					{
						IEntity cachedEntity;
						if ( snapshotContext.TryGetEntity( entityRef.Id, out cachedEntity ) )
						{
							return cachedEntity;
						}
					}
				}

				IEntity entity;

				var localEntityCache = GetLocalCache( );

				if ( entityRef.HasEntity )
				{
					/////
					// Use the stored entity.
					/////
					entity = entityRef.Entity;
				}
                else if (IsWriteable && localEntityCache.TryGetValue(entityRef.Id, out entity))
				{
					/////
					// Local Call Context contains the entity.
					/////
				}
				else if ( !EntityCache.Instance.TryGetValue( entityRef.Id, out entity ) )
				{
					/////
					// Cache hit failed, retrieve from database.
					/////
					if ( _retrievalResults == null )
					{
						_retrievalResults = _retrievalFunction( ).ToDictionary( activationDataInstance => activationDataInstance.Id );
					}

					ActivationData activationData;

					if ( !_retrievalResults.TryGetValue( entityRef.Id, out activationData ) )
					{
						if ( !localEntityCache.TryGetValue( entityRef.Id, out entity ) )
						{
							EntityCache.Instance.TryGetValue( entityRef.Id, out entity );
						}
						/////
						// Try the cache one last time
						/////
						if ( entity == null )
						{
							/////
							// Entity not found in the database, skipping.                        
							/////

							if ( snapshotContext != null )
							{
								snapshotContext.SetEntity( entityRef.Id, null );
							}

							return null;
						}
					}
					else
					{
						entity = _activationFunction( activationData );
					}
				}

				if ( snapshotContext != null )
				{
					snapshotContext.SetEntity( entity.Id, entity );
				}

				return entity;
			}

			/// <summary>
			///     Traverses the identifiers.
			/// </summary>
			/// <returns>
			///     True if the next identifier was found; False otherwise.
			/// </returns>
			private bool TraverseIdentifiers( )
			{
                /////
                // Single bulk security check
                /////
                if (_identifiersEnumerator == null)
                {
	                if ( ! SecurityBypassContext.IsActive )
	                {
		                IList<EntityRef> permissions = new List<EntityRef>( );

		                permissions.Add( Permissions.Read );

						if ( IsWriteable )
						{
							permissions.Add( Permissions.Modify );
		                }

		                IList<EntityRef> identifiers = _identifiers as IList<EntityRef> ?? _identifiers.ToList( );

		                if ( _securityOption == SecurityOption.SkipDenied )
		                {
							IDictionary<long, bool> accessControlCheckResult = EntityAccessControlService.Check( identifiers, permissions );
			                _identifiersEnumerator = _identifiers.Where( er => accessControlCheckResult[ er.Id ] ).GetEnumerator( );
		                }
		                else if ( _securityOption == SecurityOption.DemandAll )
		                {
							EntityAccessControlService.Demand( identifiers, permissions );
			                _identifiersEnumerator = _identifiers.GetEnumerator( );
		                }
		                else
		                {
			                throw new ArgumentException( string.Format( "Unknown SecurityOption: {0}", _securityOption ) );
		                }
	                }
	                else
	                {
						_identifiersEnumerator = _identifiers.GetEnumerator( );
	                }
                }

			    while ( _identifiersEnumerator.MoveNext( ) )
				{
					/////
					// Get the current identifier.
					/////
					EntityRef entityRef = _identifiersEnumerator.Current;

					IEntity entity = RetrieveEntity( entityRef );

					if ( entity != null && entity.TenantId == RequestContext.TenantId )
					{
						/////
						// Ensure the fields are loaded for this entity.
						/////
						_bulkFieldLoadAction( entity );

						if ( ( _writable && !entity.IsReadOnly ) || ( !_writable && entity.IsReadOnly ) )
						{
							/////
							// Use this instance.
							/////
							CurrentValue = entity;
							return true;
						}

						/////
						// Get a writable instance.
						/////
						CurrentValue = entity.AsWritable( );
						return true;
					}
				}

				Dispose( );
				return false;
			}

			#endregion Methods
		}

		#endregion Iterators
	}
}