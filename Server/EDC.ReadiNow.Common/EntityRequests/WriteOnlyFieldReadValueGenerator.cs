// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// Some write-only fields, such as passwords, should show some form of data when read. This class
    /// provides those default read values.
    /// </summary>
    public class WriteOnlyFieldReadValueGenerator
    {
        // In the absence of a missing password policy minimum password length, the length of the resulting value
        // for write-only string fields.
        public static int DefaultWriteOnlyStringResultLength = 8;

        /// <summary>
        /// Generate the value when the field is read.
        /// </summary>
        /// <param name="fieldId">
        /// The field being read. This cannot be null.
        /// </param>
        /// <param name="type">
        /// The type of <paramref name="fieldId"/>. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public TypedValue GenerateValue(EntityRef fieldId, DatabaseType type)
        {
            if (fieldId == null)
            {
                throw new ArgumentNullException("fieldId");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            TypedValue result;

            // Since write-only fields are usually used with passwords, send back a dummy string long enough to satisfy
            // the length requirement and rely on the client side field handling to ensure the password is not inadvertently
            // set to the dummy string.
            //
            // Ideally, we should have some form of mapping of field ID to default read value, particularly if we get
            // multiple write-only fields that need different read values.
            result = null;
            if (type is StringType)
            {
                using (new SecurityBypassContext())
                {
                    PasswordPolicy passwordPolicy;

                    passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
                    if (passwordPolicy != null)
                    {
                        result = new TypedValue
                        {
                            Type = type,
                            Value =
                                new string('*',
                                    passwordPolicy.MinimumPasswordLength ?? DefaultWriteOnlyStringResultLength)
                        };
                    }
                }
            }

            if(result == null)
            {
                result = new TypedValue
                {
                    Type = type
                };
            }

            return result;
        }
    }
}
