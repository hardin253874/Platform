// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using Irony.Parsing;
using ReadiNow.Expressions.Compiler;

namespace ReadiNow.Expressions.Parser
{
    using Parser = Irony.Parsing.Parser;
    using EDC.ReadiNow.Expressions;

    /// <summary>
    /// The parser for macros entered into the mergefield entries of the template document.
    /// </summary>
    public class ExpressionGrammar : Grammar
    {
        /// <summary>
        /// Parses a macro.
        /// </summary>
        /// <param name="inputText">The macro script text.</param>
        /// <returns>The parse tree, starting with the InstructionSet node.</returns>
        /// <exception cref="ParseException">Thrown if the text cannot be parsed.</exception>
        public static ParseTree ParseMacro(string inputText)
        {
            if (inputText == null)
                throw new ArgumentNullException();

            // Preparse ‘ ’ quotes. Given that macros are entered in Word, and that people get confused, it's easier to just treat them all the same.
            inputText = inputText.Replace('‘', '\'');
            inputText = inputText.Replace('’', '\'');

            // Parse macro
            ExpressionGrammar grammar = _grammarPool.GetObject();
            try
            {
                var parser = new Parser(grammar);
                parser.Parse(inputText);

                // Check for macro errors
                ParseTree parseTree = parser.Context.CurrentParseTree;
                grammar.CheckParseTreeOk(parseTree);
                return parseTree;
            }
            finally
            {
                _grammarPool.PutObject(grammar);
            }
        }


        /// <summary>
        /// Prewarm the grammar pool with one object.
        /// </summary>
        public static void Prewarm()
        {
            ExpressionGrammar grammar = _grammarPool.GetObject();
            _grammarPool.PutObject(grammar);            
        }


        /// <summary>
        /// Constructor. Sets grammar as case-sensitive.
        /// Inheritors should call EstablishExpressionGrammar after construction.
        /// </summary>
        protected ExpressionGrammar(bool establishGrammar)
            : base(false) // case sensitive
        {
            if (establishGrammar)
            {
                EstablishExpressionGrammar();
                Root = Expression;
            }
        }


        /// <summary>
        /// The Expression term. Exposed for use by derived types.
        /// </summary>
        protected NonTerminal Expression { get; set; }

        /// <summary>
        /// The Identifier term. Exposed for use by derived types.
        /// </summary>
        protected IdentifierTerminal Identifier { get; set; }

