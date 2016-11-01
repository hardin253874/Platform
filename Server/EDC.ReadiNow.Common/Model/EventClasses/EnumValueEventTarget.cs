// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model.EventClasses
{
	/// <summary>
	///     EnumValue event target.
	/// </summary>
	public class EnumValueEventTarget : IEntityEventSave
	{
		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
		}

		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			foreach ( IEntity entity in entities )
			{
				var enumValue = entity.As<EnumValue>( );

				if ( enumValue == null )
				{
					continue;
				}

				EnumType owner = enumValue.EnumOwner;

				if ( owner == null )
				{
					owner = Entity.Get<EnumType>( enumValue.TypeIds.First( ) );

					if ( owner != null )
					{
						enumValue.EnumOwner = owner;
					}
				}
			}
			return false;
		}
	}
}