// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	///     PerfGraphPopupViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class PerfGraphPopupViewModel : ViewModelBase
	{
		private string _message;

		/// <summary>
		///     Initializes a new instance of the <see cref="PerfGraphPopupViewModel" /> class.
		/// </summary>
		public PerfGraphPopupViewModel( )
		{
			PopupClick = new DelegateCommand( OnClick );
		}

		/// <summary>
		///     Gets or sets the message.
		/// </summary>
		/// <value>
		///     The message.
		/// </value>
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				SetProperty( ref _message, value );
			}
		}

		/// <summary>
		///     Gets the popup click.
		/// </summary>
		/// <value>
		///     The popup click.
		/// </value>
		public ICommand PopupClick
		{
			get;
			private set;
		}

		private void OnClick( )
		{
			if ( ShowPerfLog != null )
			{
				ShowPerfLog( this, Message );
			}
		}

		/// <summary>
		///     Occurs when [show perf log].
		/// </summary>
		public event EventHandler<string> ShowPerfLog;
	}
}