// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
    /// <summary>
    /// An transforming enumeration that provides batch fetching.
    /// Given an enumeration of R, it will produce a enumeration of T using a provided batching function to do the transformation 
    /// </summary>
    /// <typeparam name="R">The type of the original list elements</typeparam>
    /// <typeparam name="T">The type of the transformed list elements</typeparam>
    public class BatchingEnumeration<R, T> : IEnumerable<T>
    {
        BatchingEnumerator<R, T> _enumerator;

        /// <summary>
        /// Create a batched enumeration
        /// </summary>
        /// <param name="list">The list of things to transform</param>
        /// <param name="batchSize">The size of the batch</param>
        /// <param name="getBatchFn">The function that does the transformation.</param>
        public BatchingEnumeration(IEnumerable<R> list, int batchSize, Func<IEnumerable<R>, IEnumerable<T>> getBatchFn)
        {
            _enumerator = new BatchingEnumerator<R, T>(list, batchSize, getBatchFn);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerator;
        }
    }


    /// <summary>
    /// The enumerator
    /// </summary>
    /// <typeparam name="R">The type of the original list elements</typeparam>
    /// <typeparam name="T">The type of the transformed list elements</typeparam>
    public class BatchingEnumerator<R, T> : IEnumerator<T>
    {
        IEnumerable<R> _list;
        readonly IEnumerator<R> _listEnumerator;
        IEnumerator<T> _batchEnumerator;
        readonly int _batchSize;
        bool _listFinished;
        Func<IEnumerable<R>, IEnumerable<T>> _getBatchFn;

        public BatchingEnumerator(IEnumerable<R> list, int batchSize, Func<IEnumerable<R>, IEnumerable<T>> getBatchFn)
        {
            _list = list;
            _getBatchFn = getBatchFn;
            _listEnumerator = _list.GetEnumerator();
            _batchEnumerator = null;
            _batchSize = batchSize;

            FillBatch();
        }

        public T Current
        {
            get
            {
                return _batchEnumerator.Current;
            }
        }

        public void Dispose()
        {
            // Do nothing
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            var result = _batchEnumerator.MoveNext();

            if (!result && !_listFinished) // batch finished
            {
                FillBatch();
                result = _batchEnumerator.MoveNext();
            }

            return result;
        }

        public void Reset()
        {
            _listEnumerator.Reset();
        }


        /// <summary>
        /// Fill the cache with a batch of entities using the fields
        /// </summary>
        private void FillBatch()
        {
            var fetchList = new List<R>(_batchSize);

            for (int i = 0; i < _batchSize; i++)
            {
                _listFinished = !_listEnumerator.MoveNext();

                if (_listFinished)
                    break;

                fetchList.Add(_listEnumerator.Current);
            }

            var batch = fetchList.Count > 0 ? _getBatchFn(fetchList) : new List<T>();
            _batchEnumerator = batch.GetEnumerator();
        }
    }
    
}
