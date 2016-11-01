// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Model.Client;

// ReSharper disable once CheckNamespace
namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    /// <summary>
    ///     Console DI classes.
    /// </summary>
    public class FormModule : Module
    {
        /// <summary>
        ///     Register DI classes.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            // Cache of retrieved forms, invalidated by application metadata changes
            // (Shared per rule-set and invalidated on any application metadata changes)
            builder.Register(
                    cc =>
                        new CacheFactory {MetadataCache = true}.Create<long, EntityData>(
                            "FormController Secured Result"))
                .Named<ICache<long, EntityData>>("FormController Secured Result")
                .SingleInstance();
        }
    }
}