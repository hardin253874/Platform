// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Common;
using ReadiNow.Annotations;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
    /// <summary>
    /// Keeps track and renders SQL clauses and conjuction keywords.
    /// For example, decides if we need to render "where", "on", or "and" when rendering conditions.
    /// </summary>
    /// <remarks>
    /// This is a transient object that is only used internally during the render SQL phase.
    /// </remarks> 
    class ConjunctionTracker
    {
        private readonly string _introClause;
        private readonly string _conjunction;
        private readonly bool _inline;
        private readonly First _first;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sqlIntroClause">Introductory clause, such as "where" or "on"</param>
        /// <param name="sqlConjunction">Conjunction clause, such as "and"</param>
        /// <param name="inline">True if the conjunction should be rendered inline.</param>
        public ConjunctionTracker( [NotNull] string sqlIntroClause, [NotNull] string sqlConjunction, bool inline )
        {
            _introClause = sqlIntroClause;
            _conjunction = sqlConjunction;
            _inline = inline;
            _first = new First( );
        }

        /// <summary>
        /// Render a single conjunction (e.g.  where/on/and)
        /// </summary>
        /// <param name="sb">Sql Builder</param>
        public void RenderSql( SqlBuilderContext sb )
        {
            if ( _first )
            {
                if ( _inline )
                {
                    sb.Append( _introClause );
                    sb.Append( " " );
                }
                else
                {
                    sb.AppendOnNewLine( _introClause );
                    sb.Indent( );
                    sb.StartNewLine( );
                }
            }
            else
            {
                if ( _inline )
                {
                    sb.Append( " " );
                    sb.Append( _conjunction );
                    sb.Append( " " );
                }
                else
                {
                    sb.AppendOnNewLine( _conjunction );
                    sb.Append( " " );
                }
            }

        }

        /// <summary>
        /// Has anything been rendered yet?
        /// </summary>
        public bool AnyRendered => !_first.Peek;

        /// <summary>
        /// Finalise any processing.
        /// </summary>
        /// <param name="sb">Sql Builder</param>
        public void FinishSql( SqlBuilderContext sb )
        {
            if ( AnyRendered && !_inline )
                sb.EndIndent( );
        }
    }
}
