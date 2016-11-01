// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	public class EncodingStack : IEncodingStack
	{
		/// <summary>
		///     Stack.
		/// </summary>
		private readonly Stack<Encoding> _stack;

		/// <summary>
		///     Initializes a new instance of the <see cref="EncodingStack" /> class.
		/// </summary>
		public EncodingStack( )
		{
			_stack = new Stack<Encoding>( );
		}

		#region IEncodingStack Members

		/// <summary>
		///     Gets the current.
		/// </summary>
		/// <value>
		///     The current.
		/// </value>
		public Encoding Current
		{
			get
			{
				if ( _stack.Count > 0 )
				{
					return _stack.Peek( );
				}

				// Default to Unicode encoding
				return Encoding.Unicode;
			}
		}

		/// <summary>
		///     Pushes the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		public void Push( Encoding encoding )
		{
			if ( encoding != null )
			{
				_stack.Push( encoding );
			}
		}

		/// <summary>
		///     Pops this instance.
		/// </summary>
		/// <returns></returns>
		public Encoding Pop( )
		{
			if ( _stack.Count > 0 )
			{
				return _stack.Pop( );
			}
			return null;
		}

		#endregion
	}
}