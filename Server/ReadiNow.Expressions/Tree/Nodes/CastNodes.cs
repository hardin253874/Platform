// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using EDC.ReadiNow.Metadata;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Casts a value from an unknown type to any type.
    /// Used for untyped nulls. (Maybe also for untyped parameters one day).
    /// </summary>
    abstract class CastNode : UnaryOperatorNode
    {
        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            return default(T);
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            DatabaseType dbType = DataTypeHelper.ToDatabaseType(ResultType.Type);

            ScalarExpression argument = Argument.BuildQuery(context);
            var result = new CalculationExpression
            {
                Expressions = new List<ScalarExpression>
                {
                    argument
                },
                Operator = CalculationOperator.Cast,
                CastType = dbType,
                DisplayType = dbType,
                InputType = Argument.ResultType.Type
            };
            return result;
        }
    }


    /// <summary>
    /// Casts a value from an unknown type to any type.
    /// Used for untyped nulls. (Maybe also for untyped parameters one day).
    /// </summary>
    class CastFromNone : CastNode
    {
        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            return default(T);
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            // In SQL, return 'false' for null literals.
            if (ResultType.Type == DataType.Bool)
            {
                TypedValue falseValue = new TypedValue();
                falseValue.Type = DatabaseType.BoolType;
                falseValue.Value = false;
                return new LiteralExpression { Value = falseValue };
            }
            return base.OnBuildQuery(context);
        }
    }


    /// <summary>
    /// Casts a value from an Int32 to various types.
    /// </summary>
    class CastFromInt : CastNode
    {
        static readonly string Format = DatabaseTypeHelper.GetDisplayFormatString(DatabaseType.DecimalType);

        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            int? value = Argument.EvaluateInt(evalContext);
            if (value == null)
                return null;
            return value != 0;
        }

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            int? value = Argument.EvaluateInt(evalContext);

            if (value == null)
                return null;
            string result = string.Format(evalContext.Culture.NumberFormat, Format, value.Value);
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            int? value = Argument.EvaluateInt(evalContext);
            return value;
        }
    }


    /// <summary>
    /// Casts a value from a decimal to various types.
    /// </summary>
    class CastFromDecimal : CastNode
    {
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            decimal? value = Argument.EvaluateDecimal(evalContext);
            if (value == null)
                return null;
            return value != 0M;
        }

        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            decimal? value = Argument.EvaluateDecimal(evalContext);
            return (int?)value;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            // A faux cast node currently appears for casts between decimal/currency. We don't need it yet, but might.
            decimal? value = Argument.EvaluateDecimal(evalContext);
            return value;
        }

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            int? dp = Argument.ResultType.DecimalPlaces;
            string format = "N" + (dp ?? 3);

            decimal? value = Argument.EvaluateDecimal(evalContext);

            if (value == null)
                return null;
            string result = value.Value.ToString(format);
            return result;
        }
    }


    /// <summary>
    /// Casts a value from a string to various types.
    /// </summary>
    class CastFromString : CastNode
    {
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            if (value == null)
                return null;
            if (string.Compare(value, "true", true, CultureInfo.InvariantCulture) == 0
                || string.Compare(value, "yes", true, CultureInfo.InvariantCulture) == 0)
                return true;
            if (string.Compare(value, "false", true, CultureInfo.InvariantCulture) == 0
                || string.Compare(value, "no", true, CultureInfo.InvariantCulture) == 0)
                return false;
            return null;
        }

        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            int result;
            if (int.TryParse(value, NumberStyles.Integer, evalContext.Culture.NumberFormat, out result))
                return result;
            return null;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            decimal result;
            if (decimal.TryParse(value, NumberStyles.Float, evalContext.Culture.NumberFormat, out result))
                return result;
            return null;
        }

        protected override DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            DateTime result;
            if (DateTime.TryParse(value, evalContext.Culture.DateTimeFormat, DateTimeStyles.None, out result))
                return result;
            return null;
        }

        protected override DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            DateTime result;
            if (!DateTime.TryParse(value, evalContext.Culture.DateTimeFormat, DateTimeStyles.None, out result))
                return null;

            DateTime timeValue = TimeType.NewTime(result.TimeOfDay);
            return timeValue;
        }

        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            DateTime localTime;
            if (!DateTime.TryParse(value, evalContext.Culture.DateTimeFormat, DateTimeStyles.None, out localTime))
                return null;
            DateTime utcTime = TimeZoneHelper.ConvertToUtcTZ(localTime, evalContext.TimeZoneInfo);
            return utcTime;
        }

        protected override Guid? OnEvaluateGuid(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            Guid result;
            if (Guid.TryParse(value, out result))
                return result;
            return null;
        }
    }


    /// <summary>
    /// Casts a value from a Guid to various types.
    /// </summary>
    class CastFromGuid : CastNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            Guid? value = Argument.EvaluateGuid(evalContext);
            if (value == null)
                return null;
            return value.Value.ToString("d"); // e.g. e63c8139-1dfe-4506-a73a-afbaaf25a1b7
        }
    }


    /// <summary>
    /// Casts a value from a Bool to various types.
    /// </summary>
    class CastFromBool : CastNode
    {
        static readonly string Format = DatabaseTypeHelper.GetDisplayFormatString(DatabaseType.BoolType);
            
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            bool? value = Argument.EvaluateBool(evalContext);
            
            if (value == null)
                return null;
            string result = string.Format(Format, value == true ? "Yes" : "No");
            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            if (ResultType.Type != DataType.String)
                throw new InvalidOperationException(ResultType.Type.ToString());

            ScalarExpression argument = Argument.BuildQuery(context);
            var result = CastToString(argument);
            return result;            
        }

        public static ScalarExpression CastToString(ScalarExpression boolExpression)
        {
            var result = new IfElseExpression
            {
                BooleanExpression = boolExpression,
                IfBlockExpression = new LiteralExpression { Value = new EDC.ReadiNow.Metadata.TypedValue("Yes") },
                ElseBlockExpression = new LiteralExpression { Value = new EDC.ReadiNow.Metadata.TypedValue("No") }
            };
            return result;
        }
    }


    /// <summary>
    /// Cast an entity from one type to another.
    /// </summary>
    class EntityTypeCast : EntityNode
    {
        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            yield return new EntitySetHandle
            {
                Expression = this
            };
        }
        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity entity = Argument.EvaluateEntity(evalContext);

            if ( PerTenantEntityTypeCache.Instance.IsInstanceOf( entity, ResultType.EntityTypeId ) )
            {
                return entity;
            }
            else
            {
                return null;
            }
        }

        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            IEntity value = Argument.EvaluateEntity(evalContext);

            if (value == null)
                return false;
            return true;
        }

        public override SQ.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            var result = ChildContainer.ChildEntityNodes.Single().BuildQueryNode(context, allowReuse);
            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var result = ChildContainer.ChildEntityNodes.Single().BuildQuery(context);
            return result;
        }

        public override EntityNode GetQueryNode()
        {
            return ChildContainer.ChildEntityNodes.Single().GetQueryNode();
        }
    }


    /// <summary>
    /// Casts a value from an entity to various types.
    /// </summary>
    class CastFromEntity : CastNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            IEntity value = Argument.EvaluateEntity(evalContext);

            if (value == null)
                return null;
            return value.As<Resource>().Name;
        }

        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            IEntity value = Argument.EvaluateEntity(evalContext);

            if (value == null)
                return false;
            return true;
        }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity value = Argument.EvaluateEntity(evalContext);
            if (value == null) return null;

            // Check that the type conforms
            if (ResultType.EntityType != null)
                if (!EntityTypeCache.GetAssignableTypes(value.TypeIds).Contains(ResultType.EntityType.Id))
                    return null;

            return value;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            ScalarExpression arg = Argument.BuildQuery( context );
            
            if (ResultType.Type == DataType.Entity)
            {
                return arg;
            }

            // Special case for AggregateExpression, because if we aggregate a choicefield, then it presents
            // as an Entity type, therefore requesting this cast - but the query builder wants to receive the aggregate expression directly.
            if ( arg is EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression )
            {
                var result = new MutateExpression { Expression = arg };

                switch ( ResultType.Type )
                {
                    case DataType.String:
                        result.MutateType = MutateType.DisplaySql;
                        break;

                    case DataType.Bool:
                        result.MutateType = MutateType.BoolSql;
                        break;

                    default:
                        throw new InvalidOperationException( ResultType.Type.ToString( ) );
                }
                return result;
            }

            // Just refer to the node
            var queryNode = context.GetNode(Argument);

            switch (ResultType.Type)
            {
                case DataType.String:
                    var nameResult = new ResourceDataColumn
                    {
                        FieldId = "core:name",
                        NodeId = queryNode.NodeId
                    };
                    return nameResult;

                case DataType.Bool:
                    var boolResult = new CalculationExpression
                    {
                        Operator = CalculationOperator.IsNull,
                        Expressions = new List<ScalarExpression>
                        {
                            new IdExpression { NodeId = queryNode.NodeId }
                        }
                    };
                    return boolResult;

                default:
                    throw new InvalidOperationException(ResultType.Type.ToString());
            }
        }
    }


    /// <summary>
    /// Casts a value from a DataTime to various types.
    /// </summary>
    class CastFromDateTime : CastNode
    {
        static readonly string Format = DatabaseTypeHelper.GetDisplayFormatString(DatabaseType.DateTimeType);

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            DateTime? utcValue = Argument.EvaluateDateTime(evalContext);

            if (utcValue == null)
                return null;

            DateTime localValue = TimeZoneHelper.ConvertToLocalTimeTZ(utcValue.Value, evalContext.TimeZoneInfo);

            string result = string.Format(evalContext.Culture.DateTimeFormat, Format, localValue);
            return result;
        }

        protected override DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            // Timezone offset only for datetime
            DateTime? utcDateTime = Argument.EvaluateDateTime(evalContext);
            if (utcDateTime == null) return null;

            // Assume hh:mm:ss are already zero
            var localDateTime = TimeZoneHelper.ConvertToLocalTimeTZ(utcDateTime.Value, evalContext.TimeZoneInfo);
            var time = TimeType.NewTime(localDateTime.TimeOfDay);
            return time;
        }

        protected override DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            // Timezone offset only for datetime
            DateTime? utcDateTime = Argument.EvaluateDateTime(evalContext);
            if (utcDateTime == null) return null;

            // Assume hh:mm:ss are already zero
            var localDateTime = TimeZoneHelper.ConvertToLocalTimeTZ(utcDateTime.Value, evalContext.TimeZoneInfo);
            var date = localDateTime.Date;
            return date;
        }
    }


    /// <summary>
    /// Casts a value from a DataTime to various types.
    /// </summary>
    class CastFromDate : CastNode
    {
        static readonly string Format = DatabaseTypeHelper.GetDisplayFormatString(DatabaseType.DateType);

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            // Timezone offset only for datetime
            DateTime? value = Argument.EvaluateDate(evalContext);

            string result = string.Format(evalContext.Culture.DateTimeFormat, Format, value);
            return result;
        }

        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            // Timezone offset only for datetime
            DateTime? value = Argument.EvaluateDate(evalContext);
            if (value == null) return null;

            // Assume hh:mm:ss are already zero
            var utcDateTime = TimeZoneHelper.ConvertToUtcTZ(value.Value, evalContext.TimeZoneInfo);
            return utcDateTime;
        }
    }


    /// <summary>
    /// Casts a value from a DataTime to various types.
    /// </summary>
    class CastFromTime : CastNode
    {
        static readonly string Format = DatabaseTypeHelper.GetDisplayFormatString(DatabaseType.TimeType);

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            // Timezone offset only for datetime
            DateTime? value = Argument.EvaluateTime(evalContext);

            string result = string.Format(evalContext.Culture.DateTimeFormat, Format, value);
            return result;
        }
    }

}
