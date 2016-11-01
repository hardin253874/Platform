// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Security.AccessControl.Diagnostics
{
    /// <summary>
    /// A context that tells the access control system to trace access checks for certain entities.
    /// </summary>
    /// <seealso cref="ForceSecurityTraceContextEntry"/>
    public class ForceSecurityTraceContext : IDisposable
    {
        internal static readonly string SlotName = "ReadiNow Force Security Trace Context";
        internal static readonly string EventLogSettingsAlias = "core:tenantEventLogSettingsInstance";

        /// <summary>
        /// Don't recalculate InspectingEntities any more frequently than this.
        /// </summary>
        internal static readonly long TicksToWaitBeforeRefreshing = TimeSpan.FromSeconds( 3 ).Ticks;

        private readonly ForceSecurityTraceContextEntry _entry;
        private bool _disposed;

        /// <summary>
        /// Create a new <see cref="MessageContext"/>.
        /// </summary>
        /// <param name="entitiesToTrace">
        /// Entities to trace.
        /// </param>
        public ForceSecurityTraceContext(params long[] entitiesToTrace)
        {
            if (entitiesToTrace == null)
            {
                throw new ArgumentNullException("entitiesToTrace");
            }

            _entry = new ForceSecurityTraceContextEntry(entitiesToTrace);
            ContextHelper<ForceSecurityTraceContextEntry>.PushContextData(SlotName, _entry);
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~ForceSecurityTraceContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        /// <param name="disposing">
        /// True if called from Dispose, false if called from the finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            // No unmanaged resources so disposing is unused

            if (!_disposed)
            {
                try
                {
                    ContextHelper<ForceSecurityTraceContextEntry>.PopContextData(SlotName, _entry);
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("Unable to free force security trace context data: {0}", ex);
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Has the context been set, i.e. are we in a using block? 
        /// </summary>
        /// <returns>
        /// True if the context has been set, false otherwise.
        /// </returns>
        public static bool IsSet()
        {
            return ContextHelper<ForceSecurityTraceContextEntry>.IsSet(SlotName);
        }

        /// <summary>
        /// Get the entities whose access control checks should be traced.
        /// </summary>
        public static ISet<long> EntitiesToTrace()
        {
            IEnumerable<ForceSecurityTraceContextEntry> forceSecurityTraceContextEntries;
            ISet<long> result;

            // Get values set programmatically (e.g. for automated tests)
            if ( IsSet( ) )
            {
                result = new HashSet<long>( );

                forceSecurityTraceContextEntries =
                    ContextHelper<ForceSecurityTraceContextEntry>.GetContextDataStack( SlotName );
                foreach ( ForceSecurityTraceContextEntry entry in forceSecurityTraceContextEntries )
                {
                    result.UnionWith( entry.EntitiesToTrace );
                }

                // Get values set by the user
                ISet<long> inspectingEntities = GetInspectingEntities( );
				if ( inspectingEntities.Count > 0 )
                {
                    result.UnionWith( inspectingEntities );
                }
            }
            else
            {
                result = GetInspectingEntities( );
            }

            return result;
        }

        /// <summary>
        /// Get IDs of entities tenant admins have requested security checks be traced for.
        /// </summary>
        /// <returns>
        /// Entity IDs.
        /// </returns>
        internal static ISet<long> GetInspectingEntities()
        {
            // Lazy cache invalidation
            long now = DateTime.UtcNow.Ticks;
            if ( now > _nextClear )
            {
                _nextClear = now + TicksToWaitBeforeRefreshing;
                _inspectingEntitiesCache.ClearAll( );
            }

            return _inspectingEntitiesCache.Value;
        }

        /// <summary>
        /// Get IDs of entities tenant admins have requested security checks be traced for.
        /// </summary>
        /// <returns>
        /// Entity IDs.
        /// </returns>
        private static ISet<long> GetInspectingEntitiesImpl( long tenantId )
        {
            EventLogSettings eventLogSettings = null;
            ISet<long> result;

            using ( new SecurityBypassContext( ) )
            {
                try
                {
                    eventLogSettings = Entity.Get<EventLogSettings>( EventLogSettingsAlias,
                        new IEntityRef [ ] { EventLogSettings.InspectSecurityChecksOnResource_Field } );
                    result = new HashSet<long>( eventLogSettings.InspectSecurityChecksOnResource.Select( e => e.Id ) );
                }
                catch ( Exception ex )
                {
                    EventLog.Application.WriteWarning( "Event Log settings instance missing: " + ex );
                    result = new HashSet<long>( );
                }
            }

            return result;
        }

        private static long _nextClear = DateTime.UtcNow.Ticks;

        private static TenantLocal<ISet<long>> _inspectingEntitiesCache = new TenantLocal<ISet<long>>( GetInspectingEntitiesImpl ); 

    }


}
