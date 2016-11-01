// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Windows.Controls;

namespace ApplicationManager.Controls
{
	/// <summary>
	///     List view that does not allow selection.
	/// </summary>
	public class NonSelectableListView : ListView
	{
		/// <summary>
		///     Responds to a list box selection change by raising a
		///     <see
		///         cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" />
		///     event.
		/// </summary>
		/// <param name="e">
		///     Provides data for <see cref="T:System.Windows.Controls.SelectionChangedEventArgs" />.
		/// </param>
		protected override void OnSelectionChanged( SelectionChangedEventArgs e )
		{
			base.OnSelectionChanged( e );

			SelectedIndex = -1;
		}
	}
}