// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using EDC.ReadiNow.Expressions;
using ReadiNow.DocGen.DataSources;
using ReadiNow.DocGen.MacroParser;
using Irony.Parsing;

namespace ReadiNow.DocGen
{
    partial class OpenXmlReader
    {
        readonly ParseTreeHelper _parseTreeHelper;
        readonly ExternalServices _externalServices;

        public OpenXmlReader(ExternalServices externalServices)
        {
            if (externalServices == null)
                throw new ArgumentNullException("externalServices");

            _externalServices = externalServices;
            _parseTreeHelper = new ParseTreeHelper(externalServices);
        }

        private void ProcessValidField(ReaderContext context)
        {
            var fieldData = context.FieldData;

            // Remove the tokens associated with this field from the main token buffer, as we don't want it to be outputted.
            _tokenBuffer.RemoveTokensSince(fieldData.SourceStart);

            // Parse the macro
            ParseTree parseTree;
            try
            {
                parseTree = DocMacroGrammar.ParseMacro(fieldData.Macro);
            }
            catch (ParseException ex)
            {
                context.WriteError(ex.ShortMessage);
                return;
            }

            // Find the instruction node
            // TODO - handle multiple instructions
            var instructionSetNode = parseTree.Root;
            AssertTerm(instructionSetNode, DocTerms.InstructionSet);
            AssertChildren(instructionSetNode, 1);

            var instructionNode = instructionSetNode.ChildNodes[0];

            // And process it
            ProcessInstruction(context, instructionNode);
        }

        private void ProcessInstruction(ReaderContext context, ParseTreeNode instructionNode)
        {
            // Determine the term name
            string term = instructionNode.Term.Name;

            // And process it
            switch (term)
            {
                case Keywords.End:
                    ProcessEndInstruction(context);
                    break;

                case DocTerms.WriteInstruction:
                    ProcessWriteExpressionInstruction(context, instructionNode);
                    break;

                case DocTerms.MetadataInstruction:
                    ProcessWriteMetadataInstruction(context, instructionNode);
                    break;

                case DocTerms.RelateInstruction:
                    ProcessRelateInstruction(context, instructionNode);
                    break;

                default:
                    throw new InvalidOperationException(term);
            }
        }

        private void ProcessRelateInstruction(ReaderContext context, ParseTreeNode relateInstructionNode)
        {
            AssertTerm(relateInstructionNode, DocTerms.RelateInstruction);
            AssertChildren(relateInstructionNode, 1, 2);

            // Determine the term name
            bool isImplicitForEach = relateInstructionNode.ChildNodes[0].Term.Name == DocTerms.ListSource;
            string term;
            if (isImplicitForEach)
            {
                term = Keywords.With;
            }
            else
            {
                var behaviorNode = relateInstructionNode.ChildNodes[0];
                AssertTerm(behaviorNode, DocTerms.ListBehavior);
                AssertChildren(behaviorNode, 1);
                term = behaviorNode.ChildNodes[0].Token.ValueString;
            }

            // Process the list source
            ParseTreeNode sourceNode = relateInstructionNode.ChildNodes[isImplicitForEach ? 0 : 1];
            DataSource source;

            string error = null;
            try
            {
                source = _parseTreeHelper.ConvertListSource(context, sourceNode);
            }
            catch (ParseException ex)
            {
                error = ex.ShortMessage;
                source = new EmptySource();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                source = new EmptySource();
            }

            // And process the term
            Instruction instruction;
            switch (term)
            {
                case Keywords.With:
                    instruction = ProcessForEachInstruction(context, source);
                    break;

                case Keywords.If:
                    instruction = ProcessIfInstruction(context, source);
                    break;

                case Keywords.List:
                    instruction = ProcessListInstruction(context, source);
                    break;

                case Keywords.Rows:
                    instruction = ProcessRowsInstruction(context, source);
                    break;

                case Keywords.Force:
                    throw new NotImplementedException();

                default:
                    throw new InvalidOperationException(term);
            }

            if (instruction != null)
            {
                // Add the instruction (before push)
                AddInstruction(instruction);

                // Push the context
                _instructionStack.Push(instruction);

                if (error != null)
                {
                    instruction.HasUserError = true;
                    context.WriteErrorNoFlush(error);
                }
            }
            else
            {
                if (error != null)
                {
                    context.WriteError(error);
                }
            }
        }

        private void ProcessEndInstruction(ReaderContext context)
        {
            ProcessEndImplicit();
        }

        private void ProcessEndImplicit()
        {
            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            CurrentInstruction.ImplicitlyEndAfter = null;
            CurrentInstruction.ImplicitlyEndBefore = null;

            // Pop the current instruction
            if (_instructionStack.Count <= 1)
            {
                // log a warning .. popped off end
            }
            else
            {
                _instructionStack.Pop();
            }
        }

