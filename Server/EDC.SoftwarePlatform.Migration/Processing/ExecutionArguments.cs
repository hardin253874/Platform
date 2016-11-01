// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Execution argument container.
	/// </summary>
	/// <typeparam name="TEntry">The type of the entry.</typeparam>
	public class ExecutionArguments<TEntry>
	{
		/// <summary>
		///     Gets or sets the entries.
		/// </summary>
		/// <value>
		///     The entries.
		/// </value>
		public IEnumerable<TEntry> Entries
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the get columns action.
		/// </summary>
		/// <value>
		///     The get columns action.
		/// </value>
		public Func<DataColumn[ ]> GetColumnsAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the table.
		/// </summary>
		/// <value>
		///     The name of the table.
		/// </value>
		public string TableName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		public IProcessingContext Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the populate row action.
		/// </summary>
		/// <value>
		///     The populate row action.
		/// </value>
		public Func<TEntry, DataRow, PopulateRowResult> PopulateRowAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the debug callback.
		/// </summary>
		/// <value>
		///     The debug callback.
		/// </value>
		public Func<TEntry, bool> DebugCallback
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the command text.
		/// </summary>
		/// <value>
		///     The command text.
		/// </value>
		public string CommandText
		{
			get;
			set;
		}


	    /// <summary>
	    /// True to skip the command execution
	    /// </summary>
	    public bool SkipCommandExec
	    {
	        get;
            set;
	    }

		/// <summary>
		///     Gets or sets the execute action.
		/// </summary>
		/// <value>
		///     The execute action.
		/// </value>
		public ExecuteAction ExecuteAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the setup command action.
		/// </summary>
		/// <value>
		///     The setup command action.
		/// </value>
		public Action<IDbCommand> SetupCommandAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the custom command execute action.
		/// </summary>
		/// <value>
		///     The custom command execute action.
		/// </value>
		public Func<IDbCommand, int> CustomCommandExecuteAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the set copied count action.
		/// </summary>
		/// <value>
		///     The set copied count action.
		/// </value>
		public Action<int> SetCopiedCountAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the set execute count action.
		/// </summary>
		/// <value>
		///     The set execute count action.
		/// </value>
		public Action<int> SetExecuteCountAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the set dropped count action.
		/// </summary>
		/// <value>
		///     The set dropped count action.
		/// </value>
		public Action<int, string> SetDroppedCountAction
		{
			get;
			set;
		}
	}
}