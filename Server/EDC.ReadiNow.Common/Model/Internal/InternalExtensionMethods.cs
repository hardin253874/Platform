// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model.Internal
{
	/// <summary>
	///     Internal extension methods that are not for public use.
	/// </summary>
	internal static class InternalExtensionMethods
	{
		/// <summary>
		///     Toes the entity relationship collection.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entityCollection">The entity collection.</param>
		/// <returns></returns>
		internal static IEntityRelationshipCollection<TEntity> ToEntityRelationshipCollection<TEntity>( this IEntityCollection<TEntity> entityCollection )
			where TEntity : class, IEntity
		{
			/////
			// Throw a NullRef exception.
			/////
			if ( entityCollection == null )
			{
				throw new NullReferenceException( );
			}

			return new EntityRelationshipCollection<TEntity>( entityCollection );
		}
	}
}