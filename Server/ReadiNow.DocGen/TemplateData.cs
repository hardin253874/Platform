// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Contains information derived from parsing the template document.
    /// </summary>
    class TemplateData
    {
        /// <summary>
        /// The top-level instruction in the instruction tree.
        /// This contains the bulk of the parsed data.
        /// </summary>
        public Instruction RootInstruction { get; set; }

        /// <summary>
        /// Contains the highest ID of any image located.
        /// So when we start duplicating images, we can allocate new IDs that don't collide.
        /// </summary>
        public uint MaxImageId { get; set; }
    }
}
