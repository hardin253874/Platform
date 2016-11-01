// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using EDC.Monitoring;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Monitoring;
using EDC.ReadiNow.Monitoring.Model;
using EDC.ReadiNow.Monitoring.Workflow;

namespace EDC.ReadiNow.Diagnostics.Tracing
{
	/// <summary>
	///     Event Tracing for Windows (ETW).
	/// </summary>
	[EventSource( Name = "EDC-SoftwarePlatform" )]
	public class PlatformTrace : EventSource
	{
		/// <summary>
		///     Static sync root.
		/// </summary>
		private static readonly object StaticSyncRoot = new object( );
        private static ISingleInstancePerformanceCounterCategory perfCounters = new SingleInstancePerformanceCounterCategory(PlatformTracePerformanceCounters.CategoryName);


		/// <summary>
		///     Singleton instance.
		/// </summary>
		private static readonly Lazy<PlatformTrace> TraceInstance = new Lazy<PlatformTrace>( ( ) => new PlatformTrace( ), false );

		/// <summary>
		///     Prevents a default instance of the <see cref="PlatformTrace" /> class from being created.
		/// </summary>
		private PlatformTrace( )
		{
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		public static PlatformTrace Instance
		{
			get
			{
				lock ( StaticSyncRoot )
				{
					return TraceInstance.Value;
				}
			}
		}

		/// <summary>
		///     Traces the entity cache hit.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 15, Task = Tasks.Entity, Opcode = Opcodes.CacaheHit, Level = EventLevel.Informational )]
		public void TraceEntityCacheHit( long entityId )
		{
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(PlatformTracePerformanceCounters.EntityCacheHitRateCounterName).Increment();

			if ( IsEnabled( ) )
			{
				WriteEvent( 15, entityId );
			}
		}

		/// <summary>
		///     Traces the entity cache miss.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 16, Task = Tasks.Entity, Opcode = Opcodes.CacheMiss, Level = EventLevel.Informational )]
		public void TraceEntityCacheMiss( long entityId )
		{
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(PlatformTracePerformanceCounters.EntityCacheMissRateCounterName).Increment();

			if ( IsEnabled( ) )
			{
				WriteEvent( 16, entityId );
			}
		}

		/// <summary>
		///     Traces the entity delete.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 1, Task = Tasks.Entity, Opcode = Opcodes.Delete, Level = EventLevel.Informational )]
		public void TraceEntityDelete( long entityId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 1, entityId );
			}
		}

