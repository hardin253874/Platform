// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     ISerializationContext interface.
	/// </summary>
	public interface ISerializationContext : IServiceProvider
	{
		/// <summary>
		///     Peeks this instance.
		/// </summary>
		/// <returns></returns>
		object Peek( );

		/// <summary>
		///     Pops this instance.
		/// </summary>
		/// <returns></returns>
		object Pop( );

		/// <summary>
		///     Pushes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		void Push( object item );
	}
}