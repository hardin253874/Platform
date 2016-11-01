// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     SerializationInfo class.
	/// </summary>
	public class SerializationInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SerializationInfo" /> class.
		/// </summary>
		public SerializationInfo( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializationInfo" /> class.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="column">The column.</param>
		public SerializationInfo( int line, int column )
		{
			Line = line;
			Column = column;
		}

		/// <summary>
		///     Returns the column number where this calendar
		///     object was found during parsing.
		/// </summary>
		private int Column
		{
			get;
			set;
		}

		/// <summary>
		///     Returns the line number where this calendar
		///     object was found during parsing.
		/// </summary>
		private int Line
		{
			get;
			set;
		}
	}
}