// Copyright 2011-2016 Global Software Innovation Pty Ltd
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging
{
    /// <summary>
    /// A distributed queue
    /// </summary>
    public interface IQueue<T> 
    {
        /// <summary>
        /// The name of the queue
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Add an entry to the queue
        /// </summary>
        /// <param name="value">The value to add</param>
        /// <param name="flags">Flags to pas through to Redis</param>
        /// 
        void Enqueue(T value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Add an entry to the queue
        /// </summary>
        /// <param name="values">The values to add</param>
        /// <param name="flags">Flags to pas through to Redis</param>
        void Enqueue(T[] values, CommandFlags flags = CommandFlags.None);

	    /// <summary>
	    /// Remove an entry from the queue
	    /// </summary>
	    /// <param name="flags"></param>
	    /// <returns>True if the fetch succeeded</returns>
	    T Dequeue(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get the length of the queue
        /// </summary>
        long Length { get; }
    }
}
