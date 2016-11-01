// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Interaction logic for TestEditor.xaml
	/// </summary>
	public partial class TestEditor : Window
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TestEditor" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="query">The query.</param>
		public TestEditor( IPluginSettings settings, string query )
		{
			InitializeComponent( );

			var viewModel = new TestEditorViewModel( settings, query );

			DataContext = viewModel;

			var assemblyLocation = Assembly.GetExecutingAssembly( ).Location;

			var directoryName = Path.GetDirectoryName( assemblyLocation );

			if ( directoryName != null )
			{
				string xshdPath = Path.Combine( directoryName, "sqlSyntax.xshd" );

				using ( var reader = new XmlTextReader( xshdPath ) )
				{
					Editor.SyntaxHighlighting = HighlightingLoader.Load( reader, HighlightingManager.Instance );
				}
			}
		}
	}
}