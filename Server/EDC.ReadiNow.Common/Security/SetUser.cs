// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// Emulate a different user for security testing.
    /// </summary>
    public class SetUser: IDisposable
    {
        /// <summary>
        /// Create a new <see cref="SetUser"/>.
        /// </summary>
        /// <param name="userAccount">
        /// The <see cref="UserAccount"/> to switch to.
        /// </param>
		/// <param name="secondaryAccount">
		/// The secondary account.
		/// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="userAccount"/> cannot be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="RequestContext"/> must be set.
        /// </exception>
        public SetUser(UserAccount userAccount, UserAccount secondaryAccount = null)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException("userAccount");
            }
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("RequestContext is not set.");
            }

            UserAccount = userAccount;

            var identity = new IdentityInfo(userAccount.Id, userAccount.Name);

            var secondaryIdentity = secondaryAccount != null ? new IdentityInfo(secondaryAccount.Id, secondaryAccount.Name) : null;


            OldContext = RequestContext.GetContext();
            RequestContext.SetContext(
                identity,
                OldContext.Tenant,
                OldContext.Culture,
                OldContext.TimeZone,
                secondaryIdentity
                );
        }


        /// <summary>
        /// Restore the old context.
        /// </summary>
        public void Dispose()
        {
            if (OldContext != null)
            {
                RequestContext.SetContext(OldContext);
                OldContext = null;
            }
        }

        /// <summary>
        /// The <see cref="UserAccount"/> to emulate.
        /// </summary>
        public UserAccount UserAccount { get; private set; }

        /// <summary>
        /// The old <see cref="RequestContext"/> that was replaced.
        /// </summary>
        public RequestContext OldContext { get; private set; }
    }
}
