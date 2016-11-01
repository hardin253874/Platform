// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Evaluation;
using ReadiNow.Expressions.Tree.Nodes;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Convenient container for an expression tree.
    /// </summary>
    public class Expression : IExpression
    {

        /// <summary>
        /// The root provider of enumerated entity contexts.
        /// </summary>
        public EntityNode ListRoot { get; internal set; }

        /// <summary>
        /// The root node of the complied tree.
        /// </summary>
        public ExpressionNode Root { get; internal set; }

        /// <summary>
        /// The type of result that this entity provides.
        /// </summary>
        public ExprType ResultType
        {
            get { return Root == null ? null : Root.ResultType; }
        }

        /// <summary>
        /// Implement interface explicitly. Required because of ExprType.
        /// </summary>
        ExprType IExpression.ResultType
        {
            get { return ResultType; }
        }

        /// <summary>
        /// Evaluates the result without regard to result type.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns>Null if the expression evaluates to null, otherwise the result in its natural type. E.g. a boxed Int32.</returns>
        internal object Evaluate(EvaluationContext evalContext)
        {
            // Important: while the declarations show nullable, when you cast a nullable to an object, the underlying object gets boxed and the
            // 'nullable' factor gets lost.

            // TODO: consider maybe setting to false when ResultType.IsList==true
            evalContext.EnsureDefaultRow = true;

            IEnumerable result;

            switch (ResultType.Type)
            {
                case DataType.String:
                case DataType.Xml:
                    result = EvaluateList(evalContext, Root.EvaluateString);
                    break;

                case DataType.Int32:
                    result = EvaluateList(evalContext, Root.EvaluateInt);
                    break;

                case DataType.Decimal:
                case DataType.Currency:
                    result = EvaluateList(evalContext, Root.EvaluateDecimal);
                    break;

                case DataType.Entity:
                    result = EvaluateList(evalContext, Root.EvaluateEntity);
                    break;

                case DataType.Bool:
                    result = EvaluateList(evalContext, Root.EvaluateBool);
                    break;

                case DataType.Date:
                    result = EvaluateList(evalContext, Root.EvaluateDate);
                    break;

                case DataType.Time:
                    result = EvaluateList(evalContext, Root.EvaluateTime);
                    break;

                case DataType.DateTime:
                    result = EvaluateList(evalContext, Root.EvaluateDateTime);
                    break;

                case DataType.Guid:
                    result = EvaluateList(evalContext, Root.EvaluateGuid);
                    break;

                case DataType.None:
                    if (Root is ConstantNode)
                        return null;
                    if (Root is ParameterNode)
                        return ((ParameterNode)Root).GetParameter<object>(evalContext);
                    throw new InvalidOperationException("No type information.");

                default:
                    throw new NotImplementedException(ResultType.Type.ToString());
            }

            if (ResultType.IsList)
            {
                // Return entire list
                return result;
            }
            // Return single value
            IEnumerator enumerator = result.GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;
            return null;
        }

        private IEnumerable<T> EvaluateList<T>(EvaluationContext evalContext, Func<EvaluationContext, T> evaluator)
        {
            var list = ListRoot.VisitItems(evalContext);
            foreach (var handle in list)
            {
                handle.Activate(evalContext);
                T result = evaluator(evalContext);
                yield return result;
            }
        }

        /// <summary>
        /// Render the expression to XML. Used for diagnostic purposes.
        /// </summary>
        /// <returns>XML string.</returns>
        public string ToXml( )
        {
            ExpressionXmlContext ctx = new ExpressionXmlContext( );
            XDocument xDoc = new XDocument(
                new XDeclaration( "1.0", "UTF-16", null ),
                GetXElement( ctx ) );

            using ( TextWriter tw = new StringWriter( ) )
            {
                xDoc.Save( tw );
                return tw.ToString( );
            }
        }

        /// <summary>
        /// Generate an XElement that represents this expression.
        /// </summary>
        /// <param name="context">XML generation context.</param>
        internal XElement GetXElement( ExpressionXmlContext context )
        {
            return context.CheckElement( this, ( ) =>
                 new XElement( "Expression",
                     new XAttribute( "resultType", ResultType.ToString( ) ),
                     new XElement( "Root", Root?.GetXElement( context ) ),
                     new XElement( "ListRoot", ListRoot?.GetXElement( context ) ) ) );
        }
    }
}
