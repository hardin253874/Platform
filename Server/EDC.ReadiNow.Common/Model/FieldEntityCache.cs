// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.Cache;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Field to Entity cache.
	/// </summary>
	internal class FieldEntityCache : DistributedMemoryManagerCache<long, ISet<long>, FieldEntityCacheMessage>
	{
        /// <summary>
		///     Represents the singleton instance of the cache.
		/// </summary>
		private static readonly Lazy<FieldEntityCache> InstanceMemberInner = new Lazy<FieldEntityCache>( ( ) => new FieldEntityCache( ), true );

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldEntityCache" /> class.
        /// </summary>
        private FieldEntityCache( )
			: base( "FieldEntity" )
		{
		}

	    /// <summary>
	    ///     Gets the instance.
	    /// </summary>
	    public static FieldEntityCache InstanceInner => InstanceMemberInner.Value;

		/// <summary>
		///     Handles the MessageReceived event of the channel.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MessageEventArgs{TMessage}" /> instance containing the event data.</param>
		protected override void Channel_MessageReceived( object sender, MessageEventArgs<FieldEntityCacheMessage> e )
		{
            using (Entity.DistributedMemoryManager.Suppress())
            {
                if (MessageReceivedAction != null)
                {
                    MessageReceivedAction(e);
                }

                if (e.Message.Clear)
                {
                    ProviderClear();
                    return;
                }

                if (e.Message.RemoveKeys != null && e.Message.RemoveKeys.Count > 0)
                {
                    foreach (long key in e.Message.RemoveKeys)
                    {
                        ProviderRemove(key);
                    }
                }
            }            
		}

		/// <summary>
		///     Handles the Cleared event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="CacheEventArgs" /> instance containing the event data.</param>
		protected override void DistributedMemoryManagerCache_Cleared( object sender, CacheEventArgs e )
		{
			var message = new FieldEntityCacheMessage
			{
				Clear = true
			};

			MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
		}

		/// <summary>
		///     Handles the Removed event of the DistributedMemoryManagerCache control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyCacheEventArgs{TKey}" /> instance containing the event data.</param>
		protected override void DistributedMemoryManagerCache_Removed( object sender, KeyCacheEventArgs<long> e )
		{
			var message = new FieldEntityCacheMessage( );
			message.RemoveKeys.Add( e.Key );

			MessageChannel.Publish( message, PublishOptions.None, false, MergeMessages );
		}

		/// <summary>
		///     Merges the messages.
		/// </summary>
		/// <param name="existingMessage">The existing message.</param>
		/// <param name="newMessage">The new message.</param>
		private void MergeMessages( FieldEntityCacheMessage existingMessage, FieldEntityCacheMessage newMessage )
		{
			existingMessage.Clear |= newMessage.Clear;
			existingMessage.RemoveKeys.UnionWith( newMessage.RemoveKeys );
		}
	}
}