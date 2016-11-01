// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     This class is used by the parsing framework for iCalendar components.
	///     Generally, you should not need to use this class directly.
	/// </summary>
	[Serializable]
	[DebuggerDisplay( "Component: {Name}" )]
	public class CalendarComponent : CalendarObject, ICalendarComponent
	{
		/// <summary>
		///     Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
		/// </summary>
		public static ICalendarComponent LoadFromStream( Stream s )
		{
			return LoadFromStream( s, Encoding.UTF8 );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static ICalendarComponent LoadFromStream( Stream stream, Encoding encoding )
		{
			return LoadFromStream( stream, encoding, new ComponentSerializer( ) );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <returns></returns>
		public static T LoadFromStream<T>( Stream stream, Encoding encoding )
			where T : ICalendarComponent
		{
			var serializer = new ComponentSerializer( );
			object obj = LoadFromStream( stream, encoding, serializer );
			if ( obj is T )
			{
				return ( T ) obj;
			}
			return default( T );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="serializer">The serializer.</param>
		/// <returns></returns>
		public static ICalendarComponent LoadFromStream( Stream stream, Encoding encoding, ISerializer serializer )
		{
			return serializer.Deserialize( stream, encoding ) as ICalendarComponent;
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public static ICalendarComponent LoadFromStream( TextReader tr )
		{
			string text = tr.ReadToEnd( );
			tr.Close( );

			byte[] memoryBlock = Encoding.UTF8.GetBytes( text );
			var ms = new MemoryStream( memoryBlock );
			return LoadFromStream( ms, Encoding.UTF8 );
		}

		/// <summary>
		///     Loads from stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public static T LoadFromStream<T>( TextReader tr ) where T : ICalendarComponent
		{
			object obj = LoadFromStream( tr );
			if ( obj is T )
			{
				return ( T ) obj;
			}
			return default( T );
		}

		private ICalendarPropertyList _properties;

		#region ICalendarPropertyList Members

		/// <summary>
		///     Returns a list of properties that are associated with the iCalendar object.
		/// </summary>
		/// <value>
		///     The properties.
		/// </value>
		public virtual ICalendarPropertyList Properties
		{
			get
			{
				return _properties;
			}
			protected set
			{
				_properties = value;
			}
		}

		#endregion

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarComponent" /> class.
		/// </summary>
		public CalendarComponent( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarComponent" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public CalendarComponent( string name )
			: base( name )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_properties = new CalendarPropertyList( this, true );
		}

		/// <summary>
		///     Called when [deserializing].
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		/// <summary>
		///     Copies all relevant fields/properties from
		///     the target object to the current one.
		/// </summary>
		/// <param name="obj"></param>
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var c = obj as ICalendarComponent;
			if ( c != null )
			{
				Properties.Clear( );
				foreach ( ICalendarProperty p in c.Properties )
				{
					Properties.Add( p.Copy<ICalendarProperty>( ) );
				}
			}
		}

		/// <summary>
		///     Adds a property to this component.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public virtual void AddProperty( string name, string value )
		{
			var p = new CalendarProperty( name, value );
			AddProperty( p );
		}

		/// <summary>
		///     Adds a property to this component.
		/// </summary>
		/// <param name="p">The p.</param>
		public virtual void AddProperty( ICalendarProperty p )
		{
			p.Parent = this;
			Properties.Set( p.Name, p );
		}
	}
}