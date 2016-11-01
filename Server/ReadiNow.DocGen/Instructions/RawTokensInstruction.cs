// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using EDC.Threading;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Represents a flat list of XML tags that should be rendered directly to the output stream.
    /// </summary>
    class RawTokensInstruction : Instruction
    {
        /// <summary>
        /// The tags.
        /// </summary>
        public List<Token> Tokens;


        /// <summary>
        /// Writes the tokens to the stream.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnGenerate(WriterContext context)
        {
            ThreadCpuGovernor threadGovernor = new ThreadCpuGovernor(50);

            foreach (Token token in Tokens)
            {
                // Allocate new unique IDs for recurring images
                var docProperties = token.SourceNode as  DocProperties;
                if (token.IsOpen && docProperties != null)
                {
                    docProperties.Id = context.AllocateImageId(docProperties.Id);
                }

                // Write token
                context.Writer.WriteToken(token);

                threadGovernor.Yield();
            }
        }


        /// <summary>
        /// Show debug info for this instruction. In this case, an indented list of tokens.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="indent">The indent.</param>
        public override void OnDebug(TextWriter writer, int indent)
        {
            int xmlIndent = 0;
            int minIndent = 0;
            foreach (Token token in Tokens)
            {
                if (token.IsClose)
                    xmlIndent--;
                else
                    xmlIndent++;
                minIndent = Math.Min(xmlIndent, minIndent);
            }

            xmlIndent = -minIndent;

            foreach (Token token in Tokens)
            {
                if (token.IsClose)
                    xmlIndent--;
                
                writer.WriteLine();
                writer.Write(new string('\t', indent) + new string(' ', 3*xmlIndent) + token);

                if (token.IsOpen)
                    xmlIndent++;
            }
        }
    }
}
