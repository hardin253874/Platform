// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.Services.Console
{
	/// <summary>
	///     The console context which an action item request is being made under.
	/// </summary>
	[DataContract]
	public enum ActionContext
	{
		/// <summary>
		///     Actions will be presented in the right-click context menu on selection.
		/// </summary>
		[EnumMember( Value = "contextmenu" )]
		ContextMenu,

		/// <summary>
		///     Actions are to be presented from a dropdown in the context of a page or report.
		/// </summary>
		[EnumMember( Value = "actionsmenu" )]
		ActionsMenu,

		/// <summary>
		///     Actions that create a new type and are to be activated from a separate special menu.
		/// </summary>
		[EnumMember( Value = "quickmenu" )]
		QuickMenu,

		/// <summary>
		///     Returns all actions with no regard to context.
		/// </summary>
		[EnumMember( Value = "all" )]
		All
	}
}