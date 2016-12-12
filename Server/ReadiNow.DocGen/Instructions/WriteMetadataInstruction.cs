// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.IO;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// A piece of metadata that can be written about an entity coming from a data source.
    /// </summary>
    internal enum MetadataField
    {
        Position
    }


    // Instruction to just write a single field
    class WriteMetadataInstruction : WriteInstruction
    {
        /// <summary>
        /// Field definition of the field to be written.
        /// </summary>
        public MetadataField MetadataField;


        /// <summary>
        /// Determine the text to be shown.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        /// The text
        /// </returns>
        protected override string GetText(WriterContext context)
        {
            DataElement data = context.CurrentDataElement;

            switch (MetadataField)
            {
                case MetadataField.Position:
                    return (data.Position + 1).ToString(CultureInfo.InvariantCulture);

                default:
                    throw new InvalidOperationException(MetadataField.ToString());
            }
        }


        /// <summary>
        /// Show debug info for this instruction.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="indent">The indent.</param>
        public override void OnDebug(TextWriter writer, int indent)
        {
            writer.Write( " " + MetadataField );
        }

    }
}
