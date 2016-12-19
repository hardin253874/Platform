// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Common
{
    /// <summary>
    /// Captures a WalkGraphWithSteps graph walk step.
    /// </summary>
    /// <typeparam name="T">Type of nodes being tracked.</typeparam>
    public class WalkStep<T>
    {
        /// <summary>
        /// The node being visited in the graph walk.
        /// </summary>
        public T Node { get; set; }

        /// <summary>
        /// The previous step that led to this node, or null if this was an initial node.
        /// </summary>
        public WalkStep<T> PreviousStep { get; set; }
    }
}