        private Instruction ProcessForEachInstruction(ReaderContext context, DataSource dataSource)
        {
            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            var rel = new ForEachInstruction { DataSource = dataSource };

            // Find the paragraph that contains the run
            OpenXmlElement paragraph = context.FieldData.SourceStart.SourceNode.Parent as Paragraph;
            if (paragraph != null)
            {
                // implicitly end at the end of the container that contains the current paragraph
                rel.ImplicitlyEndBefore = new Token(paragraph.Parent, true);        
            }

            return rel;
        }

        private Instruction ProcessListInstruction(ReaderContext context, DataSource dataSource)
        {
            // Find the paragraph that contains the run
            OpenXmlElement paragraph = context.FieldData.SourceStart.SourceNode.Parent as Paragraph;
            if (paragraph == null)
                return null;

            // Capture the paragraph
            int i = _tokenBuffer.FindTokenInBuffer(paragraph);
            var newBuffer = _tokenBuffer.SnipFromBuffer(i);

            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            // Update the token buffer with the snipped portion
            _tokenBuffer = newBuffer;

            var rel = new ForEachInstruction { DataSource = dataSource };
            rel.ImplicitlyEndAfter = new Token(paragraph, true);

            return rel;
        }

        private Instruction ProcessRowsInstruction(ReaderContext context, DataSource dataSource)
        {
            // Find the paragraph that contains the run
            OpenXmlElement paragraph = context.FieldData.SourceStart.SourceNode.Parent as Paragraph;
            if (paragraph == null)
                return null;

            // Find the row that contains the paragraph
            OpenXmlElement row = paragraph.Parent.Parent as TableRow;
            if (row == null)
                return null;

            // Capture the row
            int i = _tokenBuffer.FindTokenInBuffer(row);
            var newBuffer = _tokenBuffer.SnipFromBuffer(i);

            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            // Update the token buffer with the snipped portion
            _tokenBuffer = newBuffer;

            var rel = new ForEachInstruction { DataSource = dataSource };
            rel.ImplicitlyEndAfter = new Token(row, true);

            return rel;
        }

        private Instruction ProcessIfInstruction(ReaderContext context, DataSource dataSource)
        {
            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            IfAnyInstruction rel = new IfAnyInstruction { DataSource = dataSource };

            // Find the paragraph that contains the run
            OpenXmlElement paragraph = context.FieldData.SourceStart.SourceNode.Parent as Paragraph;
            if (paragraph != null)
            {
                // implicitly end at the end of the container that contains the current paragraph
                rel.ImplicitlyEndBefore = new Token(paragraph.Parent, true);
            }

            return rel;
        }

        private void ProcessWriteExpressionInstruction(ReaderContext context, ParseTreeNode writeInstructionNode)
        {
            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            // Get the expression parse node
            ParseTreeNode expressionParseNode = writeInstructionNode.ChildNodes[0];

            // Perform static analysis on expression
            IExpression expression;
            try
            {
                expression = _parseTreeHelper.ParseExpression(expressionParseNode, context, new ExprType(EDC.Database.DataType.String));
            }
            catch (ParseException ex)
            {
                context.WriteErrorNoFlush(ex.ShortMessage);
                return;
            }

            // Create the field instruction
            WriteExpressionInstruction fieldInstruction = new WriteExpressionInstruction();
            fieldInstruction.FieldData = context.FieldData;
            fieldInstruction.Expression = expression;

            AddInstruction(fieldInstruction);
        }

        private void ProcessWriteMetadataInstruction(ReaderContext context, ParseTreeNode metadataInstructionNode)
        {
            // Create an instruction for boring data encountered so far
            FlushTokenBufferToDataInstruction();

            // Get the field
            string metadataKeyword = metadataInstructionNode.ChildNodes[0].Token.ValueString;

            MetadataField field;
            switch (metadataKeyword)
            {
                case Keywords.Position:
                    field = MetadataField.Position;
                    break;
                default:
                    throw new NotImplementedException(metadataKeyword);
            }

            // Create the field instruction
            var metadataInstruction = new WriteMetadataInstruction();
            metadataInstruction.FieldData = context.FieldData;
            metadataInstruction.MetadataField = field;

            AddInstruction(metadataInstruction);
        }

        private static void AssertTerm(ParseTreeNode node, string termName)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (node.Term == null)
                throw new InvalidOperationException("node.Term");
            if (node.Term.Name != termName)
                throw new InvalidOperationException(string.Format("Expected {0}, found {1}.", termName, node.Term.Name));
        }

        private static void AssertChildren(ParseTreeNode node, int min, int max = int.MaxValue)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (node.ChildNodes == null)
                throw new InvalidOperationException("node.ChildNodes");
            if (node.ChildNodes.Count < min)
                throw new InvalidOperationException("Not enough ChildNodes on " + node.Term.Name); 
            if (node.ChildNodes.Count > max)
                throw new InvalidOperationException("Too many ChildNodes on " + node.Term.Name);
        }
    }
}
