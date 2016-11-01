// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EntityBatch
{
    /// <summary>
    /// This collection is populated by fetching batches of entities with fields.
    /// In most cases this will be more efficient than the usual iteration behavior. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchingEntityEnumeration<T> : IEnumerable<T> where T : class, IEntity
    {
        const int DefaultBatchSize = 20;


        BatchingEnumeration<IEntityRef, T> _enumeration;

        public BatchingEntityEnumeration(IEnumerable<IEntityRef> list, params IEntityRef[] fields): this(list, DefaultBatchSize, fields)
        {
        }

        public BatchingEntityEnumeration(IEnumerable<IEntityRef> list, int batchSize, params IEntityRef[] fields)
        {   

            Func<IEnumerable<IEntityRef>, IEnumerable<T>> f = (fetchList) => Entity.Get<T>(fetchList.Select(r=>r.Id), fields);  // I don't like the convertion from a IEntityRef to a EntityRef

            _enumeration = new BatchingEnumeration<IEntityRef, T>(list, batchSize, f);
        }

        public IEnumerator<T> GetEnumerator()
        {
 	        return _enumeration.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
 	        return _enumeration.GetEnumerator();
        }
    }
       


}
