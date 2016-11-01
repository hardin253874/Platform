// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics.Tracing;

namespace EDC.Diagnostics.Tracing
{
	/// <summary>
	/// Event Tracing for Windows (ETW).
	/// </summary>
	[EventSource( Name = "EDC-SoftwarePlatform" )]
	public class PlatformTrace : EventSource
	{
		/// <summary>
		/// Static sync root.
		/// </summary>
		private static readonly object _staticSyncRoot = new object( );

		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static Lazy<PlatformTrace> _instance = new Lazy<PlatformTrace>( ( ) => new PlatformTrace( ), false );

		/// <summary>
		/// Prevents a default instance of the <see cref="PlatformTrace" /> class from being created.
		/// </summary>
		private PlatformTrace( )
		{
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		public static PlatformTrace Instance
		{
			get
			{
				lock ( _staticSyncRoot )
				{
					return _instance.Value;
				}
			}
		}

		/// <summary>
		/// Traces the entity delete.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 1, Task = PlatformTrace.Tasks.Entity, Opcode = PlatformTrace.Opcodes.Delete, Level = EventLevel.Informational )]
		public void TraceEntityDelete( long entityId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 1, entityId );
		}

		/// <summary>
		/// Traces the entity get by id.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 2, Task = PlatformTrace.Tasks.Entity, Opcode = PlatformTrace.Opcodes.Get, Level = EventLevel.Informational )]
		public void TraceEntityGetById( long entityId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 2, entityId );
		}

		/// <summary>
		/// Traces the entity get by alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		[Event( 3, Task = PlatformTrace.Tasks.Entity, Opcode = PlatformTrace.Opcodes.Get, Level = EventLevel.Informational )]
		public void TraceEntityGetByAlias( string alias )
		{
			if ( IsEnabled( ) )
				WriteEvent( 3, alias );
		}

		/// <summary>
		/// Traces the entity save.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 4, Task = PlatformTrace.Tasks.Entity, Opcode = PlatformTrace.Opcodes.Save, Level = EventLevel.Informational )]
		public void TraceEntitySave( long entityId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 4, entityId );
		}

		/// <summary>
		/// Traces the security cache deny.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="securityGroupId">The security group id.</param>
		/// <param name="permissionId">The permission id.</param>
		/// <param name="roleId">The role id.</param>
		/// <param name="userId">The user id.</param>
		[Event( 5, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Deny, Level = EventLevel.Informational )]
		public void TraceSecurityCacheDeny( long entityId, long securityGroupId, long permissionId, long roleId, long userId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 5, entityId, securityGroupId, permissionId, roleId, userId );
		}

		/// <summary>
		/// Traces the security cache failure.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="permissionId">The permission id.</param>
		/// <param name="userId">The user id.</param>
		[Event( 6, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Failure, Level = EventLevel.Informational )]
		public void TraceSecurityCacheFailure( long entityId, long permissionId, long userId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 6, entityId, permissionId, userId );
		}

		/// <summary>
		/// Traces the security cache grant.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="securityGroupId">The security group id.</param>
		/// <param name="permissionId">The permission id.</param>
		/// <param name="roleId">The role id.</param>
		/// <param name="userId">The user id.</param>
		[Event( 7, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Grant, Level = EventLevel.Informational )]
		public void TraceSecurityCacheGrant( long entityId, long securityGroupId, long permissionId, long roleId, long userId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 7, entityId, securityGroupId, permissionId, roleId, userId );
		}

		/// <summary>
		/// Traces the security cache invalidate.
		/// </summary>
		[Event( 8, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidate( )
		{
			if ( IsEnabled( ) )
				WriteEvent( 8 );
		}

		/// <summary>
		/// Traces the security cache invalidate entity.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		[Event( 9, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateEntity( long entityId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 9, entityId );
		}

		/// <summary>
		/// Traces the security cache invalidate permission.
		/// </summary>
		/// <param name="permissionId">The permission id.</param>
		[Event( 10, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidatePermission( long permissionId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 10, permissionId );
		}

		/// <summary>
		/// Traces the security cache invalidate role.
		/// </summary>
		/// <param name="roleId">The role id.</param>
		[Event( 11, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateRole( long roleId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 11, roleId );
		}

		/// <summary>
		/// Traces the security cache invalidate security group.
		/// </summary>
		/// <param name="securityGroupId">The security group id.</param>
		[Event( 12, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateSecurityGroup( long securityGroupId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 12, securityGroupId );
		}

		/// <summary>
		/// Traces the type of the security cache invalidate.
		/// </summary>
		/// <param name="typeId">The type id.</param>
		[Event( 13, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateType( long typeId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 13, typeId );
		}

		/// <summary>
		/// Traces the security cache invalidate user.
		/// </summary>
		/// <param name="userId">The user id.</param>
		[Event( 14, Task = PlatformTrace.Tasks.Security, Opcode = PlatformTrace.Opcodes.Invalidate, Level = EventLevel.Informational )]
		public void TraceSecurityCacheInvalidateUser( long userId )
		{
			if ( IsEnabled( ) )
				WriteEvent( 14, userId );
		}

		/// <summary>
		/// Writes the event.
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
			EventData* dataDesc = stackalloc PlatformTrace.EventData [ 5 ];

			dataDesc [ 0 ].DataPointer = ( IntPtr ) ( &arg1 );
			dataDesc [ 0 ].Size = 8;
			dataDesc [ 1 ].DataPointer = ( IntPtr ) ( &arg2 );
			dataDesc [ 1 ].Size = 8;
			dataDesc [ 2 ].DataPointer = ( IntPtr ) ( &arg3 );
			dataDesc [ 2 ].Size = 8;
			dataDesc [ 3 ].DataPointer = ( IntPtr ) ( &arg4 );
			dataDesc [ 3 ].Size = 8;
			dataDesc [ 4 ].DataPointer = ( IntPtr ) ( &arg5 );
			dataDesc [ 4 ].Size = 8;

			WriteEventCore( eventId, 5, dataDesc );
		}

		/// <summary>
		/// Trace keywords.
		/// </summary>
		public class Keywords
		{
		}

		/// <summary>
		/// Trace operation codes.
		/// </summary>
		/// <remarks>
		/// These must start from 0x10 since values preceding that are reserved.
		/// </remarks>
		public class Opcodes
		{
			/// <summary>
			/// Delete.
			/// </summary>
			public const EventOpcode Delete = ( EventOpcode ) 0x10;

			/// <summary>
			/// Deny.
			/// </summary>
			public const EventOpcode Deny = ( EventOpcode ) 0x11;

			/// <summary>
			/// Failure.
			/// </summary>
			public const EventOpcode Failure = ( EventOpcode ) 0x12;

			/// <summary>
			/// Get.
			/// </summary>
			public const EventOpcode Get = ( EventOpcode ) 0x13;

			/// <summary>
			/// Grant.
			/// </summary>
			public const EventOpcode Grant = ( EventOpcode ) 0x14;

			/// <summary>
			/// Invalidate.
			/// </summary>
			public const EventOpcode Invalidate = ( EventOpcode ) 0x15;

			/// <summary>
			/// Save.
			/// </summary>
			public const EventOpcode Save = ( EventOpcode ) 0x16;
		}

		/// <summary>
		/// Trace tasks.
		/// </summary>
		public class Tasks
		{
			/// <summary>
			/// Entity task.
			/// </summary>
			public const EventTask Entity = ( EventTask ) 1;

			/// <summary>
			/// Security task.
			/// </summary>
			public const EventTask Security = ( EventTask ) 2;
		}
	}
}