        /// <summary>
        /// Generate language syntax elements.
        /// Call in the constructor.
        /// </summary>
        protected void EstablishExpressionGrammar()
        {
            // Note: in the case of fields, the 'field' keyword is optional.
            // Note: in the case of relationships, the 'foreach' keyword is optional, and is presumed to be the default.
            // In both cases, the parser tree would be much more elegant if we could use ImpliedSymbolTerminal, but they don't
            // seem to work when used in conjunction with each other, or with Entity, which can match many things.
            
            // Terminals
            var identifier = CreateIdentifierTerm(Terms.Identifier);
            var stringLiteral = new StringLiteral(Terms.StringLiteral, "'", StringOptions.AllowsDoubledQuote);
            var dateTimeLiteral = new StringLiteral(Terms.DateTimeLiteral, "#", StringOptions.NoEscapes);
            var numberLiteral = new NumberLiteral(Terms.NumberLiteral, NumberOptions.AllowSign);
            var is1 = ToTerm(Keywords.Is);
            var isNot = ToTerm(Keywords.IsNot);
            var and = ToTerm(Keywords.And);
            var or = ToTerm(Keywords.Or);
            var not = ToTerm(Keywords.Not);
            var by = ToTerm(Keywords.By);
            var trueLiteral = ToTerm(Keywords.True);
            var falseLiteral = ToTerm(Keywords.False);
            var nullLiteral = ToTerm(Keywords.Null);
            var like = ToTerm(Keywords.Like);
            var notLike = ToTerm(Keywords.NotLike);
            var let = ToTerm(Keywords.Let);
            //var union = ToTerm(Keywords.Union);
            //var unionAll = ToTerm(Keywords.UnionAll);

            // NonTerminals
            var expression = new NonTerminal(Terms.Expression);
            var basicExpression = new NonTerminal(Terms.ExpressionImpl);
            var primaryExpression = new NonTerminal(Terms.PrimaryExpression);
            var memberAccess = new NonTerminal(Terms.MemberAccess);
            var functionExpression = new NonTerminal(Terms.FunctionExpression);
            var unaryExpression = new NonTerminal(Terms.UnaryExpression);
            var binaryExpression = new NonTerminal(Terms.BinaryExpression);
            var parensExpression = new NonTerminal(Terms.ParensExpression);

            var selectExpr = new NonTerminal(Terms.SelectExpression);
            var letClause = new NonTerminal(Terms.LetClause);
            var letSelectOpt = new NonTerminal("let-select-opt");
            var letOpt = new NonTerminal("let-opt");

            var whereExpression = new NonTerminal(Terms.WhereExpression);
            var whereOpt = new NonTerminal("where-opt");
            
            var orderByExpression = new NonTerminal(Terms.OrderByExpression);
            var orderByList = new NonTerminal(Terms.OrderByTermList);
            var orderByTerm = new NonTerminal(Terms.OrderByTerm);
            var orderByOpt = new NonTerminal("order-by-opt");

            var argumentList = new NonTerminal(Terms.ArgumentList);
            var parameter = new NonTerminal(Terms.Parameter);
            var prefixOperator = new NonTerminal(Terms.PrefixOperator);
            var infixOperator = new NonTerminal(Terms.InfixOperator);
            var literal = new NonTerminal(Terms.Literal);
            var queryExpression = new NonTerminal(Terms.QueryExpression);
            var queryExpressionNoOrder = new NonTerminal(Terms.QueryExpression);

            Expression = expression;
            Identifier = identifier;


            //////////// Rules ////////////
            
            // expression
            expression.Rule = 
                queryExpression;

            // query-expression
            // select is mandatory if 'let' is use 
            MarkTransient(queryExpression);
            queryExpression.Rule =
                orderByOpt;

            // query-expression-no-order
            // select is mandatory if 'let' is use 
            MarkTransient(queryExpressionNoOrder);
            queryExpressionNoOrder.Rule =
                whereOpt;

            // select-expr
            selectExpr.Rule =
                Keywords.Select + basicExpression;

            // let-select-opt
            MarkTransient(letSelectOpt);
            letSelectOpt.Rule =
                basicExpression
                | letOpt;

            // let-opt
            MarkTransient(letOpt);
            letOpt.Rule =
                selectExpr
                | letClause;

            // let-clause
            letClause.Rule =
                let + Identifier + ToTerm("=") + basicExpression + letOpt;

            // where-opt
            MarkTransient(whereOpt);
            whereOpt.Rule =
                letSelectOpt
                | whereExpression;

            // where-clause
            whereExpression.Rule =
                letSelectOpt + Keywords.Where + basicExpression;

            // orderby-opt
            MarkTransient(orderByOpt);
            orderByOpt.Rule =
                whereOpt
                | orderByExpression;

            // orderby-clause
            orderByExpression.Rule =
                whereOpt + Keywords.Order + by + orderByList;

            // orderby-term-list
            orderByList.Rule =
                MakeListRule(orderByList, ToTerm(","), orderByTerm, TermListOptions.PlusList);

            // orderby-term
            orderByTerm.Rule =
                basicExpression
                | basicExpression + Keywords.Asc
                | basicExpression + Keywords.Desc;

            // expression-impl
            basicExpression.Rule =
                primaryExpression
                | unaryExpression
                | binaryExpression;

            // primary-expression
            primaryExpression.Rule =
                parensExpression
                | literal
                | parameter
                | memberAccess
                | functionExpression;
                //| variable?

            // member-access
            memberAccess.Rule =
                identifier
                | primaryExpression + ToTerm(".") + identifier;

            ////// function-expression
            functionExpression.Rule =
                identifier + ToTerm("(") + argumentList + ToTerm(")")
                | primaryExpression + ToTerm(".") + identifier + ToTerm("(") + argumentList + ToTerm(")");
            argumentList.Rule =
                 MakeListRule(argumentList, ToTerm(","), queryExpressionNoOrder, TermListOptions.StarList);

            // unary-expression
            unaryExpression.Rule =
                prefixOperator + basicExpression;
            prefixOperator.Rule =
                not | "-";
            
            // binary-expression
            binaryExpression.Rule =
                basicExpression + infixOperator + basicExpression;
            infixOperator.Rule =
                ToTerm("+") | "-" | "*" | "/" | "%"
                 | "=" | ">" | "<" | ">=" | "<=" | "<>"
                 | and | or | like | notLike | is1 | isNot;
            
            // parens-expression
            parensExpression.Rule =
                ToTerm("(") + queryExpression + ToTerm(")");

            // parameter
            parameter.Rule =
               ToTerm("@") + identifier;

            // literal
            literal.Rule =
               stringLiteral
               | numberLiteral
               | dateTimeLiteral
               | trueLiteral
               | falseLiteral
               | nullLiteral;

           //////////// Precedences ////////////

           RegisterOperators(10, "*", "/", "%"); 
           RegisterOperators(9, "+", "-");
           RegisterOperators(8, "=", ">", "<", ">=", "<=", "<>", "!=");
           RegisterOperators(8, like, notLike, is1, isNot);
           RegisterOperators(7, "^", "&", "|");
           RegisterOperators(6, not);
           RegisterOperators(5, and);
           RegisterOperators(4, or);
           //RegisterOperators(2, union, unionAll);
            
           // Done to avoid matching keywords as entities
           var keywords = new[] {
               Keywords.Not, Keywords.And, Keywords.Or,
               Keywords.Like, Keywords.NotLike, Keywords.Is, Keywords.IsNot,
               Keywords.True, Keywords.False,
               Keywords.Null,
               Keywords.Where, Keywords.Order, Keywords.By, Keywords.Let };
           foreach (var keyword in keywords)
           {
               MarkReservedWords(keyword);
           }

           // Prevent these intermediate nodes from appearing in the result parse tree
           //MarkTransient(literal);
           MarkPunctuation(",", "(", ")", ".");

           //Note: we cannot declare binOp as transient because it includes operators "NOT LIKE", "NOT IN" consisting of two tokens. 
           // Transient non-terminals cannot have more than one non-punctuation child nodes.
           // Instead, we set flag InheritPrecedence on binOp , so that it inherits precedence value from it's children, and this precedence is used
           // in conflict resolution when binOp node is sitting on the stack
           MarkTransient(
               primaryExpression, basicExpression,
               prefixOperator, infixOperator, literal, parensExpression);
           infixOperator.SetFlag(TermFlags.InheritPrecedence);
        }


