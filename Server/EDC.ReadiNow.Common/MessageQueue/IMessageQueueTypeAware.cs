// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Describes a message that is aware of it's own type. Suitable when remote communication involves complex types in an
    /// inheritance structure and the serialization path is not clear.
    /// </summary>
    public interface IMessageQueueTypeAware
    {
        /// <summary>
        /// The assembly qualified name of the type that this message instance believes itself to be.
        /// </summary>
        string Type { get; }
    }
}
