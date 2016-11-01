// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using NUnit.Framework;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Test
{
    /// <summary>
    /// An easy way of bypassing entity model access control in a test.
    /// </summary>
    public class IgnoreSecurityAttribute : ReadiNowTestAttribute, IDisposable
    {
        private SecurityBypassContext _securityBypassContext;

        /// <summary>
        /// Create a new <see cref="IgnoreSecurityAttribute"/>.
        /// </summary>
        /// <param name="reason">
        /// The reason why security should be ignored in this test.
        /// </param>
        public IgnoreSecurityAttribute(IgnoreSecurityReason reason)
        {
			Reason = reason;
        }

        /// <summary>
        /// The reason why security should be ignored in this test. 
        /// </summary>
        public IgnoreSecurityReason Reason { get; private set; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _securityBypassContext.Dispose();
            _securityBypassContext = null;
        }

        /// <summary>
        /// Executed before each test is run
        /// </summary>
        /// <param name="testDetails">
        /// Provides details about the test that is going to be run.
        /// </param>
        public override void BeforeTest(TestDetails testDetails)
        {
            _securityBypassContext = new SecurityBypassContext();
        }

        /// <summary>
        /// Executed after each test is run
        /// </summary>
        /// <param name="testDetails">
        /// Provides details about the test that has just been run.
        /// </param>
        public override void AfterTest(TestDetails testDetails)
        {
            Dispose();
        }
    }
}
