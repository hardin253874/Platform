// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents the from clause of a SQL statement.
	///     This is a transient class only; used when generating SQL from a query.
	///     Note: only the root table needs to be directly held.
	/// </summary>
	internal class SqlFromClause
	{
		public SqlFromClause( )
		{
			ConstrainInWhereClause = new List<SqlTable>( );
		}

		/// <summary>
		///     Tables that must be constrained in the where clause, rather than their own 'ON' clause.
		/// </summary>
		public List<SqlTable> ConstrainInWhereClause
		{
			get;
			set;
		}

		/// <summary>
		///     The starting table from which all others are (directly or indirectly) joined.
		/// </summary>
		public SqlTable RootTable
		{
			get;
			set;
		}

		/// <summary>
		///     Generates the SQL text for the FROM clause.
		/// </summary>
		/// <param name="sb">The SQL text formatter.</param>
		public void RenderSql( SqlBuilderContext sb )
		{
			if ( RootTable == null )
			{
				throw new InvalidOperationException( "Cannot build SQL unless a root table has been specified." );
			}

			// Prepare join types
			PrepareJoinsRecursively( RootTable );

			// Render from
			sb.AppendOnNewLine( "from" );
			sb.Indent( );
			sb.StartNewLine( );
			RenderTableNameOrSubquery( RootTable, sb );
			RenderFromRecursive( RootTable, sb );
			sb.EndIndent( );
		}

		/// <summary>
		///     Converts a JoinType to SQL text.
		/// </summary>
		/// <param name="join">The join.</param>
		/// <param name="useApply">true if this is a table-function join that should use the apply clause.</param>
		/// <returns>SQL join keyword</returns>
		private static string GetJoinString( JoinType join, bool useApply )
		{
			switch ( join )
			{
					// Sometimes merge is better than hash,
					// but more importantly, we must make sure that it never chooses loop
				case JoinType.Inner:
					return useApply ? "cross apply" : "join";
				case JoinType.Left:
					return useApply ? "outer apply" : "left join";
				default:
					throw new InvalidOperationException( join.ToString( ) );
			}
		}


		/// <summary>
		///     Determine the exact join type required for each table by recursively examining the join hints.
		/// </summary>
		/// <param name="table">The table currently being examined.</param>
		private static void PrepareJoinsRecursively( SqlTable table )
		{
			// Move sub query based joins out of the Children table now;
			table.SubqueryChildren = table.Children.Where( t => t.JoinHint == JoinHint.ConstrainWithExists || t.JoinHint == JoinHint.NotExists ).ToList( );
			table.Children.RemoveAll( t => t.JoinHint == JoinHint.ConstrainWithExists || t.JoinHint == JoinHint.NotExists );

			// Examine children recursively.
			foreach ( SqlTable child in table.Children )
			{
				PrepareJoinsRecursively( child );
			}

			// Default leaf nodes to 'Optional'
			if ( table.Children.Count == 0 && table.JoinHint == JoinHint.Unspecified )
			{
				table.JoinHint = JoinHint.Optional;
			}

			if ( table.Parent != null )
			{
				// 'Optional' must get shifted up past any 'Unspecified' nodes, so that the left join applies as close to the top as possible.
				if ( table.JoinHint == JoinHint.Optional )
				{
                    if (table.Parent.JoinHint == JoinHint.Unspecified && table.Parent.Parent != null)
					{
						table.Parent.JoinHint = JoinHint.Optional;
						// If this is the only child, then the parent's left join will be adequate, so mark this as unspecified.
						if ( table.Parent.Children.Count == 1 )
						{
							table.JoinHint = JoinHint.Unspecified;
						}
					}
				}
				// 'Required' gets propagated up to root.
				if ( table.JoinHint == JoinHint.Required )
				{
				    if (table.Parent.JoinHint != JoinHint.DontConstrainParent)
				    {
				        table.Parent.JoinHint = JoinHint.Required;
				    }
				}
			}

            // Once hints have been adjusted, 'Optional' and 'DontConstrainParent' require a left. Others can use inner.
		    if (table.JoinHint == JoinHint.Optional || table.JoinHint == JoinHint.DontConstrainParent)
		        table.JoinType = JoinType.Left;
		    else
                table.JoinType = JoinType.Inner;
		}


        /// <summary>
        ///     Recursively steps through joined tables, rendering their join SQL.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="renderContext">The render context.</param>
        /// <param name="sb">The sb.</param>
        private static void RenderFromRecursive( SqlTable table, SqlBuilderContext sb )
		{
			// Order by join type so that inner joins appear before left joins
			// to ensure that inner joins don't coerce left joins into misbehaving.
			IOrderedEnumerable<SqlTable> orderedChildren = table.Children.OrderBy( t => t.JoinType == JoinType.Inner ? 0 : 1 );

			foreach ( SqlTable child in orderedChildren )
			{
				string joinString = GetJoinString( child.JoinType, child.ApplyTableFunction );
				sb.AppendOnNewLine( joinString );
				sb.Append( " " );

				// A join can be rendered inline so long as it is not a left join with inner joined children
				// (as the inner joins will artificially constrain the left join unless they are nested in brackets)
                // And as long as it has no special conditions, as they may refer to child nodes.
				bool nest = child.JoinType == JoinType.Left && child.Children.Any( grandChild => grandChild.JoinType == JoinType.Inner );

				if ( child.HasCustomConditions && child.Children.Count > 0 )
                {
                    nest = true;
                }
                
				if ( nest )
				{
					sb.Append( "(" );
					sb.Indent( );
					sb.StartNewLine( );

					RenderTableNameOrSubquery( child, sb );
					RenderFromRecursive( child, sb );

					sb.EndIndent( );
					sb.AppendOnNewLine( ") " );
				}
				else
				{
					RenderTableNameOrSubquery( child, sb );
					sb.Append( " " );
				}

				// Apply standard conditions
				child.PrepareTableConditions( );

				// Render 'ON' clause conditions
				if ( child.ApplyTableFunction )
				{
					child.ParentQuery.FromClause.ConstrainInWhereClause.Add( child );
				}
				else
				{
                    // Apply join condition
                    sb.AddJoinCondition( child, table, child.JoinColumn, child.ForeignColumn );

					child.RenderTableConditions( "on", true, sb );
				}

				if ( !nest )
				{
					RenderFromRecursive( child, sb );
				}
			}
		}


		/// <summary>
		///     Renders a table's name and alias for the FROM clause.
		///     Or render the entire sub query if the table represents a sub query.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="sb"></param>
		private static void RenderTableNameOrSubquery( SqlTable table, SqlBuilderContext sb )
		{
			if ( table.SubQuery != null )
			{
				// Table is sub query
				sb.Append( "(" );
				sb.Indent( );
				table.SubQuery.RenderSql( sb );
				sb.EndIndent( );
				sb.AppendOnNewLine( ")" );
			}
			else
			{
				// Table name
				sb.Append( table.NameContainsSql ? table.Name : SqlBuilder.EscapeSqlIdentifier( table.Name ) );
			}

			// Render alias
			// Note: Table aliases are assumed to not require escaping or delimiting
			// as they are generated by the alias manager
			if ( !string.IsNullOrEmpty( table.TableAlias ) )
			{
				sb.Append( " " );
				sb.Append( table.TableAlias );
			}
		}
	}
}