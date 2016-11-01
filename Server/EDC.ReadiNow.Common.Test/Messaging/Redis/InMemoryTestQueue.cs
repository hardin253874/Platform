// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Test.Messaging.Redis
{
    /// <summary>
    /// Used for testing 
    /// </summary>
    class InMemoryTestQueue<T> : IQueue<T>
    {
        readonly object _syncRoot = new object();

        Queue<T> q;

        public string Name { get; }

        public InMemoryTestQueue()
        {
            q = new Queue<T>();
            Name = "LocalQueue " + Guid.NewGuid();
        }

        public T Dequeue(CommandFlags flags = CommandFlags.None)
        {
            lock(_syncRoot)
            {
                if (q.Count > 0)
                    return q.Dequeue();

                return default(T);
            }
        }

        public void Enqueue(T[] values, CommandFlags flags = CommandFlags.None)
        {
            lock(_syncRoot)
            {
                foreach (var value in values)
                    q.Enqueue(value);
            }
        }

        public void Enqueue(T value, CommandFlags flags = CommandFlags.None)
        {
            lock(_syncRoot)
            {
                q.Enqueue(value);
            }
        }

        public long Length
        {
            get
            {
                lock (_syncRoot)
                {
                    return q.Count;
                }
            }
        }
    }
}
