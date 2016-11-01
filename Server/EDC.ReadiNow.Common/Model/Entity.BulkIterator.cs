// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Model
{
	public partial class Entity
	{
		#region Iterators

		/// <summary>
		///     Iterator used for walking over entity instances retrieved from the 'Get' method.
		/// </summary>
		private class BulkEntityIterator : Iterator<IEntity>
		{
			#region Fields

			/// <summary>
			///     Entity identifiers requested.
			/// </summary>
			private readonly IEnumerable<EntityRef> _identifiers;

			/// <summary>
			///     Retrieval function.
			/// </summary>
			private readonly Func<List<IEntity>> _retrievalFunction;

			/// <summary>
			///     Security Options.
			/// </summary>
			private readonly SecurityOption _securityOption = SecurityOption.SkipDenied;

			/// <summary>
			///     Whether the entities are to be writable or not.
			/// </summary>
			private readonly bool _writable;

			/// <summary>
			///     Entity enumerator.
			/// </summary>
			private IEnumerator<IEntity> _entityEnumerator;

			#endregion Fields

			#region Methods

			/// <summary>
			///     Initializes a new instance of the <see cref="EntityIterator" /> class.
			/// </summary>
			/// <param name="identifiers">The identifiers.</param>
			/// <param name="writable">
			///     if set to <c>true</c> [writable].
			/// </param>
			/// <param name="securityOption">The security options.</param>
			/// <param name="retrievalFunction">The function to be performed when iteration starts.</param>
			/// <exception cref="System.ArgumentNullException">identifiers</exception>
			public BulkEntityIterator( IEnumerable<EntityRef> identifiers, bool writable, SecurityOption securityOption, Func<List<IEntity>> retrievalFunction )
			{
				if ( identifiers == null )
				{
					throw new ArgumentNullException( "identifiers" );
				}

				if ( retrievalFunction == null )
				{
					throw new ArgumentNullException( "retrievalFunction" );
				}

				/////
				// Set constructor references.
				/////
				_identifiers = identifiers;
				_writable = writable;
				_retrievalFunction = retrievalFunction;
				_securityOption = securityOption;
			}

			/// <summary>
			///     Clones this instance.
			/// </summary>
			/// <returns>
			///     A clone of the iterator.
			/// </returns>
			public override Iterator<IEntity> Clone( )
			{
				return new BulkEntityIterator( _identifiers, _writable, _securityOption, _retrievalFunction );
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
						// TODO: Load entities and fields here and place into a list that TraverseIdentifiers traverses.
						/////
						List<IEntity> entities = _retrievalFunction( );

						/////
						// Start enumerating over the identifiers.
						/////
						_entityEnumerator = entities.GetEnumerator( );
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
			///     Traverses the identifiers.
			/// </summary>
			/// <returns>
			///     True if the next identifier was found; False otherwise.
			/// </returns>
			private bool TraverseIdentifiers( )
			{
				while ( _entityEnumerator.MoveNext( ) )
				{
					/////
					// Get the current identifier.
					/////
					IEntity entity = _entityEnumerator.Current;

					if ( entity != null )
					{
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