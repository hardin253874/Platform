// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     IEncodingStack interface.
	/// </summary>
	public interface IEncodingStack
	{
		/// <summary>
		///     Gets the current.
		/// </summary>
		/// <value>
		///     The current.
		/// </value>
		Encoding Current
		{
			get;
		}

		/// <summary>
		///     Pops this instance.
		/// </summary>
		/// <returns></returns>
		Encoding Pop( );

		/// <summary>
		///     Pushes the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		void Push( Encoding encoding );
	}
}