// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     History Entry.
	/// </summary>
	public class HistoryEntry
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="HistoryEntry" /> class.
		/// </summary>
		/// <param name="selectedText">The selected text.</param>
		/// <param name="selectedTenant">The selected tenant.</param>
		public HistoryEntry( string selectedText, TenantInfo selectedTenant )
		{
			SelectedText = selectedText;
			SelectedTenant = selectedTenant;
		}

		/// <summary>
		///     Gets or sets the selected tenant.
		/// </summary>
		/// <value>
		///     The selected tenant.
		/// </value>
		public TenantInfo SelectedTenant
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selected text.
		/// </summary>
		/// <value>
		///     The selected text.
		/// </value>
		public string SelectedText
		{
			get;
			set;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var historyEntry = obj as HistoryEntry;

			if ( historyEntry == null )
			{
				return false;
			}

			return SelectedText == historyEntry.SelectedText &&
			       SelectedTenant == historyEntry.SelectedTenant;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hash = 13;

			if ( SelectedTenant != null )
			{
				hash = ( hash * 7 ) + SelectedTenant.GetHashCode( );
			}

			if ( SelectedText != null )
			{
				hash = ( hash * 7 ) + SelectedText.GetHashCode( );
			}

			return hash;
		}
	}
}