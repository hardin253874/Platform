// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using EDC.Common;
using EDC.Core;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// An facade over the <see cref="EntityAccessControlChecker"/> to provide higher
    /// level access control functions.
    /// </summary>
    public class EntityAccessControlService : IEntityAccessControlService
    {
        internal const string MessageName = "Access Control Check";

        private Lazy<int> _traceLevelSetting = new Lazy<int>(
            () => ConfigurationSettings.GetServerConfigurationSection().Security.Trace,
            true);

        /// <summary>
        /// Create a new <see cref="EntityAccessControlService"/>.
        /// </summary>
        internal EntityAccessControlService()
            : this(new EntityAccessControlChecker())
        {
            // Do nothing
        }

        /// <summary>
        /// Create a new <see cref="EntityAccessControlService"/> wrapping the given <see cref="entityAccessControlChecker"/>.
        /// </summary>
        /// <param name="entityAccessControlChecker">
        /// The <see cref="IEntityAccessControlChecker"/> to use. This cannot be null.
        /// </param>
        /// <param name="traceLevelFactory">
        /// The optional factory for reading the trace level from configuration.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityAccessControlChecker"/> cannot be null.
        /// </exception>
        internal EntityAccessControlService(IEntityAccessControlChecker entityAccessControlChecker, 
            Func<int> traceLevelFactory = null)
        {
            if (entityAccessControlChecker == null)
            {
                throw new ArgumentNullException("entityAccessControlChecker");
            }

            EntityAccessControlChecker = entityAccessControlChecker;
            TraceLevelFactory = traceLevelFactory ?? GetTraceLevelSetting;
        }

        /// <summary>
        /// The <see cref="IEntityAccessControlChecker"/> used to actually perform
        /// security checks.
        /// </summary>
        internal IEntityAccessControlChecker EntityAccessControlChecker { get; private set; }

        /// <summary>
        /// The trace level read from configuration using <see cref="TraceLevelFactory"/>.
        /// </summary>
        internal SecurityTraceLevel TraceLevel
        {
            get
            {
                SecurityTraceLevel result;

                try
                {
                    result = (SecurityTraceLevel) TraceLevelFactory();
                    if (!Enum.IsDefined(typeof (SecurityTraceLevel), result))
                    {
                        throw new ArgumentException();
                    }
                }
                catch (Exception)
                {
                    EventLog.Application.WriteError("An error occured reading the 'trace' security setting. Assuming 'false' (do not trace).");
                    result = SecurityTraceLevel.DenyVerbose;
                }

                return result;
            }
        }

        /// <summary>
        /// The factory for reading <see cref="TraceLevel"/> from configuration.
        /// </summary>
        internal Func<int> TraceLevelFactory { get; private set; }

        /// <summary>
        /// Check wither the user has access to the specified entity.
        /// </summary>
        /// <param name="entity">
        ///     The entities to check for. This cannot be null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <returns>
        /// True if the current user has all the specified permissions to the specified 
        /// entity, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> cannot contain null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        public bool Check(EntityRef entity, IList<EntityRef> permissions)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            if ( SkipCheck( ) )
            {
                return true;
            }

            return Check(new[] {entity}, permissions).Values.All(x => x);
        }

        /// <summary>
        /// Check wither the user has access to the specified entity.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check for. This cannot be null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> cannot contain null.
        /// </exception>
        /// <returns>
        /// True if the current user has all the specified permissions to the specified 
        /// entity, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        public IDictionary<long, bool> Check(IList<EntityRef> entities, IList<EntityRef> permissions)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (entities.Contains(null))
            {
                throw new ArgumentException("Cannot check access for null entities", "entities");
            }
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            if (permissions.Contains(null))
            {
                throw new ArgumentException(@"Cannot contain null", "permissions");
            }
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("RequestContext not set");
            }

	        if (entities.Count == 0)
	        {
				return new Dictionary<long, bool>();
	        }
            if ( SkipCheck( ) )
            {
                return entities.ToDictionarySafe( x => x.Id, x => true );
            }

            // Only process the most specific permission
            IList<EntityRef> permissionsOptimised = permissions;
            if ( permissions.Count > 1 )
            {
                long mostSpecificPermission = Permissions.MostSpecificPermission( permissions.Select( perm => perm.Id ) );
                permissionsOptimised = new List<EntityRef> { new EntityRef( mostSpecificPermission ) };
            }

            IDictionary<long, bool> result;

            using (MessageContext messageContext = new MessageContext(MessageName, GetBehavior(entities.Select(e => e.Id))))
            {
                if (!AccessControl.EntityAccessControlChecker.SkipCheck(new EntityRef(RequestContext.GetContext().Identity.Id)))
                {
                    WriteHeaderMessage(entities, permissionsOptimised, messageContext);
                    result = EntityAccessControlChecker.CheckAccess(entities, permissionsOptimised, RequestContext.GetContext().Identity.Id);
                    WriteFooterMessage(result, messageContext);
                    if (ShouldWriteSecurityTraceMessage(result))
                    {
                        WriteSecurityTraceMessage(messageContext);
                    }
                }
                else
                {
                    result = entities.ToDictionary(e => e.Id, e => true);
                }
            }

            return result;
        }

        /// <summary>
        /// Throw a <see cref="PlatformSecurityException"/> if the user lacks access
        /// to all supplied entities.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check. This cannot be null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions to check for. This cannot be null.
        /// </param>
        /// <exception cref="PlatformSecurityException">
        /// The user lacks access to one or more entities.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Neither <paramref name="entities"/> nor <paramref name="permissions"/> cannot contain null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        public void Demand(IList<EntityRef> entities, IList<EntityRef> permissions)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if ( permissions == null )
            {
                throw new ArgumentNullException( "permissions" );
            }
