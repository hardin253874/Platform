// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AuditLog.EventTargets
{
    /// <summary>
    /// Audit log password policy event target.
    /// </summary>
    internal class AuditLogPasswordPolicyEventTarget : AuditLogEventTargetBase<PasswordPolicy, AuditLogPasswordPolicyEventTarget.AuditLogPasswordPolicyDetails>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogPasswordPolicyEventTarget"/> class.
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        public AuditLogPasswordPolicyEventTarget(IAuditLog auditLog) : base(auditLog)
        {
        }


        /// <summary>
        ///     Gets the name of save details state key.
        /// </summary>
        /// <value>
        ///     The save details state key.
        /// </value>
        protected override string SaveDetailsStateKey
        {
            get { return "AuditLogPasswordPolicyEventTarget:AuditLogSavePasswordPolicyDetails"; }
        }


        /// <summary>
        ///     Called to gather audit log entity details for save.
        /// </summary>
        /// <param name="passwordPolicy">The password policy.</param>
        /// <returns></returns>
        protected override AuditLogPasswordPolicyDetails OnGatherAuditLogEntityDetailsForSave(PasswordPolicy passwordPolicy)
        {
            IEntityFieldValues fields;
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
            IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;
            passwordPolicy.GetChanges(out fields, out forwardRelationships, out reverseRelationships, true, false, false);

            var oldPaswordPolicy = new Lazy<PasswordPolicy>(() => Entity.Get<PasswordPolicy>(passwordPolicy.Id));

            var passwordPolicyDetails = new AuditLogPasswordPolicyDetails();

            if (fields != null && fields.Any())
            {
                IEnumerable<EntityRef> idsToLoad = new List<EntityRef>
                {
                    "core:minimumPasswordLength", "core:maximumPasswordAge", "core:mustContainUpperCaseCharacters", "core:mustContainLowerCaseCharacters",
                    "core:mustContainDigits", "core:mustContainSpecialCharacters", "core:accountLockoutDuration", "core:accountLockoutThreshold"
                };

                Dictionary<string, IEntity> fieldEntities = Entity.Get(idsToLoad).ToDictionary(e => e.Alias);

                object fieldObj;

                if (fields.TryGetValue(fieldEntities["minimumPasswordLength"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewMinimumPasswordLength = fieldObj as int?;
                    passwordPolicyDetails.OldMinimumPasswordLength = oldPaswordPolicy.Value.MinimumPasswordLength;
                }

                if (fields.TryGetValue(fieldEntities["maximumPasswordAge"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewMaximumPasswordAge = fieldObj as int?;
                    passwordPolicyDetails.OldMaximumPasswordAge = oldPaswordPolicy.Value.MaximumPasswordAge;
                }

                if (fields.TryGetValue(fieldEntities["mustContainUpperCaseCharacters"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewMustContainUpperCaseCharacters = fieldObj as bool?;
                    passwordPolicyDetails.OldMustContainUpperCaseCharacters = oldPaswordPolicy.Value.MustContainUpperCaseCharacters;
                }

                if (fields.TryGetValue(fieldEntities["mustContainLowerCaseCharacters"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewMustContainLowerCaseCharacters = fieldObj as bool?;
                    passwordPolicyDetails.OldMustContainLowerCaseCharacters = oldPaswordPolicy.Value.MustContainLowerCaseCharacters;
                }

                if (fields.TryGetValue(fieldEntities["mustContainDigits"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewMustContainDigits = fieldObj as bool?;
                    passwordPolicyDetails.OldMustContainDigits = oldPaswordPolicy.Value.MustContainDigits;
                }

                if (fields.TryGetValue(fieldEntities["mustContainSpecialCharacters"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewMustContainSpecialCharacters = fieldObj as bool?;
                    passwordPolicyDetails.OldMustContainSpecialCharacters = oldPaswordPolicy.Value.MustContainSpecialCharacters;
                }

                if (fields.TryGetValue(fieldEntities["accountLockoutDuration"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewAccountLockoutDuration = fieldObj as int?;
                    passwordPolicyDetails.OldAccountLockoutDuration = oldPaswordPolicy.Value.AccountLockoutDuration;
                }

                if (fields.TryGetValue(fieldEntities["accountLockoutThreshold"].Id, out fieldObj))
                {
                    passwordPolicyDetails.NewAccountLockoutThreshold = fieldObj as int?;
                    passwordPolicyDetails.OldAccountLockoutThreshold = oldPaswordPolicy.Value.AccountLockoutThreshold;
                }
            }

            return passwordPolicyDetails;
        }


        /// <summary>
        /// Called to write save audit log entries.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="accessRuleDetails">The password policy details.</param>
        protected override void OnWriteSaveAuditLogEntries(bool success, AuditLogPasswordPolicyDetails accessRuleDetails)
        {
            if (accessRuleDetails.OldMinimumPasswordLength != accessRuleDetails.NewMinimumPasswordLength ||
                accessRuleDetails.OldMaximumPasswordAge != accessRuleDetails.NewMaximumPasswordAge ||
                accessRuleDetails.OldMustContainUpperCaseCharacters != accessRuleDetails.NewMustContainUpperCaseCharacters ||
                accessRuleDetails.OldMustContainLowerCaseCharacters != accessRuleDetails.NewMustContainLowerCaseCharacters ||
                accessRuleDetails.OldMustContainDigits != accessRuleDetails.NewMustContainDigits ||
                accessRuleDetails.OldMustContainSpecialCharacters != accessRuleDetails.NewMustContainSpecialCharacters ||
                accessRuleDetails.OldAccountLockoutDuration != accessRuleDetails.NewAccountLockoutDuration ||
                accessRuleDetails.OldAccountLockoutThreshold != accessRuleDetails.NewAccountLockoutThreshold)
            {
                // Password policy was changed
                AuditLog.OnChangePasswordPolicy(success,
                    accessRuleDetails.OldMinimumPasswordLength, accessRuleDetails.NewMinimumPasswordLength,
                    accessRuleDetails.OldMaximumPasswordAge, accessRuleDetails.NewMaximumPasswordAge,
                    accessRuleDetails.OldMustContainUpperCaseCharacters, accessRuleDetails.NewMustContainUpperCaseCharacters,
                    accessRuleDetails.OldMustContainLowerCaseCharacters, accessRuleDetails.NewMustContainLowerCaseCharacters,
                    accessRuleDetails.OldMustContainDigits, accessRuleDetails.NewMustContainDigits,
                    accessRuleDetails.OldMustContainSpecialCharacters, accessRuleDetails.NewMustContainSpecialCharacters,
                    accessRuleDetails.OldAccountLockoutDuration, accessRuleDetails.NewAccountLockoutDuration,
                    accessRuleDetails.OldAccountLockoutThreshold, accessRuleDetails.NewAccountLockoutThreshold);
            }
        }


        #region Nested type: AuditLogPasswordPolicyDetails


        /// <summary>
        ///     This class is used to hold audit log password policy changes.
        /// </summary>
        internal class AuditLogPasswordPolicyDetails
        {
            /// <summary>
            ///     Gets or sets the old length of the minimum password.
            /// </summary>
            /// <value>
            ///     The old length of the minimum password.
            /// </value>
            public int? OldMinimumPasswordLength { get; set; }


            /// <summary>
            ///     Gets or sets the old maximum password age.
            /// </summary>
            /// <value>
            ///     The old maximum password age.
            /// </value>
            public int? OldMaximumPasswordAge { get; set; }


            /// <summary>
            ///     Gets or sets the old must contain upper case characters.
            /// </summary>
            /// <value>
            ///     The old must contain upper case characters.
            /// </value>
            public bool? OldMustContainUpperCaseCharacters { get; set; }


            /// <summary>
            ///     Gets or sets the old must contain lower case characters.
            /// </summary>
            /// <value>
            ///     The old must contain lower case characters.
            /// </value>
            public bool? OldMustContainLowerCaseCharacters { get; set; }


            /// <summary>
            ///     Gets or sets the old must contain digits.
            /// </summary>
            /// <value>
            ///     The old must contain digits.
            /// </value>
            public bool? OldMustContainDigits { get; set; }


            /// <summary>
            ///     Gets or sets the old must contain special characters.
            /// </summary>
            /// <value>
            ///     The old must contain special characters.
            /// </value>
            public bool? OldMustContainSpecialCharacters { get; set; }


            /// <summary>
            ///     Gets or sets the old duration of the account lockout.
            /// </summary>
            /// <value>
            ///     The old duration of the account lockout.
            /// </value>
            public int? OldAccountLockoutDuration { get; set; }


            /// <summary>
            ///     Gets or sets the old account lockout threshold.
            /// </summary>
            /// <value>
            ///     The old account lockout threshold.
            /// </value>
            public int? OldAccountLockoutThreshold { get; set; }


            /// <summary>
            ///     Gets or sets the new length of the minimum password.
            /// </summary>
            /// <value>
            ///     The new length of the minimum password.
            /// </value>
            public int? NewMinimumPasswordLength { get; set; }


            /// <summary>
            ///     Gets or sets the new maximum password age.
            /// </summary>
            /// <value>
            ///     The new maximum password age.
            /// </value>
            public int? NewMaximumPasswordAge { get; set; }


            /// <summary>
            ///     Gets or sets the new must contain upper case characters.
            /// </summary>
            /// <value>
            ///     The new must contain upper case characters.
            /// </value>
            public bool? NewMustContainUpperCaseCharacters { get; set; }


            /// <summary>
            ///     Gets or sets the new must contain lower case characters.
            /// </summary>
            /// <value>
            ///     The new must contain lower case characters.
            /// </value>
            public bool? NewMustContainLowerCaseCharacters { get; set; }


            /// <summary>
            ///     Gets or sets the new must contain digits.
            /// </summary>
            /// <value>
            ///     The new must contain digits.
            /// </value>
            public bool? NewMustContainDigits { get; set; }


            /// <summary>
            ///     Gets or sets the new must contain special characters.
            /// </summary>
            /// <value>
            ///     The new must contain special characters.
            /// </value>
            public bool? NewMustContainSpecialCharacters { get; set; }


            /// <summary>
            ///     Gets or sets the new duration of the account lockout.
            /// </summary>
            /// <value>
            ///     The new duration of the account lockout.
            /// </value>
            public int? NewAccountLockoutDuration { get; set; }


            /// <summary>
            ///     Gets or sets the new account lockout threshold.
            /// </summary>
            /// <value>
            ///     The new account lockout threshold.
            /// </value>
            public int? NewAccountLockoutThreshold { get; set; }
        }


        #endregion
    }
}