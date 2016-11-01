// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Services
{
    /// <summary>
    /// Describes services related to account management in CAST related systems.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Verifies that a new customer is valid and allows account creation to continue.
        /// </summary>
        /// <param name="token">The verification token.</param>
        /// <returns>The verified customer.</returns>
        ManagedCustomer Verify(string token);

        /// <summary>
        /// Rejects the creation of a new account from customer registration details.
        /// </summary>
        /// <param name="token">The verification token.</param>
        void Reject(string token);
    }
}
