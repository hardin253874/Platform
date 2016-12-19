// Copyright 2011-2016 Global Software Innovation Pty Ltd
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Html;

namespace EDC.ReadiNow.Email
{
    public class EmailBodyParser
    {
        string[] ReplyDividerRegExPatterns = new string[]
            {
                @"([\S|\s]*?)([-]*Original Message[-]*)",
                @"([\S|\s]*?)(From:)"
            };

        private string _emailBody;

        public EmailBodyParser(string emailBody)
        {
            if (emailBody == null)
                throw new ArgumentNullException(nameof(emailBody));
            _emailBody = emailBody;
        }

        public bool IsHtml
        {
            get { return _emailBody.Contains("<html"); }
        }

        public string GetReplySectionFromEmailBodyAsText()
        {
            if (IsHtml)
            {
                var html = ExtractReplySectionFromHtml(_emailBody);
                return HtmlToText.ConvertHtmlToText(html);
            }
            else
                return ExtractReplySectionFromText(_emailBody);
        }

        public string GetReplySectionFromEmailAsHtml()
        {
            if (!IsHtml)
                throw new Exception("Email body is not HTML");

            return ExtractReplySectionFromHtml(_emailBody);
        }



        /// <summary>
        /// Tries to extract only the Reply part of the email message by 
        /// 1. Find the first element containing the text From:
        /// 2. Moving up the DOM until the current node is no longer the first child - this will indicate that there is other 
        ///    content in this node which is hopefully the reply part we are wanting.
        /// 3. Remove this node and all nodes after it (these nodes should all be components of the preceeding conversation exchange)
        /// 4. Return the Body element of the Html document with the body node replace by a div
        /// </summary>
        /// <returns>The Reply component of the email body nicely wrapped up in a div</returns>
        string ExtractReplySectionFromHtml(string bodyHtml)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(bodyHtml);

            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode(".//html/body");

            var fromNode = bodyNode.SelectSingleNode(".//*[text()='From:']");
            if (fromNode == null)
            {
                // failed to find From node - this may be the first email in the chain and hence will have not From: node
                return bodyNode.OuterHtml;
            }

            var replyEndNode = fromNode;
            while (true)
            {
                var firstChild = GetFirstChildNode(replyEndNode.ParentNode);

                if (firstChild != replyEndNode)
                    break;

                replyEndNode = replyEndNode.ParentNode;
            }

            if (replyEndNode == null)
            {
                // something went wrong - fall back to returning entire body part
                return bodyNode.OuterHtml;
            }

            var parent = replyEndNode.ParentNode;
            while (true)
            {
                var deleteNode = parent.LastChild;
                if (deleteNode == replyEndNode)
                    break;
                parent.RemoveChild(deleteNode);
            }
            parent.RemoveChild(replyEndNode);
            bodyNode.Name = "div";
            var html = bodyNode.OuterHtml;
            //html = html.Replace("<body>", "<div>").Replace("</body>", "</div>");
            return html;

        }

        // Gets the first child node that is either a node text node or a text node NOT conaining only \r \n ' ' chars
        HtmlNode GetFirstChildNode(HtmlNode parentNode)
        {
            foreach (var childNode in parentNode.ChildNodes)
            {
                if (childNode.NodeType != HtmlNodeType.Text)
                    return childNode;

                var text = childNode.InnerText.Trim(new char[] { '\r', '\n', ' ' });

                //text = text.Replace("\\r", "").Replace("\\n", "");  //testing/dev only
                if (text != "")
                    return childNode;
            }
            return null;
        }


        string ExtractReplySectionFromText(string bodyText)
        {

            foreach (var pattern in ReplyDividerRegExPatterns)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matches = regex.Matches(bodyText);
                if (matches.Count == 0)
                    continue;

                if (matches[0].Groups.Count < 2)
                    continue;

                return matches[0].Groups[1].Value;
            }

            return bodyText;
        }
    }
}
