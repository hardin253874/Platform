// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl.Diagnostics
{
    [TestFixture]
    public class ForceSecurityTraceContextEntryTests
    {
        [Test]
        public void Test_Ctor()
        {
            ForceSecurityTraceContextEntry forceSecurityTraceContextEntry;
            long[] entities;

            entities = new long[] { 1 };
            forceSecurityTraceContextEntry = new ForceSecurityTraceContextEntry(entities);

            Assert.That(forceSecurityTraceContextEntry, Has.Property("EntitiesToTrace").EquivalentTo(entities));
        }

        [Test]
        public void Test_Ctor_NullEntitiesToTrace()
        {
            Assert.That(() => new ForceSecurityTraceContextEntry(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entitiesToTrace"));
        }
    }
}
