// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;

namespace EDC.Collections.Generic
{
    // From sample at: http://msdn.microsoft.com/en-us/library/ff458671.aspx

    /// <summary>
    /// Thread-safe resource pool.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
                throw new ArgumentNullException( nameof( objectGenerator ) );
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
        }

        public T GetObject()
        {
            T item;
            if (_objects.TryTake(out item))
                return item;
            return _objectGenerator();
        }

        public void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}
