// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     New Password View Model
	/// </summary>
	public class NewPasswordViewModel : ViewModelBase
	{
		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     The OK enabled
		/// </summary>
		private bool _okEnabled;

		/// <summary>
		///     The password1
		/// </summary>
		private string _password1;

		/// <summary>
		///     The password2
		/// </summary>
		private string _password2;

		/// <summary>
		///     Initializes a new instance of the <see cref="NewPasswordViewModel" /> class.
		/// </summary>
		public NewPasswordViewModel( )
		{
			OkCommand = new DelegateCommand( OkClicked );
		}

		/// <summary>
		///     Gets or sets the close window.
		/// </summary>
		/// <value>
		///     The close window.
		/// </value>
		public bool? CloseWindow
		{
			get
			{
				return _closeWindow;
			}
			set
			{
				if ( _closeWindow != value )
				{
					SetProperty( ref _closeWindow, value );
				}
			}
		}

		/// <summary>
		///     Gets the OK command.
		/// </summary>
		/// <value>
		///     The OK command.
		/// </value>
		public ICommand OkCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether OK is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if OK is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool OkEnabled
		{
			get
			{
				return _okEnabled;
			}
			set
			{
				SetProperty( ref _okEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets the password1.
		/// </summary>
		/// <value>
		///     The password1.
		/// </value>
		public string Password1
		{
			get
			{
				return _password1;
			}
			set
			{
				SetProperty( ref _password1, value );

				OkEnabled = !string.IsNullOrEmpty( Password1 ) && !string.IsNullOrEmpty( Password2 ) && Password1 == Password2;
			}
		}

		/// <summary>
		///     Gets or sets the password2.
		/// </summary>
		/// <value>
		///     The password2.
		/// </value>
		public string Password2
		{
			get
			{
				return _password2;
			}
			set
			{
				SetProperty( ref _password2, value );

				OkEnabled = !string.IsNullOrEmpty( Password1 ) && !string.IsNullOrEmpty( Password2 ) && Password1 == Password2;
			}
		}

		/// <summary>
		///     OK clicked.
		/// </summary>
		private void OkClicked( )
		{
			CloseWindow = true;
		}
	}
}