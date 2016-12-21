// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    ///     Help activities deal with documents
    /// </summary>
    internal class DocHelper
    {
        static internal Document CreateDoc(string fileHash, string name, string description, string fileExtension, DocumentType fileType)
        {
            Document doc = null;

            using (var dbCtx = DatabaseContext.GetContext(requireTransaction: true))
            {
                var file = Entity.Create<DocumentRevision>();
                file.Name = EnsureValidFileName(name);
                file.Description = description;
                file.FileDataHash = fileHash;
                file.FileExtension = fileExtension;
                file.DocumentFileType = fileType;
                file.Save();

                var hash = file.FileDataHash;       // We need to save the file to trigger FileTypeEventTarget to move the temp file to the correct repository and generate the hash.

                // we are creating a document with a duplication of the file type and hash - as per the web service version.
                doc = Entity.Create<Document>();
                doc.Name = EnsureValidFileName(name);
                doc.Description = description;
                doc.DocumentHasDocumentRevision.Add(file);
                doc.CurrentDocumentRevision = file;
                doc.FileExtension = fileExtension;
                doc.DocumentFileType = fileType;
                doc.InFolder = Entity.Get<DocumentFolder>(new EntityRef("generatedDocumentFolder"));
                doc.FileDataHash = hash;

                doc.Save();

                dbCtx.CommitTransaction();
            }

            return doc;
        }

        static string EnsureValidFileName(string fileName)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(fileName, invalidRegStr, "_");
        }
    }
}
