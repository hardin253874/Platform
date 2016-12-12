// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Holds a calculated field result for a single entity.
    /// </summary>
    public class CalculatedFieldSingleResult
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="result"></param>
        public CalculatedFieldSingleResult(long entityId, object result)
        {
            EntityId = entityId;
            Result = result;
        }

        /// <summary>
        /// Evaluation exception constructor
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="exception">An EvaluationException is a calculation runtime error. Due to bad data only, not due to internal code errors or invalid scripts.</param>
        public CalculatedFieldSingleResult(long entityId, EvaluationException exception)
        {
            EntityId = entityId;
            EvaluationException = exception;
        }

        /// <summary>
        /// ID of the entity that this result applies to.
        /// </summary>
        public long EntityId { get; private set; }

        /// <summary>
        /// An EvaluationException is a calculation runtime error. Due to bad data only, not due to internal code errors or invalid scripts.
        /// </summary>
        public EvaluationException EvaluationException { get; private set; }

        /// <summary>
        /// Result of the calculation
        /// </summary>
        public object Result { get; private set; }

    }
}
