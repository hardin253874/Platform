// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Messaging.Redis;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.BackgroundTasks
{
    [TestFixture]
    public class BackgroundTaskControllerTests
    {
        [Test]
        public void Registeration()
        {
            var controller = Factory.Current.Resolve<IBackgroundTaskController>();
            
            Assert.That(controller, Is.Not.Null);
        }

        [Test]
        public void StartAll()
        {
            var redisMgr = Factory.DistributedMemoryManager;

            // active TM
            var mockTm1 = new Mock<IBackgroundTaskManager>(MockBehavior.Strict);
            mockTm1.Name = nameof(mockTm1);
            mockTm1.Setup(tm => tm.IsActive).Returns(true);
            mockTm1.Setup(tm => tm.Start());
            mockTm1.Setup(tm => tm.IsActive).Returns(true);
            mockTm1.Setup(tm => tm.Start());
            var controller1 = new BackgroundTaskController(redisMgr, mockTm1.Object);


            // passive TM
            var mockTm2 = new Mock<IBackgroundTaskManager>(MockBehavior.Strict);
            mockTm1.Name = nameof(mockTm2);
            mockTm2.Setup(tm => tm.IsActive).Returns(false);
            mockTm2.Setup(tm => tm.IsActive).Returns(false);
            var controller2 = new BackgroundTaskController(redisMgr, mockTm2.Object);

            controller1.StartAll();
            Thread.Sleep(200);

            controller2.StartAll();
            Thread.Sleep(200);

            mockTm1.VerifyAll();
            mockTm2.VerifyAll();
        }


        [Test]
        public void StopAll()
        {
            var redisMgr = Factory.DistributedMemoryManager;

            // active TM
            var mockTm1 = new Mock<IBackgroundTaskManager>(MockBehavior.Strict);
            mockTm1.Setup(tm => tm.IsActive).Returns(true);
            mockTm1.Setup(tm => tm.Stop());
            mockTm1.Setup(tm => tm.IsActive).Returns(true);
            mockTm1.Setup(tm => tm.Stop());
            var controller1 = new BackgroundTaskController(redisMgr, mockTm1.Object);


            // passive TM
            var mockTm2 = new Mock<IBackgroundTaskManager>(MockBehavior.Strict);
            mockTm2.Setup(tm => tm.IsActive).Returns(false);
            mockTm2.Setup(tm => tm.IsActive).Returns(false);
            var controller2 = new BackgroundTaskController(redisMgr, mockTm2.Object);

            controller1.StopAll();
            controller2.StopAll();

            mockTm1.VerifyAll();
            mockTm2.VerifyAll();
        }

        [Test]
        public void TwoControllersTalking()
        {

        }
    }
}
