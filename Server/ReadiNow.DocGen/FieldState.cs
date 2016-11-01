// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Merge fields are often spread over multiple elements
    /// This enum tracks the state machine as we step over those elements 
    /// </summary>
    enum FieldState
    {
        /// <summary>
        /// In the first half of a field, where the instruction is encoded.
        /// </summary>
        InFieldCode,


        /// <summary>
        /// In the second half of a field, where the display text is encoded.
        /// </summary>
        InFieldText,


        /// <summary>
        /// Not currently in a field.
        /// </summary>
        Done,
    }

    
}
