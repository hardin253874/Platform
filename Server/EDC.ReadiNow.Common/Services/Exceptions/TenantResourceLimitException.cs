// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Services.Exceptions
{
    /// <summary>
    /// Base exception class for exceptions relating to limits imposed on tenants that have been exceeded.
    /// </summary>
    public class TenantResourceLimitException : Exception
    {
        /// <summary>
        /// A reason code that will remain constant over time and can be used client side to differentiate between limit failures.
        /// </summary>
        public string ReasonCode { get; private set;}

        /// <summary>
        /// A error message that is intended to be displayed to the customer.
        /// </summary>
        public string CustomerMessage { get; private set; }

        public TenantResourceLimitException(string message, string customerMessage, string reasonCode) : base(message)
        {
            CustomerMessage = customerMessage;
            ReasonCode = reasonCode;
        }
        public TenantResourceLimitException(string message, string customerMessage, string reasonCode, Exception innerException) : base(message, innerException)
        {
            CustomerMessage = customerMessage;
            ReasonCode = reasonCode;
        }
    }
}
