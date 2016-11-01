// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     Grouped Value List class.
	/// </summary>
	/// <typeparam name="TGroup">The type of the group.</typeparam>
	/// <typeparam name="TInterface">The type of the interface.</typeparam>
	/// <typeparam name="TItem">The type of the item.</typeparam>
	/// <typeparam name="TValueType">The type of the value type.</typeparam>
	public class GroupedValueList<TGroup, TInterface, TItem, TValueType> : GroupedList<TGroup, TInterface>, IGroupedValueList<TGroup, TInterface, TValueType>
		where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
		where TItem : new( )
	{
		/// <summary>
		///     Adds to existing.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="values">The values.</param>
		/// <returns></returns>
		protected virtual bool AddToExisting( TGroup group, IEnumerable<TValueType> values )
		{
			IEnumerable<TInterface> items = AllOf( group );
			if ( items != null )
			{
				TInterface container = items.FirstOrDefault( );
				if ( container != null )
				{
					// Add a value to the first matching item in the list
					container.SetValue( values );

					return true;
				}
			}

			return false;
		}

		#region IKeyedValueList<TGroup, TObject, TValueType> Members

		/// <summary>
		///     Sets the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="value">The value.</param>
		public virtual void Set( TGroup group, TValueType value )
		{
			var values = new[]
				{
					value
				};

			// If this object is a valid container, and we don't
			// already have a container for this group, then
			// let's reuse the existing container.
			if ( value is TInterface )
			{
				var container = value as TInterface;

				// If we can't consolidate our value with another container,
				// or another container doesn't exist yet, then let's use
				// this as our container (and add it to the list)
				if ( !ContainsKey( group ) )
				{
					// Set the group on our container
					container.Group = group;

					// Add the container to the list
					Add( container );
					return;
				}

				if ( AddToExisting( @group, values ) )
				{
					// If we already have a container for this group, then 
					// pass the value along to be added to the existing container.
					return;
				}
			}

			// Otherwise, if not a valid container, pass along so a 
			// container can be generated.
			Set( group, values );
		}

		/// <summary>
		///     Sets the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="values">The values.</param>
		public virtual void Set( TGroup group, IEnumerable<TValueType> values )
		{
			IList<TValueType> valueTypes = values as IList<TValueType> ?? values.ToList( );

			// If a group already exists, and we can consolidate with it,
			// then let's do so.
			if ( ContainsKey( group ) )
			{
				if ( AddToExisting( group, valueTypes ) )
				{
					return;
				}
			}

			// No matching container was found, add a new container to the list
			var container = Activator.CreateInstance( typeof ( TItem ) ) as TInterface;

			if ( container != null )
			{
				// Set the group for the container
				container.Group = @group;

				// Add the container to the list
				Add( container );

				// Set the list of values for the container
				container.SetValue( valueTypes );
			}
		}

		/// <summary>
		///     Gets the specified group.
		/// </summary>
		/// <typeparam name="TType">The type of the type.</typeparam>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public virtual TType Get<TType>( TGroup group )
		{
			TInterface firstItem = AllOf( group ).FirstOrDefault( );
			if ( firstItem != null &&
			     firstItem.Values != null )
			{
				return firstItem
					.Values
					.OfType<TType>( )
					.FirstOrDefault( );
			}
			return default( TType );
		}

		/// <summary>
		///     Gets the many.
		/// </summary>
		/// <typeparam name="TType">The type of the type.</typeparam>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public virtual IList<TType> GetMany<TType>( TGroup group )
		{
			return new GroupedValueListProxy<TGroup, TInterface, TItem, TValueType, TType>( this, group );
		}

		#endregion
	}
}