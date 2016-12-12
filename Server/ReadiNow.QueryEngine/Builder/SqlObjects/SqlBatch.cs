// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Common;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summery>
	///     Represents a complete SQL select batch, including any temporary tables.
	///     This is a transient class only; used when generating SQL from a resource query.
	/// </summery>
	internal class SqlBatch
	{
		public SqlBatch( )
		{
			Statements = new List<SqlSelectStatement>( );
			SharedParameters = new Dictionary<ParameterValue, string>( );
		}

		/// <summary>
		///     Raw SQL to render at the start
		/// </summary>
		public string SqlPreamble
		{
			get;
			set;
		}

		/// <summary>
		///		Gets or sets the SQL postamble.
		/// </summary>
		public string SqlPostamble
		{
			get;
			set;
		}

		/// <summary>
		///		Gets or sets the shared parameters.
		/// </summary>
		/// <value>
		/// The shared parameters.
		/// </value>
		public IDictionary<ParameterValue, string> SharedParameters
		{
			get;
			set;
		}

		/// <summary>
		///     The list of statements to be rendered.
		/// </summary>
		public List<SqlSelectStatement> Statements
		{
			get;
			set;
		}


		/// <summary>
		///     Generates the SQL text for the whole query.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		public void RenderSql( SqlBuilderContext sb )
		{
			var first = new First( );
			if ( !string.IsNullOrEmpty( SqlPreamble ) )
			{
				sb.Append( SqlPreamble );
				first.Value = false;
			}

			foreach ( SqlSelectStatement statement in Statements )
			{
				if ( !first )
				{
					sb.AppendOnNewLine( "" );
				}

				statement.RenderSql( sb );
			}

			if ( !string.IsNullOrEmpty( SqlPostamble ) )
			{
				if ( !first )
				{
					sb.AppendOnNewLine( "" );
				}

				sb.Append( SqlPostamble );
			}
		}
	}
}