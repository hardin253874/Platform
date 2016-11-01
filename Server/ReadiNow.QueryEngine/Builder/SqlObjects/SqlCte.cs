// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents a common-table-expression in a SQL statement.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlCte
	{
		public SqlCte( )
		{
			DependsOn = new List<SqlCte>( );
		}

		/// <summary>
		///     List of other CTEs that must be rendered before this one.
		/// </summary>
		public List<SqlCte> DependsOn
		{
			get;
			private set;
		}

		/// <summary>
		///     A key that is used to determine if the CTE can be reused.
		/// </summary>
		public string Key
		{
			get;
			set;
		}

		/// <summary>
		///     Name that gets assigned to the CTE
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     The single query, if there is only one.
		/// </summary>
		public SqlQuery Query
		{
			get
			{
				if ( Union == null )
				{
					Union = new SqlUnion( );
				}
				if ( Union.Queries.Count > 1 )
				{
					throw new InvalidOperationException( "CTE contains multiple queries. Cannot access as though there is only one." );
				}
				return Union.Queries.FirstOrDefault( );
			}
			set
			{
				if ( Union == null )
				{
					Union = new SqlUnion( );
				}
				if ( Union.Queries.Count > 1 )
				{
					throw new InvalidOperationException( "CTE contains multiple queries. Cannot access as though there is only one." );
				}
				Union.Queries.Clear( );
				Union.Queries.Add( value );
			}
		}

		/// <summary>
		///     Raw SQL content.
		/// </summary>
		public string RawSql
		{
			get;
			set;
		}

		/// <summary>
		///     The query (or union of queries) that need to be rendered in this CTE.
		/// </summary>
		public SqlUnion Union
		{
			get;
			set;
		}
	}
}