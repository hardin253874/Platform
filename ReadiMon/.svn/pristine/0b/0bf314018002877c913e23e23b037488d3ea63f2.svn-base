// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Reflection;
using ReadiMon.Properties;
using ReadiMon.Shared.Core;

namespace ReadiMon
{
	/// <summary>
	///     ChangeLog view model.
	/// </summary>
	public class ChangeLogViewModel : ViewModelBase
	{
		private bool _neverShowThisAgain;

		/// <summary>
		///     The _text
		/// </summary>
		private string _text;

		/// <summary>
		///     Gets or sets a value indicating whether [never show this again].
		/// </summary>
		/// <value>
		///     <c>true</c> if [never show this again]; otherwise, <c>false</c>.
		/// </value>
		public bool NeverShowThisAgain
		{
			get
			{
				return _neverShowThisAgain;
			}
			set
			{
				SetProperty( ref _neverShowThisAgain, value );

				Settings.Default.NeverShowWhatsNew = value;
				Settings.Default.Save( );
			}
		}

		/// <summary>
		///     Gets or sets the text.
		/// </summary>
		/// <value>
		///     The text.
		/// </value>
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				SetProperty( ref _text, value );
			}
		}

		/// <summary>
		///     Gets the window title.
		/// </summary>
		/// <value>
		///     The window title.
		/// </value>
		public string WindowTitle
		{
			get
			{
				return "What's new in " + AssemblyName.GetAssemblyName( Assembly.GetEntryAssembly( ).Location ).Version.ToString( 3 );
			}
		}
	}
}