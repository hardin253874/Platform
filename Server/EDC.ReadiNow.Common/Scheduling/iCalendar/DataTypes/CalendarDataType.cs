// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     An abstract class from which all iCalendar data types inherit.
	/// </summary>
	[Serializable]
	public abstract class CalendarDataType : ICalendarDataType
	{
		/// <summary>
		///     Parameters.
		/// </summary>
		private ICalendarParameterCollection _parameters;

		/// <summary>
		///     Proxy.
		/// </summary>
		private ICalendarParameterCollectionProxy _proxy;

		/// <summary>
		///     Service Provider.
		/// </summary>
		private ServiceProvider _serviceProvider;

		/// <summary>
		///     Associated Object.
		/// </summary>
		protected ICalendarObject AssociatedObjectMember;

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarDataType" /> class.
		/// </summary>
		protected CalendarDataType( )
		{
			Initialize( );
		}

		private void Initialize( )
		{
			_parameters = new CalendarParameterList( );
			_proxy = new CalendarParameterCollectionProxy( _parameters );
			_serviceProvider = new ServiceProvider( );
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
		///     Called when deserializing.
		/// </summary>
		/// <param name="context">The context.</param>
		protected virtual void OnDeserializing( StreamingContext context )
		{
			Initialize( );
		}

		/// <summary>
		///     Called when deserialized.
		/// </summary>
		/// <param name="context">The context.</param>
		protected virtual void OnDeserialized( StreamingContext context )
		{
		}

		#region ICalendarDataType Members

		/// <summary>
		///     Gets the type of the value.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public virtual Type GetValueType( )
		{
			// See RFC 5545 Section 3.2.20.
			if ( _proxy != null && _proxy.ContainsKey( "VALUE" ) )
			{
				switch ( _proxy.Get( "VALUE" ) )
				{
					case "BINARY":
						return typeof ( byte[] );
					case "BOOLEAN":
						return typeof ( bool );
					case "CAL-ADDRESS":
						return typeof ( Uri );
					case "DATE":
						return typeof ( IDateTime );
					case "DATE-TIME":
						return typeof ( IDateTime );
					case "DURATION":
						return typeof ( TimeSpan );
					case "FLOAT":
						return typeof ( double );
					case "INTEGER":
						return typeof ( int );
					case "PERIOD":
						return typeof ( IPeriod );
					case "RECUR":
						return typeof ( IRecurrencePattern );
					case "TEXT":
						return typeof ( string );
					case "TIME":
						// FIXME: implement ISO.8601.2004
						throw new NotImplementedException( );
					case "URI":
						return typeof ( Uri );
					case "UTC-OFFSET":
						return typeof ( IUtcOffset );
					default:
						return null;
				}
			}
			return null;
		}

		/// <summary>
		///     Sets the type of the value.
		/// </summary>
		/// <param name="type">The type.</param>
		public virtual void SetValueType( string type )
		{
			if ( _proxy != null )
			{
				if ( type != null )
				{
					_proxy.Set( "VALUE", type );
				}
			}
		}

		/// <summary>
		///     Gets or sets the associated object.
		/// </summary>
		/// <value>
		///     The associated object.
		/// </value>
		public virtual ICalendarObject AssociatedObject
		{
			get
			{
				return AssociatedObjectMember;
			}
			set
			{
				if ( !Equals( AssociatedObjectMember, value ) )
				{
					AssociatedObjectMember = value;
					if ( AssociatedObjectMember != null )
					{
						_proxy.SetParent( AssociatedObjectMember );

						var calendarParameterCollectionContainer = AssociatedObjectMember as ICalendarParameterCollectionContainer;

						if ( calendarParameterCollectionContainer != null )
						{
							_proxy.SetProxiedObject( calendarParameterCollectionContainer.Parameters );
						}
					}
					else
					{
						_proxy.SetParent( null );
						_proxy.SetProxiedObject( _parameters );
					}
				}
			}
		}

		/// <summary>
		///     Gets the calendar.
		/// </summary>
		/// <value>
		///     The calendar.
		/// </value>
		public virtual IICalendar Calendar
		{
			get
			{
				if ( AssociatedObjectMember != null )
				{
					return AssociatedObjectMember.Calendar;
				}
				return null;
			}
		}

		/// <summary>
		///     Gets or sets the language.
		/// </summary>
		/// <value>
		///     The language.
		/// </value>
		public virtual string Language
		{
			get
			{
				return Parameters.Get( "LANGUAGE" );
			}
			set
			{
				Parameters.Set( "LANGUAGE", value );
			}
		}

		#endregion

		#region ICopyable Members

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public virtual void CopyFrom( ICopyable obj )
		{
			var calendarDataType = obj as ICalendarDataType;

			if ( calendarDataType != null )
			{
				ICalendarDataType dt = calendarDataType;
				AssociatedObjectMember = dt.AssociatedObject;
				_proxy.SetParent( AssociatedObjectMember );
				_proxy.SetProxiedObject( dt.Parameters );
			}
		}

		/// <summary>
		///     Creates a copy of the object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>
		///     The copy of the object.
		/// </returns>
		public virtual T Copy<T>( )
		{
			Type type = GetType( );
			var obj = Activator.CreateInstance( type ) as ICopyable;

			// Duplicate our values
			if ( obj is T )
			{
				obj.CopyFrom( this );
				return ( T ) obj;
			}
			return default( T );
		}

		#endregion

		#region ICalendarParameterCollectionContainer Members

		/// <summary>
		///     Gets the parameters.
		/// </summary>
		/// <value>
		///     The parameters.
		/// </value>
		public virtual ICalendarParameterCollection Parameters
		{
			get
			{
				return _proxy;
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
		public object GetService( string name )
		{
			return _serviceProvider.GetService( name );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetService<T>( )
		{
			return _serviceProvider.GetService<T>( );
		}

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public T GetService<T>( string name )
		{
			return _serviceProvider.GetService<T>( name );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="obj">The obj.</param>
		public void SetService( string name, object obj )
		{
			_serviceProvider.SetService( name, obj );
		}

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public void SetService( object obj )
		{
			_serviceProvider.SetService( obj );
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="type">The type.</param>
		public void RemoveService( Type type )
		{
			_serviceProvider.RemoveService( type );
		}

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemoveService( string name )
		{
			_serviceProvider.RemoveService( name );
		}

		#endregion
	}
}