// Copyright 2011-2016 Global Software Innovation Pty Ltd
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Writer for pushing content to the target Open-XML document.
    /// </summary>
    class OpenXmlWriter
    {
        readonly OpenXmlElement _root;
        readonly OpenXmlElement _sourceRoot;
        bool _realignNextWrite;

        /// <summary>
        /// The current element in the output stream.
        /// </summary>
        public OpenXmlElement Current { get; private set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="targetRoot">The target element into which content will be written.</param>
        /// <param name="sourceRoot">The source element that contains content to be written. The sourceRoot itself is not copied.</param>
        public OpenXmlWriter(OpenXmlElement targetRoot, OpenXmlElement sourceRoot)
        {
            _root = targetRoot;
            _sourceRoot = sourceRoot;
            Current = targetRoot;
        }


        /// <summary>
        /// Writes a token to the output stream. (May be an open or close).
        /// </summary>
        /// <param name="token"></param>
        public void WriteToken(Token token)
        {
            if (token.IsOpen)
            {
                //RealignNextWrite = false;
                if (_realignNextWrite)
                    RealignTreeDepth(token.SourceNode.Parent);    // uh oh, we need a node from the target tree

                OpenXmlElement newElem = token.SourceNode.CloneNode(false);
                Current.AppendChild(newElem);
                Current = newElem;
            }
            else
            {
                if (_realignNextWrite)
                    RealignTreeDepth(token.SourceNode);

                if (Current.GetType() != token.SourceNode.GetType())
                    throw new Exception(string.Format("Unbalanced tokens. Asked to close {0}: Expected to close: {1}",
                        token.SourceNode.GetType().Name, Current.GetType().Name));

                Current = Current.Parent;
            }         
        }


        /// <summary>
        /// Request that a nesting realignment be performed when the next token is written.
        /// </summary>
        public void RealignOnNextWrite()
        {
            _realignNextWrite = true;
        }


        /// <summary>
        /// Indents or outdents until we are at an equivalent tree depth 
        /// </summary>
        /// <remarks>
        /// The resulting XML must be well-formed - i.e. with tags balanced. The writer is aware that at certain points, it should be at a certain depth
        /// of nesting, however mismatched instructions can disrupt this. This method artificially closes, or manufactures tags until balance is restored.
        /// </remarks>
        /// <param name="reference">The node that we must match.</param>
        private void RealignTreeDepth(OpenXmlElement reference)
        {
            _realignNextWrite = false;

            // We need to close unbalanced elements on the current node,
            // and open unbalanced elements on the reference node, so that we are back to having an equivalent stack as the reference node.

            List<OpenXmlElement> refPath = GetPath(reference);
            List<OpenXmlElement> curPath = GetPath(Current);

            // Skip common ancestry
            int common = 0;
            while (common < refPath.Count && common < curPath.Count)
            {
                if (refPath[common].GetType() == curPath[common].GetType())
                    common++;
            }
            
            // Close unbalanced elements on current
            for (int i = curPath.Count - 1; i >= common; i--)
            {
                Token closeToken = new Token(curPath[i], true);
                WriteToken(closeToken);
            }

            // Open unbalanced elements on refrence
            for (int i = common; i < refPath.Count; i++)
            {
                Token openToken = new Token(refPath[i], false);
                WriteToken(openToken);

                // Attempt to copy properties for some known elements
                CopyPropertiesIfPossible(refPath[i], Current);
            }
        }


        /// <summary>
        /// Returns the full path from root to element, inclusive, in that order. 
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Path elements, ordered from root to element. </returns>
        private List<OpenXmlElement> GetPath(OpenXmlElement element)
        {
            OpenXmlElement current = element;
            List<OpenXmlElement> result = new List<OpenXmlElement>();

            result.Add(current);
            while (current != _root && current != _sourceRoot)
            {
                current = current.Parent;
                result.Add(current);
            }

            // correct order
            result.Reverse(); 

            return result;
        }


        /// <summary>
        /// Duplicate OpenXML element properties from one element to another.
        /// </summary>
        /// <remarks>Used when manufacturing artificial elements.</remarks>
        /// <param name="source">Source element.</param>
        /// <param name="target">Target element.</param>
        private void CopyPropertiesIfPossible(OpenXmlElement source, OpenXmlElement target)
        {
            OpenXmlElement properties = null;

            var paragraph = source as Paragraph;
            if (paragraph != null)
            {
                properties = paragraph.ParagraphProperties;
            }

            var run = source as Run;
            if (run != null)
            {
                properties = run.RunProperties;
            }

            if (properties != null)
            {
                target.AppendChild(properties.CloneNode(true));
            }
        }

    }
}
