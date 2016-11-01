// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.QueryEngine.ReportConverter;
using EDC.ReadiNow.Test;

namespace ReadiNow.QueryEngine.Test.ReportConverter
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class ReportToQueryConverterTests
    {
        [Test]
        public void Test_Convert_Null()
        {
            Assert.That(() => new ReportToQueryConverter().Convert(null, null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("report"));
        }
    }
}