#if DEBUG
            if ( entities.Contains( null ) )
            {
                throw new ArgumentException("Cannot demand access for null entities", "entities");
            }
            if (permissions.Contains(null))
            {
                throw new ArgumentException(@"Cannot contain null", "permissions");
            }
#endif
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("RequestContext not set");
            }

            IDictionary<long, bool> results;
            using (EDC.ReadiNow.Core.Cache.CacheManager.ExpectCacheMisses())
            {
                results = Check(entities, permissions);
            }

            if (results.Values.Any(x => !x))
            {
                throw new PlatformSecurityException(RequestContext.GetContext().Identity.Name, permissions, entities.Where(x => !results[x.Id]));
            }
        }

        /// <summary>
        /// Can the current user create an instance of the given <paramref name="entityType"/>?
        /// </summary>
        /// <param name="entityType">
        /// The <see cref="EntityType"/> to check. This cannot be null.
        /// </param>
        /// <returns>
        /// True if the user can create an instance of that type, false otherwise.
        /// </returns>
        public bool CanCreate(EntityType entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }

            if ( SkipCheck( ) )
            {
                return true;
            }

            return CanCreate(new[] { entityType }).Values.All(x => x);
        }

        /// <summary>
        /// Can the current user create an instance of the given <paramref name="entityTypes"/>?
        /// </summary>
        /// <param name="entityTypes">
        /// The <see cref="EntityType"/>s to check. This cannot be null or contain null.
        /// </param>
        /// <returns>
        /// A mapping of each entity type ID to whether they can be accessed (true) or not (false).
        /// </returns>
        public IDictionary<long, bool> CanCreate( IList<EntityType> entityTypes )
        {
            return CheckAccessRuleApplesToType( entityTypes, Permissions.Create );
        }

        /// <summary>
        /// Determines whether the specified access is granted to the type.
        /// For creates this means the user can create new instances of the type.
        /// For other permissions, it means there is at least one applicable access rule that may grant that permission to instances of that type.
        /// This IS NOT the API to use if you want to see if the type itself can be modified. Use Check or Demand instead.
        /// </summary>
        /// <param name="entityTypes">The entity types to check. This cannot be null.</param>
        /// <param name="permission">The permissions to check for. This cannot be null.</param>
        /// <returns>
        /// A mapping of the entity ID to a flag. The flag is true if
        /// the user has access, false if not.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        public IDictionary<long, bool> CheckAccessRuleApplesToType( IList<EntityType> entityTypes, EntityRef permission )
        { 
            if (entityTypes == null)
            {
                throw new ArgumentNullException("entityTypes");
            }
            if ( permission == null )
            {
                throw new ArgumentNullException( "permission" );
            }
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("RequestContext not set");
            }

            if ( SkipCheck( ) )
            {
                return entityTypes.ToDictionarySafe( x => x.Id, x => true );
            }

            IDictionary<long, bool> result;

            using (MessageContext messageContext = new MessageContext(MessageName, GetBehavior(entityTypes.Select(e => e.Id))))
            {
                WriteHeaderMessage(
                    entityTypes.Select(et => new EntityRef(et)).ToList(), 
                    new [] { Permissions.Create },
                    messageContext);
                EntityRef user = RequestContext.GetContext( ).Identity.Id;
                result = EntityAccessControlChecker.CheckTypeAccess(entityTypes, Permissions.Create, user);
                WriteFooterMessage(result, messageContext);
                if (ShouldWriteSecurityTraceMessage(result))
                {
                    WriteSecurityTraceMessage(messageContext);
                }
            }

            return result;
        }

        /// <summary>
        /// Clear all the security caches.
        /// </summary>
        public void ClearCaches()
        {
            foreach ( ICacheService cache in Factory.Current.Resolve<IEnumerable<ICacheService>>())
            {
                cache.Clear( );
            }
        }

        /// <summary>
        /// Get the <see cref="MessageContextBehavior"/> used for diagnostic logging.
        /// </summary>
        /// <param name="entityIds">
        /// The entity IDs the security check is being performed on.
        /// </param>
        /// <returns></returns>
        internal MessageContextBehavior GetBehavior(IEnumerable<long> entityIds)
        {
            if (entityIds == null)
            {
                throw new ArgumentNullException("entityIds");
            }
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("Request context not set");
            }

            MessageContextBehavior behavior;
            behavior = MessageContextBehavior.New;
            if (!SkipCheck()
                && (ForceSecurityTraceContext.EntitiesToTrace().Overlaps(entityIds)
                    || TraceLevel != SecurityTraceLevel.None))
            {
                behavior |= MessageContextBehavior.Capturing;
            }
            return behavior;
        }

        /// <summary>
        /// Should the security trace message be written?
        /// </summary>
        /// <param name="result">
        /// The results of the check. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="result"/> cannot be null.
        /// </exception>
        /// <returns>
        /// True if the message should be written out, false otherwise.
        /// </returns>
        internal bool ShouldWriteSecurityTraceMessage(IDictionary<long, bool> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            bool writeMessage;

            writeMessage = false;
            if (SkipCheck())
            {
                writeMessage = false;
            }
            else if (ForceSecurityTraceContext.EntitiesToTrace().Overlaps(result.Keys))
            {
                writeMessage = true;
            }
            else
            {
                switch (TraceLevel)
                {
                    case SecurityTraceLevel.None:
                        break;
                    case SecurityTraceLevel.DenyBasic:
                    case SecurityTraceLevel.DenyVerbose:
                        writeMessage = result.Any(kvp => !kvp.Value);
                        break;
                    case SecurityTraceLevel.AllBasic:
                    case SecurityTraceLevel.AllVerbose:
                        writeMessage = true;
                        break;
                    default:
                        EventLog.Application.WriteWarning("Unknown security trace level {0}", TraceLevel);
                        break;
                }
            }

            return writeMessage;
        }

        /// <summary>
        /// Write out the security trace message.
        /// </summary>
        /// <param name="messageContext">
        /// The <see cref="MessageContext"/> to get the message from. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal void WriteSecurityTraceMessage(MessageContext messageContext)
        {
            if (messageContext == null)
            {
                throw new ArgumentNullException("messageContext");
            }

            string message;

            message = messageContext.GetMessage();
            if (message.Length > 0)
            {
                WriteMessageToActivityLog(message);
                EventLog.Application.WriteTrace(message);
            }
        }

        /// <summary>
        /// Write the message to the activity log.
        /// </summary>
        /// <param name="message">
        /// The message to write. This cannot be null or empty.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="message"/> cannot be null or empty.
        /// </exception>
        internal static void WriteMessageToActivityLog(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            LogActivityLogEntry logActivityLogEntry;
            const int maxLength = 10000; // Max length of description field
            ICollection<string> messageChunks;

            messageChunks = message.Chunk(maxLength);
            using (new SecurityBypassContext())
            {
                foreach (string messageChunk in messageChunks)
                {
                    logActivityLogEntry = new LogActivityLogEntry
                    {
                        Name = "Access Check",
                        Description = messageChunk,
                        LogEventTime = DateTime.UtcNow,
                        LogEntrySeverity_Enum = LogSeverityEnum_Enumeration.InformationSeverity
                    };

                    Factory.ActivityLogWriter.WriteLogEntry(logActivityLogEntry.As<TenantLogEntry>());
                }
            }
        }

        /// <summary>
        /// Read the cache tracing setting in the security section of the config file.
        /// </summary>
        /// <returns>
        /// The setting.
        /// </returns>
        internal int GetTraceLevelSetting()
        {
            return _traceLevelSetting.Value;
        }

        /// <summary>
        /// Create a human readable description of the security check.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="permissions"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        internal void WriteHeaderMessage(IList<EntityRef> entities, IList<EntityRef> permissions, MessageContext messageContext)
        {
            if (messageContext == null)
            {
                throw new ArgumentNullException("messageContext");
            }

            messageContext.Append(
                () => string.Format(
                    "Access control check: Does user '{0}' have '{1}' access to entity(ies) '{2}'?",
                    RequestContext.GetContext().Identity.Name ?? "(null)",
                    permissions != null ? string.Join(", ", permissions.Select(Permissions.GetPermissionByAlias)) : "(null)",
                    entities != null ? string.Join(", ", entities) : "(null)"));

            // Problem: Need a way to get names without loading the entities. Otherwise, it creates an infinite loop.
            //messageContext.Append(
            //    () =>
            //    {
            //        IList<Resource> entitiesToCheck;

            //        using (new SecurityBypassContext())
            //        {
            //            entitiesToCheck = Entity.Get<Resource>(entities, new EntityRef("core:name")).ToList();
            //        }

            //        return string.Format(
            //            "Access control check: Does user '{0}' have '{1}' access to entity(ies) '{2}'?",
            //            RequestContext.GetContext().Identity.Name ?? "(null)",
            //            permissions != null
            //                ? string.Join(", ", permissions.Select(Permissions.GetPermissionAlias))
            //                : "(null)",
            //            entities != null
            //                ? string.Join(", ",
            //                    entitiesToCheck.Select(e => string.Format("'{0}' ({1})", e.Name, e.Id)))
            //                : "(null)");
            //    });
        }

        /// <summary>
        /// Create a human readable descriptio0n of the result of the security check.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        internal void WriteFooterMessage(IDictionary<long, bool> result, MessageContext messageContext)
        {
            if (messageContext == null)
            {
                throw new ArgumentNullException("messageContext");
            }

            messageContext.Append(() => "Result:");
            using (MessageContext innerMessageContext = new MessageContext(MessageName))
            {
                innerMessageContext.Append(() => string.Format("Allowed: {0}",
                     string.Join(", ", result.Where(kvp => kvp.Value)
                                            .Select(kvp => kvp.Key))));
                innerMessageContext.Append(() => string.Format("Denied: {0}",
                     string.Join(", ", result.Where(kvp => !kvp.Value)
                                            .Select(kvp => kvp.Key))));
            }
        }

        /// <summary>
        /// Returns true if we should skip the check, and immediately grant access, for the current user in the current context.
        /// </summary>
        /// <returns>True to skip checks and allow access.</returns>
        private bool SkipCheck( )
        {
            return AccessControl.EntityAccessControlChecker.SkipCheck( new EntityRef( RequestContext.GetContext( ).Identity.Id ) );
        }
    }
}
