// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ReadiMon.AttachedProperties
{
	/// <summary>
	///     ToolBarTrayItemSource class.
	/// </summary>
	public class ToolBarTrayItemSource : DependencyObject
	{
		/// <summary>
		///     The item source property
		/// </summary>
		public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.RegisterAttached( "ItemSource", typeof ( ObservableCollection<ToolBar> ), typeof ( ToolBarTrayItemSource ), new PropertyMetadata( OnItemSourceChanged ) );

		/// <summary>
		///     Gets the item source.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static ObservableCollection<ToolBar> GetItemSource( DependencyObject obj )
		{
			return null;
		}

		/// <summary>
		///     Called when the item source changes.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void OnItemSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			var tray = d as ToolBarTray;

			tray.ToolBars.Clear( );

			var toolBars = e.NewValue as ObservableCollection<ToolBar>;

			if ( toolBars != null )
			{
				foreach ( ToolBar toolBar in toolBars )
				{
					tray.ToolBars.Add( toolBar );
				}
			}
		}

		/// <summary>
		///     Sets the item source.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="source">The source.</param>
		public static void SetItemSource( DependencyObject obj, ObservableCollection<ToolBar> source )
		{
		}
	}
}