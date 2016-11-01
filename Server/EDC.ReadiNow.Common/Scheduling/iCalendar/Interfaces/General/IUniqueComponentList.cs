// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IUniqueComponentList interface.
	/// </summary>
	/// <typeparam name="TComponentType">The type of the component type.</typeparam>
	public interface IUniqueComponentList<TComponentType> : ICalendarObjectList<TComponentType>
		where TComponentType : class, IUniqueComponent
	{
		/// <summary>
		///     Gets or sets the <see cref="TComponentType" /> with the specified UID.
		/// </summary>
		/// <value>
		///     The <see cref="TComponentType" />.
		/// </value>
		/// <param name="uid">The UID.</param>
		/// <returns></returns>
		TComponentType this[ string uid ]
		{
			get;
			set;
		}
	}
}