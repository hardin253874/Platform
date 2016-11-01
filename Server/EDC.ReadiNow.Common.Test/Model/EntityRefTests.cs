// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Data;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
    /// <summary>
    ///     Entity tests.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class EntityRefTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void CanCreateEntityRefFromEntityAndNullAlias()
        {
            IEntity r = new Resource();
            EntityRef er = new EntityRef(r, null);
            Assert.IsNull(er.Alias);
        }

        [Test]
        [RunAsDefaultTenant]
        public void CanCreateEntityRefFromEntityWithNamespaceAndAlias()
        {
            IEntity r = new Resource();
            r.SetField(Resource.Alias_Field, "mynamespace:myalias");

            EntityRef er = new EntityRef(r);

            Assert.That(er.Namespace, Is.EqualTo("mynamespace"));
            Assert.That(er.Alias, Is.EqualTo("myalias"));
        }
   }
}