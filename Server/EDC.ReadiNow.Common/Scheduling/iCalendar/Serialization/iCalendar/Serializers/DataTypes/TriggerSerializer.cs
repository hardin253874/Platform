// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     TriggerSerializer class.
	/// </summary>
	public class TriggerSerializer : StringSerializer
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
				return typeof ( Trigger );
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

			var t = CreateAndAssociate( ) as ITrigger;
			if ( t != null )
			{
				// Push the trigger onto the serialization stack
				SerializationContext.Push( t );
				try
				{
					// Decode the value as needed
					value = Decode( t, value );

					// Set the trigger relation
					if ( t.Parameters.ContainsKey( "RELATED" ) &&
					     t.Parameters.Get( "RELATED" ).Equals( "END" ) )
					{
						t.Related = TriggerRelation.End;
					}

					var factory = GetService<ISerializerFactory>( );
					if ( factory != null )
					{
						Type valueType = t.GetValueType( ) ?? typeof ( TimeSpan );
						var serializer = factory.Build( valueType, SerializationContext ) as IStringSerializer;
						if ( serializer != null )
						{
							object obj = serializer.Deserialize( new StringReader( value ) );
							if ( obj != null )
							{
								var dateTime = obj as IDateTime;

								if ( dateTime != null )
								{
									t.DateTime = dateTime;
								}
								else
								{
									t.Duration = ( TimeSpan ) obj;
								}

								return t;
							}
						}
					}
				}
				finally
				{
					// Pop the trigger off the serialization stack
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
				var t = obj as ITrigger;
				if ( t != null )
				{
					// Push the trigger onto the serialization stack
					SerializationContext.Push( t );
					try
					{
						var factory = GetService<ISerializerFactory>( );
						if ( factory != null )
						{
							Type valueType = t.GetValueType( ) ?? typeof ( TimeSpan );
							var serializer = factory.Build( valueType, SerializationContext ) as IStringSerializer;
							if ( serializer != null )
							{
								object value = ( valueType == typeof ( IDateTime ) ) ? t.DateTime : ( object ) t.Duration;
								return serializer.SerializeToString( value );
							}
						}
					}
					finally
					{
						// Pop the trigger off the serialization stack
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