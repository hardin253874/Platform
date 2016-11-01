// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarParameterCollectionProxy class.
	/// </summary>
	public class CalendarParameterCollectionProxy : GroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>, ICalendarParameterCollectionProxy
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarParameterCollectionProxy" /> class.
		/// </summary>
		/// <param name="realObject">The real object.</param>
		public CalendarParameterCollectionProxy( IGroupedList<string, ICalendarParameter> realObject )
			:
				base( realObject )
		{
		}

		/// <summary>
		///     Gets the parameters.
		/// </summary>
		/// <value>
		///     The parameters.
		/// </value>
		protected IGroupedValueList<string, ICalendarParameter, string> Parameters
		{
			get
			{
				return RealObject as IGroupedValueList<string, ICalendarParameter, string>;
			}
		}

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
			RealObject.Add( new CalendarParameter( name, value ) );
		}

		/// <summary>
		///     Gets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual string Get( string name )
		{
			ICalendarParameter parameter = RealObject
				.AllOf( name )
				.FirstOrDefault( );

			if ( parameter != null )
			{
				return parameter.Value;
			}
			return default( string );
		}

		/// <summary>
		///     Gets the many.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual IList<string> GetMany( string name )
		{
			return new GroupedValueListProxy<string, ICalendarParameter, CalendarParameter, string, string>(
				Parameters,
				name
				);
		}

		/// <summary>
		///     Sets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public virtual void Set( string name, string value )
		{
			ICalendarParameter parameter = RealObject
				.AllOf( name )
				.FirstOrDefault( );

			if ( parameter == null )
			{
				RealObject.Add( new CalendarParameter( name, value ) );
			}
			else
			{
				parameter.SetValue( value );
			}
		}

		/// <summary>
		///     Sets the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="values">The values.</param>
		public virtual void Set( string name, IEnumerable<string> values )
		{
			ICalendarParameter parameter = RealObject
				.AllOf( name )
				.FirstOrDefault( );

			if ( parameter == null )
			{
				RealObject.Add( new CalendarParameter( name, values ) );
			}
			else
			{
				parameter.SetValue( values );
			}
		}

		/// <summary>
		///     Indexes the of.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public virtual int IndexOf( ICalendarParameter obj )
		{
			return Parameters.IndexOf( obj );
		}

		/// <summary>
		///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">
		///     The zero-based index at which <paramref name="item" /> should be inserted.
		/// </param>
		/// <param name="item">
		///     The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </param>
		public virtual void Insert( int index, ICalendarParameter item )
		{
			Parameters.Insert( index, item );
		}

		/// <summary>
		///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public virtual void RemoveAt( int index )
		{
			Parameters.RemoveAt( index );
		}

		/// <summary>
		///     Gets or sets the <see cref="ICalendarParameter" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="ICalendarParameter" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public virtual ICalendarParameter this[ int index ]
		{
			get
			{
				return Parameters[ index ];
			}
			set
			{
			}
		}
	}
}