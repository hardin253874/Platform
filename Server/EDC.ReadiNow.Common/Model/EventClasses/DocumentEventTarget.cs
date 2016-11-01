// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.EventClasses
{
    internal class DocumentEventTarget : IEntityEventSave
    {
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // I wish interfaces in C# had an optional modifier!
        }

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (Document document in from entity in entities where entity.Is<Document>() select entity.AsWritable<Document>())
            {
                // Populate the document revision
                DocumentRevision revision = Entity.Create<DocumentRevision>();
                bool createNewRevision = false;
                bool initialRevision = false;
                if (EntityTemporaryIdAllocator.IsAllocatedId(document.Id))
                {
                    // Populate the initial version string for the document
                    revision.VersionComments = @"Initial Version";
                    revision.Version = @"1.0";
                    revision.Name = revision.Version;
                    createNewRevision = true;
                    initialRevision = true;
                }
                else
                {
                    DocumentRevision currentRevision = document.CurrentDocumentRevision;

                    if (currentRevision?.FileDataHash != document.FileDataHash)
                    {
                        // Extract the version number and increment it
                        string newRevision = string.Format("{0}.0", Convert.ToInt32(currentRevision.Version.Split('.')[0]) + 1);
                        revision.VersionComments = document.Description;  // Need to modify the document entity to include a hidden field for comments ??
                        revision.Version = newRevision;
                        revision.Name = newRevision;
                        createNewRevision = true;
                    }
                }

                if (createNewRevision)
                {
                    revision.FileExtension = document.FileExtension;
                    revision.ModifiedDate = DateTime.UtcNow;
                    revision.Size = document.Size;

                    // Get the entity that represents the user
                    UserAccount userAccount = Entity.Get<UserAccount>(RequestContext.GetContext().Identity.Id);

                    // Associate the revision to the account
                    revision.RevisionUpdatedBy = userAccount;
                    // Associate the file to this revision
                    revision.FileDataHash = document.FileDataHash;


                    SaveGraph saveGraph = EventTargetStateHelper.GetSaveGraph(state);
                    saveGraph.Entities[revision.Id] = revision;

                    document.CurrentDocumentRevision = revision;

                    // Associate the document to the revision (documentHasDocumentRevision - used for version history listing)
                    document.DocumentHasDocumentRevision.Add(revision);

                    // Associate the last created user to the document
                    if (initialRevision)
                    {
                        document.DocumentCreatedBy = userAccount;
                    }

                    // Associate the last modified user to the document
                    document.DocumentModifiedBy = userAccount;
                }

                document.DocumentHasDocumentType = document.DocumentFileType;

                if (document.InFolder == null)
                {
                    // Default document dumping ground if not specified
                    document.InFolder = Entity.Get<DocumentFolder>(new EntityRef("core", "documentsDocumentFolder"));
                }

            }
            return false;
        }
    }
}
