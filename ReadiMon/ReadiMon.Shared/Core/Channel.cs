// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Channel class.
	/// </summary>
	public class Channel : IChannel
	{
		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="message">The message.</param>
		public void SendMessage( string message )
		{
			var deserializeObject = Serializer.DeserializeObject<object>( message );

			var statusTextMessage = deserializeObject as StatusTextMessage;

			if ( statusTextMessage != null )
			{
				OnStatusTextMessageReceived( statusTextMessage );
				return;
			}

			var entityBrowserMessage = deserializeObject as EntityBrowserMessage;

			if ( entityBrowserMessage != null )
			{
				OnEntityBrowserMessageReceived( entityBrowserMessage );
				return;
			}

			var restoreUiMessage = deserializeObject as RestoreUiMessage;

			if ( restoreUiMessage != null )
			{
				OnRestoreUiMessageReceived( restoreUiMessage );
				return;
			}

			var exitMessage = deserializeObject as ExitMessage;

			if ( exitMessage != null )
			{
				OnExitMessageReceived( exitMessage );
				return;
			}

			var showWhenMinimizedMessage = deserializeObject as ShowWhenMinimizedMessage;

			if ( showWhenMinimizedMessage != null )
			{
				OnShowWhenMinimizedMessageReceived( showWhenMinimizedMessage );
				return;
			}

			var hideWhenMinimizedMessage = deserializeObject as HideWhenMinimizedMessage;

			if ( hideWhenMinimizedMessage != null )
			{
				OnHideWhenMinimizedMessageReceived( hideWhenMinimizedMessage );
				return;
			}

			var balloonSettingsMessage = deserializeObject as BalloonSettingsMessage;

			if ( balloonSettingsMessage != null )
			{
				OnBalloonSettingsMessageReceived( balloonSettingsMessage );
			}

		    var perfGraphMessage = deserializeObject as PerfGraphMessage;

		    if ( perfGraphMessage != null )
		    {
		        OnPerfGraphMessageReceived( perfGraphMessage );
		    }

		    var metricsUpdateMessage = deserializeObject as MetricsUpdateMessage;

		    if ( metricsUpdateMessage != null )
		    {
		        OnMetricsUpdateMessageReceived( metricsUpdateMessage );
		    }
		}

        /// <summary>
        /// Called when the metrics update message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnMetricsUpdateMessageReceived(MetricsUpdateMessage message)
        {
            var evt = OnMetricsUpdateMessage;

            if (evt != null)
            {
                evt(this, message);
            }
        }

        /// <summary>
        /// Called when the perf graph message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void OnPerfGraphMessageReceived(PerfGraphMessage message)
        {
            var evt = OnPerfGraphMessage;

            if (evt != null)
            {
                evt(this, message);
            }
        }

		/// <summary>
		///     Called when the balloon settings message is received.
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnBalloonSettingsMessageReceived( BalloonSettingsMessage message )
		{
			var evt = OnBalloonSettingsMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Called when [entity browser message received].
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnEntityBrowserMessageReceived( EntityBrowserMessage message )
		{
			var evt = OnEntityBrowserMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Called when [exit message received].
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnExitMessageReceived( ExitMessage message )
		{
			var evt = OnExitMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Called when a hide when minimized message is received.
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnHideWhenMinimizedMessageReceived( HideWhenMinimizedMessage message )
		{
			var evt = OnHideWhenMinimizedMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Called when a restore UI message is received.
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnRestoreUiMessageReceived( RestoreUiMessage message )
		{
			var evt = OnRestoreUiMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Called when a show when minimized message is received.
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnShowWhenMinimizedMessageReceived( ShowWhenMinimizedMessage message )
		{
			var evt = OnShowWhenMinimizedMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Called when a status text message is received.
		/// </summary>
		/// <param name="message">The message.</param>
		protected void OnStatusTextMessageReceived( StatusTextMessage message )
		{
			var evt = OnStatusTextMessage;

			if ( evt != null )
			{
				evt( this, message );
			}
		}

		/// <summary>
		///     Occurs when a status text message arrives.
		/// </summary>
		public static event EventHandler<StatusTextMessage> OnStatusTextMessage;

		/// <summary>
		///     Occurs when an entity browser message arrives.
		/// </summary>
		public static event EventHandler<EntityBrowserMessage> OnEntityBrowserMessage;

		/// <summary>
		///     Occurs when a restore UI message arrives.
		/// </summary>
		public static event EventHandler<RestoreUiMessage> OnRestoreUiMessage;

		/// <summary>
		///     Occurs when an exit message arrives.
		/// </summary>
		public static event EventHandler<ExitMessage> OnExitMessage;

		/// <summary>
		///     Occurs when a show when minimized message arrives.
		/// </summary>
		public static event EventHandler<ShowWhenMinimizedMessage> OnShowWhenMinimizedMessage;

		/// <summary>
		///     Occurs when a hide when minimized message arrives.
		/// </summary>
		public static event EventHandler<HideWhenMinimizedMessage> OnHideWhenMinimizedMessage;

		/// <summary>
		///     Occurs when the balloon settings change.
		/// </summary>
		public static event EventHandler<BalloonSettingsMessage> OnBalloonSettingsMessage;

        /// <summary>
        ///     Occurs when a message to do with perf log has been received.
        /// </summary>
	    public static event EventHandler<PerfGraphMessage> OnPerfGraphMessage;

        /// <summary>
        ///     Occurs when a message to do with updated licensing metrics has been received.
        /// </summary>
	    public static event EventHandler<MetricsUpdateMessage> OnMetricsUpdateMessage;
	}
}