// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Common;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Type of union.
	/// </summary>
	internal enum SqlUnionType
	{
		/// <summary>
		///     Only distinct rows are returned.
		/// </summary>
		Union,

		/// <summary>
		///     Duplicate rows are returned.
		/// </summary>
		UnionAll,

		/// <summary>
		///     Return first query except second query.
		/// </summary>
		Except
	}

	/// <summary>
	///     Represents a union of multiple query objects.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlUnion
	{
		public SqlUnion( )
		{
			Queries = new List<SqlQuery>( );
			UnionType = SqlUnionType.UnionAll;
		}

		public SqlUnion( SqlQuery query )
			: this( )
		{
			Queries.Add( query );
		}

		/// <summary>
		///     List of queries being unioned.
		///     Can be a list of one, in which case no union is performed, but the single query is still rendered.
		/// </summary>
		public List<SqlQuery> Queries
		{
			get;
			set;
		}

		/// <summary>
		///     The type of union being performed
		/// </summary>
		public SqlUnionType UnionType
		{
			get;
			set;
		}

		/// <summary>
		///     Writes the select clause of the query.
		/// </summary>
		/// <param name="sb"></param>
		public void RenderSql( SqlBuilderContext sb )
		{
			var first = new First( );

			// Render each query
			foreach ( SqlQuery query in Queries )
			{
				if ( !first )
				{
					// Write the union statement
					switch ( UnionType )
					{
						case SqlUnionType.Union:
							sb.AppendOnNewLine( "union" );
							break;
						case SqlUnionType.Except:
							sb.AppendOnNewLine( "except" );
							break;
						case SqlUnionType.UnionAll:
							sb.AppendOnNewLine( "union all" );
							break;
					}
				}

				query.RenderSql( sb );
			}
		}
	}
}