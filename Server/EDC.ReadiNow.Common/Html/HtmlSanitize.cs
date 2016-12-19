using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EDC.ReadiNow.Html
{
    public class HtmlSanitizer
    {
        List<string> HtmlElementWhiteList = new List<string>()
        {
            "html",
            "body", 
            "a",
            "abbr",
            "acronym",
            "address",
            "area",
            "b",
            "bdo",
            "big",
            "blockquote",
            "br",
            "button",
            "caption",
            "center",
            "cite",
            "code",
            "col",
            "colgroup",
            "dd",
            "del",
            "dfn",
            "dir",
            "div",
            "dl",
            "dt",
            "em",
            "fieldset",
            "font",
            "form",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "hr",
            "i",
            "img",
            "input",
            "ins",
            "kbd",
            "label",
            "legend",
            "li",
            "map",
            "menu",
            "ol",
            "optgroup",
            "option",
            "p",
            "pre",
            "q",
            "s",
            "samp",
            "select",
            "small",
            "span",
            "strike",
            "strong",
            "sub",
            "sup",
            "table",
            "tbody",
            "td",
            "textarea",
            "tfoot",
            "th",
            "thead",
            "u",
            "tr",
            "tt",
            "u",
            "ul",
            "var"
        };


        // Catch all of potentially dangerous stuff
        List <string> BlackListStrings = new List<string>()
        {
            "script",
            "expression",
            "eval"
        };

        /// <summary>
        /// Cleans up an HTML string by removing all of the following :
        /// all elements not in the white list
        /// all attributes who's name or value contains any of the strings in the BlackList
        /// all attributes who's name begins with 'on' (this removes all event handlers)
        /// </summary>
        public string Sanitize(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            SanitizeHtmlNode(doc.DocumentNode);
            return doc.DocumentNode.WriteTo();
        }

        private void SanitizeHtmlNode(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                // remove all html elements not in the white list
                if (!HtmlElementWhiteList.Contains(node.Name.ToLower()))
                {
                    node.Remove();
                    return;
                }

                // filter the attributes and remove any containing strings in the blacklist
                if (node.HasAttributes)
                {
                    for (int i = node.Attributes.Count - 1; i >= 0; i--)
                    {
                        HtmlAttribute currentAttribute = node.Attributes[i];
                        var attr = currentAttribute.Name?.ToLower();
                        var val = currentAttribute.Value?.ToLower();

                        foreach(var str in BlackListStrings)
                        {
                            if ( ((attr != null) && attr.Contains(str)) ||
                                 ((val != null) && val.Contains(str)) )
                                node.Attributes.Remove(currentAttribute);
                        }
                        
                        // remove event handlers
                        if (attr.StartsWith("on"))
                            node.Attributes.Remove(currentAttribute);
                    }
                }
            }

            // Look through child nodes recursively
            if (node.HasChildNodes)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                {
                    SanitizeHtmlNode(node.ChildNodes[i]);
                }
            }
            else
            {
                // lowest level node. check that it does not contain blacklisted text
                foreach (var str in BlackListStrings)
                {
                    if (node.InnerText.ToLower().Contains(str))
                        node.ParentNode.RemoveChild(node);
                }
            }
        }
    }
}
