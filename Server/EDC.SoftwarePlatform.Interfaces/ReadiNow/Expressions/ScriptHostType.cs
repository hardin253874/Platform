// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Specifies which engine will be performing the calculation. 
    /// Special restrictions may apply to scripts in certain circumstances.
    /// </summary>
    public enum ScriptHostType
    {
        /// <summary>
        /// Used by function that work in any scripting host.
        /// Used by syntax checkers that want to make available only options that are supported by any/every engine.
        /// </summary>
        Any,

        /// <summary>
        /// Declared on functions that are only supported in the report engine.
        /// Used by syntax checkers that want to make available options supported by the report engine.
        /// </summary>
        Report,

        /// <summary>
        /// Declared on functions that are only supported in the calculation engine.
        /// Used by syntax checkers that want to make available options supported by the evaluate engine.
        /// </summary>
        Evaluate
    }
}
