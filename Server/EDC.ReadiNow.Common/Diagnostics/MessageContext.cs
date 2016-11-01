// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Diagnostics
{
    /// <summary>
    /// A context that allows aggregation of messages, usually for logging, across multiple, weakly-coupled classes.
    /// </summary>
    /// <seealso cref="MessageContextEntry"/>
    public class MessageContext : IDisposable
    {
        internal static readonly string SlotNamePrefix = "ReadiNow Message Context ";
        internal const string Indent = "    ";

        private readonly MessageContextEntry _entry;
        private readonly Action<MessageContext> _disposeAction;
        private bool _disposed;

        /// <summary>
        /// Create a new <see cref="MessageContext"/>.
        /// </summary>
        /// <param name="name">
        /// The message name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="behavior">
        /// (Optional) Modifications to the default behavior of <see cref="MessageContext"/>.
        /// </param>
        /// <param name="disposeAction">
        /// (Optional) Callback to invoke on disposal.
        /// </param>
        public MessageContext(string name, MessageContextBehavior behavior = MessageContextBehavior.Default, Action<MessageContext> disposeAction = null)
            : this(name, ContextType.New, behavior, disposeAction)
        {
            // Do nothing
        }

        /// <summary>
        /// Creata a new <see cref="MessageContext"/> from
        /// an existing set of entities.
        /// </summary>
        /// <param name="name">
        /// The message name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="contextType">
        /// Whether this is a new or attached context.
        /// </param>
        /// <param name="behavior">
        /// (Optional) Modifications to the default behavior of <see cref="MessageContext"/>.
        /// </param>
        /// <param name="disposeAction">
        /// (Optional) Callback to invoke on disposal.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="contextType"/> is unknown or invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        internal MessageContext( string name, ContextType contextType, MessageContextBehavior behavior = MessageContextBehavior.Default, Action<MessageContext> disposeAction = null )
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(name);
            }

            Name = name;
            ContextType = contextType;
            SlotName = SlotNamePrefix + name;

            if (contextType == ContextType.New)
            {
                _entry = new MessageContextEntry(behavior);
                ContextHelper<MessageContextEntry>.PushContextData(SlotName, _entry);
            }
            else if (contextType == ContextType.Attached)
            {
                ContextHelper<MessageContextEntry>.GetContextDataStack(SlotName).TryPeek(out _entry);
            }
            else if (contextType == ContextType.Detached)
            {
                _entry = new MessageContextEntry(behavior);
            }
            else
            {
                throw new ArgumentException(string.Format("Unknown contextType '{0}'", contextType), "contextType");
            }

            _disposeAction = disposeAction;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~MessageContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        /// <param name="disposing">
        /// True if called from Dispose, false if called from the finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            // No unmanaged resources so disposing is unused

            if (!_disposed)
            {
                if ( _disposeAction != null )
                {
                    _disposeAction( this );
                }

                try
                {
                    if (ContextType == ContextType.New)
                    {
                        ContextHelper<MessageContextEntry>.PopContextData(SlotName, _entry);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("Unable to free cache context data: {0}", ex);
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Get the current (top most) cache context if the context exists or a 
        /// detached context otherwise.
        /// </summary>
        /// <param name="name">
        /// The name of this message. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="behavior">
        /// (Optional) Modifications to the default behavior of <see cref="MessageContext"/>.
        /// This is ignored for an existing message context.
        /// </param>
        /// <returns>
        /// The current <see cref="CacheContext"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// There is no current cache context.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        /// <seealso cref="IsSet"/>
        public static MessageContext GetContext(string name, MessageContextBehavior behavior = MessageContextBehavior.Default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(name);
            }

            return new MessageContext(
                name, 
                IsSet(name) ? ContextType.Attached : ContextType.Detached,
                behavior);
        }

        /// <summary>
        /// Has the context been set, i.e. are we in a using block? 
        /// </summary>
        /// <param name="name">
        /// The name of this message. This cannot be null, empty or whitespace.
        /// </param>
        /// <returns>
        /// True if the context has been set, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> cannot be null, empty or whitespace.
        /// </exception>
        public static bool IsSet(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(name);
            }

            return ContextHelper<MessageContextEntry>.IsSet(SlotNamePrefix + name);
        }

        /// <summary>
        /// Whether this context instance is new or attached.
        /// </summary>
        public ContextType ContextType { get; private set; }

        /// <summary>
        /// The message name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The message behavior.
        /// </summary>
        public MessageContextBehavior Behavior
        {
            get { return _entry.Behavior; }
        }

        /// <summary>
        /// Append a line of text to the event log.
        /// </summary>
        /// <param name="messageFactory">
        /// If one or more message contexts are constructing a message,
        /// this is called to get a line to append to the message. This
        /// cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="messageFactory"/> cannot be null.
        /// </exception>
        public void Append(Func<string> messageFactory)
        {
            if (messageFactory == null)
            {
                throw new ArgumentNullException("messageFactory");
            }

            string currentIndent;
            string line;
            IList<MessageContextEntry> messageContextEntries;

            line = null;
            currentIndent = string.Empty;
            messageContextEntries =
                ContextHelper<MessageContextEntry>
                    .GetContextDataStack(SlotName).SkipWhile(x => x != _entry)
                    .ToList();
            foreach (MessageContextEntry entry in messageContextEntries)
            {
                if (entry.Message != null)
                {
                    if (line == null)
                    {
                        line = messageFactory();
                    }

                    if (entry.Message.Length > 0)
                    {
                        entry.Message.AppendLine();
                    }
                    entry.Message.Append(currentIndent);
                    entry.Message.Append(line);
                }

                currentIndent += Indent;

                if ((entry.Behavior & MessageContextBehavior.New) == MessageContextBehavior.New)
                {
                    break;
                }
            }  
        }

        /// <summary>
        /// Construct the message from this context and any child contexts.
        /// </summary>
        /// <returns>
        /// The concatenated message.
        /// </returns>
        public string GetMessage()
        {
            return _entry.Message != null ? _entry.Message.ToString() : string.Empty;
        }

        /// <summary>
        /// The slot name used.
        /// </summary>
        internal string SlotName { get; private set; }
    }
}
