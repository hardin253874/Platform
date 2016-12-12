// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System.Collections.Generic;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// The context used when traversing the instruction tree for writing output.
    /// </summary>
    class WriterContext
    {
        public WriterContext()
        {
            DataElementStack = new Stack<DataElement>();
            ImageIdsUsed = new HashSet<uint>();
        }


        /// <summary>
        /// Stack of data elements.
        /// </summary>
        public GeneratorSettings Settings { get; set; }


        /// <summary>
        /// External DI services.
        /// </summary>
        public ExternalServices ExternalServices { get; set; }


        /// <summary>
        /// Stack of data elements.
        /// </summary>
        public Stack<DataElement> DataElementStack { get; }


        /// <summary>
        /// The data element currently being visited. Always set.
        /// </summary>
        public DataElement CurrentDataElement
        {
            get
            {
				if ( DataElementStack.Count <= 0 )
                    return new DataElement(null, 0);
                return DataElementStack.Peek();
            }
        }


        /// <summary>
        /// The entity currently being visited. May be null.
        /// </summary>
        public IEntity CurrentEntity
        {
            get { return CurrentDataElement.Entity; }
        }


        /// <summary>
        /// The template document.
        /// </summary>
        public TemplateData Template { get; set; }


        /// <summary>
        /// The output document writer.
        /// </summary>
        public OpenXmlWriter Writer { get; set; }


        /// <summary>
        /// Set of image IDs that we've written in the output document.
        /// </summary>
        public HashSet<uint> ImageIdsUsed { get; }


        /// <summary>
        /// Accepts an image ID, and if it's already been used, allocate a different ID instead.
        /// </summary>
        /// <param name="existingId">The existing id.</param>
        /// <returns></returns>
        public uint AllocateImageId(uint existingId)
        {
            uint result = existingId;
            if (ImageIdsUsed.Contains(result))
            {
                Template.MaxImageId++;
                result = Template.MaxImageId;
            }
            ImageIdsUsed.Add(result);
            return result;
        }
    }
}
