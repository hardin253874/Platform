// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     RequestStatusSerializer class.
	/// </summary>
	public class RequestStatusSerializer : StringSerializer
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
				return typeof ( RequestStatus );
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

			var rs = CreateAndAssociate( ) as IRequestStatus;
			if ( rs != null )
			{
				// Decode the value as needed
				value = Decode( rs, value );

				// Push the object onto the serialization stack
				SerializationContext.Push( rs );

				try
				{
					var factory = GetService<ISerializerFactory>( );
					if ( factory != null )
					{
						Match match = Regex.Match( value, @"(.*?[^\\]);(.*?[^\\]);(.+)" );
						if ( !match.Success )
						{
							match = Regex.Match( value, @"(.*?[^\\]);(.+)" );
						}

						if ( match.Success )
						{
							var serializer = factory.Build( typeof ( IStatusCode ), SerializationContext ) as IStringSerializer;
							if ( serializer != null )
							{
								rs.StatusCode = serializer.Deserialize( new StringReader( Unescape( match.Groups[ 1 ].Value ) ) ) as IStatusCode;
								rs.Description = Unescape( match.Groups[ 2 ].Value );
								if ( match.Groups.Count == 4 )
								{
									rs.ExtraData = Unescape( match.Groups[ 3 ].Value );
								}

								return rs;
							}
						}
					}
				}
				finally
				{
					// Pop the object off the serialization stack
					SerializationContext.Pop( );
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
			try
			{
				var rs = obj as IRequestStatus;
				if ( rs != null )
				{
					// Push the object onto the serialization stack
					SerializationContext.Push( rs );

					try
					{
						var factory = GetService<ISerializerFactory>( );
						if ( factory != null )
						{
							var serializer = factory.Build( typeof ( IStatusCode ), SerializationContext ) as IStringSerializer;
							if ( serializer != null )
							{
								string value = Escape( serializer.SerializeToString( rs.StatusCode ) );
								value += ";" + Escape( rs.Description );
								if ( !string.IsNullOrEmpty( rs.ExtraData ) )
								{
									value += ";" + Escape( rs.ExtraData );
								}

								return Encode( rs, value );
							}
						}
					}
					finally
					{
						// Pop the object off the serialization stack
						SerializationContext.Pop( );
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}