		/// <summary>
		///     Traces the entity get by alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		[Event( 3, Task = Tasks.Entity, Opcode = Opcodes.Get, Level = EventLevel.Informational )]
		public void TraceEntityGetByAlias( string alias )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 3, alias );
			}
		}

		/// <summary>
		///     Traces the entity get by id.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 2, Task = Tasks.Entity, Opcode = Opcodes.Get, Level = EventLevel.Informational )]
		public void TraceEntityGetById( long entityId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 2, entityId );
			}
		}

		/// <summary>
		///     Traces the entity save.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 4, Task = Tasks.Entity, Opcode = Opcodes.Save, Level = EventLevel.Informational )]
		public void TraceEntitySave( long entityId )
		{
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(PlatformTracePerformanceCounters.EntitySaveRateCounterName).Increment();

			if ( IsEnabled( ) )
			{
				WriteEvent( 4, entityId );
			}
		}

		/// <summary>
		///     Traces the IPC broadcast.
		/// </summary>
		/// <param name="appDomainId">The app domain id.</param>
		[Event( 17, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.Broadcast, Level = EventLevel.Informational )]
		public void TraceIpcBroadcast( string appDomainId )
		{
            perfCounters.GetPerformanceCounter<RatePerSecond32PerformanceCounter>(PlatformTracePerformanceCounters.IpcBroadcastRateCounterName).Increment();

			if ( IsEnabled( ) )
			{
				WriteEvent( 17, appDomainId );
			}
		}

		/// <summary>
		///     Traces the IPC create event.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="appDomainId">The app domain id.</param>
		[Event( 18, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.CreateEvent, Level = EventLevel.Informational )]
		public void TraceIpcCreateEvent( string eventName, string appDomainId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 18, eventName, appDomainId );
			}
		}

		/// <summary>
		///     Traces the IPC create memory mapped file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="appDomainId">The app domain id.</param>
		[Event( 19, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.CreateMemoryMappedFile, Level = EventLevel.Informational )]
		public void TraceIpcCreateMemoryMappedFile( string fileName, string appDomainId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 19, fileName, appDomainId );
			}
		}

		/// <summary>
		///     Traces the IPC receive.
		/// </summary>
		/// <param name="appDomainId">The app domain id.</param>
		[Event( 22, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.Receive, Level = EventLevel.Informational )]
		public void TraceIpcReceive( string appDomainId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 22, appDomainId );
			}
		}

		/// <summary>
		///     Traces the IPC register.
		/// </summary>
		/// <param name="appDomainId">The app domain id.</param>
		[Event( 21, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.Register, Level = EventLevel.Informational )]
		public void TraceIpcRegister( string appDomainId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 21, appDomainId );
			}
		}

		/// <summary>
		///     Traces the IPC send.
		/// </summary>
		/// <param name="sourceAppDomainId">The source app domain id.</param>
		/// <param name="destinationAppDomainId">The destination app domain id.</param>
		[Event( 23, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.Send, Level = EventLevel.Informational )]
		public void TraceIpcSend( string sourceAppDomainId, string destinationAppDomainId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 23, sourceAppDomainId, destinationAppDomainId );
			}
		}

		/// <summary>
		///     Traces the IPC unregister.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="appDomainId">The app domain id.</param>
		[Event( 20, Task = Tasks.InterprocessCommunications, Opcode = Opcodes.Unregister, Level = EventLevel.Informational )]
		public void TraceIpcUnregister( string target, string appDomainId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 20, target, appDomainId );
			}
		}

		/// <summary>
		///     Traces the security cache deny.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="securityGroupId">The security group id.</param>
		/// <param name="permissionId">The permission id.</param>
		/// <param name="roleId">The role id.</param>
		/// <param name="userId">The user id.</param>
		[Event( 5, Task = Tasks.Security, Opcode = Opcodes.Deny, Level = EventLevel.Informational )]
		public void TraceSecurityCacheDeny( long entityId, long securityGroupId, long permissionId, long roleId, long userId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 5, entityId, securityGroupId, permissionId, roleId, userId );
			}
		}

		/// <summary>
		///     Traces the security cache failure.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="permissionId">The permission id.</param>
		/// <param name="userId">The user id.</param>
		[Event( 6, Task = Tasks.Security, Opcode = Opcodes.Failure, Level = EventLevel.Informational )]
		public void TraceSecurityCacheFailure( long entityId, long permissionId, long userId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 6, entityId, permissionId, userId );
			}
		}

		/// <summary>
		///     Traces the security cache grant.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="securityGroupId">The security group id.</param>
		/// <param name="permissionId">The permission id.</param>
		/// <param name="roleId">The role id.</param>
		/// <param name="userId">The user id.</param>
		[Event( 7, Task = Tasks.Security, Opcode = Opcodes.Grant, Level = EventLevel.Informational )]
		public void TraceSecurityCacheGrant( long entityId, long securityGroupId, long permissionId, long roleId, long userId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 7, entityId, securityGroupId, permissionId, roleId, userId );
			}
		}

		/// <summary>
		///     Traces the security cache invalidate.
		/// </summary>
		[Event( 8, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidate( )
		{

			if ( IsEnabled( ) )
			{
				WriteEvent( 8 );
			}
		}

		/// <summary>
		///     Traces the security cache invalidate entity.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 9, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateEntity( long entityId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 9, entityId );
			}
		}

		/// <summary>
		///     Traces the security cache invalidate permission.
		/// </summary>
		/// <param name="permissionId">The permission id.</param>
		[Event( 10, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidatePermission( long permissionId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 10, permissionId );
			}
		}

		/// <summary>
		///     Traces the security cache invalidate role.
		/// </summary>
		/// <param name="roleId">The role id.</param>
		[Event( 11, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateRole( long roleId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 11, roleId );
			}
		}

		/// <summary>
		///     Traces the security cache invalidate security group.
		/// </summary>
		/// <param name="securityGroupId">The security group id.</param>
		[Event( 12, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateSecurityGroup( long securityGroupId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 12, securityGroupId );
			}
		}

		/// <summary>
		///     Traces the type of the security cache invalidate.
		/// </summary>
		/// <param name="typeId">The type id.</param>
		[Event( 13, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateType( long typeId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 13, typeId );
			}
		}

		/// <summary>
		///     Traces the security cache invalidate user.
		/// </summary>
		/// <param name="userId">The user id.</param>
		[Event( 14, Task = Tasks.Security, Opcode = Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateUser( long userId )
		{
			if ( IsEnabled( ) )
			{
				WriteEvent( 14, userId );
			}
		}

        /// <summary>
        ///     Trace the entity access control check.
        /// </summary>
        [Event(24, Task = Tasks.Security, Opcode = Opcodes.AccessControlCheck, Level = EventLevel.Informational)]
        public void TraceSecurityCheck(string results, string permissions, string user, string cacheResults, long duration)
        {
            if (IsEnabled())
            {
                WriteEvent(24, results, permissions, user, cacheResults, duration);
            }
        }

        /// <summary>
        ///     Trace the whether the user can create an entity.
        /// </summary>
        [Event(25, Task = Tasks.Security, Opcode = Opcodes.AccessControlCanCreate, Level = EventLevel.Informational)]
        public void TraceSecurityCheckType(string results, string permission, string user, string entityType, long duration)
        {
            if (IsEnabled())
            {
                WriteEvent(25, results, permission, user, entityType, duration);
            }
        }

        /// <summary>
		///     Writes the event.
		/// </summary>
		/// <param name="eventId">The event id.</param>
		/// <param name="arg1">The arg1.</param>
		/// <param name="arg2">The arg2.</param>
		/// <param name="arg3">The arg3.</param>
		/// <param name="arg4">The arg4.</param>
		/// <param name="arg5">The arg5.</param>
		[NonEvent]
		public unsafe void WriteEvent( int eventId, long arg1, long arg2, long arg3, long arg4, long arg5 )
		{
			EventData* dataDesc = stackalloc EventData[5];

			dataDesc[ 0 ].DataPointer = ( IntPtr ) ( &arg1 );
			dataDesc[ 0 ].Size = 8;
			dataDesc[ 1 ].DataPointer = ( IntPtr ) ( &arg2 );
			dataDesc[ 1 ].Size = 8;
			dataDesc[ 2 ].DataPointer = ( IntPtr ) ( &arg3 );
			dataDesc[ 2 ].Size = 8;
			dataDesc[ 3 ].DataPointer = ( IntPtr ) ( &arg4 );
			dataDesc[ 3 ].Size = 8;
			dataDesc[ 4 ].DataPointer = ( IntPtr ) ( &arg5 );
			dataDesc[ 4 ].Size = 8;

			WriteEventCore( eventId, 5, dataDesc );
		}

		/// <summary>
		///     Trace keywords.
		/// </summary>
		public class Keywords
		{
		}

		/// <summary>
		///     Trace operation codes.
		/// </summary>
		/// <remarks>
		///     These must start from 0x10 since values preceding that are reserved.
		/// </remarks>
		public class Opcodes
		{
			/// <summary>
			///     Delete.
			/// </summary>
			public const EventOpcode Delete = ( EventOpcode ) 0x10;

			/// <summary>
			///     Deny.
			/// </summary>
			public const EventOpcode Deny = ( EventOpcode ) 0x11;

			/// <summary>
			///     Failure.
			/// </summary>
			public const EventOpcode Failure = ( EventOpcode ) 0x12;

			/// <summary>
			///     Get.
			/// </summary>
			public const EventOpcode Get = ( EventOpcode ) 0x13;

			/// <summary>
			///     Grant.
			/// </summary>
			public const EventOpcode Grant = ( EventOpcode ) 0x14;

			/// <summary>
			///     Invalidate.
			/// </summary>
			public const EventOpcode Invalidate = ( EventOpcode ) 0x15;

			/// <summary>
			///     Save.
			/// </summary>
			public const EventOpcode Save = ( EventOpcode ) 0x16;

			/// <summary>
			///     Cache hit.
			/// </summary>
			public const EventOpcode CacaheHit = ( EventOpcode ) 0x17;

			/// <summary>
			///     Cache miss.
			/// </summary>
			public const EventOpcode CacheMiss = ( EventOpcode ) 0x18;

			/// <summary>
			///     Broadcast.
			/// </summary>
			public const EventOpcode Broadcast = ( EventOpcode ) 0x19;

			/// <summary>
			///     Create Event.
			/// </summary>
			public const EventOpcode CreateEvent = ( EventOpcode ) 0x1A;

			/// <summary>
			///     Create Memory Mapped File.
			/// </summary>
			public const EventOpcode CreateMemoryMappedFile = ( EventOpcode ) 0x1B;

			/// <summary>
			///     Unregisters the AppDomain.
			/// </summary>
			public const EventOpcode Unregister = ( EventOpcode ) 0x1C;

			/// <summary>
			///     Registers the AppDomain.
			/// </summary>
			public const EventOpcode Register = ( EventOpcode ) 0x1D;

			/// <summary>
			///     Receive.
			/// </summary>
			public const EventOpcode Receive = ( EventOpcode ) 0x1E;

			/// <summary>
			///     Send
			/// </summary>
			public const EventOpcode Send = ( EventOpcode ) 0x1F;

            /// <summary>
            /// An entity access control check.
            /// </summary>
		    public const EventOpcode AccessControlCheck = (EventOpcode) 0x20;

            /// <summary>
            /// An entity access control "can create" check.
            /// </summary>
		    public const EventOpcode AccessControlCanCreate = (EventOpcode) 0x21;
		}

		/// <summary>
		///     Trace tasks.
		/// </summary>
		public class Tasks
		{
			/// <summary>
			///     Entity task.
			/// </summary>
			public const EventTask Entity = ( EventTask ) 1;

			/// <summary>
			///     Security task.
			/// </summary>
			public const EventTask Security = ( EventTask ) 2;

			/// <summary>
			///     IPC task.
			/// </summary>
			public const EventTask InterprocessCommunications = ( EventTask ) 3;
		}
	}
}