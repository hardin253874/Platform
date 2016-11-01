// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ReadiMon.Shared;
using ReadiMon.Shared.Controls;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Interaction logic for RedisPubSubMonitor.xaml
	/// </summary>
	public partial class RedisPubSubMonitor
	{
		/// <summary>
		///     The tool tip
		/// </summary>
		private readonly ToolTip _toolTip = new ToolTip( );

		/// <summary>
		///     The view model
		/// </summary>
		private readonly RedisPubSubMonitorViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPubSubMonitor" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public RedisPubSubMonitor( IPluginSettings settings )
		{
			InitializeComponent( );

			_viewModel = new RedisPubSubMonitorViewModel( settings );

			DataContext = _viewModel;

			var assemblyLocation = Assembly.GetExecutingAssembly( ).Location;

			var directoryName = Path.GetDirectoryName( assemblyLocation );

			if ( directoryName != null )
			{
				string xshdPath = Path.Combine( directoryName, "redisSyntax.xshd" );

				using ( var reader = new XmlTextReader( xshdPath ) )
				{
					editor.SyntaxHighlighting = HighlightingLoader.Load( reader, HighlightingManager.Instance );
				}
			}

			editor.MouseHover += editor_MouseHover;
			editor.MouseHoverStopped += editor_MouseHoverStopped;
			editor.MouseMove += editor_MouseMove;
			editor.TextArea.PreviewMouseLeftButtonUp += TextArea_PreviewMouseLeftButtonUp;
		}

		/// <summary>
		///     Determines whether [is long or unique identifier or alias] [the specified input].
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		private bool IsLongOrGuidOrAlias( string input )
		{
			long l;

			if ( long.TryParse( input, out l ) )
			{
				return true;
			}

			Guid g;

			if ( Guid.TryParse( input, out g ) )
			{
				return true;
			}

			return false;
		}

		/// <summary>
		///     Handles the PreviewMouseLeftButtonUp event of the TextArea control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
		private void TextArea_PreviewMouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			var position = editor.GetPositionFromPoint( e.GetPosition( editor ) );

			if ( position != null )
			{
				var wordUnderMouse = editor.Document.GetWordUnderMouse( position.Value );

				if ( !string.IsNullOrEmpty( wordUnderMouse ) )
				{
					_viewModel.PluginSettings.Channel.SendMessage( new EntityBrowserMessage( wordUnderMouse ).ToString( ) );
				}
			}
		}

		/// <summary>
		///     Handles the MouseHover event of the editor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs" /> instance containing the event data.</param>
		private void editor_MouseHover( object sender, MouseEventArgs e )
		{
			var position = editor.GetPositionFromPoint( e.GetPosition( editor ) );

			if ( position != null )
			{
				var wordUnderMouse = editor.Document.GetWordUnderMouse( position.Value );

				if ( !string.IsNullOrEmpty( wordUnderMouse ) && IsLongOrGuidOrAlias( wordUnderMouse ) )
				{
					string text;

					if ( !_viewModel.IsEntity( wordUnderMouse, out text ) )
					{
						text = string.Format( "'{0}' not found", wordUnderMouse );
					}

					_toolTip.Placement = PlacementMode.MousePoint;
					_toolTip.HorizontalOffset = 15;
					_toolTip.VerticalOffset = 10;
					_toolTip.Content = text;
					_toolTip.IsOpen = true;

					e.Handled = true;
				}
			}
		}

		/// <summary>
		///     Handles the MouseHoverStopped event of the editor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs" /> instance containing the event data.</param>
		private void editor_MouseHoverStopped( object sender, MouseEventArgs e )
		{
			_toolTip.IsOpen = false;
		}

		/// <summary>
		///     Handles the MouseMove event of the editor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
		private void editor_MouseMove( object sender, MouseEventArgs e )
		{
			var position = editor.GetPositionFromPoint( e.GetPosition( editor ) );

			if ( position != null )
			{
				var wordUnderMouse = editor.Document.GetWordUnderMouse( position.Value );

				if ( !string.IsNullOrEmpty( wordUnderMouse ) )
				{
					editor.TextArea.Cursor = IsLongOrGuidOrAlias( wordUnderMouse ) ? Cursors.Hand : Cursors.IBeam;
				}
			}
		}
	}
}