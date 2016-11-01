// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ISerializationProcessor interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISerializationProcessor<in T>
	{
		/// <summary>
		///     Posts the deserialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		void PostDeserialization( T obj );

		/// <summary>
		///     Posts the serialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		void PostSerialization( T obj );

		/// <summary>
		///     Pres the deserialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		void PreDeserialization( T obj );

		/// <summary>
		///     Pres the serialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		void PreSerialization( T obj );
	}
}