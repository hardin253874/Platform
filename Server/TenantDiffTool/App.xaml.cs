// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows;

namespace TenantDiffTool
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public static String[ ] StartupArgs;

		/// <summary>
		///     Handles the Startup event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="StartupEventArgs" /> instance containing the event data.
		/// </param>
		private void Application_Startup( object sender, StartupEventArgs e )
		{
			if ( e.Args.Length > 0 )
			{
				StartupArgs = e.Args;
			}
		}
	}
}