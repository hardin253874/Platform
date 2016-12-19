// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// A wrapper around the static, interface-less entity apis, so they can be mocked out and remove the dependency
    /// on the CAST application being installed to run unit tests.
    /// </summary>
    public interface ICastEntityHelper
    {
        IManagedPlatform CreatePlatform();

        IPlatformFrontEnd CreatePlatformFrontEnd();

        IPlatformDatabase CreatePlatformDatabase();

        IManagedTenant CreateTenant();

        IManagedApp CreateApp();

        IManagedAppVersion CreateAppVersion();

        IManagedRole CreateRole();

        IManagedUser CreateUser();

        void Save(IEnumerable<IEntity> entities);

        void Delete(IEnumerable<EntityRef> entityRefs);

        T GetEntityByField<T>(EntityRef field, string value) where T : class, IEntity;

        IEnumerable<T> GetEntitiesByType<T>() where T : class, IEntity;
    }
}
