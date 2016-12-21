// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Threading;
using NUnit.Framework;
using System;
using System.Threading;

namespace EDC.Test.Threading
{
    [TestFixture]
    public class ThreadCpuGovernorTests
    {        
        /// <summary>
        /// Test creating the governor with invalid parameters.
        /// </summary>
        [Test]
        public void TestInvalidConstructorParams()
        {            
            Assert.Throws<ArgumentOutOfRangeException>(() => new ThreadCpuGovernor(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ThreadCpuGovernor(1000));            
        }


        /// <summary>
        /// Test calling yield on a governor.
        /// </summary>
        [Test]
        public void TestYield()
        {
            ThreadCpuGovernor governor = new ThreadCpuGovernor(50);

            Assert.DoesNotThrow(() => governor.Yield());
            Assert.DoesNotThrow(() => governor.Yield());
            Assert.DoesNotThrow(() => governor.Yield());
        }
    }
}
