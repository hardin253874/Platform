// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.Collections.Generic;
using NUnit.Framework;

namespace EDC.Test.Collections.Generic
{
    [TestFixture]
    public class AddOnlySetTests
    {
        [Test]
        public void Test_Ctor()
        {
            AddOnlySet<string> addOnlySet;

            addOnlySet = new AddOnlySet<string>();

            Assert.That(addOnlySet, Has.Count.EqualTo(0));
        }

        [Test]
        public void Test_AddSingle()
        {
            AddOnlySet<string> addOnlySet;

            addOnlySet = new AddOnlySet<string>();
            addOnlySet.Add("foo");

            Assert.That(addOnlySet, Has.Exactly(1).EqualTo("foo"));
        }

        [Test]
        public void Test_AddMultiple()
        {
            AddOnlySet<string> addOnlySet;

            addOnlySet = new AddOnlySet<string>();
            addOnlySet.Add("foo", "bar");

            Assert.That(addOnlySet, Has.Count.EqualTo(2));
            Assert.That(addOnlySet, Has.Exactly(1).EqualTo("foo"));
            Assert.That(addOnlySet, Has.Exactly(1).EqualTo("bar"));
        }

        [Test]
        public void Test_ItemsAdded()
        {
            AddOnlySet<string> addOnlySet;
            List<string> itemsAdded;

            itemsAdded = new List<string>();
            addOnlySet = new AddOnlySet<string>();
            addOnlySet.ItemsAdded += (sender, args) => itemsAdded.AddRange(args.Items);

            addOnlySet.Add("foo", "bar");

            Assert.That(itemsAdded, Is.EquivalentTo(new [] { "foo", "bar"}));
        }

        [Test]
        public void Test_ItemsAddedDuplicateInList()
        {
            AddOnlySet<string> addOnlySet;
            List<string> itemsAdded;

            itemsAdded = new List<string>();
            addOnlySet = new AddOnlySet<string>();
            addOnlySet.ItemsAdded += (sender, args) => itemsAdded.AddRange(args.Items);

            addOnlySet.Add("foo", "foo");

            Assert.That(itemsAdded, Is.EquivalentTo(new[] { "foo" }));
        }

        [Test]
        public void Test_ItemsAddedDuplicateInSet()
        {
            AddOnlySet<string> addOnlySet;
            List<string> itemsAdded;

            itemsAdded = new List<string>();
            addOnlySet = new AddOnlySet<string>();
            addOnlySet.Add("foo");

            addOnlySet.ItemsAdded += (sender, args) => itemsAdded.AddRange(args.Items);

            addOnlySet.Add("foo");

            Assert.That(itemsAdded, Is.Empty);
        }
    }
}
