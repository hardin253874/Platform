// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Represents a compiled expression.
    /// </summary>
    /// <remarks>
    /// This exists so we can pass it in and out of the expression interfaces, but implementations must always be ReadiNow.Expressions.Expression.
    /// </remarks>
    public interface IExpression
    {
        ExprType ResultType { get; }

        string ToXml( );
    }
}
