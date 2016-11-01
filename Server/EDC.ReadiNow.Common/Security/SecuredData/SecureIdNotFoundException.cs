// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    /// Thrown when a request was made for secure data and no data was found
    /// </summary>
    public class SecureIdNotFoundException: Exception
    {
        public SecureIdNotFoundException(string message): base(message)
        { }

        public SecureIdNotFoundException(string message, Exception ex) : base(message, ex)
        { }
    }
}
