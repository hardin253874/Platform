// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Represents the type of field represented in the template Word document.
    /// </summary>
    enum FieldType
    {
        /// <summary>
        /// Consists of a single fldSimple element.
        /// </summary>
        Simple,
        
        /// <summary>
        /// Consists of multiple fldChar elements over multiple runs.
        /// </summary>
        Complex
    }


    /// <summary>
    /// Represents a field found in the template Word document.
    /// </summary>
    class FieldData
    {
        /// <summary>
        /// Keeps track of where we're up to with parsing this field if it is a complex field.
        /// </summary>
        public FieldState FieldState;


        /// <summary>
        /// Element in the source stream where the field starts
        /// Note: for complex fields this is the 'Run' that contains the first 'begin' instruction.
        /// For simple fields, this is the fldSimple element.
        /// In both cases, it is the element that appears within the paragraph (as peers to other runs)
        /// </summary>
        public Token SourceStart;


        /// <summary>
        /// Element in the source stream where the field ends.
        /// </summary>
        public Token SourceEnd;

        /// <summary>
        /// Full instruction (inclusive of MERGEFIELD details).
        /// </summary>
        public string Instruction;


        /// <summary>
        /// Macro portion of the instruction, often just the field name (decoded raw data entered into mergefield).
        /// </summary>
        public string Macro;


        /// <summary>
        /// If true, then this is a SimpleField.
        /// </summary>
        public FieldType FieldType;


        /// <summary>
        /// If true, then this field contains other fields within its instruction or data (and therefore cannot be processed).
        /// </summary>
        public bool ContainsNestedFields;
    }
}
