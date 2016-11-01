// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     UriSerializer class.
	/// </summary>
	public class UriSerializer : EncodableDataTypeSerializer
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UriSerializer" /> class.
		/// </summary>
		public UriSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UriSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public UriSerializer( ISerializationContext ctx )
			: base( ctx )
		{
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
				return typeof ( string );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			if ( tr != null )
			{
				string value = tr.ReadToEnd( );

				var co = SerializationContext.Peek( ) as ICalendarObject;
				if ( co != null )
				{
					var dt = new EncodableDataType
						{
							AssociatedObject = co
						};
					value = Decode( dt, value );
				}

				value = TextUtil.Normalize( value, SerializationContext ).ReadToEnd( );

				try
				{
					var uri = new Uri( value );
					return uri;
				}
// ReSharper disable EmptyGeneralCatchClause
				catch
// ReSharper restore EmptyGeneralCatchClause
				{
				}
			}
			return null;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var uri1 = obj as Uri;

			if ( uri1 != null )
			{
				Uri uri = uri1;

				var co = SerializationContext.Peek( ) as ICalendarObject;
				if ( co != null )
				{
					var dt = new EncodableDataType
						{
							AssociatedObject = co
						};
					return Encode( dt, uri.OriginalString );
				}
				return uri.OriginalString;
			}
			return null;
		}
	}
}