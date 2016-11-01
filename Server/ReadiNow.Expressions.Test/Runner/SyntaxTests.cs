// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Test;
using ReadiNow.Expressions.Parser;
using NUnit.Framework;
using Irony.Parsing;

namespace ReadiNow.Expressions.Test.Runner
{
    [TestFixture]
	[RunWithTransaction]
    public class SyntaxTests
    {

        #region Test Broken Queries
        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_Chars()
        {
            RunNonQueryTest("@#$@!");
        }

        //[Test]
        //[ExpectedException(typeof(ParseException))]
        //public void TestInvalid_AndOperator()
        //{
        //    RunSingleTest("and");
        //}

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_PlusOperator()
        {
            RunNonQueryTest("+");
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_UnbalancedBrackets_1()
        {
            RunNonQueryTest("(");
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_UnbalancedBrackets_2()
        {
            RunNonQueryTest("((1)))");
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_DoubleParen()
        {
            RunNonQueryTest("@@test");
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_HangingDot()
        {
            RunNonQueryTest("@test.");
        }

        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestInvalid_MissingContex()
        {
            RunNonQueryTest(".Member");
        }
        #endregion

        #region Test Combinations
        [Test]
        public void TestCombo_1()
        {
            RunNonQueryTest("[First name] + ' ' + [Last name]");
        }

        [Test]
        public void TestCombo_2()
        {
            RunNonQueryTest("Order.Items.Cost * Order.Items.Quantity");
        }

        [Test]
        public void TestCombo_3()
        {
            RunNonQueryTest("Revenue / Costs * 100");
        }
        #endregion

        #region Test Parameters
        [Test]
        public void TestParameter_Basic()
        {
            var node = RunNonQueryTest("@myparam");
            CheckTerm(node, Terms.Parameter);
        }

        [Test]
        public void TestParameter_Escaped()
        {
            var node = RunNonQueryTest("@[Hello world]");
            CheckTerm(node, Terms.Parameter);
        }
        #endregion

        #region Test Identifiers
        [Test]
        public void TestIdentifiers_Simple()
        {
            var node = RunNonQueryTest("HelloWorld");
            CheckTerm(node, Terms.MemberAccess);
        }

        [Test]
        public void TestIdentifiers_Alphanumeric()
        {
            var node = RunNonQueryTest("a1234");
            CheckTerm(node, Terms.MemberAccess);
        }

        [Test]
        public void TestIdentifiers_DelimitedSimple()
        {
            var node = RunNonQueryTest("[Hello world]");
            CheckTerm(node, Terms.MemberAccess);
        }

        [Test]
        public void TestIdentifiers_DelimitedAll()
        {
            var node = RunNonQueryTest(@"[!@#$%^&*()_+-={}|\:"";'<>?,./[]");
            CheckTerm(node, Terms.MemberAccess);
        }

        [Test]
        public void TestIdentifiers_DelimitedClose()
        {
            var node = RunNonQueryTest(@"[]]]");
            CheckTerm(node, Terms.MemberAccess);
        }
        #endregion

        #region Test Parens
        [Test]
        [ExpectedException(typeof(ParseException))]
        public void TestParens()
        {
            RunNonQueryTest("()");
        }

        [Test]
        public void TestParens_Basic()
        {
            RunNonQueryTest("(1+2)+3");
        }

        [Test]
        public void TestParens_Nested()
        {
            RunNonQueryTest("4+((1+2)+3)");
        }
        
        [Test]
        public void TestParens_InFunc()
        {
            RunNonQueryTest("abs((1+2))");
        }
        #endregion

        #region Static Function

        [Test]
        public void TestStaticFunction_0arg()
        {
            var node = RunNonQueryTest("abs()");
            CheckTerm(node, Terms.FunctionExpression);

            CheckTerm(node.ChildNodes[0], Terms.Identifier);
            CheckTerm(node.ChildNodes[1], Terms.ArgumentList);
        }


        [Test]
        public void TestStaticFunction_1arg()
        {
            var node = RunNonQueryTest("abs(1)");
            CheckTerm(node, Terms.FunctionExpression);

            CheckTerm(node.ChildNodes[0], Terms.Identifier);
            CheckTerm(node.ChildNodes[1], Terms.ArgumentList);
            Assert.AreEqual(1, node.ChildNodes[1].ChildNodes.Count);
        }


        [Test]
        public void TestStaticFunction_2arg()
        {
            var node = RunNonQueryTest("abs(1,2)");
            CheckTerm(node, Terms.FunctionExpression);

            CheckTerm(node.ChildNodes[0], Terms.Identifier);
            CheckTerm(node.ChildNodes[1], Terms.ArgumentList);
            Assert.AreEqual(2, node.ChildNodes[1].ChildNodes.Count);
        }
        #endregion

        #region Member Function

        [Test]
        public void TestMemberFunction_0arg()
        {
            var node = RunNonQueryTest("@p.abs()");
            CheckTerm(node, Terms.FunctionExpression);

            CheckTerm(node.ChildNodes[0], Terms.Parameter);
            CheckTerm(node.ChildNodes[1], Terms.Identifier);
            CheckTerm(node.ChildNodes[2], Terms.ArgumentList);
        }


        [Test]
        public void TestMemberFunction_1arg()
        {
            var node = RunNonQueryTest("@p.abs(1)");
            CheckTerm(node, Terms.FunctionExpression);

            CheckTerm(node.ChildNodes[0], Terms.Parameter);
            CheckTerm(node.ChildNodes[1], Terms.Identifier);
            CheckTerm(node.ChildNodes[2], Terms.ArgumentList);

            Assert.AreEqual(1, node.ChildNodes[2].ChildNodes.Count);
        }


        [Test]
        public void TestMemberFunction_2arg()
        {
            var node = RunNonQueryTest("@p.abs(1,2)");
            CheckTerm(node, Terms.FunctionExpression);

            CheckTerm(node.ChildNodes[0], Terms.Parameter);
            CheckTerm(node.ChildNodes[1], Terms.Identifier);
            CheckTerm(node.ChildNodes[2], Terms.ArgumentList);

            Assert.AreEqual(2, node.ChildNodes[2].ChildNodes.Count);
        }
        #endregion

        #region Member Access

        [Test]
        public void TestMemberAccess_Simple()
        {
            var node = RunNonQueryTest("@p.HelloWorld");
            CheckTerm(node, Terms.MemberAccess);
        }

        [Test]
        public void TestMemberAccess_Nested()
        {
            var node = RunNonQueryTest("@p.Hello.World");
            CheckTerm(node, Terms.MemberAccess);
        }

        [Test]
        public void TestMemberAccess_Escaped()
        {
            var node = RunNonQueryTest("@p.[Hello world]");
            CheckTerm(node, Terms.MemberAccess);
        }
        #endregion

        #region Where clause

        [Test]
        public void TestWhere()
        {
            var node = RunQueryTest("Somerelationship where abc='123'");
            CheckTerm(node, Terms.WhereExpression);
            Assert.AreEqual(3, node.ChildNodes.Count);
        }

        [TestCase("Somerelationship where a")]
        [TestCase("select Somerelationship where a")]
        [TestCase("let x=y select Somerelationship where a")]
        [TestCase("let x=y select Somerelationship where a order by b")]
        public void TestWhereValidScripts(string script)
        {
            RunQueryTest(script);
        }
        #endregion

        #region Order By

        [TestCase("Somerelationship order by a", 1)]
        [TestCase("Somerelationship where x=y order by a", 1)]
        [TestCase("Somerelationship order by abc asc", 1)]
        [TestCase("Somerelationship order by abc desc", 1)]
        [TestCase("Somerelationship order by abc, def", 2)]
        [TestCase("Somerelationship order by abc asc, def desc", 2)]
        public void TestOrderBy(string script, int orderTerms)
        {
            var node = RunQueryTest(script);
            CheckTerm(node, Terms.OrderByExpression);
            var orderList = node.ChildNodes[3];
            Assert.AreEqual(orderTerms, orderList.ChildNodes.Count);
        }

        [TestCase("Somerelationship order by a")]
        [TestCase("select Somerelationship order by a")]
        [TestCase("let x=y select Somerelationship order by a")]
        public void TestOrderByValidScripts(string script)
        {
            RunQueryTest(script);
        }
        #endregion

        #region String Literals
        [Test]
        public void TestStringLiteral()
        {
            var node = RunNonQueryTest("'hello'");
            CheckTerm(node, Terms.StringLiteral);

            Assert.IsAssignableFrom<string>(node.Token.Value, "Value type");
            Assert.AreEqual("hello", (string)node.Token.Value, "Value");
        }

        [Test]
        public void TestStringLiteral_Delimit()
        {
            var node = RunNonQueryTest("'abc''def'");
            CheckTerm(node, Terms.StringLiteral);
        }
        
        [Test]
        public void TestStringLiteral_Delimit2()
        {
            var node = RunNonQueryTest("''''");
            CheckTerm(node, Terms.StringLiteral);
        }
        #endregion

        #region Numeric Literals
        [Test]
        public void TestNumericLiteral_Basic()
        {
            var node = RunNonQueryTest("123");
            CheckTerm(node, Terms.NumberLiteral);

            Assert.IsAssignableFrom<int>(node.Token.Value, "Value type");
            Assert.AreEqual(123, (int)node.Token.Value, "Value");
        }

        [Test]
        public void TestNumericLiteral_Decimal()
        {
            var node = RunNonQueryTest("123.123");
            CheckTerm(node, Terms.NumberLiteral);

            Assert.IsAssignableFrom<double>(node.Token.Value, "Value type");
            Assert.AreEqual(123.123, (double)node.Token.Value, "Value");
        }

        [Test]
        public void TestNumericLiteral_Negatives()
        {
            var node = RunNonQueryTest("-123");
            CheckTerm(node, Terms.NumberLiteral);

            Assert.IsAssignableFrom<int>(node.Token.Value, "Value type");
            Assert.AreEqual(-123, (int)node.Token.Value, "Value");
        }
        #endregion

        #region Keyword Literals
        [Test]
        public void TestBoolLiteral_True()
        {
            var node = RunNonQueryTest("true");
            CheckTerm(node, Keywords.True);
        }

        [Test]
        public void TestBoolLiteral_False()
        {
            var node = RunNonQueryTest("false");
            CheckTerm(node, Keywords.False);
        }

        [Test]
        public void TestNullLiteral()
        {
            var node = RunNonQueryTest("null");
            CheckTerm(node, Keywords.Null);
        }
        #endregion

        #region Binary Operators
        [Test]
        public void TestBinaryOperators()
        {
            string[] tests = "+ - * / % = > < >= <= <> and or".Split(' ');

            foreach (string test in tests)
            {
                var node = RunNonQueryTest("123 " + test + " 123");
                CheckTerm(node, Terms.BinaryExpression);
                if (!char.IsLetter(test[0]))
                {
                    node = RunNonQueryTest("123" + test + "123");
                    CheckTerm(node, Terms.BinaryExpression);
                }
            }
        }

        [Test]
        public void TestBinaryOperators_2()
        {
            string[] tests = "+ - * / % = > < >= <= <> and or".Split(' ');

            foreach (string test in tests)
            {
                var node = RunNonQueryTest("@p " + test + " @p");
                CheckTerm(node, Terms.BinaryExpression);
                if (!char.IsLetter(test[0]))
                {
                    node = RunNonQueryTest("@p" + test + "@p");
                    CheckTerm(node, Terms.BinaryExpression);
                }
            }
        }

        [Test]
        public void TestBinaryOperators_3()
        {
            string[] tests = "+ - * / % = > < >= <= <> and or".Split(' ');

            foreach (string test in tests)
            {
                ParseTreeNode node;
                node = RunNonQueryTest("'abc' " + test + " 'abc'");
                CheckTerm(node, Terms.BinaryExpression);

                node = RunNonQueryTest("'abc'" + test + "'abc'");
                CheckTerm(node, Terms.BinaryExpression);
            }
        }
        #endregion

        #region Unary Operators
        [Test]
        public void TestUnaryOperators()
        {
            ParseTreeNode node;

            node = RunNonQueryTest("- 123");
            CheckTerm(node, Terms.UnaryExpression);

            node = RunNonQueryTest("-123");
            CheckTerm(node, Terms.NumberLiteral);

            node = RunNonQueryTest("--123");
            CheckTerm(node, Terms.UnaryExpression);

            node = RunNonQueryTest("-@p");
            CheckTerm(node, Terms.UnaryExpression);

            node = RunNonQueryTest("not @p");
            CheckTerm(node, Terms.UnaryExpression);
        }
        #endregion



        #region Test Helpers

        /// <summary>
        /// Parse a query and return the first instruction.
        /// </summary>
        private ParseTreeNode RunQueryTest(string script)
        {
            var result = ExpressionGrammar.ParseMacro(script);

            var expression = CheckTerm(result.Root, Terms.Expression);
            var queryExpression = expression.ChildNodes[0];
            return queryExpression;
        }

        /// <summary>
        /// Parse a query and return the first instruction.
        /// </summary>
        private ParseTreeNode RunNonQueryTest(string script)
        {
            var result = ExpressionGrammar.ParseMacro(script);

            var expression = CheckTerm(result.Root, Terms.Expression);
            var instruction = expression.ChildNodes[0];
            return instruction;
        }

        /// <summary>
        /// Validate that the node represents the expected parse-term.
        /// </summary>
        private ParseTreeNode CheckTerm(ParseTreeNode node, string expectedTerm)
        {
            Assert.AreEqual(expectedTerm, node.Term.Name);
            return node;
        }

        /// <summary>
        /// Validate that the node represents the expected keyword.
        /// </summary>
        private void CheckKeyword(ParseTreeNode node, string expectedKeyword)
        {
            Assert.IsInstanceOf<KeyTerm>(node.Term, "Found: " + node.Term.Name);
            Assert.AreEqual(expectedKeyword, node.Token.ValueString);
        }

        /// <summary>
        /// Validate that the node represents the expected identifier.
        /// </summary>
        private void CheckIdentifier(ParseTreeNode node, string expectedIdentifier)
        {
            Assert.IsInstanceOf<IdentifierTerminal>(node.Term);
            Assert.AreEqual(expectedIdentifier, node.Token.ValueString);
        }

        ///// <summary>
        ///// Validate that the node represents the expected relationship instruction.
        ///// </summary>
        //private void CheckRelInstruction(ParseTreeNode node, string expectedBehavior, string expectedSource, string sourceArg1 = null, string sourceArg2 = null)
        //{
        //    var relInstr = CheckTerm(node, Terms.RelateInstruction);

        //    ParseTreeNode listBehavior = null;
        //    ParseTreeNode listSource;
        //    Assert.IsTrue(relInstr.ChildNodes.Count < 3);
        //    if (relInstr.ChildNodes.Count == 1)
        //    {
        //        listSource = CheckTerm(relInstr.ChildNodes[0], Terms.ListSource);
        //    }
        //    else
        //    {
        //        listBehavior = CheckTerm(relInstr.ChildNodes[0], Terms.ListBehavior);
        //        listSource = CheckTerm(relInstr.ChildNodes[1], Terms.ListSource);
        //    }

        //    if (listBehavior != null)
        //    {
        //        CheckKeyword(listBehavior.ChildNodes[0], expectedBehavior);
        //    }
        //    CheckKeyword(listSource.ChildNodes[0], expectedSource);

        //    if (sourceArg1 != null)
        //    {
        //        var entity1 = CheckTerm(listSource.ChildNodes[1], Terms.Entity);
        //        CheckIdentifier(entity1, sourceArg1);
        //    }

        //    if (sourceArg2 != null)
        //    {
        //        var entity2 = CheckTerm(listSource.ChildNodes[2], Terms.Entity);
        //        CheckIdentifier(entity2, sourceArg2);
        //    }
        //}

        ///// <summary>
        ///// Test various ways of identifying an entity literal.
        ///// </summary>
        //public void TestEntity(string inputText, string expected = null)
        //{
        //    var instruction = RunSingleTest("if rel " + inputText);
        //    CheckRelInstruction(instruction, Keywords.If, Keywords.Rel, expected);
        //}

        ///// <summary>
        ///// Validate that the node represents the expected field instruction.
        ///// </summary>
        //public void CheckField(ParseTreeNode instruction, string expectedFieldIdentifier)
        //{
        //    var fieldInstruction = CheckTerm(instruction, Terms.FieldInstruction);

        //    if (fieldInstruction.ChildNodes[0].Term.Name == Keywords.Field)
        //        CheckIdentifier(fieldInstruction.ChildNodes[1], expectedFieldIdentifier);
        //    else
        //        CheckIdentifier(fieldInstruction.ChildNodes[0], expectedFieldIdentifier);
        //}

        ///// <summary>
        ///// Validate that the node represents the expected metadata instruction.
        ///// </summary>
        //public void CheckMetadata(ParseTreeNode instruction, string expectedKeyword)
        //{
        //    var fieldInstruction = CheckTerm(instruction, Terms.MetadataInstruction);

        //    CheckKeyword(fieldInstruction.ChildNodes[0], expectedKeyword);
        //}

        #endregion


    }
}
