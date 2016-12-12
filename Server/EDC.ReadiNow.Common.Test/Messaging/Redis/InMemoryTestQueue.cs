// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
    /// <summary>
    /// Used for testing 
    /// </summary>
    class InMemoryTestQueue<T> : IQueue<T>
    {
	    readonly ConcurrentQueue<T> _queue;

        public string Name { get; }

        public InMemoryTestQueue()
        {
            _queue = new ConcurrentQueue<T>();
            Name = "LocalQueue " + Guid.NewGuid();
        }

        public T Dequeue(CommandFlags flags = CommandFlags.None)
        {
			T msg;
			if ( _queue.TryDequeue( out msg ) )
			{
				return msg;
			}

            return default(T);
        }

        public void Enqueue(T[] values, CommandFlags flags = CommandFlags.None)
        {
			foreach ( var value in values )
			{
				_queue.Enqueue( value );
			}
        }

        public void Enqueue(T value, CommandFlags flags = CommandFlags.None)
        {
            _queue.Enqueue(value);
        }

        public long Length => _queue.Count;
    }
}
