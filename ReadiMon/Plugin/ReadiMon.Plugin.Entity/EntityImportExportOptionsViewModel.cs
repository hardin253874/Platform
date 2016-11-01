// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Entity Import Export Options View Model.
	/// </summary>
	public class EntityImportExportOptionsViewModel : OptionsViewModelBase
	{
		/// <summary>
		///     The include comments
		/// </summary>
		private bool _includeComments;

		/// <summary>
		///     The include longs
		/// </summary>
		private bool _includeLongs;

		/// <summary>
		///     Gets or sets a value indicating whether [include comments].
		/// </summary>
		/// <value>
		///     <c>true</c> if [include comments]; otherwise, <c>false</c>.
		/// </value>
		public bool IncludeComments
		{
			get
			{
				return _includeComments;
			}
			set
			{
				SetProperty( ref _includeComments, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [include longs].
		/// </summary>
		/// <value>
		///     <c>true</c> if [include longs]; otherwise, <c>false</c>.
		/// </value>
		public bool IncludeLongs
		{
			get
			{
				return _includeLongs;
			}
			set
			{
				SetProperty( ref _includeLongs, value );
			}
		}

		/// <summary>
		///     Called when loading.
		/// </summary>
		public override void OnLoad( )
		{
			IncludeLongs = Settings.Default.IncludeLongs;
			IncludeComments = Settings.Default.IncludeComments;
		}

		/// <summary>
		///     Called when saving.
		/// </summary>
		public override void OnSave( )
		{
			Settings.Default.IncludeLongs = IncludeLongs;
			Settings.Default.IncludeComments = IncludeComments;
			Settings.Default.Save( );
		}
	}
}