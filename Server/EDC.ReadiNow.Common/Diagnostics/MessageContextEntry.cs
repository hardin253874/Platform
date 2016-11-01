// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Diagnostics;

namespace EDC.ReadiNow.Diagnostics
{
    /// <summary>
    /// A message entry in the message context (which contains the actual message being assembled).
    /// </summary>
    /// <seealso cref="MessageContext"/>
    public class MessageContextEntry
    {
        /// <summary>
        /// Create a new <see cref="MessageContext"/>.
        /// </summary>
        /// <param name="behavior">
        /// Modifications to the default behavior of <see cref="MessageContextEntry"/>.
        /// </param>
        public MessageContextEntry(MessageContextBehavior behavior = MessageContextBehavior.Default)
        {
            Message = (behavior & MessageContextBehavior.Capturing) == MessageContextBehavior.Capturing ? new StringBuilder() : null;
            Behavior = behavior;
        }

        /// <summary>
        /// The message.
        /// </summary>
        public StringBuilder Message { get; private set; }

        /// <summary>
        /// Modifications to the default behavior of <see cref="MessageContextEntry"/>.
        /// </summary>
        public MessageContextBehavior Behavior { get; private set; }
    }
}
