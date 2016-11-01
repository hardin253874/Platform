// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics.Request
{
	/// <summary>
	///     Workflow Request
	/// </summary>
	[ProtoContract]
	public class WorkflowRequest : DiagnosticRequest
	{
		/// <summary>
		/// Occurs when a workflow diagnostics request is received.
		/// </summary>
		public static event EventHandler<WorkflowRequestEventArgs> WorkflowDiagnosticsRequestReceived;

		/// <summary>
		/// Gets a value indicating whether this instance is enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
		/// </value>
		public static bool IsEnabled
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="WorkflowRequest" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 1 )]
		public bool Enabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the response.
		/// </summary>
		/// <returns></returns>
		public override DiagnosticResponse GetResponse( )
		{
			try
			{
				IsEnabled = Enabled;

				var handler = WorkflowDiagnosticsRequestReceived;

				if ( handler != null )
				{
					handler( this, new WorkflowRequestEventArgs( Enabled ) );
				}

				return null;
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc.Message );
			}

			return null;
		}

		/// <summary>
		/// Workflow Request event args.
		/// </summary>
		public class WorkflowRequestEventArgs : EventArgs
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="WorkflowRequestEventArgs"/> class.
			/// </summary>
			/// <param name="enabled">if set to <c>true</c> [enabled].</param>
			public WorkflowRequestEventArgs( bool enabled )
				: base( )
			{
				Enabled = enabled;
			}

			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="WorkflowRequestEventArgs"/> is enabled.
			/// </summary>
			/// <value>
			///   <c>true</c> if enabled; otherwise, <c>false</c>.
			/// </value>
			public bool Enabled
			{
				get;
				set;
			}
		}
	}
}