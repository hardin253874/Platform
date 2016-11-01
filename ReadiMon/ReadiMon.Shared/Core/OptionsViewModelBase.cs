// Copyright 2011-2015 Global Software Innovation Pty Ltd

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Options ViewModel Base
	/// </summary>
	public class OptionsViewModelBase : ViewModelBase
	{
		/// <summary>
		///     Called when loading.
		/// </summary>
		public virtual void OnLoad( )
		{
		}

		/// <summary>
		///     Called when saving.
		/// </summary>
		public virtual void OnSave( )
		{
		}
	}
}