// Copyright 2011-2016 Global Software Innovation Pty Ltd
using DocumentFormat.OpenXml.Wordprocessing;

namespace ReadiNow.DocGen
{
    class WriteMessageInstruction : Instruction
    {
        public string Message { get; set; }

        /// <summary>
        /// Render content for this iteration of this instruction.
        /// </summary>
        /// <param name="context">The writing context.</param>
        protected override void OnGenerate(WriterContext context)
        {
            WriteRun(context);
        }

        private void WriteRun(WriterContext context)
        {
            Run run = new Run();

            // Format run
            RunProperties runProperties = run.AppendChild(new RunProperties());
            Color color = new Color { Val = "CC0000" };
            runProperties.AppendChild(color);

            // Add text
            Text text = new Text(Message + " ");
            run.AppendChild(text);

            // Output run
            context.Writer.Current.AppendChild(run);
        }


    }
}
