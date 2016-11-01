// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     ICopyable interface.
	/// </summary>
	public interface ICopyable
	{
		/// <summary>
		///     Returns a copy of the current object, including
		///     all relevant fields/properties, resulting in a
		///     semantically equivalent copy of the object.
		///     (which consequently passes an object.Equals(obj1, obj2)
		///     test).
		/// </summary>
		T Copy<T>( );

		/// <summary>
		///     Copies all relevant fields/properties from
		///     the target object to the current one.
		/// </summary>
		void CopyFrom( ICopyable obj );
	}
}