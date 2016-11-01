// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Data.SqlClient;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	public class TenantMergeTargetExecutionArguments<TEntry> : ExecutionArguments<TEntry>
	{
		/// <summary>
		///     Gets or sets the get column mappings.
		/// </summary>
		/// <value>
		///     The get column mappings.
		/// </value>
		public Func<SqlBulkCopyColumnMapping[ ]> GetColumnMappings
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the get missing dependencies column mappings.
		/// </summary>
		/// <value>
		///     The get missing dependencies column mappings.
		/// </value>
		public Func<SqlBulkCopyColumnMapping[ ]> GetMissingDependenciesColumnMappings
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the get missing dependencies columns.
		/// </summary>
		/// <value>
		///     The get missing dependencies columns.
		/// </value>
		public Func<DataColumn[ ]> GetMissingDependenciesColumns
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the missing data command text.
		/// </summary>
		/// <value>
		///     The missing data command text.
		/// </value>
		public string MissingDataCommandText
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the missing dependencies table.
		/// </summary>
		/// <value>
		///     The name of the missing dependencies table.
		/// </value>
		public string MissingDependenciesTableName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the missing dependency action.
		/// </summary>
		/// <value>
		///     The missing dependency action.
		/// </value>
		public Action<TEntry, PopulateRowResult> MissingDependencyAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the populate missing dependencies action.
		/// </summary>
		/// <value>
		///     The populate missing dependencies action.
		/// </value>
		public Func<TEntry, DataRow, PopulateRowResult> PopulateMissingDependenciesAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [record missing dependencies].
		/// </summary>
		/// <value>
		///     <c>true</c> if [record missing dependencies]; otherwise, <c>false</c>.
		/// </value>
		public bool RecordMissingDependencies
		{
			get;
			set;
		}
	}
}