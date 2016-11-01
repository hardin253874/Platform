// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Diagnostics
{
    /// <summary>
    /// Modifiers for the <see cref="MessageContextEntry"/> behavior.
    /// </summary>
    [Flags]
    public enum MessageContextBehavior
    {
        /// <summary>
        /// The message context does not capture or store the message itself. It 
        /// writes the message to any capturing parent contexts but groups 
        /// (usually by an indent) any messages written to this message context 
        /// or child contexts.
        /// </summary>
        Default = 0,
        /// <summary>
        /// The message context stores any messages written to it or child contexts.
        /// </summary>
        Capturing = 1,
        /// <summary>
        /// The message context is a new context, meaning messages written to 
        /// this context entry or content entries beneath it should not be 
        /// written to parent contexts.
        /// </summary>
        New = 2
    }
}