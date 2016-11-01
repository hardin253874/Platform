// Copyright 2011-2016 Global Software Innovation Pty Ltd
extern alias EdcReadinowCommon;
using EntityType = EdcReadinowCommon::EDC.ReadiNow.Model.EntityType;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Context information passed around while reading the template document.
    /// </summary>
    class ReaderContext
    {
        /// <summary>
        /// The current XML template doc field.
        /// </summary>
        public FieldData FieldData { get; set; }


        /// <summary>
        /// The current parent instruction
        /// </summary>
        public Instruction CurrentInstruction { get; set; }


        /// <summary>
        /// The type of entity that is represented by the current instruction.
        /// </summary>
        public EntityType CurrentEntityType
        {
            get { return CurrentInstruction == null ? null : CurrentInstruction.ContextEntityType; }
        }


        /// <summary>
        /// The current parent instruction
        /// </summary>
        public OpenXmlReader OpenXmlReader { get; set; }


        /// <summary>
        /// Writes an error message instruction.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        public void WriteError(string message, params object[] args)
        {
            // Create an instruction for boring data encountered so far
            OpenXmlReader.FlushTokenBufferToDataInstruction();

            WriteErrorNoFlush(message, args);
        }

        /// <summary>
        /// Writes an error message instruction.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        public void WriteErrorNoFlush(string message, params object[] args)
        {
            // Create an instruction for boring data encountered so far
            OpenXmlReader.FlushTokenBufferToDataInstruction();

            string fullMsg = "«" + FieldData.Macro + "»: " + string.Format(message, args);

            var msg = new WriteMessageInstruction { Message = fullMsg };

            OpenXmlReader.AddInstruction(msg);

            if (OpenXmlReader.GeneratorSettings.ThrowOnError)
                throw new DocGenException(fullMsg);
        }


    }
}
