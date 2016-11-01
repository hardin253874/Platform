// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Parser
{
    /// <summary>
    /// Parse tree node identifiers. Refer to language spec.
    /// </summary>
    public static class Terms
    {
        public const string Expression = "expression";

        public const string ExpressionImpl = "basic-expression";

        public const string PrimaryExpression = "primary-expression";

        public const string MemberAccess = "member-access";

        public const string FunctionExpression = "function-expression";

        public const string UnaryExpression = "unary-expression";

        public const string BinaryExpression = "binary-expression";

        public const string ParensExpression = "parens-expression";



        public const string QueryExpression = "query-expression";

        public const string SelectExpression = "select-expression";

        public const string LetClause = "let-clause";

        public const string WhereExpression = "where-expression";

        public const string OrderByExpression = "orderby-expression";

        public const string OrderByTermList = "orderby-termlist";

        public const string OrderByTerm = "orderby-term";

        public const int LetIndex = 0;

        public const int SelectIndex = 1;

        public const int WhereIndex = 2;

        public const int OrderByIndex = 3;


        
        public const string ArgumentList = "argument-list";

        public const string Parameter = "parameter";

        public const string PrefixOperator = "prefix-operator";

        public const string InfixOperator = "infix-operator";

        public const string Identifier = "identifier";

        public const string Literal = "literal";

        public const string StringLiteral = "string";

        public const string DateTimeLiteral = "datetime";

        public const string NumberLiteral = "number";

        public const string ImplicitContext = "implicit-context";
    }
}
