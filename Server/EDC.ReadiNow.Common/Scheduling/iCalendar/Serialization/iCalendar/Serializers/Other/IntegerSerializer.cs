// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     IntegerSerializer class.
	/// </summary>
	public class IntegerSerializer : EncodableDataTypeSerializer
	{
		/// <summary>
		///     Gets the type of the target.
		/// </summary>
		/// <value>
		///     The type of the target.
		/// </value>
		public override Type TargetType
		{
			get
			{
				return typeof ( int );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			string value = tr.ReadToEnd( );

			try
			{
				var obj = SerializationContext.Peek( ) as ICalendarObject;
				if ( obj != null )
				{
					// Decode the value, if necessary!
					var dt = new EncodableDataType
						{
							AssociatedObject = obj
						};
					value = Decode( dt, value );
				}

				int i;
				if ( Int32.TryParse( value, out i ) )
				{
					return i;
				}
			}
// ReSharper disable EmptyGeneralCatchClause
			catch
// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return value;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="integer">The integer.</param>
		/// <returns></returns>
		public override string SerializeToString( object integer )
		{
			try
			{
				int i = Convert.ToInt32( integer );

				var obj = SerializationContext.Peek( ) as ICalendarObject;
				if ( obj != null )
				{
					// Encode the value as needed.
					var dt = new EncodableDataType
						{
							AssociatedObject = obj
						};
					return Encode( dt, i.ToString( CultureInfo.InvariantCulture ) );
				}
				return i.ToString( CultureInfo.InvariantCulture );
			}
			catch
			{
				return null;
			}
		}
	}
}