// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Email;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.EmailListener;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test.EmailListener
{
    [TestFixture]
    public class MatchSentToReceivedEmailsActionTest
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test()
        {
	        string messageIdLocalPart;

		    var sentMessage = new SentEmailMessage
			    {
				    EmTo = "bob@wibble.com",
				    EmFrom = "action@sp.com",
				    EmBody = "body",
				    EmIsHtml = false,
				    EmSubject = "subject"
			    };

		    sentMessage.Save( );

			messageIdLocalPart = sentMessage.SemSequenceNumber;

		    var receivedMessage = new ReceivedEmailMessage
			    {
				    EmTo = "action@sp.com",
				    EmFrom = "bob@wibble.com",
				    EmSubject = "Re: " + sentMessage.EmSubject,
				    EmBody = "body",
				    EmIsHtml = false,
				    EmReferences = EmailHelper.GenerateMessageId( messageIdLocalPart, "localhost" )
			    };

		    var matchAction = new MatchSentToReceivedEmailsAction( );

		    Action postSaveAction;
		    Assert.IsFalse( matchAction.BeforeSave( receivedMessage, out postSaveAction ) );
		    Assert.IsNull( postSaveAction );
		    Assert.IsNotNull( receivedMessage.OriginalMessage, "The original message has been found." );
	        Assert.AreEqual( sentMessage.Id, receivedMessage.OriginalMessage.Id, "The original message has been set correctly." );
        }
    }
}
