// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadiNow.Expressions.Parser;
using NUnit.Framework;
using Irony.Parsing;
using ReadiNow.DocGen.MacroParser;

namespace ReadiNow.DocGen.Test.MacroParser
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        [ExpectedException]
        public void TestInvalid()
        {
            RunSingleTest("@#$@!");
        }

        [Test]
        [ExpectedException]
        public void TestEmpty()
        {
            RunSingleTest("");
        }

        [Test]
        public void TestEnd()
        {
            var instruction = RunSingleTest("end");

            CheckKeyword(instruction, "end");
        }

        #region Misc Integration
        [Test]
        public void TestCalculation1()
        {
            RunSingleTest("1+2");
        }

        [Test]
        public void TestCalculation2()
        {
            RunSingleTest("any(all(Employee).Age>20)");
        }
        #endregion

        #region Fields

        [Test]
        public void TestFieldImplicit()
        {
            var instruction = RunSingleTest("Age");
            CheckField(instruction, "Age");
        }

        [Test]
        [ExpectedException]
        public void TestFieldImplicitWithInvalidSpace()
        {
            RunSingleTest("First Name");
        }

        [Test]
        public void TestFieldImplicitBracketed()
        {
            var instruction = RunSingleTest("[First Name]");
            CheckField(instruction, "First Name");
        }

        [Test]
        public void TestFieldExplicit()
        {
            var instruction = RunSingleTest("show Age");
            CheckField(instruction, "Age");
        }

        [Test]
        public void TestFieldExplicitBracketed()
        {
            var instruction = RunSingleTest("show [First Name]");
            CheckField(instruction, "First Name");
        }

        #endregion

        #region Metadata

        [Test]
        public void TestPosition()
        {
            var instruction = RunSingleTest("position");
            CheckMetadata(instruction, Keywords.Position);
        }

        #endregion

        #region Parse Entity

        [Test]
        public void TestEntityUnbracketed()
        {
            TestEntity("Employees", "Employees");
        }

        [Test]
        public void TestEntityUnbracketedWithNumbers()
        {
            TestEntity("A123", "A123");
        }

        [Test]
        [ExpectedException]
        public void TestEntityUnbracketedWithInvalidLeadingNumbers()
        {
            TestEntity("09", "09");
        }

        [Test]
        [ExpectedException]
        public void TestEntityUnbracketedWithInvalidSpace()
        {
            TestEntity("Direct Reports", "Direct Reports");
        }

        [Test]
        public void TestEntityBracketed()
        {
            TestEntity("[Employees]", "Employees");
        }

        [Test]
        public void TestEntityBracketedWithSpace()
        {
            TestEntity("[Direct Reports]", "Direct Reports");
        }

        [Test]
        public void TestEntityBracketedWithNumbers()
        {
            TestEntity("[09]", "09");
        }

        [Test]
        public void TestEntityBracketedWithBasicPunctuation()
        {
            TestEntity("[~!@#$%^&*()_+-={}:;/.,?><]", "~!@#$%^&*()_+-={}:;/.,?><");
        }

        [Test]
        public void TestEntityBracketedWithTrickyPunctuation()
        {
            TestEntity("[[]", "[");
            TestEntity("[\"]", "\"");
            TestEntity("[']", "'");
        }

        [Test]
        public void TestEntityBracketedWithSingleEscapeSequence()
        {
            TestEntity("[a]]b]", "a]b");
        }

        [Test]
        public void TestEntityBracketedWithMixedEscapeSequences()
        {
            TestEntity("[a]]]", "a]");
            TestEntity("[]]b]", "]b");
            TestEntity("[a]]]]b]", "a]]b");
        }


        #endregion

        #region Entity Source Tests

        [Test]
        public void TestSourceTestData()
        {
            var instruction = RunSingleTest("if testdata");
            CheckRepeatInstruction(instruction, "if", "testdata");
        }

        [Test]
        public void TestSourceLoad()
        {
            var instruction = RunSingleTest("if load Building Norwest");
            CheckRepeatInstruction(instruction, "if", "load", "Building", "Norwest");
        }

        [Test]
        [ExpectedException]
        public void TestSourceLoad_MissingName()
        {
            RunSingleTest("if load Building");
        }

        [Test]
        [ExpectedException]
        public void TestSourceLoad_MissingType()
        {
            RunSingleTest("if load");
        }
        #endregion

        #region Entity Behavior

        [Test]
        public void TestBehaviorWith()
        {
            var instruction = RunSingleTest("with testdata");
            CheckRepeatInstruction(instruction, "with", "testdata");
        }

        [Test]
        public void TestBehaviorIf()
        {
            var instruction = RunSingleTest("if testdata");
            CheckRepeatInstruction(instruction, "if", "testdata");
        }

        [Test]
        public void TestBehaviorForce()
        {
            var instruction = RunSingleTest("force testdata");
            CheckRepeatInstruction(instruction, "force", "testdata");
        }

        [Test]
        public void TestBehaviorRows()
        {
            var instruction = RunSingleTest("rows testdata");
            CheckRepeatInstruction(instruction, "rows", "testdata");
        }

        [Test]
        public void TestBehaviorList()
        {
            var instruction = RunSingleTest("list testdata");
            CheckRepeatInstruction(instruction, "list", "testdata");
        }

        #endregion



        #region Test Helpers

        /// <summary>
        /// Parse a query and return the first instruction.
        /// </summary>
        private ParseTreeNode RunSingleTest(string script)
        {
            var result = DocMacroGrammar.ParseMacro(script);

            var root = CheckTerm(result.Root, DocTerms.InstructionSet);
            var instruction = root.ChildNodes[0];
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

        /// <summary>
        /// Validate that the node represents the expected relationship instruction.
        /// </summary>
        private void CheckRepeatInstruction(ParseTreeNode node, string expectedBehavior, string expectedSource, string sourceArg1 = null, string sourceArg2 = null)
        {
            var relInstr = CheckTerm(node, DocTerms.RelateInstruction);

            ParseTreeNode listBehavior = null;
            ParseTreeNode listSource;
            Assert.IsTrue(relInstr.ChildNodes.Count < 3);
            if (relInstr.ChildNodes.Count == 1)
            {
                listSource = CheckTerm(relInstr.ChildNodes[0], DocTerms.ListSource);
            }
            else
            {
                listBehavior = CheckTerm(relInstr.ChildNodes[0], DocTerms.ListBehavior);
                listSource = CheckTerm(relInstr.ChildNodes[1], DocTerms.ListSource);
            }

            if (listBehavior != null)
            {
                CheckKeyword(listBehavior.ChildNodes[0], expectedBehavior);
            }
            CheckKeyword(listSource.ChildNodes[0], expectedSource);

            if (sourceArg1 != null)
            {
                var entity1 = CheckTerm(listSource.ChildNodes[1], Terms.Identifier);
                CheckIdentifier(entity1, sourceArg1);
            }

            if (sourceArg2 != null)
            {
                var entity2 = CheckTerm(listSource.ChildNodes[2], Terms.Identifier);
                CheckIdentifier(entity2, sourceArg2);
            }
        }

        /// <summary>
        /// Test various ways of identifying an entity literal.
        /// </summary>
        public void TestEntity(string inputText, string expected = null)
        {
            var instruction = RunSingleTest(inputText);
            CheckField(instruction, expected);
        }

        /// <summary>
        /// Validate that the node represents the expected field instruction.
        /// </summary>
        public void CheckField(ParseTreeNode instruction, string expectedFieldIdentifier)
        {
            var fieldInstruction = CheckTerm(instruction, DocTerms.WriteInstruction);

            ParseTreeNode exprNode;
            if (fieldInstruction.ChildNodes[0].Term.Name == Keywords.Show)
                exprNode = fieldInstruction.ChildNodes[1];
            else
                exprNode = fieldInstruction.ChildNodes[0];

            CheckTerm(exprNode, Terms.Expression);
            var basicExpr = exprNode.ChildNodes[0];
            CheckTerm(basicExpr, Terms.MemberAccess);
            CheckIdentifier(basicExpr.ChildNodes[0], expectedFieldIdentifier);
        }

        /// <summary>
        /// Validate that the node represents the expected metadata instruction.
        /// </summary>
        public void CheckMetadata(ParseTreeNode instruction, string expectedKeyword)
        {
            var fieldInstruction = CheckTerm(instruction, DocTerms.MetadataInstruction);

            CheckKeyword(fieldInstruction.ChildNodes[0], expectedKeyword);
        }

        #endregion


    }
}
