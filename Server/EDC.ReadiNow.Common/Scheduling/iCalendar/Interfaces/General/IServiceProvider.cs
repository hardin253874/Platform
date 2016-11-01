// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IServiceProvider interface.
	/// </summary>
	public interface IServiceProvider : System.IServiceProvider
	{
		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		object GetService( string name );

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetService<T>( );

		/// <summary>
		///     Gets the service.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		T GetService<T>( string name );

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="type">The type.</param>
		void RemoveService( Type type );

		/// <summary>
		///     Removes the service.
		/// </summary>
		/// <param name="name">The name.</param>
		void RemoveService( string name );

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="obj">The obj.</param>
		void SetService( string name, object obj );

		/// <summary>
		///     Sets the service.
		/// </summary>
		/// <param name="obj">The obj.</param>
		void SetService( object obj );
	}
}