        /// <summary>
        /// Create the term that represents an Entity identifier.
        /// </summary>
        public IdentifierTerminal CreateIdentifierTerm(string name)
        {
            var id = new IdentifierTerminal(name, IdOptions.IsNotKeyword);
            var term = new StringLiteral(name + "_quoted");
            term.AddStartEnd("[", "]", StringOptions.AllowsDoubledQuote);
            term.SetOutputTerminal(this, id);
            return id;
        }

        #region Grammar Pool

        /// <summary>
        /// Singleton instance of grammar.
        /// Thread-safe. Grammar is verified on first access.
        /// </summary>
        private static ExpressionGrammar CreateGrammar()
        {
            var grammar = new ExpressionGrammar(true);

            // Verify the grammar on first load
            var parser = new Parser(grammar);
            var errors = parser.Language.Errors;
            if (errors.Count > 0)
                throw new Exception("Internal error: macro grammar contains error(s): " + string.Join("\n", errors));

            return grammar;
        }

        /// <summary>
        /// Thread-safe pool of grammars.
        /// </summary>
        private static ObjectPool<ExpressionGrammar> _grammarPool = new ObjectPool<ExpressionGrammar>(CreateGrammar);
        #endregion


        /// <summary>
        /// Checks the parse tree for errors.
        /// </summary>
        /// <param name="parseTree">The parse tree.</param>
        /// <exception cref="ParseException">Thrown if there are any parse errors.</exception>
        public void CheckParseTreeOk(ParseTree parseTree)
        {
            if (parseTree == null || parseTree.ParserMessages.Count == 0)
                return;

            if (parseTree.ParserMessages.Count == 1)
            {
                string message = parseTree.ParserMessages[0].Message;
                if (message.StartsWith("Syntax error, expected"))
                {
                    message = "Invalid script syntax.";
                }

                throw ParseExceptionHelper.New(message, parseTree.ParserMessages[0].Location);
            }

            throw new ParseException(string.Join(", ", parseTree.ParserMessages.Select(m => string.Format("{0} pos {1}", m.Message, m.Location.Position))));
        }
    }
}

