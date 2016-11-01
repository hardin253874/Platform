// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadiNow.Integration.Sms
{
    public class TwilioException: Exception
    {
        public TwilioException(string message) : base(message)
        {
        }
    }
}
