// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     The base class for all iCalendar objects and components.
	/// </summary>
	[Serializable]
	public class CalendarObject : CalendarObjectBase, ICalendarObject
	{
		/// <summary>
		///     Parent.
		/// </summary>
		private ICalendarObject _parent;

		/// <summary>
		///     Children.
		/// </summary>
		private ICalendarObjectList<ICalendarObject> _children;

		/// <summary>
		///     Service Provider.
		/// </summary>
		private ServiceProvider _serviceProvider;

		/// <summary>
		///     Name.
		/// </summary>
		private string _name;

		/// <summary>
		///     Line.
		/// </summary>
		private int _line;

		/// <summary>
		///     Column.
		/// </summary>
		private int _column;

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarObject" /> class.
		/// </summary>
		internal CalendarObject( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarObject" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public CalendarObject( string name )
			: this( )
		{
			Name = name;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarObject" /> class.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		public CalendarObject( int line, int col )
			: this( )
		{
			Line = line;
			Column = col;
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_children = new CalendarObjectList( this );
			_serviceProvider = new ServiceProvider( );

			_children.ItemAdded += _Children_ItemAdded;
			_children.ItemRemoved += _Children_ItemRemoved;
		}

		/// <summary>
		///     Called when deserializing.
		/// </summary>
		/// <param name="context">The context.</param>
		[OnDeserializing]
		internal void DeserializingInternal( StreamingContext context )
		{
			OnDeserializing( context );
		}

		/// <summary>
		///     Called when deserialized.
		/// </summary>
		/// <param name="context">The context.</param>
		[OnDeserialized]
		internal void DeserializedInternal( StreamingContext context )
		{
			OnDeserialized( context );
		}

		/// <summary>
		///     Called when [deserializing].
		/// </summary>
		/// <param name="context">The context.</param>
		protected virtual void OnDeserializing( StreamingContext context )
		{
			Initialize( );
		}

		/// <summary>
		///     Called when [deserialized].
		/// </summary>
		/// <param name="context">The context.</param>
		protected virtual void OnDeserialized( StreamingContext context )
		{
		}

		/// <summary>
		///     _s the children_ item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _Children_ItemRemoved( object sender, ObjectEventArgs<ICalendarObject, int> e )
		{
			e.First.Parent = null;
		}

		/// <summary>
		///     _s the children_ item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _Children_ItemAdded( object sender, ObjectEventArgs<ICalendarObject, int> e )
		{
			e.First.Parent = this;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var o = obj as ICalendarObject;
			if ( o != null )
			{
				return Equals( o.Name, Name );
			}
// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			if ( Name != null )
			{
				return Name.GetHashCode( );
			}

// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode( );
// ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="c">The c.</param>
		public override void CopyFrom( ICopyable c )
		{
			var obj = c as ICalendarObject;
			if ( obj != null )
			{
				// Copy the name and basic information
				Name = obj.Name;
				Parent = obj.Parent;
				Line = obj.Line;
				Column = obj.Column;

				// Add each child
				Children.Clear( );
				foreach ( ICalendarObject child in obj.Children )
				{
					this.AddChild( child.Copy<ICalendarObject>( ) );
				}
			}
		}

		#region ICalendarObject Members

		/// <summary>
		///     Returns the parent <see cref="ICalendarObject" /> that owns this one.
		/// </summary>
		public virtual ICalendarObject Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				_parent = value;
			}
		}

		/// <summary>
		///     A collection of <see cref="ICalendarObject" />s that are children
		///     of the current object.
		/// </summary>
		public virtual ICalendarObjectList<ICalendarObject> Children
		{
			get
			{
				return _children;
			}
		}

		/// <summary>
		///     Gets or sets the name of the <see cref="ICalendarObject" />.  For iCalendar components,
		///     this is the RFC 5545 name of the component.
		///     <example>
		///         <list type="bullet">
		///             <item>Event - "VEVENT"</item>
		///             <item>Todo - "VTODO"</item>
		///             <item>TimeZone - "VTIMEZONE"</item>
		///             <item>etc.</item>
		///         </list>
		///     </example>
		/// </summary>
		public virtual string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if ( !Equals( _name, value ) )
				{
					string old = _name;
					_name = value;
					OnGroupChanged( old, _name );
				}
			}
		}

		/// <summary>
		///     Returns the <see cref="EDC.ReadiNow.Scheduling.iCalendar.iCalendar" /> that this <see cref="ICalendarObject" />
		///     belongs to.
		/// </summary>
		public virtual IICalendar Calendar
		{
			get
			{
				ICalendarObject obj = this;
				while ( !( obj is IICalendar ) && obj.Parent != null )
				{
					obj = obj.Parent;
				}

				var calendar = obj as IICalendar;

				if ( calendar != null )
				{
					return calendar;
				}
				return null;
			}
			protected set
			{
				_parent = value;
			}
		}

		/// <summary>
		///     Gets or sets the i calendar.
		/// </summary>
		/// <value>
		///     The i calendar.
		/// </value>
		public virtual IICalendar iCalendar
		{
			get
			{
				return Calendar;
			}
			protected set
			{
				Calendar = value;
			}
		}

		/// <summary>
		///     Returns the line number where this calendar
		///     object was found during parsing.
		/// </summary>
		public virtual int Line
		{
			get
			{
				return _line;
			}
			set
			{
				_line = value;
			}
		}

		/// <summary>
		///     Returns the column number where this calendar
		///     object was found during parsing.
		/// </summary>
		public virtual int Column
		{
			get
			{
				return _column;
			}
			set
			{
				_column = value;
			}
		}

		#endregion

		#region IServiceProvider Members

		/// <summary>
		///     Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		///     A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type
		///     <paramref
		///         name="serviceType" />
		///     .
		/// </returns>
		public virtual object GetService( Type serviceType )
		{
			return _serviceProvider.GetService( serviceType );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual object GetService( string name )
		{
			return _serviceProvider.GetService( name );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public virtual T GetService<T>( )
		{
			return _serviceProvider.GetService<T>( );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public virtual T GetService<T>( string name )
		{
			return _serviceProvider.GetService<T>( name );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="obj">The obj.</param>
		public virtual void SetService( string name, object obj )
		{
			_serviceProvider.SetService( name, obj );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void SetService( object obj )
		{
			_serviceProvider.SetService( obj );
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="type">The type.</param>
		public virtual void RemoveService( Type type )
		{
			_serviceProvider.RemoveService( type );
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="name">The name.</param>
		public virtual void RemoveService( string name )
		{
			_serviceProvider.RemoveService( name );
		}

		#endregion

		#region IGroupedObject Members

		/// <summary>
		///     Occurs when the group has changed.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler<ObjectEventArgs<string, string>> GroupChanged;

		/// <summary>
		///     Called when [group changed].
		/// </summary>
		/// <param name="old">The old.</param>
		/// <param name="new">The new.</param>
		protected void OnGroupChanged( string @old, string @new )
		{
			if ( GroupChanged != null )
			{
				GroupChanged( this, new ObjectEventArgs<string, string>( @old, @new ) );
			}
		}

		/// <summary>
		///     Gets or sets the group.
		/// </summary>
		/// <value>
		///     The group.
		/// </value>
		public virtual string Group
		{
			get
			{
				return Name;
			}
			set
			{
				Name = value;
			}
		}

		#endregion
	}
}