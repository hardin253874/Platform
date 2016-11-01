// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that represents the return status of an iCalendar request.
	/// </summary>
	[Serializable]
	public class RequestStatus : EncodableDataType, IRequestStatus
	{
		/// <summary>
		///     Description.
		/// </summary>
		private string _description;

		/// <summary>
		///     Extra Data.
		/// </summary>
		private string _extraData;

		/// <summary>
		///     Status Code.
		/// </summary>
		private IStatusCode _statusCode;

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public virtual string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		/// <summary>
		///     Gets or sets the extra data.
		/// </summary>
		/// <value>
		///     The extra data.
		/// </value>
		public virtual string ExtraData
		{
			get
			{
				return _extraData;
			}
			set
			{
				_extraData = value;
			}
		}

		/// <summary>
		///     Gets or sets the status code.
		/// </summary>
		/// <value>
		///     The status code.
		/// </value>
		public virtual IStatusCode StatusCode
		{
			get
			{
				return _statusCode;
			}
			set
			{
				_statusCode = value;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RequestStatus" /> class.
		/// </summary>
		public RequestStatus( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RequestStatus" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public RequestStatus( string value )
			: this( )
		{
			var serializer = new RequestStatusSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override sealed void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );
			var requestStatus = obj as IRequestStatus;
			if ( requestStatus != null )
			{
				IRequestStatus rs = requestStatus;
				if ( rs.StatusCode != null )
				{
					StatusCode = rs.StatusCode.Copy<IStatusCode>( );
				}
				Description = rs.Description;
				rs.ExtraData = rs.ExtraData;
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
			var serializer = new RequestStatusSerializer( );
			return serializer.SerializeToString( this );
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
			var rs = obj as IRequestStatus;
			if ( rs != null )
			{
				return Equals( StatusCode, rs.StatusCode );
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
			int hash = 13;

			if ( StatusCode != null )
			{
				hash = ( hash * 7 ) + StatusCode.GetHashCode( );
			}

			return hash;
		}
	}
}