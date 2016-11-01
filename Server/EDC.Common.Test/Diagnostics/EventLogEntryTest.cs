// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;
using System.Threading;
using EDC.Diagnostics;
using NUnit.Framework;
using EventLogEntry = EDC.Diagnostics.EventLogEntry;

namespace EDC.Test.Diagnostics
{
	/// <summary>
	///     This class is responsible for testing the EventLogEntry class
	/// </summary>
	[TestFixture]
	public class EventLogEntryTest
	{
		/// <summary>
		///     Test the EventLogEntry constructor
		/// </summary>
		[Test]
		public void ConstructorValidParameters( )
		{
			Guid id = Guid.NewGuid( );
			DateTime date = DateTime.Now;
			long timestamp = Stopwatch.GetTimestamp( );
			const EventLogLevel logLevel = EventLogLevel.Error;
			string machineName = Environment.MachineName;
			string process = Process.GetCurrentProcess( ).ProcessName;
			int threadId = Thread.CurrentThread.ManagedThreadId;
			const string source = "Source";
			const string message = "Message";
			const long tenantId = 5555;
			const string userName = "UserName";
            const string tenantName = "EDC";

            var entry = new EventLogEntry(id, date, timestamp, logLevel, machineName, process, threadId, source, message, tenantId, tenantName, userName);

			Assert.AreEqual( id, entry.Id, "The Id is invalid" );
			Assert.AreEqual( date, entry.Date, "The Date is invalid" );
			Assert.AreEqual( timestamp, entry.Timestamp, "The Timestamp is invalid" );
			Assert.AreEqual( logLevel, entry.Level, "The Level is invalid" );
			Assert.AreEqual( machineName, entry.Machine, "The Machine is invalid" );
			Assert.AreEqual( process, entry.Process, "The Process is invalid" );
			Assert.AreEqual( threadId, entry.ThreadId, "The ThreadId is invalid" );
			Assert.AreEqual( source, entry.Source, "The Source is invalid" );
			Assert.AreEqual( tenantId, entry.TenantId, "The TenantId is invalid" );
            Assert.AreEqual(tenantName, entry.TenantName, "The TenantName is invalid");
			Assert.AreEqual( userName, entry.UserName, "The UserName is invalid" );
		}
	}
}