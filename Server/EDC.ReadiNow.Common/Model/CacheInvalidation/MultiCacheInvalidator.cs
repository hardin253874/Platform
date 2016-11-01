// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
    /// <summary>
    /// Cache invalidator that broadcasts to multiple cache invalidators.
    /// </summary>
    public class MultiCacheInvalidator : ICacheInvalidator
    {
        public static readonly string Autofac_Key = "Multi cache invalidator";

        string _name = "Multi cache invalidator";
        IEnumerable<ICacheInvalidator> _cacheInvalidators;

        public MultiCacheInvalidator( IEnumerable<ICacheInvalidator> cacheInvalidators, string name = null )
        {
            if ( cacheInvalidators == null )
                throw new ArgumentNullException( "cacheInvalidators" );
            
            _cacheInvalidators = cacheInvalidators;
            
            if ( name != null )
                _name = name;
        }

        /// <summary>
        /// Cache invalidator name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Notify of entity changes.
        /// </summary>
        public void OnEntityChange( IList<IEntity> entities, InvalidationCause cause, Func<long, EntityChanges> preActionModifiedRelatedEntities )
        {
            foreach ( ICacheInvalidator cacheInvalidator in _cacheInvalidators )
            {
                cacheInvalidator.OnEntityChange( entities, cause, preActionModifiedRelatedEntities );
            }
        }

        /// <summary>
        /// Notify of relationship instances created/removed of a particular relationship type.
        /// </summary>
        public void OnRelationshipChange( IList<EntityRef> relationshipTypes )
        {
            foreach ( ICacheInvalidator cacheInvalidator in _cacheInvalidators )
            {
                cacheInvalidator.OnRelationshipChange( relationshipTypes );
            }
        }

        /// <summary>
        /// Notify of field changes of a particular type.
        /// </summary>
        public void OnFieldChange( IList<long> fieldTypes )
        {
            foreach ( ICacheInvalidator cacheInvalidator in _cacheInvalidators )
            {
                cacheInvalidator.OnFieldChange( fieldTypes );
            }
        }
    }
}
