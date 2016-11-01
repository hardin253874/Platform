// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    class CookieInfo
    {
        public Cookie AuthCookie { get; set; }
        
        public Cookie XsrfCookie { get; set; }

        public DateTime Expires { get; set; }
    }
}
