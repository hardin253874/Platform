// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using EDC.Database;
using EDC.ReadiNow.Database;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Reflection;

namespace EDC.ReadiNow.Test.Database
{
	/// <summary>
	///     This class is responsible for testing DatabaseContext.
	/// </summary>
    /// <remarks>All the tests have a top level context to emulate how all the service calls work.</remarks>
	[TestFixture]
	public class DatabaseContextTests_RunWithRetry
	{

        [Test]
        public void NoDeadlock()
        {
            using (DatabaseContext.GetContext())
            {
                try
                {
                    DatabaseContext.RunWithRetry(() =>
                    {

                    });
                }
                catch (SqlException)
                {
                    Assert.Fail("We should never get here");
                }
            }

        }


		[Test]
        public void ExceedsRetries()
        {
            using (DatabaseContext.GetContext())
            {
                int called = 0;

                try
                {


                    DatabaseContext.RunWithRetry(() =>
                        {
                            called++;
                            throw SqlExceptionMocker.MakeSqlException(1205);
                        });

                    Assert.Fail("We should never get here");
                }
                catch (SqlException)
                {
                    Assert.That(called, Is.EqualTo(DatabaseContext.DeadlockRetryCount));
                }
            }
        }

        [Test] 
        public void OnlyOuterDeadlockRetries()
        {
            using (DatabaseContext.GetContext())
            {
                int innerCalls = 0, outerCalls = 0;

                try
                {

                    DatabaseContext.RunWithRetry(() =>
                    {
                        outerCalls++;
                        DatabaseContext.RunWithRetry(() =>
                        {
                            innerCalls++;
                            throw SqlExceptionMocker.MakeSqlException(1205);
                        });
                    });

                    Assert.Fail("We should never get here");
                }
                catch (SqlException)
                {
                    Assert.That(innerCalls, Is.EqualTo(DatabaseContext.DeadlockRetryCount));
                    Assert.That(outerCalls, Is.EqualTo(DatabaseContext.DeadlockRetryCount));
                }
            }
        }


        [Test]
        public void DeadlockInTransactionDoesntRetry()
        {
            using (DatabaseContext.GetContext())
            {
                int calls = 0;

                try
                {
                    using (DatabaseContext.GetContext(true))
                    {
                        DatabaseContext.RunWithRetry(() =>
                        {
                            calls++;
                            throw SqlExceptionMocker.MakeSqlException(1205);
                        });

                        Assert.Fail("We should never get here");
                    }
                }
                catch (SqlException)
                {
                    Assert.That(calls, Is.EqualTo(1));
                }
            }
        }




        [Test]
        [TestCase(547)]     // foreign key violation
        [TestCase(2601)]    // primary key violation
        [TestCase(1205)]    // deadlock
        [TestCase(823)]    // Sql IO
        [TestCase(824)]    // Sql IO
        [TestCase(829)]    // Sql IO
        public void OneDeadlock(int code )
        {
            using (DatabaseContext.GetContext())
            {
                bool firstTime = true;
                try
                {

                    DatabaseContext.RunWithRetry(() =>
                    {
                        if (firstTime)
                        {
                            firstTime = false;
                            throw SqlExceptionMocker.MakeSqlException(code);
                        }
                    });

                    Assert.That(firstTime, Is.False);
                }
                catch (SqlException)
                {
                    Assert.Fail("We should never get here");
                }
            }
        }


    }
}