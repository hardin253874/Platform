// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Represents the audit log.
    /// </summary>
    internal class AuditLog : IAuditLog
    {
        /// <summary>
        ///     The audit log writers.
        /// </summary>
        private readonly ICollection<IAuditLogWriter> _auditLogWriters;


        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLog" /> class.
        /// </summary>
        /// <param name="auditLogWriters">The audit log writers.</param>
        /// <exception cref="System.ArgumentNullException">auditLogWriters</exception>
        public AuditLog(IEnumerable<IAuditLogWriter> auditLogWriters)
        {
            if (auditLogWriters == null)
            {
                throw new ArgumentNullException(nameof(auditLogWriters));
            }

            _auditLogWriters = new List<IAuditLogWriter>(auditLogWriters);
        }


        #region IAuditLog Members


        /// <summary>
        ///     Writes an event to the audit log when a user account is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account being created.</param>
        public void OnCreateUserAccount(bool success, string userName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"createdUserName", userName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "createUserAccountAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user account is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account being deleted.</param>
        public void OnDeleteUserAccount(bool success, string userName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"deletedUserName", userName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "deleteUserAccountAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user account is renamed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="oldUserName">The old user name.</param>
        /// <param name="newUserName">The new user name.</param>
        public void OnRenameUserAccount(bool success, string oldUserName, string newUserName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"oldUserName", oldUserName},
                {"newUserName", newUserName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "renameUserAccountAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user account expires.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account that expired.</param>
        public void OnExpiredUserAccount(bool success, string userName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"expiredUserName", userName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "expireUserAccountAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when the user account password is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account whose password was changed.</param>
        public void OnChangeUserAccountPassword(bool success, string userName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"changedPasswordUserName", userName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changeUserAccountPasswordAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when the user account expiry is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account whose expiry was changed.</param>
        /// <param name="oldExpirationDate">The old expiration date.</param>
        /// <param name="newExpirationDate">The new expiration date.</param>
        public void OnChangeUserAccountExpiry(bool success, string userName, DateTime? oldExpirationDate, DateTime? newExpirationDate)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"changedExpiryUserName", userName},
                {"oldExpirationDate", oldExpirationDate},
                {"newExpirationDate", newExpirationDate}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changeUserAccountExpiryAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when the user account status is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account whose status was changed.</param>
        /// <param name="oldStatus">The old account status.</param>
        /// <param name="newStatus">The new account status.</param>
        public void OnChangeUserAccountStatus(bool success, string userName, string oldStatus, string newStatus)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"changedStatusUserName", userName},
                {"oldStatus", oldStatus},
                {"newStatus", newStatus}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changeUserAccountStatusAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        /// Writes an event to the audit log when the user account is locked.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account which was locked.</param>
        public void OnLockUserAccount(bool success, string userName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"lockedUserName", userName}                
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "lockUserAccountAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user logs on.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user who logged on.</param>
		/// <param name="userAgent">The agent that is used for making the request.</param>
        public void OnLogon(bool success, string userName, string userAgent)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"loggedOnUserName", userName},
                {"userAgent", userAgent ?? string.Empty}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "logonAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user logs off.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user who logged off.</param>
        public void OnLogoff(bool success, string userName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"loggedOffUserName", userName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "logoffAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user role is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="roleName">The name of the role being created.</param>
        public void OnCreateUserRole(bool success, string roleName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"createdRoleName", roleName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "createUserRoleAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user role is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="roleName">The name of the role being deleted.</param>
        public void OnDeleteUserRole(bool success, string roleName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"deletedRoleName", roleName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "deleteUserRoleAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user role is renamed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="oldRoleName">The old role name.</param>
        /// <param name="newRoleName">The new role name.</param>
        public void OnRenameUserRole(bool success, string oldRoleName, string newRoleName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"oldRoleName", oldRoleName},
                {"newRoleName", newRoleName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "renameUserRoleAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a user role's members are changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="addedMemberNames">The added member names.</param>
        /// <param name="removedMemberNames">The deleted member names.</param>
        public void OnChangeUserRoleMembers(bool success, string roleName, ISet<string> addedMemberNames, ISet<string> removedMemberNames)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"roleNameChangeMembers", roleName},
                {"addedMemberNames", addedMemberNames != null ? string.Join(",", addedMemberNames) : null},
                {"removedMemberNames", removedMemberNames != null ? string.Join(",", removedMemberNames) : null}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changeUserRoleMembersAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a tenant is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="tenantName">The name of the created tenant.</param>
        public void OnCreateTenant(bool success, string tenantName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"createdTenantName", tenantName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "createTenantAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when a tenant is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="tenantName">The name of the deleted tenant.</param>
        public void OnDeleteTenant(bool success, string tenantName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"deletedTenantName", tenantName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "deleteTenantAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an application is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">Name of the application being created.</param>
        public void OnCreateApplication(bool success, string applicationName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"createdApplicationName", applicationName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "createApplicationAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an application is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">The name of the application being deleted.</param>
        public void OnDeleteApplication(bool success, string applicationName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"deletedApplicationName", applicationName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "deleteApplicationAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an application is deployed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">The name of the application being deployed.</param>
        /// <param name="applicationVersion">The version of the application being deployed.</param>
        public void OnDeployApplication(bool success, string applicationName, string applicationVersion)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"deployedApplicationName", applicationName},
                {"deployedApplicationVersion", applicationVersion}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "deployApplicationAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an application is published.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">The name of the application being published.</param>
        /// <param name="applicationVersion">The version of the application being published.</param>
        public void OnPublishApplication(bool success, string applicationName, string applicationVersion)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"publishedApplicationName", applicationName},
                {"publishedApplicationVersion", applicationVersion}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "publishApplicationAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when the password policy is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="oldMinPasswordLength">The old minimum password length.</param>
        /// <param name="newMinPasswordLength">The new minimum password length.</param>
        /// <param name="oldMaxPasswordAge">The old maximum password age.</param>
        /// <param name="newMaxPasswordAge">The new maximum password age.</param>
        /// <param name="oldMustContainUpperCaseChars">The old must contain upper case chars setting.</param>
        /// <param name="newMustContainUpperCaseChars">The new must contain upper case chars setting.</param>
        /// <param name="oldMustContainLowerCaseChars">The old must contain lower case chars setting.</param>
        /// <param name="newMustContainLowerCaseChars">The new must contain lower case chars setting.</param>
        /// <param name="oldMustContainDigits">The old must contain digits setting.</param>
        /// <param name="newMustContainDigits">The new must contain digits setting.</param>
        /// <param name="oldMustContainSpecialChars">The old must contain special chars setting.</param>
        /// <param name="newMustContainSpecialChars">The new must contain special chars setting.</param>
        /// <param name="oldAccountLockoutDuration">The old account lockout duration.</param>
        /// <param name="newAccountLockoutDuration">The new account lockout duration.</param>
        /// <param name="oldAccountLockoutThreshold">The old account lockout threshold.</param>
        /// <param name="newAccountLockoutThreshold">The new account lockout threshold.</param>
        public void OnChangePasswordPolicy(bool success, int? oldMinPasswordLength, int? newMinPasswordLength, int? oldMaxPasswordAge, int? newMaxPasswordAge, bool? oldMustContainUpperCaseChars, bool? newMustContainUpperCaseChars, bool? oldMustContainLowerCaseChars, bool? newMustContainLowerCaseChars, bool? oldMustContainDigits, bool? newMustContainDigits, bool? oldMustContainSpecialChars, bool? newMustContainSpecialChars, int? oldAccountLockoutDuration, int? newAccountLockoutDuration, int? oldAccountLockoutThreshold, int? newAccountLockoutThreshold)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"oldMinimumPasswordLength", oldMinPasswordLength},
                {"newMinimumPasswordLength", newMinPasswordLength},
                {"oldMaximumPasswordAge", oldMaxPasswordAge},
                {"newMaximumPasswordAge", newMaxPasswordAge},
                {"oldMustContainUpperCaseCharacters", oldMustContainUpperCaseChars},
                {"newMustContainUpperCaseCharacters", newMustContainUpperCaseChars},
                {"oldMustContainLowerCaseCharacters", oldMustContainLowerCaseChars},
                {"newMustContainLowerCaseCharacters", newMustContainLowerCaseChars},
                {"oldMustContainDigits", oldMustContainDigits},
                {"newMustContainDigits", newMustContainDigits},
                {"oldMustContainSpecialCharacters", oldMustContainSpecialChars},
                {"newMustContainSpecialCharacters", newMustContainSpecialChars},
                {"oldAccountLockoutDuration", oldAccountLockoutDuration},
                {"newAccountLockoutDuration", newAccountLockoutDuration},
                {"oldAccountLockoutThreshold", oldAccountLockoutThreshold},
                {"newAccountLockoutThreshold", newAccountLockoutThreshold}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changePasswordPolicyAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an access rule is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="securedTypeName">Name of the secured type.</param>
        public void OnCreateAccessRule(bool success, string subjectName, string securedTypeName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"subjectNameCreateAccessRule", subjectName},
                {"securedTypeNameCreateAccessRule", securedTypeName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "createAccessRuleAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an access rule is enabled/disbaled.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        /// <param name="oldEnabled">The old enabled state.</param>
        /// <param name="newEnabled">The new enabled state.</param>
        public void OnEnableAccessRule(bool success, string subjectName, string securedTypeName, string accessRuleReportName, bool? oldEnabled, bool? newEnabled)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"oldAccessRuleEnabled", oldEnabled},
                {"newAccessRuleEnabled", newEnabled},
                {"subjectNameEnableAccessRule", subjectName},
                {"securedTypeNameEnableAccessRule", securedTypeName},
                {"accessRuleReportNameEnableAccessRule", accessRuleReportName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "enableAccessRuleAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an access rule is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        public void OnChangeAccessRuleQuery(bool success, string subjectName, string securedTypeName, string accessRuleReportName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"subjectNameChangeAccessRuleQuery", subjectName},
                {"securedTypeNameChangeAccessRuleQuery", securedTypeName},
                {"accessRuleReportNameChangeAccessRuleQuery", accessRuleReportName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changeAccessRuleQueryLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an access rule's permission are changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        /// <param name="oldPermissions">The old permissions.</param>
        /// <param name="newPermissions">The new permissions.</param>
        public void OnChangeAccessRulePermissions(bool success, string subjectName, string securedTypeName, string accessRuleReportName, ISet<string> oldPermissions, ISet<string> newPermissions)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"oldPermissionNamesChangeAccessRulePerm", oldPermissions != null ? string.Join(",", oldPermissions) : null},
                {"newPermissionNamesChangeAccessRulePerm", newPermissions != null ? string.Join(",", newPermissions) : null},
                {"subjectNameChangeAccessRulePerm", subjectName},
                {"securedTypeNameChangeAccessRulePerm", securedTypeName},
                {"accessRuleReportNameChangeAccessRulePerm", accessRuleReportName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "changeAccessRulePermissionsAuditLogEntryMetadata", messageParameters);
        }


        /// <summary>
        ///     Writes an event to the audit log when an access rule is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        public void OnDeleteAccessRule(bool success, string subjectName, string securedTypeName, string accessRuleReportName)
        {
            IDictionary<string, object> messageParameters = new Dictionary<string, object>
            {
                {"subjectNameDeleteAccessRule", subjectName},
                {"securedTypeNameDeleteAccessRule", securedTypeName},
                {"accessRuleReportNameDeleteAccessRule", accessRuleReportName}
            };

            // Create audit log entry
            WriteAuditLogEntry(success, "deleteAccessRuleAuditLogEntryMetadata", messageParameters);
        }

		/// <summary>
		///		Writes an event to the audit log when the tenant performs a rollback operation.
		/// </summary>
		/// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
		/// <param name="transactionId">The id of the transaction rolled back to.</param>
		public void OnTenantRollback( bool success, string transactionId )
		{
			IDictionary<string, object> messageParameters = new Dictionary<string, object>
			{
				{"transactionId", transactionId}
			};

			// Create audit log entry
			WriteAuditLogEntry( success, "tenantRollbackAuditLogEntryMetadata", messageParameters );
		}

        #endregion


        /// <summary>
        ///     Writes the audit log entry.
        /// </summary>
        /// WriteAuditLogEntry
        /// <param name="success">if set to <c>true</c> the state is success, false otherwise .</param>
        /// <param name="logEntryMetadataId">The log entry metadata identifier.</param>
        /// <param name="parameters">The parameters.</param>
        private void WriteAuditLogEntry(bool success, string logEntryMetadataId, IDictionary<string, object> parameters)
        {
            try
            {
                using (new SecurityBypassContext())
                {
                    if (_auditLogWriters.Count <= 0)
                    {
                        return;
                    }

                    var entryData = new AuditLogEntryData(success, logEntryMetadataId, parameters);

                    foreach (IAuditLogWriter auditLogWriter in _auditLogWriters)
                    {
                        // Don't let failures in one writer affect the others.
                        try
                        {
                            auditLogWriter.Write(entryData);
                        }
                        catch (Exception ex)
                        {
                            EventLog.Application.WriteError(
                                "An error occurred writing to the audit log. Writer type: {0}. Error: {1}.",
                                auditLogWriter.GetType().Name, ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Prevent audit log failures from affecting normal operations.
                EventLog.Application.WriteError("An error occurred writing event {0} to the audit log. Error: {1}.", logEntryMetadataId, ex.ToString());
            }
        }
    }
}