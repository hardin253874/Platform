// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.ReadiNow.Utc;
using ReadiNow.Expressions.Tree.Nodes;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Evaluation
{
    /// <summary>
    /// Object that is passed around during the evaluation of an expression.
    /// Note that this context may be used to accumulate runtime information. (whereas the tree nodes themselves should not).
    /// </summary>
    public class EvaluationContext
    {
        public EvaluationContext()
        {
            _currentEntities = new Dictionary<ExpressionNode,IEntity>();
            TypeIsEnum = new Dictionary<long, bool>( );
        }

        private readonly Dictionary<ExpressionNode, IEntity> _currentEntities;

        public Dictionary<long, bool> TypeIsEnum { get; private set; }

        public void SetCurrentEntity(ExpressionNode entityForNode, IEntity entity)
        {
            _currentEntities[entityForNode] = entity;
        }

        public IEntity GetCurrentEntity(ExpressionNode node)
        {
            if (node is GetRootContextEntityNode)   // hack
            {
                return Settings.ContextEntity; 
            }

            IEntity result;
            if (!_currentEntities.TryGetValue(node, out result))
                return null; //throw new InvalidOperationException("Expression is out of scope.");     // Bug #22454
            return result;
        }

        /// <summary>
        /// If true, it ensures that every OnVisitItems fanout guarantees at least one row (even if it's null).
        /// Allows us to use potentially null relationships in expressions, without causing the result to disappear, but
        /// simultaneously without causing aggregates to give incorrect values.
        /// </summary>
        public bool EnsureDefaultRow { get; set; }

        /// <summary>
        /// The execution settings object passed in from the consumer.
        /// </summary>
        public EvaluationSettings Settings { get; set; }


        /// <summary>
        /// Time-zone information that should be used whenever date-times need to be converted to local time.
        /// </summary>
        internal TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (Settings == null)
                    throw new InvalidOperationException("Settings is not set.");
                if (Settings.TimeZoneName == null)
                    throw new InvalidOperationException("TimeZoneName is not set.");
                if (_timeZoneInfo == null)
                    _timeZoneInfo = TimeZoneHelper.GetTimeZoneInfo(Settings.TimeZoneName);
                return _timeZoneInfo;
            }
        }
        TimeZoneInfo _timeZoneInfo;


        /// <summary>
        /// The format provider to use for conversion between strings and date-times.
        /// </summary>
        internal CultureInfo Culture
        {
            get
            {
                if (_culture == null)
                {
                    _culture = new CultureInfo("en-AU");
                }
                return _culture;
            }
        }
        CultureInfo _culture;


        /// <summary>
        /// Resolves a parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns></returns>
        public object ResolveParameter(string parameterName)
        {
            if (_parameterValues == null)
                _parameterValues = new Dictionary<string, object>();

            // Cache the parameter result
            object result;
            if (!_parameterValues.TryGetValue(parameterName, out result))
            {
                if (Settings == null)
                    throw new InvalidOperationException("Settings were not set.");
                if (Settings.ParameterResolver == null)
                    throw new InvalidOperationException("ParameterResolver was not set.");

                result = Settings.ParameterResolver(parameterName);

                _parameterValues[parameterName] = result;
            }

            // TODO: Check result type
            return result;
        }

        private Dictionary<string, object> _parameterValues;

    }
}
