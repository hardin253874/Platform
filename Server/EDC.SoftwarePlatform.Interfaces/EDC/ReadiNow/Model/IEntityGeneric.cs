// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IEntityGeneric interface.
	/// </summary>
	public interface IEntityGeneric<in TGeneric>
	{
		/// <summary>
		///     Gets the value of the specified field for the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <returns>
		///     The value of the specified field if found, null otherwise.
		/// </returns>
		object GetField( TGeneric field );

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		IEntityRelationshipCollection<IEntity> GetRelationships( TGeneric relationshipDefinition );

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		IEntityRelationshipCollection<IEntity> GetRelationships( TGeneric relationshipDefinition, Direction direction );

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <returns></returns>
		IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( TGeneric relationshipDefinition )
			where TEntity : class, IEntity;

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		IEntityRelationshipCollection<TEntity> GetRelationships<TEntity>( TGeneric relationshipDefinition, Direction direction )
			where TEntity : class, IEntity;

		/// <summary>
		///     Sets the value of the specified field on the current entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="value">The value.</param>
		void SetField( TGeneric field, object value );

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		void SetRelationships( TGeneric relationshipDefinition, IEntityRelationshipCollection relationships );

		/// <summary>
		///     Sets the relationships for the specified relationship definition.
		/// </summary>
		/// <param name="relationshipDefinition">The relationship definition.</param>
		/// <param name="relationships">The relationships.</param>
		/// <param name="direction">The direction.</param>
		void SetRelationships( TGeneric relationshipDefinition, IEntityRelationshipCollection relationships, Direction direction );
	}
}