// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Irony.Parsing;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Represents a node in an expression tree.
    /// This is not a parse tree node. This is the result of static evaluation.
    /// The static building process will have already performed name lookups and type casting, to ensure that nodes are always wired to their expected native input types.
    /// </summary>
    public abstract class ExpressionNode
    {
        /// <summary>
        /// The type of expression returned by this node.
        /// This information is available after static evaluation.
        /// </summary>
        public ExprType ResultType { get; set; }

        /// <summary>
        /// The type of expression returned by this node.
        /// This information is available after static evaluation.
        /// </summary>
        public ExprType InputType { get; set; }

        /// <summary>
        /// The read-only list of sub expressions that contribute to this expression.
        /// </summary>
        public abstract List<ExpressionNode> Arguments { get; }

        ///// <summary>
        ///// The entity list expression that would apply to this expression scope.
        ///// </summary>
        //public IListSourceNode FinalListSource { get; set; }

        #region Static Processing
        /// <summary>
        /// Set the token name.
        /// </summary>
        public virtual void SetToken(string token)
        {
        }

        /// <summary>
        /// Allow the node to perform any compile-time validation.
        /// </summary>
        /// <param name="settings">Settings for performing validation.</param>
        /// <param name="exceptionNode">Raise exceptions on this node.</param>
        public virtual void OnStaticValidation(BuilderSettings settings, ParseTreeNode exceptionNode)
        {
        }

        /// <summary>
        /// Setter for input arguments
        /// </summary>
        /// <param name="argNumber">Zero based index of arguments. Note that for member accessors, the zero argument is 'this'.</param>
        /// <param name="argument">The expression node that will evaluate the arugment.</param>
        public abstract void SetArgument(int argNumber, ExpressionNode argument);

        /// <summary>
        /// Give the node an opportunity to determine its own result type.
        /// </summary>
        public virtual void OnDetermineResultType(BuilderSettings settings)
        {
        }

        /// <summary>
        /// Give the node an opportunity to determine its own number of decimal places.
        /// </summary>
        /// <remarks>
        /// Run during static processing.
        /// </remarks>
        internal virtual int? OnDetermineDecimalPlaces(CompileContext context)
        {
            int? result = Arguments.Select(a => a.ResultType.DecimalPlaces).Max();
            return result;
        }

        /// <summary>
        /// Determines what entity node should be used if a context is requested against this expression.
        /// </summary>
        public virtual EntityNode DetermineContextNode()
        {
            if (this is EntityNode)
            {
                return (EntityNode)this;
            }
            else
            {
                var options = Arguments.Select(a => a.DetermineContextNode()).Distinct().Where(x => x != null).ToArray();
                if (options.Length == 1)
                    return options[0];
                return null;
            }
        }

        /// <summary>
        /// Implementors should add the entity ID of any entities they reference to the appropriate sets(s).
        /// Note: add everything to identifiedEntities.
        /// </summary>
        /// <param name="identifiedEntities"></param>
        /// <param name="fields"></param>
        /// <param name="relationships"></param>
        public virtual void RegisterDependencies(ISet<long> identifiedEntities, ISet<long> fields, ISet<long> relationships)
        {
        }

        /// <summary>
        /// Helper function for implementation of RegisterDependencies.
        /// </summary>
        /// <param name="child">A child expression.</param>
        /// <param name="collection">Dependency collection that the child should be added to, if it's a static entity.</param>
        protected void RegisterChildAs(ExpressionNode child, ISet<long> collection)
        {
            ConstantEntityNode constEntity = child as ConstantEntityNode;
            if (constEntity != null)
            {
                collection.Add(constEntity.Instance.Id);
            }
        }
        #endregion

        #region Virtual Evaluation Handlers

        /// <summary>
        /// This is for inheritors to override, not to call.
        /// Override this for functions/operators that accept any type, return the same type, and apply the same logic in each case.
        /// Use default(T) to return null. Use ==null to test for null. (And ignore the warning).
        /// </summary>
        /// <typeparam name="T">Type of data being processed.</typeparam>
        /// <param name="evalContext">Evaluation context.</param>
        /// <param name="childEvaluator">Strongly typed function that can be used to evaluate child nodes of the same type, whatever that is.</param>
        /// <returns>The result of the evaluation.</returns>
        protected virtual T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            string name = typeof(T).Name;
            if (name.StartsWith("Nullable"))
                name = typeof(T).GenericTypeArguments[0].Name;
            throw new InvalidOperationException("Cannot evaluate " + GetType().Name + " as " + name);
        }

        /// <summary>
        /// Evaluates a bool result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateBool(evalContext));
        }

        /// <summary>
        /// Evaluates an int result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual int? OnEvaluateInt(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateInt(evalContext));
        }

        /// <summary>
        /// Evaluates a string result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual string OnEvaluateString(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateString(evalContext));
        }

        /// <summary>
        /// Evaluates a decimal result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateDecimal(evalContext));
        }

        /// <summary>
        /// Evaluates a Date result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateDate(evalContext));
        }

        /// <summary>
        /// Evaluates a Time result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateTime(evalContext));
        }

        /// <summary>
        /// Evaluates a DateTime result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateDateTime(evalContext));
        }

        /// <summary>
        /// Evaluates a Guid result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual Guid? OnEvaluateGuid(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateGuid(evalContext));
        }

        /// <summary>
        /// Evaluates an entity result. This is for inheritors to override, not to call.
        /// </summary>
        protected virtual IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            return OnEvaluateGeneric(evalContext, node => node.EvaluateEntity(evalContext));
        }
        #endregion

        #region Evaluation methods
        internal bool? EvaluateBool(EvaluationContext evalContext)
        {
            bool? result = OnEvaluateBool(evalContext);

            // we no longer support null bools, so this converts all nulls to false
            return result == true; 
        }

        internal int? EvaluateInt(EvaluationContext evalContext)
        {
            return OnEvaluateInt(evalContext);
        }

        internal string EvaluateString(EvaluationContext evalContext)
        {
            return OnEvaluateString(evalContext);
        }

        internal decimal? EvaluateDecimal(EvaluationContext evalContext)
        {
            return OnEvaluateDecimal(evalContext);
        }

        internal DateTime? EvaluateDate(EvaluationContext evalContext)
        {
            DateTime? result = OnEvaluateDate(evalContext);
            return EnsureDateIsUtcKind(result);
        }

        internal DateTime? EvaluateTime(EvaluationContext evalContext)
        {
            DateTime? result = OnEvaluateTime(evalContext);
            return EnsureDateIsUtcKind(result);
        }

        internal DateTime? EvaluateDateTime(EvaluationContext evalContext)
        {
            DateTime? result = OnEvaluateDateTime(evalContext);
            return EnsureDateIsUtcKind(result);
        }

        internal IEntity EvaluateEntity(EvaluationContext evalContext)
        {
            return OnEvaluateEntity(evalContext);
        }

        internal Guid? EvaluateGuid(EvaluationContext evalContext)
        {
            return OnEvaluateGuid(evalContext);
        }

        /// <summary>
        /// Evaluates the result without regard to result type.
        /// Don't use this unless you have a really good reason.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns>Null if the expression evaluates to null, otherwise the result in its natural type. E.g. a boxed Int32.</returns>
        internal object EvaluateObject(EvaluationContext evalContext)
        {
            object result;

            switch (ResultType.Type)
            {
                case DataType.String:
                case DataType.Xml:
                    result = EvaluateString(evalContext);
                    break;

                case DataType.Int32:
                    result = EvaluateInt(evalContext);
                    break;

                case DataType.Decimal:
                case DataType.Currency:
                    result = EvaluateDecimal(evalContext);
                    break;

                case DataType.Entity:
                    result = EvaluateEntity(evalContext);
                    break;

                case DataType.Bool:
                    result = EvaluateBool(evalContext);
                    break;

                case DataType.Date:
                    result = EvaluateDate(evalContext);
                    break;

                case DataType.Time:
                    result = EvaluateTime(evalContext);
                    break;

                case DataType.DateTime:
                    result = EvaluateDateTime(evalContext);
                    break;

                case DataType.Guid:
                    result = EvaluateGuid(evalContext);
                    break;

                case DataType.None:
                    if (this is ConstantNode)
                        result = null;
                    else if (this is ParameterNode)
                        result = ((ParameterNode)this).GetParameter<object>(evalContext);
                    else 
                        throw new InvalidOperationException("No type information.");
                    break;

                default:
                    throw new NotImplementedException(ResultType.Type.ToString());
            }
            return result;
        }

        /// <summary>
        /// Ensure that 'unspecified' kinds are recognised as UTC to avoid unexpected timezone shifting.
        /// </summary>
        private static DateTime? EnsureDateIsUtcKind(DateTime? value)
        {
            DateTime? result = value;

            // Note: if we encounter a DateTimeKind.Local, then there's probably some horrible UTC bug somewhere.
            if (value != null && value.Value.Kind == DateTimeKind.Unspecified)
            {
                result = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
            }
            return result;
        }

        #endregion

        #region Static Evaluation

        /// <summary>
        /// Evaluates an int statically.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual int? OnStaticEvaluateInt(CompileContext context)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Evaluates an int statically.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal int? StaticEvaluateInt(CompileContext context)
        {
            return OnStaticEvaluateInt(context);
        }
        #endregion

        #region Query Builder methods

        /// <summary>
        /// Convert the expression tree into a structured query expression.
        /// </summary>
        /// <param name="context">Context information about this query building session, including the target structured query object.</param>
        /// <returns>A scalar expression that can be used within the query.</returns>
        public ScalarExpression BuildQuery(QueryBuilderContext context)
        {
            ScalarExpression result = OnBuildQuery(context);

            //if (!context.ExpressionCache.TryGetValue(this, out result))
            //{
            //    result = OnBuildQuery(context);
            //    context.ExpressionCache[this] = result;
            //}
            return result;
        }

        /// <summary>
        /// Convert the expression tree into a structured query expression.
        /// </summary>
        /// <param name="context">Context information about this query building session, including the target structured query object.</param>
        /// <returns>A scalar expression that can be used within the query.</returns>
        protected virtual ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            // Reflect for attributes
            Type type = GetType();
            var queryEngAttribute = GetQueryEngineOperator(type);

            if (queryEngAttribute == null)
            {
                throw new NotImplementedException(type.Name);
            }

            // Build arguments
            var arguments = new List<ScalarExpression>();
            foreach (ExpressionNode argument in Arguments)
            {
                ScalarExpression queryExpr = argument.BuildQuery(context);
                arguments.Add(queryExpr);
            }

            // Generic Calculation Expression
            if (queryEngAttribute.CalculationOperator != null)
            {
                var result = new CalculationExpression();
                result.Operator = queryEngAttribute.CalculationOperator.Value;
                result.Expressions = arguments;
                return result;
            }

            // Generic Comparison Expression
            if (queryEngAttribute.ComparisonOperator != null)
            {
                var result = new ComparisonExpression();
                result.Operator = queryEngAttribute.ComparisonOperator.Value;
                result.Expressions = arguments;
                return result;
            }

            // Generic Comparison Expression
            if (queryEngAttribute.LogicalOperator != null)
            {
                var result = new LogicalExpression();
                result.Operator = queryEngAttribute.LogicalOperator.Value;
                result.Expressions = arguments;
                return result;
            }

            throw new InvalidOperationException(type.Name);
        }


        /// <summary>
        /// Reflect for the QueryEngineOperator attribute.
        /// </summary>
        private static QueryEngineAttribute GetQueryEngineOperator(Type type)
        {
            lock (_calculationOperator)
            {
                QueryEngineAttribute result;
                if (!_calculationOperator.TryGetValue(type, out result))
                {
                    object[] attributes = type.GetCustomAttributes(typeof(QueryEngineAttribute), false);
                    if (attributes.Length > 0)
                    {
                        result = (QueryEngineAttribute)attributes[0];
                    }
                    _calculationOperator[type] = result;
                }
                return result;
                
            }
        }
        static readonly Dictionary<Type, QueryEngineAttribute> _calculationOperator = new Dictionary<Type, QueryEngineAttribute>();

        #endregion

        #region XML methods

        /// <summary>
        /// Generate XML that represents this expression node, and children. Used for diagnostics.
        /// </summary>
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
        /// Return an XElement for XML generation.
        /// </summary>
        /// <param name="context">XML generation context.</param>
        internal XElement GetXElement( ExpressionXmlContext context )
        {
            var res = context.CheckElement( this, ( ) =>
                new XElement( GetType( ).Name,
                    new XAttribute( "resultType", ResultType?.ToString( ) ?? "null" ),
                    context.CreateList( "Arguments", Arguments, n => n.GetXElement( context ) ) ) );

            if ( res.Name == "Ref" )
                return res;

            if ( InputType != null )
                res.Add( new XAttribute( "inputType", InputType ) );

            OnGetXElement( res, context );
            return res;
        }

        /// <summary>
        /// Override for class-specific XML generation.
        /// </summary>
        /// <param name="element">The element that represents this node, to which attributes and children should be added.</param>
        /// <param name="context">XML generation context.</param>
        protected virtual void OnGetXElement( XElement element, ExpressionXmlContext context )
        {
        }
        #endregion

    }

}
