// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IGroupedValueCollection type.
	/// </summary>
	/// <typeparam name="TGroup">The type of the group.</typeparam>
	/// <typeparam name="TInterface">The type of the interface.</typeparam>
	/// <typeparam name="TValueType">The type of the value type.</typeparam>
	public interface IGroupedValueCollection<TGroup, TInterface, in TValueType> : IGroupedCollection<TGroup, TInterface>
		where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
	{
		/// <summary>
		///     Gets the specified group.
		/// </summary>
		/// <typeparam name="TType">The type of the type.</typeparam>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		TType Get<TType>( TGroup group );

		/// <summary>
		///     Gets the many.
		/// </summary>
		/// <typeparam name="TType">The type of the type.</typeparam>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		IList<TType> GetMany<TType>( TGroup group );

		/// <summary>
		///     Sets the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="value">The value.</param>
		void Set( TGroup group, TValueType value );

		/// <summary>
		///     Sets the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="values">The values.</param>
		void Set( TGroup group, IEnumerable<TValueType> values );
	}
}