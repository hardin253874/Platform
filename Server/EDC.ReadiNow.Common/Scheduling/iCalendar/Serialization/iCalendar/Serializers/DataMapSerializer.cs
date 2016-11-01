// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     DataMapSerializer class.
	/// </summary>
	public class DataMapSerializer : SerializerBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataMapSerializer" /> class.
		/// </summary>
		public DataMapSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DataMapSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public DataMapSerializer( ISerializationContext ctx )
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
				ISerializer serializer = GetMappedSerializer( );
				if ( serializer != null )
				{
					return serializer.TargetType;
				}
				return null;
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			IStringSerializer serializer = GetMappedSerializer( );
			if ( serializer != null )
			{
				string value = tr.ReadToEnd( );
				object returnValue = serializer.Deserialize( new StringReader( value ) );

				// Default to returning the string representation of the value
				// if the value wasn't formatted correctly.
				// FIXME: should this be a try/catch?  Should serializers be throwing
				// an InvalidFormatException?  This may have some performance issues
				// as try/catch is much slower than other means.
				return returnValue ?? value;
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
			IStringSerializer serializer = GetMappedSerializer( );
			if ( serializer != null )
			{
				return serializer.SerializeToString( obj );
			}
			return null;
		}

		/// <summary>
		///     Gets the mapped serializer.
		/// </summary>
		/// <returns></returns>
		protected IStringSerializer GetMappedSerializer( )
		{
			var sf = GetService<ISerializerFactory>( );
			var mapper = GetService<IDataTypeMapper>( );
			if ( sf != null &&
			     mapper != null )
			{
				object obj = SerializationContext.Peek( );

				// Get the data type for this object
				Type type = mapper.GetPropertyMapping( obj );

				if ( type != null )
				{
					return sf.Build( type, SerializationContext ) as IStringSerializer;
				}

				return new StringSerializer( SerializationContext );
			}
			return null;
		}
	}
}