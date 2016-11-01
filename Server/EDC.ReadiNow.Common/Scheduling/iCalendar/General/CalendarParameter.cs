// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Scheduling.iCalendar.Collections;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarParameter class.
	/// </summary>
	[Serializable]
	public sealed class CalendarParameter : CalendarObject, ICalendarParameter
	{
		/// <summary>
		///     Values.
		/// </summary>
		private List<string> _values;

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarParameter" /> class.
		/// </summary>
		public CalendarParameter( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarParameter" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public CalendarParameter( string name )
			: base( name )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarParameter" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public CalendarParameter( string name, string value )
			: base( name )
		{
			Initialize( );
			AddValue( value );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarParameter" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="values">The values.</param>
		public CalendarParameter( string name, IEnumerable<string> values )
			: base( name )
		{
			Initialize( );
			foreach ( string v in values )
			{
				AddValue( v );
			}
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_values = new List<string>( );
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
		///     Copies from.
		/// </summary>
		/// <param name="c">The c.</param>
		public override void CopyFrom( ICopyable c )
		{
			base.CopyFrom( c );

			var p = c as ICalendarParameter;
			if ( p != null )
			{
				if ( p.Values != null )
				{
					_values = new List<string>( p.Values );
				}
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			if ( Values != null )
			{
				return string.Format( "{0}={1}", Name, string.Join( ",", Values.ToArray( ) ) );
			}

			return string.Format( "{0}=", Name );
		}

		#region IValueObject<string> Members

		/// <summary>
		///     Occurs when the value has changed.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler<ValueChangedEventArgs<string>> ValueChanged;

		/// <summary>
		///     Called when [value changed].
		/// </summary>
		/// <param name="removedValues">The removed values.</param>
		/// <param name="addedValues">The added values.</param>
		private void OnValueChanged( IEnumerable<string> removedValues, IEnumerable<string> addedValues )
		{
			if ( ValueChanged != null )
			{
				ValueChanged( this, new ValueChangedEventArgs<string>( removedValues, addedValues ) );
			}
		}

		/// <summary>
		///     Gets the values.
		/// </summary>
		/// <value>
		///     The values.
		/// </value>
		public IEnumerable<string> Values
		{
			get
			{
				return _values;
			}
		}

		/// <summary>
		///     Determines whether the specified value contains value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the specified value contains value; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsValue( string value )
		{
			return _values.Contains( value );
		}

		/// <summary>
		///     Gets the value count.
		/// </summary>
		/// <value>
		///     The value count.
		/// </value>
		public int ValueCount
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
		public void SetValue( string value )
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
				string oldValue = _values[ 0 ];
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
				var values = new List<string>( Values );
				_values.Clear( );
				OnValueChanged( values, null );
			}
		}

		/// <summary>
		///     Sets the value.
		/// </summary>
		/// <param name="values">The values.</param>
		public void SetValue( IEnumerable<string> values )
		{
			// Remove all previous values
			List<string> removedValues = _values.ToList( );
			_values.Clear( );

			IEnumerable<string> addedValues = values as IList<string> ?? values.ToList( );

			_values.AddRange( addedValues );

			OnValueChanged( removedValues, addedValues );
		}

		/// <summary>
		///     Adds the value.
		/// </summary>
		/// <param name="value">The value.</param>
		public void AddValue( string value )
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
		public void RemoveValue( string value )
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

		#region ICalendarParameter Members

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public string Value
		{
			get
			{
				if ( Values != null )
				{
					return Values.FirstOrDefault( );
				}
				return default( string );
			}
			set
			{
				SetValue( value );
			}
		}

		#endregion
	}
}