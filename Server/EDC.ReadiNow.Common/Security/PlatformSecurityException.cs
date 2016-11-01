// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	/// Thrown when an access control check fails.
	/// </summary>
	[Serializable]
    public class PlatformSecurityException : Exception
	{
	    private const string UserNameSerializationKey = "user";
        private const string PermissionsSerializationKey = "permissions";
        private const string EntitiesSerializationKey = "entities";

	    /// <summary>
		/// Initializes a new instance of the <see cref="PlatformSecurityException" /> class.
		/// </summary>
        public PlatformSecurityException()
			: this( "The requested security demand failed." )
		{
            // Do nothing
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformSecurityException" /> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public PlatformSecurityException( string message )
			: base( message )
		{
            UserName = string.Empty;
            PermissionAliases = new List<string>().AsReadOnly();
            EntityIds = new List<long>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformSecurityException" /> class.
        /// </summary>
        /// <paramref name="userName">
        /// The user the security check failed for. This cannot be null, empty or whitespace.
        /// </paramref>
        /// <paramref name="permissions">
        /// The permissions requested. This cannot be null or contain null.
        /// </paramref>
        /// <paramref name="entities">
        /// The entities the security check failed for. This cannot be null or contain null.
        /// </paramref>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <seealso cref="UserName"/>
        /// <seealso cref="PermissionAliases"/>
        /// <seealso cref="EntityIds"/>
        public PlatformSecurityException(string userName, IEnumerable<EntityRef> permissions, IEnumerable<EntityRef> entities)
            : base(CreateMessage(userName, permissions, entities))
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException("userName");
            }
            if (permissions == null || permissions.Any(p => p == null))
            {
                throw new ArgumentNullException("permissions");
            }
            if (entities == null || entities.Any(p => p == null))
            {
                throw new ArgumentNullException("entities");
            }

            UserName = userName;
            PermissionAliases = new List<string>(permissions.Select(x => x.Alias)).AsReadOnly();
            EntityIds = new List<long>(entities.Select(x => x.Id)).AsReadOnly();
        }

        /// <summary>
        /// Create the string for the Message property.
        /// </summary>
        /// <param name="userName">
        /// The user name. This can be null.
        /// </param>
        /// <param name="permissions">
        /// The permissions requested. This can be null.
        /// </param>
        /// <param name="entities">
        /// The entities access is requested to. This can be null.
        /// </param>
        /// <returns>
        /// The message.
        /// </returns>
	    internal static string CreateMessage(string userName, IEnumerable<EntityRef> permissions, IEnumerable<EntityRef> entities)
        {
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("Request context must be set");
            }

            IList<EntityRef> permissionList;
            IList<EntityRef> entityList;
            IList<EntityRef> typeBasedPermissions;
            IList<EntityRef> instanceBasedPermissions;
            StringBuilder message;
            Dictionary<string, string> permissionMap;
            IList<string> unknownPermissions;
            IList<EntityType> entityTypes;

            using (new SecurityBypassContext())
            {
                permissionMap = new Dictionary<string, string>();
                permissionMap["read"] = "view";
                permissionMap["modify"] = "edit";
                permissionMap["delete"] = "delete";
                permissionMap["create"] = "create";

                // Handle invalid arguments
                if (string.IsNullOrWhiteSpace(userName))
                {
                    userName = "(null)";
                }
                if (permissions == null)
                {
                    permissionList = new List<EntityRef>();
                }
                else
                {
                    permissionList = permissions.ToList();
                }
                if (entities == null)
                {
                    entityList = Enumerable.Empty<EntityRef>().ToList();
                }
                else
                {
                    entityList = entities.ToList();
                }

                // Flag unknown permissions
                unknownPermissions = permissionList
                    .Select(p => p.Alias ?? "(null)")
                    .Where(pa => !permissionMap.ContainsKey(pa))
                    .ToList();
				if ( unknownPermissions.Count > 0 )
                {
                    throw new ArgumentException(
                        string.Format(
                            "Unknown permission(s) {0}",
                            string.Join(", ", unknownPermissions)),
                        "permissions");
                }

                typeBasedPermissions = permissionList.Where(p => p.Alias == "create").ToList();
                instanceBasedPermissions = permissionList.Where(p => p.Alias != "create").ToList();

                message = new StringBuilder();
                using (new SecurityBypassContext())
                {
					if ( instanceBasedPermissions.Count > 0 )
                    {
                        message.AppendFormat("{0} does not have {1} access to {2}. ",
                            userName,
                            string.Join(
                                ", ", instanceBasedPermissions.Select(x => permissionMap[x.Alias])),
                            string.Join(
                                ", ",
                                entityList
                                    .Select(er =>
                                    {
                                        string result;
                                        if (er.Entity != null)
                                        {
                                            result = string.Format("{0} ({1})",
                                                er.Entity.GetField("core:name") ?? "(unknown)", er.Id);
                                        }
                                        else
                                        {
                                            result = string.Format("#{0}", er.Id);
                                        }
                                        return result;
                                    })));
                    }
					if ( typeBasedPermissions.Count > 0 )
                    {
                        entityTypes =
                            entityList
                                .Select(er => er.Entity.EntityTypes.First().As<EntityType>())
                                .Where(et => et != null)
                                .Distinct(new EntityIdEqualityComparer<EntityType>())
                                .ToList();

                        message.AppendFormat("{0} cannot create {1} objects. ",
                            userName,
                            string.Join(
                                ", ",
                                entityTypes
                                    .Select(et => string.Format("{0} ({1})", et.Name ?? "(unknown)", et.Id))));
                    }
                }
				if ( instanceBasedPermissions.Count <= 0 && typeBasedPermissions.Count <= 0 )
                {
                    message.AppendFormat("No permissions requested");
                }
            }

            return message.ToString();
	    }

	    /// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client.
		/// </summary>
		/// <param name="info">
		/// The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
		/// </param>
		protected PlatformSecurityException( SerializationInfo info, StreamingContext context )
			: base( info, context )
	    {
            UserName = info.GetString(UserNameSerializationKey);
            PermissionAliases = (ReadOnlyCollection<string>)info.GetValue(PermissionsSerializationKey, typeof(ReadOnlyCollection<string>));
            EntityIds = (ReadOnlyCollection<long>)info.GetValue(EntitiesSerializationKey, typeof(ReadOnlyCollection<long>));
        }

        /// <summary>
        /// Used for serialization.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(UserNameSerializationKey, UserName);
            info.AddValue(PermissionsSerializationKey, PermissionAliases, typeof(ReadOnlyCollection<string>));
            info.AddValue(EntitiesSerializationKey, EntityIds, typeof(ReadOnlyCollection<long>));
        }

        /// <summary>
        /// The user the security check failed for.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The permissions requested.
        /// </summary>
        public ReadOnlyCollection<string> PermissionAliases { get; private set; }

        /// <summary>
        /// The entities the security check failed for.
        /// </summary>
        public ReadOnlyCollection<long> EntityIds { get; private set; }

        /// <summary>
        /// Are the objects equal?
        /// </summary>
        /// <param name="obj">
        /// The object to compare.
        /// </param>
        /// <returns>
        /// True if they are equal, false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            PlatformSecurityException platformSecurityException;

            platformSecurityException = obj as PlatformSecurityException;
            return platformSecurityException != null
                && (UserName == platformSecurityException.UserName)
                && PermissionAliases.SequenceEqual(platformSecurityException.PermissionAliases)
                && EntityIds.SequenceEqual(platformSecurityException.EntityIds);
        }

        /// <summary>
        /// Construct a suitable hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
				int hashCode = 17;

                hashCode = hashCode * 92821 + base.GetHashCode( );

	            if ( UserName != null )
	            {
					hashCode = hashCode * 92821 + UserName.GetHashCode( );
	            }

	            if ( PermissionAliases != null )
	            {
					hashCode = PermissionAliases.Aggregate( hashCode, ( hc, alias ) => hc * 92821 + alias.GetHashCode( ) );
	            }

	            if ( EntityIds != null )
	            {
					hashCode = EntityIds.Aggregate( hashCode, ( hc, id ) => hc * 92821 + id.GetHashCode( ) );
	            }

				return hashCode;
            }
        }
	}
}