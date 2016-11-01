// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model.CacheInvalidation;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Diagnostics
{
    [TestFixture]
	[RunWithTransaction]
    public class MessageContextTests
    {
        [Test]
        public void Test_Ctor_SingleLevel()
        {
            const string testName = "a";

            Assert.That(MessageContext.IsSet(testName), Is.False, "Set before creation");
            using (MessageContext messageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
            {
                Assert.That(MessageContext.IsSet(testName), Is.True, "Not set after creation");
                Assert.That(messageContext, Has.Property("ContextType").EqualTo(ContextType.New));
                Assert.That(messageContext, Has.Property("Name").EqualTo(testName));
                Assert.That(messageContext.GetMessage(), Is.Empty, "Incorrect message");
            }
            Assert.That(MessageContext.IsSet(testName), Is.False, "Set after dispose");
        }

        [Test]
        public void Test_Ctor_MultiLevelNew()
        {
            const string testName = "a";

            Assert.That(MessageContext.IsSet(testName), Is.False, "Set before creation");
            using (MessageContext outerMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
            {
                Assert.That(MessageContext.IsSet(testName), Is.True, "Not set after creation");
                Assert.That(outerMessageContext, Has.Property("ContextType").EqualTo(ContextType.New));
                using (MessageContext innerMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
                {
                    Assert.That(innerMessageContext, Has.Property("ContextType").EqualTo(ContextType.New));
                    Assert.That(MessageContext.IsSet(testName), Is.True, "Not set in inner context");
                }
            }
            Assert.That(MessageContext.IsSet(testName), Is.False, "Set after dispose");
        }

        [Test]
        public void Test_Append_SingleLevel()
        {
            const string testName = "a";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";

            using (MessageContext messageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
            {
                Assert.That(messageContext.GetMessage(), Is.Empty, 
                    "Incorrect message before first append line");
                messageContext.Append(() => testLine1);
                Assert.That(messageContext.GetMessage(), Is.EqualTo(testLine1), 
                    "Incorrect message after first append line");
                messageContext.Append(() => testLine2);
                Assert.That(messageContext.GetMessage(), Is.EqualTo(testLine1 + Environment.NewLine + testLine2), 
                    "Incorrect message after second append line");
            }
        }

        [Test]
        public void Test_Append_TwoLevels()
        {
            const string testName = "a";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";
            const string testLine3 = "Line 3";

            using (MessageContext outerMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
            {
                outerMessageContext.Append(() => testLine1);
                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                    "Incorrect out message after first append");
                using (MessageContext innerMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
                {
                    innerMessageContext.Append(() => testLine2);
                    Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1 + Environment.NewLine + MessageContext.Indent + testLine2),
                        "Incorrect outer message after second append");
                    Assert.That(innerMessageContext.GetMessage(), Is.EqualTo(testLine2),
                        "Incorrect inner message after second append");
                }

                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1 + Environment.NewLine + MessageContext.Indent + testLine2),
                    "Incorrect outer message after inner dispose");

                outerMessageContext.Append(() => testLine3);
                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1 + Environment.NewLine + MessageContext.Indent + testLine2 + Environment.NewLine + testLine3),
                    "Incorrect outer message after third append");
            }
        }

        [Test]
        public void Test_Append_MultipleLevels()
        {
            const string testName = "a";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";
            const string testLine3 = "Line 3";

            using (MessageContext outerMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
            {
                outerMessageContext.Append(() => testLine1);
                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                    "Incorrect message after first append");
                using (MessageContext middleMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
                {
                    middleMessageContext.Append(() => testLine2);
                    Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1 + Environment.NewLine + MessageContext.Indent + testLine2),
                        "Incorrect outer message after second append");
                    Assert.That(middleMessageContext.GetMessage(), Is.EqualTo(testLine2),
                        "Incorrect middle message after second append");

                    using (MessageContext innerMessageContext = new MessageContext(testName, MessageContextBehavior.Capturing))
                    {
                        innerMessageContext.Append(() => testLine3);
                        Assert.That(innerMessageContext.GetMessage(), 
                            Is.EqualTo(testLine3),
                            "Incorrect inner message after third append");
                        Assert.That(middleMessageContext.GetMessage(), 
                            Is.EqualTo(testLine2 + Environment.NewLine + MessageContext.Indent + testLine3),
                            "Incorrect middle message after second append");
                        Assert.That(outerMessageContext.GetMessage(), 
                            Is.EqualTo(testLine1 + Environment.NewLine + MessageContext.Indent + testLine2 + Environment.NewLine + MessageContext.Indent + MessageContext.Indent + testLine3),
                            "Incorrect outer message after second append");
                    }
                }
            }
        }

        [Test]
        public void Test_Append_DifferentMessages()
        {
            const string testName1 = "a";
            const string testName2 = "b";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";

            using (MessageContext outerMessageContext = new MessageContext(testName1, MessageContextBehavior.Capturing))
            {
                outerMessageContext.Append(() => testLine1);
                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                    "Incorrect outer message after first append");

                using (MessageContext innerMessageContext = new MessageContext(testName2, MessageContextBehavior.Capturing))
                {
                    innerMessageContext.Append(() => testLine2);
                    Assert.That(innerMessageContext.GetMessage(), Is.EqualTo(testLine2),
                        "Incorrect inner message after second append");
                    Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                        "Incorrect outer message after second append");
                }
            }
        }

        [Test]
        public void Test_Append_NotCapturing()
        {
            const string testName1 = "a";
            const string testLine1 = "Line 1";
            bool called;

            called = false;
            using (MessageContext messageContext = new MessageContext(testName1, MessageContextBehavior.Default))
            {
                messageContext.Append(() =>
                {
                    called = true;
                    return testLine1;
                });
                Assert.That(called, Is.False, "Called");
                Assert.That(messageContext.GetMessage(), Is.Empty,
                    "Message is not empty");
            }
        }

        [Test]
        public void Test_Append_InnerCapturingOnly()
        {
            const string testName1 = "a";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";

            using (MessageContext outerMessageContext = new MessageContext(testName1))
            {
                outerMessageContext.Append(() => testLine1);
                Assert.That(outerMessageContext.GetMessage(), Is.Empty,
                    "Incorrect outer message after first append");

                using (MessageContext innerMessageContext = new MessageContext(testName1, MessageContextBehavior.Capturing))
                {
                    innerMessageContext.Append(() => testLine2);
                    Assert.That(innerMessageContext.GetMessage(), Is.EqualTo(testLine2),
                        "Incorrect inner message after second append");
                    Assert.That(outerMessageContext.GetMessage(), Is.Empty,
                        "Incorrect outer message after second append");
                }
            }
        }

        [Test]
        public void Test_Append_OuterCapturingOnly()
        {
            const string testName1 = "a";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";

            using (MessageContext outerMessageContext = new MessageContext(testName1, MessageContextBehavior.Capturing))
            {
                outerMessageContext.Append(() => testLine1);
                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                    "Incorrect outer message after first append");

                using (MessageContext innerMessageContext = new MessageContext(testName1))
                {
                    innerMessageContext.Append(() => testLine2);
                    Assert.That(innerMessageContext.GetMessage(), Is.Empty,
                        "Incorrect inner message after second append");
                    Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1 + Environment.NewLine + MessageContext.Indent + testLine2),
                        "Incorrect outer message after second append");
                }
            }
        }

        [TestCase( MessageContextBehavior.Capturing )]
        [TestCase( MessageContextBehavior.Default )]
        [TestCase( MessageContextBehavior.New )]
        public void Test_DisposeAction(MessageContextBehavior behavior )
        {
            const string testName = "a";
            bool wasCalled = false;
            Action<MessageContext> callback = ( ctx ) =>
            {
                Assert.That( ctx, Is.Not.Null );
                wasCalled = true;
            };
            
            using ( MessageContext messageContext = new MessageContext( testName, MessageContextBehavior.Capturing, callback ) )
            {
            }
            Assert.That( wasCalled, Is.True );
        }

        [Test]
        public void Test_NewContext()
        {
            const string testName1 = "a";
            const string testLine1 = "Line 1";
            const string testLine2 = "Line 2";

            using (MessageContext outerMessageContext = new MessageContext(testName1, MessageContextBehavior.Capturing))
            {
                outerMessageContext.Append(() => testLine1);
                Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                    "Incorrect outer message after first append");

                using (MessageContext innerMessageContext = new MessageContext(testName1, MessageContextBehavior.Capturing | MessageContextBehavior.New))
                {
                    innerMessageContext.Append(() => testLine2);
                    Assert.That(outerMessageContext.GetMessage(), Is.EqualTo(testLine1),
                        "Incorrect outer message after second append");
                    Assert.That(innerMessageContext.GetMessage(), Is.EqualTo(testLine2),
                        "Incorrect inner message after second append");
                }
            }
        }

        [Test]
        public void Test_Sample()
        {
            const string testName = "a";

            using (
                MessageContext messageContext1 = new MessageContext(testName,
                    MessageContextBehavior.Capturing | MessageContextBehavior.New))
            {
                messageContext1.Append(() => "Start:");
                using (MessageContext messageContext2 = new MessageContext(testName))
                {
                    messageContext2.Append(() => "Addends:");
                    using (MessageContext messageContext3 = new MessageContext(testName))
                    {
                        messageContext3.Append(() => "Addend 1: 2");
                        messageContext3.Append(() => "Addend 2: 3");
                    }

                    messageContext2.Append(() => "Result:");
                    using (MessageContext messageContext4 = new MessageContext(testName))
                    {
                        messageContext4.Append(() => "Sum: 5");
                    }
                }

                Assert.That(messageContext1.GetMessage(),
                    Is.EqualTo(string.Format(
                        @"Start:
{0}Addends:
{0}{0}Addend 1: 2
{0}{0}Addend 2: 3
{0}Result:
{0}{0}Sum: 5", MessageContext.Indent)));
            }
        }
    }
}
