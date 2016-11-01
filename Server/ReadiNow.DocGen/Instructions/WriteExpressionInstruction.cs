// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.DocGen
{
    // Instruction to just write a single field
    class WriteExpressionInstruction : WriteInstruction
    {
        /// <summary>
        /// Field definition of the field to be written.
        /// </summary>
        public IExpression Expression { get; set; }


        /// <summary>
        /// Determine the text to be shown.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The text</returns>
        protected override string GetText(WriterContext context)
        {
            if (context.CurrentEntity == TestSource.TestEntity)
                return "Test";

            // Get current entity
            var evalSettings = new EvaluationSettings
            {
                ContextEntity = context.CurrentEntity,
                TimeZoneName = context.Settings.TimeZoneName
            };

            try
            {
                ExpressionRunResult runResult = context.ExternalServices.ExpressionRunner.Run( Expression, evalSettings );
                object oResult = runResult.Value;

                string result;
                if (oResult is IEnumerable<string> )
                {
                    result = string.Join(", ", (IEnumerable<string>) oResult);
                }
                else
                {
                    result = (string) oResult;
                }
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        /// <summary>
        /// Show debug info for this instruction.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="indent">The indent.</param>
        public override void OnDebug(TextWriter writer, int indent)
        {
            writer.Write(" expression: " + FieldData.Macro);
        }


    }
}
