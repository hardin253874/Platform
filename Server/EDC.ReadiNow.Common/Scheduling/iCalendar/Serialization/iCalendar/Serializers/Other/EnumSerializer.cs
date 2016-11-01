// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     EnumSerializer class.
	/// </summary>
	public class EnumSerializer : EncodableDataTypeSerializer
	{
		/// <summary>
		///     Enumeration type.
		/// </summary>
		private readonly Type _enumType;

		/// <summary>
		///     Initializes a new instance of the <see cref="EnumSerializer" /> class.
		/// </summary>
		/// <param name="enumType">Type of the enum.</param>
		public EnumSerializer( Type enumType )
		{
			_enumType = enumType;
		}

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
				return _enumType;
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

				// Remove "-" characters while parsing Enumeration values.
				return Enum.Parse( _enumType, value.Replace( "-", "" ), true );
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
		/// <param name="enumValue">The enum value.</param>
		/// <returns></returns>
		public override string SerializeToString( object enumValue )
		{
			try
			{
				var obj = SerializationContext.Peek( ) as ICalendarObject;
				if ( obj != null )
				{
					// Encode the value as needed.
					var dt = new EncodableDataType
						{
							AssociatedObject = obj
						};
					return Encode( dt, enumValue.ToString( ) );
				}
				return enumValue.ToString( );
			}
			catch
			{
				return null;
			}
		}
	}
}