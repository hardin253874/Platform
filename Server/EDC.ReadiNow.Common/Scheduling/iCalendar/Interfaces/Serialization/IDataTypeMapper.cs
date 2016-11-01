// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     IDataTypeMapper interface.
	/// </summary>
	public interface IDataTypeMapper
	{
		/// <summary>
		///     Adds the property mapping.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="allowsMultipleValuesPerProperty">
		///     if set to <c>true</c> [allows multiple values per property].
		/// </param>
		void AddPropertyMapping( string name, Type objectType, bool allowsMultipleValuesPerProperty );

		/// <summary>
		///     Adds the property mapping.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="resolver">The resolver.</param>
		/// <param name="allowsMultipleValuesPerProperty">
		///     if set to <c>true</c> [allows multiple values per property].
		/// </param>
		void AddPropertyMapping( string name, TypeResolverDelegate resolver, bool allowsMultipleValuesPerProperty );

		/// <summary>
		///     Gets the property allows multiple values.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		bool GetPropertyAllowsMultipleValues( object obj );

		/// <summary>
		///     Gets the property mapping.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		Type GetPropertyMapping( object obj );

		/// <summary>
		///     Removes the property mapping.
		/// </summary>
		/// <param name="name">The name.</param>
		void RemovePropertyMapping( string name );
	}
}