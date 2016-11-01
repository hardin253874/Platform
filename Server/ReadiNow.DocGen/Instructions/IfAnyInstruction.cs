// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReadiNow.DocGen
{
    // Render a block if there is at least one relationship. Do not switch context
    class IfAnyInstruction : Instruction
    {
        /// <summary>
        /// Gets or sets the entity data source.
        /// </summary>
        public DataSource DataSource { get; set; }


        /// <summary>
        /// Generates the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnGenerate(WriterContext context)
        {
            // Get entities
            IEnumerable<DataElement> data = DataSource.GetData(context);

            if (data.Any())
            {
                WriteChildren(context);
            }
            else
            {
                context.Writer.RealignOnNextWrite();
            }
        }


        /// <summary>
        /// Show debug info for this instruction.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="indent">The indent.</param>
        public override void OnDebug(TextWriter writer, int indent)
        {
            DataSource.OnDebug(writer, indent);
        }

    }
}
