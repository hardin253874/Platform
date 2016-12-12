// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Collections.Generic;
using ReadiNow.Expressions.Parser;
using Irony.Parsing;

namespace ReadiNow.DocGen.MacroParser
{
    /// <summary>
    /// The parser for macros entered into the mergefield entries of the template document.
    /// </summary>
    public class DocMacroGrammar : ExpressionGrammar
    {
        /// <summary>
        /// Parses a macro.
        /// </summary>
        /// <param name="inputText">The macro script text.</param>
        /// <returns>The parse tree, starting with the InstructionSet node.</returns>
        public new static ParseTree ParseMacro(string inputText)
        {
            if (inputText == null)
                throw new ArgumentNullException();

            // Parse macro
            DocMacroGrammar grammar = _grammarPool.GetObject();
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
        /// Grammar constructor. Defines the grammar.
        /// </summary>
        private DocMacroGrammar() : base(false) // Disable constructor for expression grammar
        {
            EstablishExpressionGrammar();

            // Note: in the case of fields, the 'field' keyword is optional.
            // Note: in the case of relationships, the 'foreach' keyword is optional, and is presumed to be the default.
            // In both cases, the parser tree would be much more elegant if we could use ImpliedSymbolTerminal, but they don't
            // seem to work when used in conjunction with each other, or with Entity, which can match many things.

            // ReSharper disable InconsistentNaming

            // NonTerminals
            var InstructionSet = new NonTerminal(DocTerms.InstructionSet);
            var Instruction = new NonTerminal(DocTerms.Instruction);
            var FieldInstruction = new NonTerminal(DocTerms.WriteInstruction);
            var RelateInstruction = new NonTerminal(DocTerms.RelateInstruction);
            var ListBehavior = new NonTerminal(DocTerms.ListBehavior);
            var ListSource = new NonTerminal(DocTerms.ListSource);
            var MetadataInstruction = new NonTerminal(DocTerms.MetadataInstruction);

            // ReSharper restore InconsistentNaming

            Root = InstructionSet;

            // instruction-set :=
            //   instruction [ ';' instruction ]*
            InstructionSet.Rule = MakeListRule(InstructionSet, ToTerm(";"), Instruction);

            // instruction
            Instruction.Rule =
                ToTerm(Keywords.End)
                | RelateInstruction
                | MetadataInstruction
                | FieldInstruction;

            // field-instruction
            FieldInstruction.Rule =
                Expression
                | Keywords.Show + Expression;

            // metadata-instruction
            MetadataInstruction.Rule =
                ToTerm(Keywords.Position);

            // relate-instruction :=
            //   [list-behavior] list-source
            RelateInstruction.Rule =
                ListBehavior + ListSource;

            // list-behavior
            ListBehavior.Rule = ToTerm(Keywords.With) | Keywords.If | Keywords.Force | Keywords.Rows | Keywords.List;
            
            // list-source
            ListSource.Rule = 
                ToTerm(Keywords.TestData)
                | Expression
                | Keywords.Load + Identifier + Identifier;  // args: type, instance

            // Done to avoid matching keywords as entities
            MarkReservedWords(Keywords.AllKeywords);

            // Prevent these intermediate nodes from appearing in the result parse tree
            MarkTransient(Instruction);
        }

        #region Grammar Pool

        /// <summary>
        /// Singleton instance of grammar.
        /// Thread-safe. Grammar is verified on first access.
        /// </summary>
        private static DocMacroGrammar CreateGrammar()
        {
            var grammar = new DocMacroGrammar();

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
        private static readonly ObjectPool<DocMacroGrammar> _grammarPool = new ObjectPool<DocMacroGrammar>( CreateGrammar );

        #endregion
    }
}
