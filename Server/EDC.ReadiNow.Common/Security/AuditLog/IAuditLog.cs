// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AuditLog
{
    /// <summary>
    ///     Handle audit log events.
    /// </summary>
    public interface IAuditLog
    {
        /// <summary>
        ///     Writes an event to the audit log when a user account is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account being created.</param>
        void OnCreateUserAccount(bool success, string userName);


        /// <summary>
        ///     Writes an event to the audit log when a user account is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account being deleted.</param>
        void OnDeleteUserAccount(bool success, string userName);


        /// <summary>
        ///     Writes an event to the audit log when a user account is renamed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="oldUserName">The old user name.</param>
        /// <param name="newUserName">The new user name.</param>
        void OnRenameUserAccount(bool success, string oldUserName, string newUserName);


        /// <summary>
        ///     Writes an event to the audit log when a user account expires.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account that expired.</param>
        void OnExpiredUserAccount(bool success, string userName);


        /// <summary>
        ///     Writes an event to the audit log when the user account password is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account whose password was changed.</param>
        void OnChangeUserAccountPassword(bool success, string userName);


        /// <summary>
        ///     Writes an event to the audit log when the user account expiry is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account whose expiry was changed.</param>
        /// <param name="oldExpirationDate">The old expiration date.</param>
        /// <param name="newExpirationDate">The new expiration date.</param>
        void OnChangeUserAccountExpiry(bool success, string userName, DateTime? oldExpirationDate, DateTime? newExpirationDate);


        /// <summary>
        ///     Writes an event to the audit log when the user account status is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account whose status was changed.</param>
        /// <param name="oldStatus">The old account status.</param>
        /// <param name="newStatus">The new account status.</param>
        void OnChangeUserAccountStatus(bool success, string userName, string oldStatus, string newStatus);


        /// <summary>
        /// Writes an event to the audit log when the user account is locked.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user account which was locked.</param>
        void OnLockUserAccount(bool success, string userName);


        /// <summary>
        ///     Writes an event to the audit log when a user logs on.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user who logged on.</param>
        /// <param name="userAgent">The agent that is used to make the connection.</param>
        void OnLogon(bool success, string userName, string userAgent);


        /// <summary>
        ///     Writes an event to the audit log when a user logs off.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="userName">The name of the user who logged off.</param>
        void OnLogoff(bool success, string userName);


        /// <summary>
        ///     Writes an event to the audit log when a user role is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="roleName">The name of the role being created.</param>
        void OnCreateUserRole(bool success, string roleName);


        /// <summary>
        ///     Writes an event to the audit log when a user role is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="roleName">The name of the role being deleted.</param>
        void OnDeleteUserRole(bool success, string roleName);


        /// <summary>
        ///     Writes an event to the audit log when a user role is renamed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="oldRoleName">The old role name.</param>
        /// <param name="newRoleName">The new role name.</param>
        void OnRenameUserRole(bool success, string oldRoleName, string newRoleName);


        /// <summary>
        /// Writes an event to the audit log when a user role's members are changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="addedMemberNames">The added member names.</param>
        /// <param name="removedMemberNames">The deleted member names.</param>
        void OnChangeUserRoleMembers(bool success, string roleName, ISet<string> addedMemberNames, ISet<string> removedMemberNames);


        /// <summary>
        ///     Writes an event to the audit log when a tenant is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="tenantName">The name of the created tenant.</param>
        void OnCreateTenant(bool success, string tenantName);


        /// <summary>
        ///     Writes an event to the audit log when a tenant is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="tenantName">The name of the deleted tenant.</param>
        void OnDeleteTenant(bool success, string tenantName);


        /// <summary>
        /// Writes an event to the audit log when an application is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">Name of the application being created.</param>
        void OnCreateApplication(bool success, string applicationName);


        /// <summary>
        ///     Writes an event to the audit log when an application is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">The name of the application being deleted.</param>
        void OnDeleteApplication(bool success, string applicationName);


        /// <summary>
        ///     Writes an event to the audit log when an application is deployed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">The name of the application being deployed.</param>
        /// <param name="applicationVersion">The version of the application being deployed.</param>
        void OnDeployApplication(bool success, string applicationName, string applicationVersion);


        /// <summary>
        ///     Writes an event to the audit log when an application is published.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="applicationName">The name of the application being published.</param>
        /// <param name="applicationVersion">The version of the application being published.</param>
        void OnPublishApplication(bool success, string applicationName, string applicationVersion);


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
        void OnChangePasswordPolicy(bool success, int? oldMinPasswordLength, int? newMinPasswordLength, int? oldMaxPasswordAge, int? newMaxPasswordAge, bool? oldMustContainUpperCaseChars, bool? newMustContainUpperCaseChars,
            bool? oldMustContainLowerCaseChars, bool? newMustContainLowerCaseChars, bool? oldMustContainDigits, bool? newMustContainDigits, bool? oldMustContainSpecialChars, bool? newMustContainSpecialChars,
            int? oldAccountLockoutDuration, int? newAccountLockoutDuration, int? oldAccountLockoutThreshold, int? newAccountLockoutThreshold);


        /// <summary>
        ///     Writes an event to the audit log when an access rule is created.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="securedTypeName">Name of the secured type.</param>
        void OnCreateAccessRule(bool success, string subjectName, string securedTypeName);


        /// <summary>
        /// Writes an event to the audit log when an access rule is enabled/disbaled.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        /// <param name="oldEnabled">The old enabled state.</param>
        /// <param name="newEnabled">The new enabled state.</param>
        void OnEnableAccessRule(bool success, string subjectName, string securedTypeName, string accessRuleReportName, bool? oldEnabled, bool? newEnabled);


        /// <summary>
        ///     Writes an event to the audit log when an access rule is changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        void OnChangeAccessRuleQuery(bool success, string subjectName, string securedTypeName, string accessRuleReportName);


        /// <summary>
        /// Writes an event to the audit log when an access rule's permission are changed.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        /// <param name="oldPermissions">The old permissions.</param>
        /// <param name="newPermissions">The new permissions.</param>
        void OnChangeAccessRulePermissions(bool success, string subjectName, string securedTypeName, string accessRuleReportName, ISet<string> oldPermissions, ISet<string> newPermissions);


        /// <summary>
        ///     Writes an event to the audit log when an access rule is deleted.
        /// </summary>
        /// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
        /// <param name="subjectName">Name of the access rule subject.</param>
        /// <param name="securedTypeName">Name of the access rule secured type.</param>
        /// <param name="accessRuleReportName">Name of the access rule report.</param>
        void OnDeleteAccessRule(bool success, string subjectName, string securedTypeName, string accessRuleReportName);

		/// <summary>
		///		Writes an event to the audit log when the tenant performs a rollback operation.
		/// </summary>
		/// <param name="success"><c>true</c> if the event succeeded, <c>false</c> otherwise.</param>
		/// <param name="transactionId">The id of the transaction rolled back to.</param>
		void OnTenantRollback( bool success, string transactionId );
    }
}