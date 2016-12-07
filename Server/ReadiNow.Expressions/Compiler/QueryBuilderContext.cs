// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Expressions.Tree.Nodes;
using EDC.ReadiNow.Expressions;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Context information that is passed around while building a structured query.
    /// </summary>
    public class QueryBuilderContext
    {
        public QueryBuilderContext( )
        {
            NodeCache = new Dictionary<ExpressionNode, SQ.Entity>( );
            ParentNodeStack = new Stack<SQ.Entity>( );
        }

        /// <summary>
        /// Settings that are passed into the expression engine when building a structured query.
        /// </summary>
        public QueryBuilderSettings Settings { get; set; }

        /// <summary>
        /// Local map of expression nodes to query nodes.
        /// </summary>
        public Dictionary<ExpressionNode, SQ.Entity> NodeCache { get; }

        public SQ.Entity ParentNode
        {
            get { return ParentNodeStack.Peek( ); }
        }

        public Stack<SQ.Entity> ParentNodeStack { get; private set; }

        public SQ.Entity GetNode( ExpressionNode expression )
        {
            EntityNode entityNode = ( ( EntityNode ) expression ).GetQueryNode( );
            return NodeCache [ entityNode ];
        }

        /// <summary>
        /// Resolves a parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns></returns>
        public SQ.ResourceEntity ResolveParameterNode( string parameterName )
        {
            if ( _parameterNodes == null )
                _parameterNodes = new Dictionary<string, SQ.ResourceEntity>( );

            // Cache the parameter result
            SQ.ResourceEntity result;
            if ( !_parameterNodes.TryGetValue( parameterName, out result ) )
            {
                if ( Settings == null )
                    throw new InvalidOperationException( "Settings were not set." );
                if ( Settings.ParameterNodeResolver == null )
                    throw new InvalidOperationException( "ParameterNodeResolver was not set." );

                result = Settings.ParameterNodeResolver( parameterName );

                _parameterNodes[ parameterName ] = result;
            }

            return result;
        }

        private Dictionary<string, SQ.ResourceEntity> _parameterNodes;
    }
}
