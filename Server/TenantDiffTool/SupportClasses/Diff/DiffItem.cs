// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Media;
using TenantDiffTool.Core;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     Diff Item
	/// </summary>
	public class DiffItem : ViewModelBase
	{
		/// <summary>
		///     Gets or sets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		public string Data
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the row background.
		/// </summary>
		/// <value>
		///     The row background.
		/// </value>
		public SolidColorBrush RowBackground
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the source.
		/// </summary>
		/// <value>
		///     The source.
		/// </value>
		public DiffBase Source
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tool tip.
		/// </summary>
		/// <value>
		///     The tool tip.
		/// </value>
		public string ToolTip
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public string Type
		{
			get;
			set;
		}
	}
}