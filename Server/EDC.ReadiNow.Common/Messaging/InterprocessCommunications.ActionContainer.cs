// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Interprocess communications.
	/// </summary>
	public static partial class InterprocessCommunications
	{
		/// <summary>
		///     Action container.
		/// </summary>
		private class ActionContainer
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="ActionContainer" /> class.
			/// </summary>
			/// <param name="action">The action.</param>
			public ActionContainer( Action action )
			{
				Action = action;
			}

			/// <summary>
			///     Initializes a new instance of the <see cref="ActionContainer" /> class.
			/// </summary>
			/// <param name="action">The action.</param>
			/// <param name="payload">The payload.</param>
			/// <param name="method">The method.</param>
			public ActionContainer( Action<object> action, object payload, EncodingMethod method = EncodingMethod.Binary )
			{
				Action = action;
				Payload = payload;
				EncodingMethod = method;
			}

			/// <summary>
			///     Gets or sets the action.
			/// </summary>
			/// <value>
			///     The action.
			/// </value>
			public Delegate Action
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets or sets the encoding method.
			/// </summary>
			/// <value>
			///     The encoding method.
			/// </value>
			public EncodingMethod EncodingMethod
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets or sets the payload.
			/// </summary>
			/// <value>
			///     The payload.
			/// </value>
			public object Payload
			{
				get;
				private set;
			}
		}
	}
}