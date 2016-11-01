// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     UniqueComponentListProxy class.
	/// </summary>
	/// <typeparam name="TComponentType">The type of the component type.</typeparam>
	public class UniqueComponentListProxy<TComponentType> : CalendarObjectListProxy<TComponentType>, IUniqueComponentList<TComponentType>
		where TComponentType : class, IUniqueComponent
	{
		/// <summary>
		///     Lookup.
		/// </summary>
		private readonly Dictionary<string, TComponentType> _lookup;

		/// <summary>
		///     Initializes a new instance of the <see cref="UniqueComponentListProxy{TComponentType}" /> class.
		/// </summary>
		/// <param name="children">The children.</param>
		public UniqueComponentListProxy( IGroupedCollection<string, ICalendarObject> children )
			: base( children )
		{
			_lookup = new Dictionary<string, TComponentType>( );

			children.ItemAdded += children_ItemAdded;
			children.ItemRemoved += children_ItemRemoved;
		}

		/// <summary>
		///     Gets or sets the <see cref="TComponentType" /> with the specified UID.
		/// </summary>
		/// <value>
		///     The <see cref="TComponentType" />.
		/// </value>
		/// <param name="uid">The UID.</param>
		/// <returns></returns>
		public virtual TComponentType this[ string uid ]
		{
			get
			{
				return Search( uid );
			}
			set
			{
				// Find the item matching the UID
				TComponentType item = Search( uid );

				if ( item != null )
				{
					Remove( item );
				}

				if ( value != null )
				{
					Add( value );
				}
			}
		}

		/// <summary>
		///     Searches the specified UID.
		/// </summary>
		/// <param name="uid">The UID.</param>
		/// <returns></returns>
		private TComponentType Search( string uid )
		{
			if ( _lookup.ContainsKey( uid ) )
			{
				return _lookup[ uid ];
			}

// ReSharper disable RedundantEnumerableCastCall
			TComponentType item = this.OfType<TComponentType>( ).FirstOrDefault( c => string.Equals( c.Uid, uid ) );
// ReSharper restore RedundantEnumerableCastCall

			if ( item != null )
			{
				_lookup[ uid ] = item;
				return item;
			}
			return default( TComponentType );
		}

		/// <summary>
		///     UID has changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void UidChanged( object sender, ObjectEventArgs<string, string> e )
		{
			if ( e.First != null &&
			     _lookup.ContainsKey( e.First ) )
			{
				_lookup.Remove( e.First );
			}

			if ( e.Second != null )
			{
				_lookup[ e.Second ] = ( TComponentType ) sender;
			}
		}

		/// <summary>
		///     Children_s the item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void children_ItemAdded( object sender, ObjectEventArgs<ICalendarObject, int> e )
		{
			if ( e.First is TComponentType )
			{
				var component = ( TComponentType ) e.First;
				component.UidChanged += UidChanged;

				if ( !string.IsNullOrEmpty( component.Uid ) )
				{
					_lookup[ component.Uid ] = component;
				}
			}
		}

		/// <summary>
		///     Children_s the item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void children_ItemRemoved( object sender, ObjectEventArgs<ICalendarObject, int> e )
		{
			if ( e.First is TComponentType )
			{
				var component = ( TComponentType ) e.First;
				component.UidChanged -= UidChanged;

				if ( !string.IsNullOrEmpty( component.Uid ) &&
				     _lookup.ContainsKey( component.Uid ) )
				{
					_lookup.Remove( component.Uid );
				}
			}
		}
	}
}