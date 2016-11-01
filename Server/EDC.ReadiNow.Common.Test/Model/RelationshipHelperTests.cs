// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class RelationshipHelperTests
    {

        [TestCase("owner", null, null, true, "owner")]
        [TestCase("owner", null, null, false, "owner")]
        [TestCase("owner", "owned by", null, true, "owned by")]
        [TestCase("owner", "owned by", null, false, "owner")]
        [TestCase("owner", "owned by", "owns", true, "owned by")]
        [TestCase("owner", "owned by", "owns", false, "owns")]
        [TestCase("owner", null, "owns", true, "owner")]
        [TestCase("owner", null, "owns", false, "owns")]
        public void DisplayName(string name, string toName, string fromName, bool isForward, string expected)
        {
            var rel = Entity.Create<Relationship>();
            rel.Name = name;
            rel.ToName = toName;
            rel.FromName = fromName;

            var direction = isForward ? Direction.Forward : Direction.Reverse;

            var displayName = rel.DisplayName(direction);

            Assert.That(displayName, Is.EqualTo(expected));

        }
    }
}
