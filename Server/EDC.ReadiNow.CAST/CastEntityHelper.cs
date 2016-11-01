// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST
{
    public class CastEntityHelper : ICastEntityHelper
    {
        private IEntityRepository EntityRepository { get; set; }

        public CastEntityHelper()
        {
            EntityRepository = Factory.Current.Resolve<IEntityRepository>();
        }

        public IManagedPlatform CreatePlatform()
        {
            return EntityRepository.Create<ManagedPlatform>();
        }

        public IPlatformFrontEnd CreatePlatformFrontEnd()
        {
            return EntityRepository.Create<PlatformFrontEnd>();
        }

        public IPlatformDatabase CreatePlatformDatabase()
        {
            return EntityRepository.Create<PlatformDatabase>();
        }

        public IManagedTenant CreateTenant()
        {
            return EntityRepository.Create<ManagedTenant>();
        }

        public IManagedApp CreateApp()
        {
            return EntityRepository.Create<ManagedApp>();
        }

        public IManagedAppVersion CreateAppVersion()
        {
            return EntityRepository.Create<ManagedAppVersion>();
        }

        public IManagedUserRole CreateRole()
        {
            return EntityRepository.Create<ManagedUserRole>();
        }

        public IManagedUser CreateUser()
        {
            return EntityRepository.Create<ManagedUser>();
        }

        public void Save(IEnumerable<IEntity> entities)
        {
            Entity.Save(entities.Where(e => e != null), false);
        }

        public void Delete(IEnumerable<EntityRef> entityRefs)
        {
            Entity.Delete(entityRefs.Where(e => e != null));
        }

        public T GetEntityByField<T>(EntityRef field, string value) where T : class, IEntity
        {
            return Entity.GetByField<T>(value, true, field).FirstOrDefault();
        }

        public IEnumerable<T> GetEntitiesByType<T>() where T : class, IEntity
        {
            return Entity.GetInstancesOfType<T>();
        }
    }
}
