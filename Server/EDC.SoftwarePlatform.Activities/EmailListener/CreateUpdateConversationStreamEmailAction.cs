// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
    public class CreateUpdateConversationStreamEmailAction : IEmailAction
    {

        public bool BeforeSave(ReceivedEmailMessage message, out Action postSaveAction)
        {
            postSaveAction = () => { CreateUpdateFromReceivedEmail(message); };
            return true;
        }

        public void CreateUpdateFromReceivedEmail(ReceivedEmailMessage message)
        {
            if (message.EmailForCorrespondence != null)
                return;

            if (message.OriginalMessage?.EmailForCorrespondence != null)
            {
                message.EmailForCorrespondence = message.OriginalMessage.EmailForCorrespondence;
                message.Save();
                return;
            }

            Func<string, string> formatEntityName = (s) =>  s.Replace('<', '(').Replace('>', ')') ;

            // create a new Correspondence Stream
            var corrStream = Entity.Create<CorrespondenceStream>();
            corrStream.Name = $"Correspondence initiated from received email from {formatEntityName(message.EmFrom)}";
            
            var corr = Entity.Create<Correspondence>();
            corr.Name = $"Email Correspondence to {formatEntityName(message.EmTo)} from {formatEntityName(message.EmFrom)}";
            corr.CorrFrom = message.EmFrom;
            corr.CorrTo = message.EmFrom;
            corr.CorrContent = ExtractUserResponseFromBody(message.EmBody);
            corr.CorrespondenceAttachments.AddRange(message.EmAttachments);
            corr.CorrespondenceSourceEmail = Entity.As<EmailMessage>(message);
            corrStream.CorrespondenceLog.Add(corr);
            
            corrStream.Save();
        }

        

        string ExtractUserResponseFromBody(string body)
        {
            //TODO
            return body;
        }

        /*string ExtractUserResponseFromBody(string body)
	    {
        signature = regex.match("^[\s]*--*[\s]*[a-z \.]*$).*", message)

        new Regex("From:\\s*" + Regex.Escape(_mail), RegexOptions.IgnoreCase);
new Regex("<" + Regex.Escape(_mail) + ">", RegexOptions.IgnoreCase);
new Regex(Regex.Escape(_mail) + "\\s+wrote:", RegexOptions.IgnoreCase);
new Regex("\\n.*On.*(\\r\\n)?wrote:\\r\\n", RegexOptions.IgnoreCase | RegexOptions.Multiline);
new Regex("-+original\\s+message-+\\s*$", RegexOptions.IgnoreCase);
new Regex("from:\\s*$", RegexOptions.IgnoreCase);

        To remove quotation in the end:

new Regex("^>.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);


            var regex = new Regex(@"(?<=%download%#)\d+");
            //return regex.Matches(strInput);

        }
        
         
        public string ExtractReply(string text, string address)
        {
            var regexes = new List<Regex>() 
            { 
                new Regex("From:\\s*" + Regex.Escape(address), RegexOptions.IgnoreCase),
                new Regex("<" + Regex.Escape(address) + ">", RegexOptions.IgnoreCase),
                new Regex(Regex.Escape(address) + "\\s+wrote:", RegexOptions.IgnoreCase),
                new Regex("\\n.*On.*(\\r\\n)?wrote:\\r\\n", RegexOptions.IgnoreCase | RegexOptions.Multiline),
                new Regex("-+original\\s+message-+\\s*$", RegexOptions.IgnoreCase),
                new Regex("from:\\s*$", RegexOptions.IgnoreCase),
                new Regex("^>.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            };

            var index = text.Length;

            foreach(var regex in regexes)
            {
                var match = regex.Match(text);

                if (match.Success && match.Index < index)
                    index = match.Index;
            }

            return text.Substring(0, index).Trim();
        }

 */
    }
}
