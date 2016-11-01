// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IEntityFieldSave interface.
	/// </summary>
	public interface IEntityFieldSave
	{
		/// <summary>
		///     Called when an entity field is saved.
		/// </summary>
		/// <param name="entity">The entity.</param>
		void OnSave( IEntity entity );
	}
}