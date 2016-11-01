// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     An abstract class from which all iCalendar data types inherit.
	/// </summary>
	[Serializable]
	public class EncodableDataType : CalendarDataType, IEncodableDataType
	{
		#region IEncodableDataType Members

		/// <summary>
		///     Gets or sets the encoding.
		/// </summary>
		/// <value>
		///     The encoding.
		/// </value>
		public virtual string Encoding
		{
			get
			{
				return Parameters.Get( "ENCODING" );
			}
			set
			{
				Parameters.Set( "ENCODING", value );
			}
		}

		#endregion
	}
}