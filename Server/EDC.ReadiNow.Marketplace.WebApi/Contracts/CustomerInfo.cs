// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Marketplace.WebApi.Contracts
{
    /// <summary>
    /// Designed to hold registration information about the customer including (possibly) an application they
    /// wish to purchase once registered.
    /// </summary>
    [DataContract]
    public class CustomerInfo
    {
        /// <summary>
        /// The customer's first name.
        /// </summary>
        [DataMember(Name = "firstname")]
        public string FirstName { get; set; }

        /// <summary>
        /// The customer's last name.
        /// </summary>
        [DataMember(Name = "lastname")]
        public string LastName { get; set; }

        /// <summary>
        /// A string pertaining to the password that the user wishes to use.
        /// </summary>
        [DataMember(Name = "passwordhash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// The customer's email address.
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }
                
        /// <summary>
        /// A dictionary of registration information that was gathered about the customer.
        /// </summary>
        [DataMember(Name = "args")]
        public Dictionary<string, string> Arguments { get; set; }
    }
}