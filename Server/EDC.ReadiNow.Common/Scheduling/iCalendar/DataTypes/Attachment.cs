// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class to handle attachments, or URIs as attachments, within an iCalendar.
	/// </summary>
	[Serializable]
	public class Attachment : EncodableDataType, IAttachment
	{
		/// <summary>
		///     Uri.
		/// </summary>
		private Uri _uri;

		/// <summary>
		///     Data.
		/// </summary>
		private byte[] _data;

		/// <summary>
		///     Encoding.
		/// </summary>
		private Encoding _encoding;

		/// <summary>
		///     Initializes a new instance of the <see cref="Attachment" /> class.
		/// </summary>
		public Attachment( )
		{
			Initialize( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Attachment" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public Attachment( string value )
			: this( )
		{
			var serializer = new AttachmentSerializer( );

			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Attachment" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public Attachment( byte[] value )
			: this( )
		{
			_data = value;
		}

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		private void Initialize( )
		{
			_encoding = System.Text.Encoding.Unicode;
		}

		/// <summary>
		///     Called when deserializing.
		/// </summary>
		/// <param name="context">The context.</param>
		protected override void OnDeserializing( StreamingContext context )
		{
			base.OnDeserializing( context );

			Initialize( );
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var attachment = obj as IAttachment;

			if ( attachment != null )
			{
				IAttachment a = attachment;

				if ( Data == null && a.Data == null )
				{
					return Uri.Equals( a.Uri );
				}

				if ( Data == null || a.Data == null )
				{
					// One item is null, but the other isn't                    
					return false;
				}

				if ( Data.Length != a.Data.Length )
				{
					return false;
				}

				return !Data.Where( ( t, i ) => t != a.Data[ i ] ).Any( );
			}

// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			if ( Uri != null )
			{
				return Uri.GetHashCode( );
			}

			if ( Data != null )
			{
				return Data.GetHashCode( );
			}

// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode( );
// ReSharper restore BaseObjectGetHashCodeCallInGetHashCode
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var attachment = obj as IAttachment;

			if ( attachment != null )
			{
				IAttachment a = attachment;
				ValueEncoding = a.ValueEncoding;

				if ( a.Data != null )
				{
					Data = new byte[a.Data.Length];
					a.Data.CopyTo( Data, 0 );
				}
				else
				{
					Data = null;
				}

				Uri = a.Uri;
			}
		}

		#region IAttachment Members

		/// <summary>
		///     The URI where the attachment information can be located.
		/// </summary>
		/// <value>
		///     The URI.
		/// </value>
		public virtual Uri Uri
		{
			get
			{
				return _uri;
			}
			set
			{
				_uri = value;
			}
		}

		/// <summary>
		///     A binary representation of the data that was loaded.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		public virtual byte[] Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
			}
		}

		/// <summary>
		///     Gets/sets the encoding used to store the value.
		/// </summary>
		/// <value>
		///     The value encoding.
		/// </value>
		public virtual Encoding ValueEncoding
		{
			get
			{
				return _encoding;
			}
			set
			{
				_encoding = value;
			}
		}

		/// <summary>
		///     A Unicode-encoded version of the data that was loaded.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public virtual string Value
		{
			get
			{
				if ( Data != null )
				{
					return _encoding.GetString( Data );
				}
				return null;
			}
			set
			{
				Data = value != null ? _encoding.GetBytes( value ) : null;
			}
		}

		/// <summary>
		///     To specify the content type of a referenced object.
		///     This optional value should be an IANA-registered
		///     MIME type, if specified.
		/// </summary>
		/// <value>
		///     The type of the format.
		/// </value>
		public virtual string FormatType
		{
			get
			{
				return Parameters.Get( "FMTTYPE" );
			}
			set
			{
				Parameters.Set( "FMTTYPE", value );
			}
		}

		/// <summary>
		///     Loads (fills) the <c>Data</c> property with the file designated
		///     at the given <see cref="Uri" />.
		/// </summary>
		public virtual void LoadDataFromUri( )
		{
			LoadDataFromUri( null, null, null );
		}

		/// <summary>
		///     Loads (fills) the <c>Data</c> property with the file designated
		///     at the given <see cref="Uri" />.
		/// </summary>
		/// <param name="username">The username to supply for credentials</param>
		/// <param name="password">The password to supply for credentials</param>
		public virtual void LoadDataFromUri( string username, string password )
		{
			LoadDataFromUri( null, username, password );
		}

		/// <summary>
		///     Loads (fills) the <c>Data</c> property with the contents of the
		///     given <see cref="Uri" />.
		/// </summary>
		/// <param name="uri">
		///     The Uri from which to download the <c>Data</c>
		/// </param>
		/// <param name="username">The username to supply for credentials</param>
		/// <param name="password">The password to supply for credentials</param>
		public virtual void LoadDataFromUri( Uri uri, string username, string password )
		{
			using ( var client = new WebClient( ) )
			{
				if ( username != null &&
				     password != null )
				{
					client.Credentials = new NetworkCredential( username, password );
				}

				if ( uri == null )
				{
					if ( Uri == null )
					{
						throw new ArgumentException( "A URI was not provided for the LoadDataFromUri() method" );
					}
					uri = new Uri( Uri.OriginalString );
				}

				Data = client.DownloadData( uri );
			}
		}

		#endregion
	}
}