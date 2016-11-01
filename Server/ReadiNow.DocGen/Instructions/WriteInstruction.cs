// Copyright 2011-2016 Global Software Innovation Pty Ltd
using DocumentFormat.OpenXml.Wordprocessing;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Instruction to just write a single field 
    /// </summary>
    abstract class WriteInstruction : Instruction
    {
        /// <summary>
        /// Doc template instruction info
        /// </summary>
        public FieldData FieldData;


        /// <summary>
        /// Generates the content represented by this write instruction.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnGenerate(WriterContext context)
        {
            string value = GetText(context);

            // Find a run to clone
            Run sourceRun;
            if (FieldData.FieldType == FieldType.Simple)
            {
                // Get from fldSimple to rPr
                // i.e. <fldSimple> <r> <rPr>
                sourceRun = FieldData.SourceStart.SourceNode.FirstChild as Run;
            }
            else
            {
                // Get from the first run in the complex field to its properties
                // i.e. <r><rPr>..</rPr><w:fldChar w:fldCharType="begin" /></r>
                sourceRun = FieldData.SourceStart.SourceNode as Run;
            }
            if (sourceRun == null)
            {
                return; // assert false
            }

            // Clone the run
            Run run = (Run)sourceRun.CloneNode(false);
            if (sourceRun.RunProperties != null)
            {
                run.RunProperties = sourceRun.RunProperties.CloneNode(true) as RunProperties;
            }

            // Add text
            Text text = new Text(value);
            run.AppendChild(text);

            // Output the run
            context.Writer.Current.AppendChild(run);
        }


        /// <summary>
        /// Determine the text to be shown.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The text</returns>
        protected abstract string GetText(WriterContext context);

    }
}
