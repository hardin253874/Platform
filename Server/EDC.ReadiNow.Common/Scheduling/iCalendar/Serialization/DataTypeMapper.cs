// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     Type resolver delegate.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <returns></returns>
	public delegate Type TypeResolverDelegate( object context );

	/// <summary>
	///     DataTypeMapper class.
	/// </summary>
	public class DataTypeMapper : IDataTypeMapper
	{
		/// <summary>
		///     Property map.
		/// </summary>
		private readonly IDictionary<string, PropertyMapping> _propertyMap = new Dictionary<string, PropertyMapping>( );

		/// <summary>
		///     Initializes a new instance of the <see cref="DataTypeMapper" /> class.
		/// </summary>
		public DataTypeMapper( )
		{
			AddPropertyMapping( "ACTION", typeof ( AlarmAction ), false );
			AddPropertyMapping( "ATTACH", typeof ( IAttachment ), false );
			AddPropertyMapping( "ATTENDEE", typeof ( IAttendee ), false );
			AddPropertyMapping( "CATEGORIES", typeof ( string ), true );
			AddPropertyMapping( "COMMENT", typeof ( string ), false );
			AddPropertyMapping( "COMPLETED", typeof ( IDateTime ), false );
			AddPropertyMapping( "CONTACT", typeof ( string ), false );
			AddPropertyMapping( "CREATED", typeof ( IDateTime ), false );
			AddPropertyMapping( "DTEND", typeof ( IDateTime ), false );
			AddPropertyMapping( "DTSTAMP", typeof ( IDateTime ), false );
			AddPropertyMapping( "DTSTART", typeof ( IDateTime ), false );
			AddPropertyMapping( "DUE", typeof ( IDateTime ), false );
			AddPropertyMapping( "DURATION", typeof ( TimeSpan ), false );
			AddPropertyMapping( "EXDATE", typeof ( IPeriodList ), false );
			AddPropertyMapping( "EXRULE", typeof ( IRecurrencePattern ), false );
			AddPropertyMapping( "FREEBUSY", typeof ( IFreeBusyEntry ), true );
			AddPropertyMapping( "GEO", typeof ( IGeographicLocation ), false );
			AddPropertyMapping( "LAST-MODIFIED", typeof ( IDateTime ), false );
			AddPropertyMapping( "ORGANIZER", typeof ( IOrganizer ), false );
			AddPropertyMapping( "PERCENT-COMPLETE", typeof ( int ), false );
			AddPropertyMapping( "PRIORITY", typeof ( int ), false );
			AddPropertyMapping( "RDATE", typeof ( IPeriodList ), false );
			AddPropertyMapping( "RECURRENCE-ID", typeof ( IDateTime ), false );
			AddPropertyMapping( "RELATED-TO", typeof ( string ), false );
			AddPropertyMapping( "REQUEST-STATUS", typeof ( IRequestStatus ), false );
			AddPropertyMapping( "REPEAT", typeof ( int ), false );
			AddPropertyMapping( "RESOURCES", typeof ( string ), true );
			AddPropertyMapping( "RRULE", typeof ( IRecurrencePattern ), false );
			AddPropertyMapping( "SEQUENCE", typeof ( int ), false );
			AddPropertyMapping( "STATUS", ResolveStatusProperty, false );
			AddPropertyMapping( "TRANSP", typeof ( TransparencyType ), false );
			AddPropertyMapping( "TRIGGER", typeof ( ITrigger ), false );
			AddPropertyMapping( "TZNAME", typeof ( string ), false );
			AddPropertyMapping( "TZOFFSETFROM", typeof ( IUtcOffset ), false );
			AddPropertyMapping( "TZOFFSETTO", typeof ( IUtcOffset ), false );
			AddPropertyMapping( "TZURL", typeof ( Uri ), false );
			AddPropertyMapping( "URL", typeof ( Uri ), false );
		}

		/// <summary>
		///     Resolves the status property.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		protected Type ResolveStatusProperty( object context )
		{
			var obj = context as ICalendarObject;
			if ( obj != null )
			{
				if ( obj.Parent is IEvent )
				{
					return typeof ( EventStatus );
				}

				if ( obj.Parent is ITodo )
				{
					return typeof ( TodoStatus );
				}

				if ( obj.Parent is IJournal )
				{
					return typeof ( JournalStatus );
				}
			}

			return null;
		}

		#region IDefaultTypeMapper Members

		/// <summary>
		///     Adds the property mapping.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="allowsMultipleValues">
		///     if set to <c>true</c> [allows multiple values].
		/// </param>
		public void AddPropertyMapping( string name, Type objectType, bool allowsMultipleValues )
		{
			if ( name != null && objectType != null )
			{
				var m = new PropertyMapping
					{
						ObjectType = objectType,
						AllowsMultipleValuesPerProperty = allowsMultipleValues
					};

				_propertyMap[ name.ToUpper( ) ] = m;
			}
		}

		/// <summary>
		///     Adds the property mapping.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="resolver">The resolver.</param>
		/// <param name="allowsMultipleValues">
		///     if set to <c>true</c> [allows multiple values].
		/// </param>
		public void AddPropertyMapping( string name, TypeResolverDelegate resolver, bool allowsMultipleValues )
		{
			if ( name != null && resolver != null )
			{
				var m = new PropertyMapping
					{
						Resolver = resolver,
						AllowsMultipleValuesPerProperty = allowsMultipleValues
					};

				_propertyMap[ name.ToUpper( ) ] = m;
			}
		}

		/// <summary>
		///     Removes the property mapping.
		/// </summary>
		/// <param name="name">The name.</param>
		public void RemovePropertyMapping( string name )
		{
			if ( name != null &&
			     _propertyMap.ContainsKey( name.ToUpper( ) ) )
			{
				_propertyMap.Remove( name.ToUpper( ) );
			}
		}

		/// <summary>
		///     Gets the property allows multiple values.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public virtual bool GetPropertyAllowsMultipleValues( object obj )
		{
			var p = obj as ICalendarProperty;
			if ( p != null && p.Name != null )
			{
				string name = p.Name.ToUpper( );
				if ( _propertyMap.ContainsKey( name ) )
				{
					PropertyMapping m = _propertyMap[ name ];
					return m.AllowsMultipleValuesPerProperty;
				}
			}
			return false;
		}

		/// <summary>
		///     Gets the property mapping.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public virtual Type GetPropertyMapping( object obj )
		{
			var p = obj as ICalendarProperty;
			if ( p != null && p.Name != null )
			{
				string name = p.Name.ToUpper( );
				if ( _propertyMap.ContainsKey( name ) )
				{
					PropertyMapping m = _propertyMap[ name ];
					if ( m.Resolver != null )
					{
						return m.Resolver( p );
					}

					return m.ObjectType;
				}
			}
			return null;
		}

		#endregion

		/// <summary>
		///     PropertyMapping structure.
		/// </summary>
		private struct PropertyMapping
		{
			/// <summary>
			///     Gets or sets a value indicating whether [allows multiple values per property].
			/// </summary>
			/// <value>
			///     <c>true</c> if [allows multiple values per property]; otherwise, <c>false</c>.
			/// </value>
			public bool AllowsMultipleValuesPerProperty
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the type of the object.
			/// </summary>
			/// <value>
			///     The type of the object.
			/// </value>
			public Type ObjectType
			{
				get;
				set;
			}

			/// <summary>
			///     Gets or sets the resolver.
			/// </summary>
			/// <value>
			///     The resolver.
			/// </value>
			public TypeResolverDelegate Resolver
			{
				get;
				set;
			}
		}
	}
}