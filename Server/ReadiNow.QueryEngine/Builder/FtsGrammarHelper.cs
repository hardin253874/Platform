// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using Irony.Parsing;

namespace ReadiNow.QueryEngine.Builder
{
    public class FtsGrammarHelper : Grammar
    {
        public static string ConvertGoogleSpeakToFtsSpeak(string queryToConvert)
        {
            var grammar = new FtsGrammarHelper();
            var parser = new Parser(grammar);
            parser.Parse(queryToConvert.ToLower());
            ParseTree parseTree = parser.Context.CurrentParseTree;
            return !grammar.CheckParseErrors(parseTree) ? string.Empty : grammar.ConvertQuery(parseTree.Root);
        }

        private bool CheckParseErrors(ParseTree parseTree)
        {
            if (parseTree == null || parseTree.ParserMessages.Count == 0) 
                return true;
            /*
             TODO:- Add in log error messages for duff queries????
            string errs = 
                parseTree.ParserMessages.Aggregate("Errors: \r\n", (current, parserMessage) => current + 
                    string.Format("{0} Error: {1}, Parser State: {2}.\r\n", parserMessage.Location.ToString(), parserMessage.Message, parserMessage.ParserState));
            */
            return false;
        }

        private FtsGrammarHelper() : base(false) // Set to case insensitive
        {
            // ReSharper disable InconsistentNaming

            // Terminals
            var Term = CreateTerm("Term");
            var Phrase = new StringLiteral("Phrase", "\"");
            var ImpliedAnd = new ImpliedSymbolTerminal("ImpliedAnd");

            // NonTerminals
            var BinaryExpression = new NonTerminal("BinaryExpression");
            var BinaryOp = new NonTerminal("BinaryOp");
            var Expression = new NonTerminal("Expression");
            var PrimaryExpression = new NonTerminal("PrimaryExpression");
            var ThesaurusExpression = new NonTerminal("ThesaurusExpression");
            var ExactExpression = new NonTerminal("ExactExpression");
            var ParenthesizedExpression = new NonTerminal("ParenthesizedExpression");
            var ProximityExpression = new NonTerminal("ProximityExpression");
            var ProximityList = new NonTerminal("ProximityList");

            // ReSharper restore InconsistentNaming

            Root = Expression;
            Expression.Rule = PrimaryExpression | BinaryExpression;
            BinaryExpression.Rule = Expression + BinaryOp + Expression;
            BinaryOp.Rule = ImpliedAnd | "and" | "&" | "-" | "or" | "|";
            PrimaryExpression.Rule = Term
                                   | ThesaurusExpression
                                   | ExactExpression
                                   | ParenthesizedExpression
                                   | Phrase
                                   | ProximityExpression;
            ThesaurusExpression.Rule = "~" + Term;
            ExactExpression.Rule = "+" + Term | "+" + Phrase;
            ParenthesizedExpression.Rule = "(" + Expression + ")";
            ProximityExpression.Rule = "<" + ProximityList + ">";
            MakePlusRule(ProximityList, Term);

            MarkTransient(PrimaryExpression, Expression, ProximityExpression, ParenthesizedExpression, BinaryOp);
            MarkPunctuation("<", ">", "(", ")");
            RegisterOperators(10, "or", "|");
            RegisterOperators(20, "and", "&", "-");
            RegisterOperators(20, ImpliedAnd);
            //Register brace pairs to improve error reporting
            RegisterBracePair("(", ")");
            RegisterBracePair("<", ">");
            //Do not report ImpliedAnd as expected symbol - it is not really a symbol
            AddToNoReportGroup(ImpliedAnd);
            //also do not report braces as expected
            AddToNoReportGroup("(", ")", "<", ">");

        }
        private IdentifierTerminal CreateTerm(string name)
        {
            var term = new IdentifierTerminal(name, "!@#$%^*_'.?-", "!@#$%^*_'.?0123456789");
            term.CharCategories.AddRange(new[] {
             UnicodeCategory.UppercaseLetter, //Ul
             UnicodeCategory.LowercaseLetter, //Ll
             UnicodeCategory.TitlecaseLetter, //Lt
             UnicodeCategory.ModifierLetter,  //Lm
             UnicodeCategory.OtherLetter,     //Lo
             UnicodeCategory.LetterNumber,     //Nl
             UnicodeCategory.DecimalDigitNumber, //Nd
             UnicodeCategory.ConnectorPunctuation, //Pc
             UnicodeCategory.SpacingCombiningMark, //Mc
             UnicodeCategory.NonSpacingMark,       //Mn
             UnicodeCategory.Format                //Cf
          });
            //StartCharCategories are the same
            term.StartCharCategories.AddRange(term.CharCategories);
            return term;
        }

        private enum TermType
        {
            Inflectional = 1,
            Exact = 3
        }

        public string ConvertQuery(ParseTreeNode node)
        {
            return ConvertQuery(node, TermType.Inflectional);
        }        

        private static string ConvertQuery(ParseTreeNode node, TermType type)
        {
            string result = "";
            switch (node.Term.Name)
            {
                case "BinaryExpression":
                    string op = node.ChildNodes[1].FindTokenAndGetText().ToLower();
                    string sqlOp = "";
                    switch (op)
                    {
                        case "":
                        case "&":
                        case "and":
                            sqlOp = " AND ";
                            type = TermType.Inflectional;
                            break;
                        case "-":
                            sqlOp = " AND NOT ";
                            break;
                        case "|":
                        case "or":
                            sqlOp = " OR ";
                            break;
                    }//switch

                    result = "(" + ConvertQuery(node.ChildNodes[0], type) + sqlOp + ConvertQuery(node.ChildNodes[2], type) + ")";
                    break;

                case "PrimaryExpression":
                    result = "(" + ConvertQuery(node.ChildNodes[0], type) + ")";
                    break;

                case "ProximityList":
                    var tmp = new string[node.ChildNodes.Count];
                    type = TermType.Exact;
                    for (int i = 0; i < node.ChildNodes.Count; i++)
                    {
                        tmp[i] = ConvertQuery(node.ChildNodes[i], type);
                    }
                    result = "(" + string.Join(" NEAR ", tmp) + ")";
                    //type = TermType.Inflectional;
                    break;

                case "Phrase":
                    result = '"' + node.Token.ValueString + '"';
                    break;

                case "ThesaurusExpression":
                    result = " FORMSOF (THESAURUS, " +
                        node.ChildNodes[1].Token.ValueString + ") ";
                    break;

                case "ExactExpression":
                    result = " \"" + node.ChildNodes[1].Token.ValueString + "\" ";
                    break;

                case "Term":
                    switch (type)
                    {
                        case TermType.Inflectional:
                            result = node.Token.ValueString;
                            if (result.EndsWith("*"))
                                result = "\"" + result + "\"";
                            else
                                result = " FORMSOF (INFLECTIONAL, " + result + ") ";
                            break;
                        case TermType.Exact:
                            result = node.Token.ValueString;

                            break;
                    }
                    break;
                default:
                    throw new InvalidOperationException( $"Unknown node type {node.Term.Name}" );
            }
            return result;
        }    
    }
}
