// Copyright 2011-2016 Global Software Innovation Pty Ltd
using ReadiNow.Annotations;
using System.IO;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Interface for generating a document from a template.
    /// </summary>
    public interface IDocumentGenerator
    {
        /// <summary>
        /// Generate a document from a template.
        /// </summary>
        /// <param name="templateStream">File stream of the template Word document.</param>
        /// <param name="outputStream">Output stream to receive the generated document.</param>
        /// <param name="settings">Various settings to pass into generation.</param>
        void CreateDocument( [NotNull] Stream templateStream, [NotNull] Stream outputStream, [CanBeNull] GeneratorSettings settings);
    }
}
