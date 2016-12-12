// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents the WITH clause of a SQL statement.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlWithClause
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlWithClause" /> class.
		/// </summary>
		public SqlWithClause( )
		{
			Items = new List<SqlCte>( );
		}


		/// <summary>
		///     The unordered list of CTEs
		/// </summary>
		public List<SqlCte> Items
		{
			get;
		}


		/// <summary>
		///     Directly adds raw SQL.
		/// </summary>
		/// <param name="sql">The SQL for the CTE, including the CTE name and enclosing brackets.</param>
		public SqlCte AddRawCte( string sql )
		{
			var cte = new SqlCte
				{
					RawSql = sql
				};
			Items.Add( cte );
			return cte;
		}


		/// <summary>
		///     Writes the select clause of the query.
		/// </summary>
		/// <param name="sb"></param>
		public void RenderSql( SqlBuilderContext sb )
		{
			if ( Items == null || Items.Count == 0 )
			{
				return;
			}

			sb.AppendOnNewLine( ";with" );

			var first = new First( );
			foreach ( SqlCte cte in OrderCtesByDependencies( ) )
			{
				if ( !first )
				{
					sb.Append( "," );
				}

				if ( !string.IsNullOrEmpty( cte.RawSql ) )
				{
					sb.AppendOnNewLine( cte.RawSql );
				}
				else
				{
					sb.AppendOnNewLine( SqlBuilder.EscapeSqlIdentifier( cte.Name ) );
					sb.Append( " as" );
					sb.AppendOnNewLine( "(" );
					sb.Indent( );
					cte.Union.RenderSql( sb );
					sb.EndIndent( );
					sb.AppendOnNewLine( ")" );
				}
			}
            sb.AppendOnNewLine( "" );
            sb.AppendOnNewLine( "--Main query" );
		}

		/// <summary>
		///     Looks up a registered CTE by its reuse key.
		/// </summary>
		/// <returns>The CTE, or null if not found</returns>
		internal SqlCte FindByKey( string key )
		{
			return Items.FirstOrDefault( cte => cte.Key == key );
		}

		/// <summary>
		///     Reorders the CTEs so that every CTE appears after CTEs that it depends on.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<SqlCte> OrderCtesByDependencies( )
		{
			var done = new List<SqlCte>( );
			var todo = new List<SqlCte>( Items );
			while ( todo.Count > 0 )
			{
				bool progress = false;
				foreach ( SqlCte cte in todo )
				{
					if ( cte.DependsOn.Except( done ).Any( ) )
					{
						continue;
					}
					progress = true;
					done.Add( cte );
					todo.Remove( cte );
					yield return cte;
					break;
				}
				if ( !progress )
				{
					throw new Exception( "Circular dependencies detected." );
				}
			}
		}
	}
}