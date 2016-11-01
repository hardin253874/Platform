// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IEntityFieldCreate interface.
	/// </summary>
	public interface IEntityFieldCreate
	{
		/// <summary>
		///     Called when the entity field is created on the specified entity.
		/// </summary>
		void OnCreate( IEntity entity );
	}
}