// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Utility
{
	/// <summary>
	///     Enumeration Description attribute.
	/// </summary>
	public class EnumDescriptionAttribute : Attribute
	{
		/// <summary>
		/// </summary>
		private readonly string _description;

		/// <summary>
		///     Initializes a new instance of the <see cref="EnumDescriptionAttribute" /> class.
		/// </summary>
		/// <param name="description">The description.</param>
		public EnumDescriptionAttribute( string description )
		{
			_description = description;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get
			{
				return _description;
			}
		}
	}
}