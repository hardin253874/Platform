// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Autofac;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security.AccessControl;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
    /// <summary>
    /// Autofac dependency injection module for report Web API.
    /// </summary>
    public class ReportApiModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // ReportResultCache 
            // (Register so that the cache invalidator receives notifications)
            builder.RegisterType<ReportResultCache>( )
                .As<ReportResultCache>( )
                .As<ICacheService>( )
                .SingleInstance( );            
        }
    }
}
