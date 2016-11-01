// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.CacheInvalidation
{
    [TestFixture]
	[RunWithTransaction]
    public class ContextHelperTests
    {
        public const string SlotName = "Test Slot Name";

        [Test]
        public void Test_PushContextData_NullSlotName()
        {
            try
            {
                Assert.That(() => ContextHelper<string>.PushContextData(null, "foo"),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("slotName"));
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_PushContextData()
        {
            string data;
            string newValue;

            Assert.That(
                ContextHelper<string>.Context.Value.ContainsKey(SlotName),
                Is.False, "Existing data");

            try
            {
                data = "foo";
                ContextHelper<string>.PushContextData(SlotName, data);

                Assert.That(ContextHelper<string>.Context.Value.Keys,
                    Has.Exactly(1).EqualTo(SlotName));
                Assert.That(ContextHelper<string>.Context.Value[SlotName],
                    Is.TypeOf<ConcurrentStack<string>>());
                ContextHelper<string>.Context.Value[SlotName].TryPeek(out newValue);
                Assert.That(newValue, Is.SameAs(data));
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_PushContextData_DataPresent()
        {
            string[] data =
                {
                    "foo",
                    "bar",
                    "baz"
                };
            ConcurrentStack<string> contextData;

            try
            {
                foreach(string datum in data)
                {
                    ContextHelper<string>.PushContextData(SlotName, datum);                
                }

                contextData = ContextHelper<string>.GetContextDataStack(SlotName);
                Assert.That(contextData,
                    Is.EquivalentTo(data.Reverse()));
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_PushContextData_WrongTypeInSlot()
        {
            try
            {
                Assert.That(
                    ContextHelper<string>.Context.Value.ContainsKey(SlotName),
                    Is.False, "Existing data");

                ContextHelper<Tuple<long>>.PushContextData(SlotName, new Tuple<long>(123));

                Assert.That(() => ContextHelper<string>.PushContextData(SlotName, "foo"),
                            Throws.Nothing);
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_PopContextData_NullSlotName()
        {
            Assert.That(() => ContextHelper<string>.PopContextData(null, "foo"),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("slotName"));
        }

        [Test]
        public void Test_PopContextData_WrongTypeInSlot()
        {
            try
            {
                Assert.That(
                    ContextHelper<string>.Context.Value.ContainsKey(SlotName),
                    Is.False, "Existing data");

                ContextHelper<Tuple<long>>.PushContextData(SlotName, new Tuple<long>(123));

                Assert.That(() => ContextHelper<string>.PopContextData(SlotName, "foo"),
                            Throws.InvalidOperationException);
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_PopContextData()
        {
            try
            {
                string data = "foo";

                Assert.That(ContextHelper<string>.GetContextDataStack(SlotName),
                    Is.Empty, "Existing data");

                ContextHelper<string>.GetContextDataStack(SlotName).Push(data);

                data = "foo";
                Assert.That(() => ContextHelper<string>.PopContextData(SlotName, data),
                            Throws.Nothing);
                Assert.That(ContextHelper<string>.GetContextDataStack(SlotName),
                    Is.Empty, "Not empty");
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            } 
        }

        [Test]
        public void Test_PopContextData_DataPresent()
        {
            string[] data =
                {
                    "foo",
                    "bar"
                };

            try
            {
                Assert.That(ContextHelper<string>.GetContextDataStack(SlotName),
                    Is.Empty, "Existing data");

                foreach (string datum in data)
                {
                    ContextHelper<string>.PushContextData(SlotName, datum);
                }

                ContextHelper<string>.PopContextData(SlotName, data.Last());

                Assert.That(ContextHelper<string>.GetContextDataStack(SlotName),
                    Is.EquivalentTo(new Stack<string>(data.Reverse().Skip(1))), "Not popped");
            }
            finally 
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_PopContextData_WrongExpectedData()
        {
            try
            {
                ContextHelper<string>.PushContextData(SlotName, "foo");
                Assert.That(() => ContextHelper<string>.PopContextData(SlotName, "bar"),
                    Throws.ArgumentException.And.Property("ParamName").EqualTo("expectedData"));
            }
            finally 
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_GetContextDataStack_NullSlotName()
        {
            try
            {
                Assert.That(() => ContextHelper<string>.GetContextDataStack(null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("slotName"));
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_GetContextDataStack_WrongTypeInSlot()
        {
            string slotName = "Test slot name " + Guid.NewGuid();

            Assert.That(
                ContextHelper<string>.Context.Value.ContainsKey(slotName),
                Is.False, "Existing data");

            try
            {
                ContextHelper<Tuple<long>>.GetContextDataStack(slotName).Push(new Tuple<long>(1));

                Assert.That(() => ContextHelper<string>.GetContextDataStack(slotName),
                            Is.Empty);
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(slotName, out value);
                ConcurrentStack<Tuple<long>> tupleValue;
                ContextHelper<Tuple<long>>.Context.Value.TryRemove(slotName, out tupleValue);
            }
        }

        // GetContextData tested in PopContextData tests

        [Test]
        public void Test_IsSet_NullSlotName()
        {
            try
            {
                Assert.That(() => ContextHelper<string>.IsSet(null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("slotName"));
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }

        [Test]
        public void Test_IsSet_WrongTypeInSlot()
        {
            Assert.That(ContextHelper<string>.GetContextDataStack(SlotName),
                Is.Empty, "Existing data");

            try
            {
                ConcurrentStack<string> stack;

                ContextHelper<string>.Context.Value.TryRemove(SlotName, out stack);
                ContextHelper<Tuple<long>>.GetContextDataStack(SlotName).Push(new Tuple<long>(123));

                Assert.That(() => ContextHelper<string>.IsSet(SlotName), Is.False);
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
                ConcurrentStack<Tuple<long>> tupleValue;
                ContextHelper<Tuple<long>>.Context.Value.TryRemove(SlotName, out tupleValue);
            }
        }

        [Test]
        public void Test_IsSet_True()
        {
            try
            {
                ContextHelper<string>.PushContextData(SlotName, "foo");

                Assert.That(ContextHelper<string>.IsSet(SlotName), Is.True);
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }           
        }

        [Test]
        public void Test_IsSet_False()
        {
            string slotName = "Test slot name " + Guid.NewGuid();

            try
            {
                Assert.That(ContextHelper<string>.IsSet(slotName), Is.False);
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(slotName, out value);
            }
        }

        [Test]
        public void Test_IsSet_PostUse()
        {
            try
            {
                ContextHelper<string>.PushContextData(SlotName, "foo");
                ContextHelper<string>.PopContextData(SlotName, "foo");

                Assert.That(ContextHelper<string>.IsSet(SlotName), Is.False, "Not set");
                Assert.That(ContextHelper<string>.Context.Value[SlotName], Is.Empty, "Not empty");
            }
            finally
            {
                ConcurrentStack<string> value;
                ContextHelper<string>.Context.Value.TryRemove(SlotName, out value);
            }
        }
    }
}
