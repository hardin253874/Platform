// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace ReadiNow.EntityGraph.GraphModel
{
    /// <summary>
    /// Wrapper implementation of IEntityRepository that handles the various overloads of argument types.
    /// </summary>
    /// <remarks>
    /// This class is primarily intended to declutter GraphEntityRepository.
    /// </remarks>
    public class IdResolvingEntityRepository : IEntityRepository
    {
        readonly IEntityRepository<long> _innerRepository;

        public IdResolvingEntityRepository( IEntityRepository<long> innerRepository )
        {
            if (innerRepository == null)
                throw new ArgumentNullException("innerRepository");

            _innerRepository = innerRepository;
        }

        public IEntityRepository<long> Inner
        {
            get { return _innerRepository; }
        }

        private long GetId( string alias )
        {
            return EntityIdentificationCache.GetId( alias );
        }

        private long GetId( IEntityRef reference )
        {
            return reference.Id;
        }

        public IEntity Create( long type )
        {
            return _innerRepository.Create( type );
        }

        public T Create<T>( ) where T : class, IEntity
        {
            return _innerRepository.Create<T>( );
        }

        public IEntity Get( long id )
        {
            return _innerRepository.Get( id );
        }

        public T Get<T>( long id ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( id );
        }

        public IEntity Create( string type )
        {
            return _innerRepository.Create( GetId( type ) );
        }

        public IEntity Get( string id )
        {
            return _innerRepository.Get( GetId( id ) );
        }

        public T Get<T>( string id ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( GetId( id ) );
        }

        public IEntity Create( IEntityRef type )
        {
            return _innerRepository.Create( GetId( type ) );
        }

        public IEntity Get( IEntityRef id )
        {
            return _innerRepository.Get( GetId( id ) );
        }

        public T Get<T>( IEntityRef id ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( GetId( id ) );
        }
        
        public IEntity Get( long id, string preloadQuery )
        {
            return _innerRepository.Get( id, preloadQuery );
        }

        public T Get<T>( long id, string preloadQuery ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( id, preloadQuery );
        }

        public IEntity Get( string id, string preloadQuery )
        {
            return _innerRepository.Get( GetId( id ), preloadQuery );
        }

        public T Get<T>( string id, string preloadQuery ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( GetId( id ), preloadQuery );
        }

        public IEntity Get( IEntityRef id, string preloadQuery )
        {
            return _innerRepository.Get( GetId( id ), preloadQuery );
        }

        public T Get<T>( IEntityRef id, string preloadQuery ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( GetId( id ), preloadQuery );
        }

        public IReadOnlyCollection<IEntity> Get( IEnumerable<long> ids )
        {
            return _innerRepository.Get( ids );
        }

        public IReadOnlyCollection<T> Get<T>( IEnumerable<long> ids ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( ids );
        }

        public IReadOnlyCollection<IEntity> Get( IEnumerable<long> ids, string preloadQuery )
        {
            return _innerRepository.Get( ids, preloadQuery );
        }

        public IReadOnlyCollection<T> Get<T>( IEnumerable<long> ids, string preloadQuery ) where T : class, IEntity
        {
            return _innerRepository.Get<T>( ids, preloadQuery );
        }
    }
}
