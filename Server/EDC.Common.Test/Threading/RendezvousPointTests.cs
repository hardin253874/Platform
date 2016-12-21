// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using EDC.Threading;
using NUnit.Framework;

namespace EDC.Test.Threading
{
    [TestFixture]
    public class RendezvousPointTests
    {
        /// <summary>
        ///     Verifies that creating a RendezvousPoint with an existing id fails.
        /// </summary>
        [Test]
        public void TestExistingRendezvousPoint()
        {
            string rpId = "RpId" + Guid.NewGuid();

            using (new RendezvousPoint(rpId, 0, EventLog.Application))
            {
                Assert.Throws<ArgumentException>(() => new RendezvousPoint(rpId, 0, EventLog.Application));
            }
        }


        /// <summary>
        ///     Test registering an action.
        /// </summary>
        [Test]
        public void TestRegisterAction()
        {
            bool actionCalled = false;

            string rpId = "RpId" + Guid.NewGuid();

            using (new RendezvousPoint(rpId, 1000, EventLog.Application))
            {
                RendezvousPoint.RegisterAction(rpId, () => { actionCalled = true; });
            }

            Assert.IsTrue(actionCalled, "The action should be called");
        }


        /// <summary>
        ///     Verifies that a rendezvous point only runs for the specified timeout.
        /// </summary>
        [Test]
        public void TestRegisterActionExceedingTimeout()
        {
			using ( ManualResetEvent rendezvousCompleted = new ManualResetEvent( false ) )
			{
				bool actionCalled = false;

				RendezvousPoint rendezvousPoint = null;

				string rpId = "RpId" + Guid.NewGuid( );

				try
				{
					rendezvousPoint = new RendezvousPoint( rpId, 500, EventLog.Application );
					// Register an action that never completes
					RendezvousPoint.RegisterAction( rpId, ( ) =>
					{
						// ReSharper disable once AccessToDisposedClosure
						rendezvousCompleted.WaitOne( 5000 );

						 actionCalled = true;
					 } );
				}
				finally
				{
					Assert.IsFalse( actionCalled, "Action should not be called." );
					rendezvousPoint?.Dispose( );
					// If we are here the rendezvous has completed but the action has not.
					// That's a good thing !
					Assert.IsFalse( actionCalled, "Action should not be called." );
					rendezvousCompleted.Set( );
				}
			}
        }


        /// <summary>
        ///     Tests registering an action that throws an exception.
        /// </summary>
        [Test]
        public void TestRegisterActionThrowingException()
        {
            bool actionCalled = false;

            string rpId = "RpId" + Guid.NewGuid();

            using (new RendezvousPoint(rpId, 1000, EventLog.Application))
            {
                RendezvousPoint.RegisterAction(rpId, () =>
                {
                    actionCalled = true;
                    throw new Exception("Test");
                });
            }

            Assert.IsTrue(actionCalled, "The action should be called");
        }


        /// <summary>
        ///     Test that registering an action with a non-existant RendezvousPoint does nothing.
        /// </summary>
        [Test]
        public void TestRegisterActionWithNonExistantRendezvousPoint()
        {
            bool actionCalled = false;

            string rpId1 = "RpId1" + Guid.NewGuid();
            string rpId2 = "RpId2" + Guid.NewGuid();

            using (new RendezvousPoint(rpId1, 1000, EventLog.Application))
            {
                RendezvousPoint.RegisterAction(rpId2, () => { actionCalled = true; });
            }

            Assert.IsFalse(actionCalled, "The action should not be called");
        }


        /// <summary>
        ///     Test registering multiple actions.
        /// </summary>
        [Test]
        public void TestRegisterMultipleActions()
        {
            bool actionCalled1 = false;
            bool actionCalled2 = false;
            bool actionCalled3 = false;

            string rpId = "RpId" + Guid.NewGuid();

            using (new RendezvousPoint(rpId, 1000, EventLog.Application))
            {
                RendezvousPoint.RegisterAction(rpId, () => { actionCalled1 = true; });

                RendezvousPoint.RegisterAction(rpId, () => { actionCalled2 = true; });

                RendezvousPoint.RegisterAction(rpId, () => { actionCalled3 = true; });
            }

            Assert.IsTrue(actionCalled1, "The action1 should be called");
            Assert.IsTrue(actionCalled2, "The action2 should be called");
            Assert.IsTrue(actionCalled3, "The action3 should be called");
        }


        /// <summary>
        ///     Test registering a null action.
        /// </summary>
        [Test]
        public void TestRegisterNullAction()
        {
            Assert.Throws<ArgumentNullException>(() => RendezvousPoint.RegisterAction("id", null));
        }


        /// <summary>
        ///     Test registering a null id.
        /// </summary>
        [Test]
        public void TestRegisterNullId()
        {
            Assert.Throws<ArgumentNullException>(() => RendezvousPoint.RegisterAction(null, () => { }));
        }


        /// <summary>
        ///     Verifies that creating a RendezvousPoint with a null if fails.
        /// </summary>
        [Test]
        public void TestRendezvousPointCtrNullId()
        {
            Assert.Throws<ArgumentNullException>(() => new RendezvousPoint(null, 0, EventLog.Application));
        }
    }
}