// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Specialized;

namespace EDC.Core
{
	/// <summary>
	///     Defines the base methods and properties for any callback arguments
	/// </summary>
	[Serializable]
	public class CallbackArgs
	{
		private StringDictionary _parameters = new StringDictionary( );

		/// <summary>
		///     Initializes a new instance of the CallbackArgs class.
		/// </summary>
		public CallbackArgs( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the CallbackArgs class.
		/// </summary>
		/// <param name="parameters">
		///     A dictionary containing any optional additional parameters.
		/// </param>
		public CallbackArgs( StringDictionary parameters )
		{
			_parameters = parameters;
		}

		/// <summary>
		///     Gets or sets any callback parameters
		/// </summary>
		public StringDictionary Parameters
		{
			get
			{
				return _parameters;
			}

			set
			{
				_parameters = value;
			}
		}
	}
}