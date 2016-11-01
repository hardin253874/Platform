// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     UID Factory class.
	/// </summary>
	public class UidFactory
	{
		/// <summary>
		///     Builds this instance.
		/// </summary>
		/// <returns></returns>
		public virtual string Build( )
		{
			return Guid.NewGuid( ).ToString( );
		}
	}
}