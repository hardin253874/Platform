// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Known Type helper.
	/// </summary>
	public static class KnownTypeHelper
	{
		/// <summary>
		///     Gets the known types.
		/// </summary>
		/// <returns></returns>
		public static IList<Type> GetKnownTypes( )
		{
			var types = new List<Type>
				{
					typeof ( CalendarPropertyList ),
					typeof ( CalendarParameterList )
				};

			return types;
		}
	}
}