// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarPropertyList class.
	/// </summary>
	[Serializable]
	public class CalendarPropertyList : GroupedValueList<string, ICalendarProperty, CalendarProperty, object>, ICalendarPropertyList
	{
		/// <summary>
		///     Parent.
		/// </summary>
		private readonly ICalendarObject _parent;

		/// <summary>
		///     Case insensitive.
		/// </summary>
		private readonly bool _caseInsensitive;

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarPropertyList" /> class.
		/// </summary>
		public CalendarPropertyList( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarPropertyList" /> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="caseInsensitive">
		///     if set to <c>true</c> [case insensitive].
		/// </param>
		public CalendarPropertyList( ICalendarObject parent, bool caseInsensitive )
		{
			_parent = parent;
			_caseInsensitive = caseInsensitive;

			ItemAdded += CalendarPropertyList_ItemAdded;
			ItemRemoved += CalendarPropertyList_ItemRemoved;
		}

		/// <summary>
		///     Calendars the property list_ item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void CalendarPropertyList_ItemRemoved( object sender, ObjectEventArgs<ICalendarProperty, int> e )
		{
			e.First.Parent = null;
		}

		/// <summary>
		///     Calendars the property list_ item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void CalendarPropertyList_ItemAdded( object sender, ObjectEventArgs<ICalendarProperty, int> e )
		{
			e.First.Parent = _parent;
		}

		/// <summary>
		///     Groups the modifier.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		protected override string GroupModifier( string group )
		{
			if ( _caseInsensitive && group != null )
			{
				return group.ToUpper( );
			}
			return group;
		}

		/// <summary>
		///     Gets the <see cref="ICalendarProperty" /> with the specified name.
		/// </summary>
		/// <value>
		///     The <see cref="ICalendarProperty" />.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ICalendarProperty this[ string name ]
		{
			get
			{
				if ( ContainsKey( name ) )
				{
					return AllOf( name )
						.FirstOrDefault( );
				}
				return null;
			}
		}
	}
}