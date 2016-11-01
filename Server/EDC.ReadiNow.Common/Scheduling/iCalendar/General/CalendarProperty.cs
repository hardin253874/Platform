// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents a property of the <see cref="iCalendar" />
	///     itself or one of its components.  It can also represent non-standard
	///     (X-) properties of an iCalendar component, as seen with many
	///     applications, such as with Apple's iCal.
	///     X-WR-CALNAME:US Holidays
	/// </summary>
	/// <remarks>
	///     Currently, the "known" properties for an iCalendar are as
	///     follows:
	///     <list type="bullet">
	///         <item>ProdID</item>
	///         <item>Version</item>
	///         <item>CalScale</item>
	///         <item>Method</item>
	///     </list>
	///     There may be other, custom X-properties applied to the calendar,
	///     and X-properties may be applied to calendar components.
	/// </remarks>
	[DebuggerDisplay( "{Name}:{Value}" )]
	[Serializable]
	public class CalendarProperty : CalendarObject, ICalendarProperty
	{
		/// <summary>
		///     Values.
		/// </summary>
		private IList<object> _values;

		/// <summary>
		///     Parameters.
		/// </summary>
		private ICalendarParameterCollection _parameters;

		/// <summary>
		///     Returns a list of parameters that are associated with the iCalendar object.
		/// </summary>
		/// <value>
		///     The parameters.
		/// </value>
		public virtual ICalendarParameterCollection Parameters
		{
			get
			{
				return _parameters;
			}
			protected set
			{
				_parameters = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarProperty" /> class.
		/// </summary>
		public CalendarProperty( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarProperty" /> class.
		/// </summary>
		/// <param name="other">The other.</param>
		public CalendarProperty( ICalendarProperty other )
			: this( )
		{
			CopyFrom( other );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarProperty" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public CalendarProperty( string name )
			: base( name )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarProperty" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public CalendarProperty( string name, object value )
			: base( name )
		{
			Initialize( );
			_values.Add( value );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarProperty" /> class.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		public CalendarProperty( int line, int col )
			: base( line, col )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_values = new List<object>( );
			_parameters = new CalendarParameterList( this, true );
		}

		/// <summary>
		///     Adds a parameter to the iCalendar object.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public virtual void AddParameter( string name, string value )
		{
			var p = new CalendarParameter( name, value );
			AddParameter( p );
		}

		/// <summary>
		///     Adds a parameter to the iCalendar object.
		/// </summary>
		/// <param name="p">The p.</param>
		public virtual void AddParameter( ICalendarParameter p )
		{
			Parameters.Add( p );
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
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var p = obj as ICalendarProperty;
			if ( p != null )
			{
				// Copy/clone the object if possible (deep copy)
				var copyable = p.Values as ICopyable;

				if ( copyable != null )
				{
					SetValue( copyable.Copy<object>( ) );
				}
				else
				{
					var cloneable = p.Values as ICloneable;

					if ( cloneable != null )
					{
						SetValue( cloneable.Clone( ) );
					}
					else
					{
						SetValue( p.Values );
					}
				}

				// Copy parameters
				foreach ( ICalendarParameter parm in p.Parameters )
				{
					AddParameter( parm.Copy<ICalendarParameter>( ) );
				}
			}
		}

		#region IValueObject<object> Members

		/// <summary>
		///     Occurs when the value has changed.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler<ValueChangedEventArgs<object>> ValueChanged;

		/// <summary>
		///     Called when [value changed].
		/// </summary>
		/// <param name="removedValue">The removed value.</param>
		/// <param name="addedValue">The added value.</param>
		protected void OnValueChanged( object removedValue, object addedValue )
		{
			if ( ValueChanged != null )
			{
				ValueChanged( this, new ValueChangedEventArgs<object>( ( IEnumerable<object> ) removedValue, ( IEnumerable<object> ) addedValue ) );
			}
		}

		/// <summary>
		///     Gets the values.
		/// </summary>
		/// <value>
		///     The values.
		/// </value>
		public virtual IEnumerable<object> Values
		{
			get
			{
				return _values;
			}
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public object Value
		{
			get
			{
				if ( _values != null )
				{
					return _values.FirstOrDefault( );
				}
				return null;
			}
			set
			{
				if ( value != null )
				{
					if ( _values != null && _values.Count > 0 )
					{
						_values[ 0 ] = value;
					}
					else
					{
						if ( _values != null )
						{
							_values.Clear( );
							_values.Add( value );
						}
					}
				}
				else
				{
					_values = null;
				}
			}
		}

		/// <summary>
		///     Determines whether the specified value contains value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the specified value contains value; otherwise, <c>false</c>.
		/// </returns>
		public virtual bool ContainsValue( object value )
		{
			return _values.Contains( value );
		}

		/// <summary>
		///     Gets the value count.
		/// </summary>
		/// <value>
		///     The value count.
		/// </value>
		public virtual int ValueCount
		{
			get
			{
				return _values != null ? _values.Count : 0;
			}
		}

		/// <summary>
		///     Sets the value.
		/// </summary>
		/// <param name="value">The value.</param>
		public virtual void SetValue( object value )
		{
			if ( _values.Count == 0 )
			{
				// Our list doesn't contain any values.  Let's add one!
				_values.Add( value );
				OnValueChanged( null, new[]
					{
						value
					} );
			}
			else if ( value != null )
			{
				// Our list contains values.  Let's set the first value!
				object oldValue = _values[ 0 ];
				_values[ 0 ] = value;
				OnValueChanged( new[]
					{
						oldValue
					}, new[]
						{
							value
						} );
			}
			else
			{
				// Remove all values
				var values = new List<object>( Values );
				_values.Clear( );
				OnValueChanged( values, null );
			}
		}

		/// <summary>
		///     Sets the value.
		/// </summary>
		/// <param name="values">The values.</param>
		public virtual void SetValue( IEnumerable<object> values )
		{
			// Remove all previous values
			List<object> removedValues = _values.ToList( );
			_values.Clear( );
			_values.AddRange( values );
			OnValueChanged( removedValues, values );
		}

		/// <summary>
		///     Adds the value.
		/// </summary>
		/// <param name="value">The value.</param>
		public virtual void AddValue( object value )
		{
			if ( value != null )
			{
				_values.Add( value );
				OnValueChanged( null, new[]
					{
						value
					} );
			}
		}

		/// <summary>
		///     Removes the value.
		/// </summary>
		/// <param name="value">The value.</param>
		public virtual void RemoveValue( object value )
		{
			if ( value != null &&
			     _values.Contains( value ) &&
			     _values.Remove( value ) )
			{
				OnValueChanged( new[]
					{
						value
					}, null );
			}
		}

		#endregion
	}
}