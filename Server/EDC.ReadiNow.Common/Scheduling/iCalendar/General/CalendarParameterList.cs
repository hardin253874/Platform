// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarParameterList class.
	/// </summary>
	[Serializable]
	public class CalendarParameterList : GroupedValueList<string, ICalendarParameter, CalendarParameter, string>, ICalendarParameterCollection
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
		///     Initializes a new instance of the <see cref="CalendarParameterList" /> class.
		/// </summary>
		public CalendarParameterList( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarParameterList" /> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="caseInsensitive">
		///     if set to <c>true</c> [case insensitive].
		/// </param>
		public CalendarParameterList( ICalendarObject parent, bool caseInsensitive )
		{
			_parent = parent;
			_caseInsensitive = caseInsensitive;


			ItemAdded += OnParameterAdded;
			ItemRemoved += OnParameterRemoved;
		}

		/// <summary>
		///     Called when [parameter removed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		protected void OnParameterRemoved( object sender, ObjectEventArgs<ICalendarParameter, int> e )
		{
			e.First.Parent = null;
		}

		/// <summary>
		///     Called when [parameter added].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		protected void OnParameterAdded( object sender, ObjectEventArgs<ICalendarParameter, int> e )
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

		#region ICalendarParameterCollection Members

		/// <summary>
		///     Sets the parent.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public virtual void SetParent( ICalendarObject parent )
		{
			foreach ( ICalendarParameter parameter in this )
			{
				parameter.Parent = parent;
			}
		}

		/// <summary>
		///     Adds the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public virtual void Add( string name, string value )
		{
			Add( new CalendarParameter( name, value ) );
		}

		/// <summary>
		///     Gets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual string Get( string name )
		{
			return Get<string>( name );
		}

		/// <summary>
		///     Gets the many.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual IList<string> GetMany( string name )
		{
			return GetMany<string>( name );
		}

		#endregion
	}
}