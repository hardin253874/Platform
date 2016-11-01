// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     AttachmentSerializer class.
	/// </summary>
	public class AttachmentSerializer : EncodableDataTypeSerializer
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
				return typeof ( Attachment );
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
				var a = CreateAndAssociate( ) as IAttachment;
				if ( a != null )
				{
					// Decode the value, if necessary
					byte[] data = DecodeData( a, value );

					// Get the currently-used encoding off the encoding stack.
					var encodingStack = GetService<IEncodingStack>( );
					a.ValueEncoding = encodingStack.Current;

					// Get the format of the attachment
					Type valueType = a.GetValueType( );
					if ( valueType == typeof ( byte[] ) )
					{
						// If the VALUE type is specifically set to BINARY,
						// then set the Data property instead.                    
						a.Data = data;
						return a;
					}

					// The default VALUE type for attachments is URI.  So, let's
					// grab the URI by default.
					string uriValue = Decode( a, value );
					a.Uri = new Uri( uriValue );

					return a;
				}
			}
// ReSharper disable EmptyGeneralCatchClause
			catch
// ReSharper restore EmptyGeneralCatchClause
			{
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
			var a = obj as IAttachment;
			if ( a != null )
			{
				if ( a.Uri != null )
				{
					if ( a.Parameters.ContainsKey( "VALUE" ) )
					{
						// Ensure no VALUE type is provided
						a.Parameters.Remove( "VALUE" );
					}

					return Encode( a, a.Uri.OriginalString );
				}

				if ( a.Data != null )
				{
					// Ensure the VALUE type is set to BINARY
					a.SetValueType( "BINARY" );

					// BASE64 encoding for BINARY inline attachments.
					a.Parameters.Set( "ENCODING", "BASE64" );

					return Encode( a, a.Data );
				}
			}
			return null;
		}
	}
}