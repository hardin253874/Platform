// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IGroupedValueList type.
	/// </summary>
	/// <typeparam name="TGroup">The type of the group.</typeparam>
	/// <typeparam name="TInterface">The type of the interface.</typeparam>
	/// <typeparam name="TValueType">The type of the value type.</typeparam>
	public interface IGroupedValueList<TGroup, TInterface, in TValueType> : IGroupedValueCollection<TGroup, TInterface, TValueType>, IGroupedList<TGroup, TInterface>
		where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
	{
	}
}