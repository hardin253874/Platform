// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Messaging;
using System.Text;

namespace EDC.Messaging
{
	/// <summary>
	///     Provides helper methods for interacting with message queues.
	/// </summary>
	public static class MessageQueueHelper
	{
		/// <summary>
		///     Broadcasts the specified message to all private queue that match the filter criteria.
		/// </summary>
		/// <param name="message">
		///     A string containing the message to broadcast.
		/// </param>
		/// <param name="filter">
		///     A string containing the path-based filter criteria.
		/// </param>
		public static void BroadcastMessageToPrivateQueues( string message, string filter )
		{
			if ( String.IsNullOrEmpty( message ) )
			{
				throw new ArgumentException( "The specified message parameter is invalid." );
			}

			// Find the message queues that match the criteria
			List<MessageQueue> queues = FindPrivateQueues( filter );

			// Send the message to the selected queues
			foreach ( MessageQueue queue in queues )
			{
				// ReSharper disable EmptyGeneralCatchClause
				try
				{
					// Send the message to the specified queue.
					queue.Send( message );
				}
				catch
				{
					// Do nothing
				}
				// ReSharper restore EmptyGeneralCatchClause
			}
		}

		/// <summary>
		///     Gets a collection of private queue that match the filter criteria.
		/// </summary>
		/// <param name="filter">
		///     A string containing the path-based filter criteria.
		/// </param>
		/// <returns>
		///     A collection of private queues that match the filter criteria.
		/// </returns>
		public static List<MessageQueue> FindPrivateQueues( string filter )
		{
			var queues = new List<MessageQueue>( );

			// Get the private queues on the local machine
			MessageQueue[] list = MessageQueue.GetPrivateQueuesByMachine( "." );

			if ( ( list.Length > 0 ) )
			{
				string token = string.Empty;

				// Iterate through the queue to see if any match the filter
				foreach ( MessageQueue queue in list )
				{
					if ( !String.IsNullOrEmpty( filter ) )
					{
						if ( String.IsNullOrEmpty( token ) )
						{
							token = filter.ToLower( );
						}

						string path = queue.Path;
						if ( !String.IsNullOrEmpty( path ) )
						{
							if ( path.Contains( token ) )
							{
								queues.Add( queue );
							}
						}
					}
					else
					{
						queues.Add( queue );
					}
				}
			}

			return queues;
		}

		/// <summary>
		///     Gets a private queue with the specified name.
		/// </summary>
		/// <param name="name">
		///     A string containing the name of the queue.
		/// </param>
		/// <returns>
		///     An object that represents the message queue.
		/// </returns>
		/// <remarks>
		///     The name should not contains any path elements.
		/// </remarks>
		public static MessageQueue GetPrivateQueue( string name )
		{
			if ( String.IsNullOrEmpty( name ) )
			{
				throw new ArgumentException( "The specified name parameter is invalid." );
			}

			if ( name.IndexOfAny( new[]
				{
					'\\'
				} ) != -1 )
			{
				throw new ArgumentException( "The specified name parameter contains invalid or reserved characters." );
			}

			string path = String.Format( ".\\Private$\\{0}", name );

			// Get a private queue with the specified name
			MessageQueue queue = GetQueue( path );

			return queue;
		}

		/// <summary>
		///     Generates a unique name for a private queue.
		/// </summary>
		/// <param name="prefix">
		///     A string containing an optional prefix.
		/// </param>
		/// <param name="unique">
		///     true if the name is unique within the current app domain; otherwise false.
		/// </param>
		/// <returns>
		///     A string containing the unique name of the private queue.
		/// </returns>
		/// <remarks>
		///     If the unique flag is set to false, then the queue name will be shared across the app domain for the specified prefix.
		/// </remarks>
		public static string GetPrivateQueueName( string prefix, bool unique )
		{
			return GetPrivateQueueName( prefix, null, unique );
		}

		/// <summary>
		///     Generates a unique name for a private queue.
		/// </summary>
		/// <param name="prefix">
		///     A string containing an optional prefix.
		/// </param>
		/// <param name="postfix">
		///     A string containing an optional postfix.
		/// </param>
		/// <param name="unique">
		///     true if the name is unique within the current app domain; otherwise false.
		/// </param>
		/// <returns>
		///     A string containing the unique name of the private queue.
		/// </returns>
		/// <remarks>
		///     If the unique flag is set to false, then the queue name will be shared across the app domain for the specified prefix and postfix.
		/// </remarks>
		public static string GetPrivateQueueName( string prefix, string postfix, bool unique )
		{
			var builder = new StringBuilder( );

			// Add the prefix (if defined)
			if ( !String.IsNullOrEmpty( prefix ) )
			{
				builder.Append( prefix );
				builder.Append( '_' );
			}

			string body = !unique ? GetSafeQueueName( AppDomain.CurrentDomain.FriendlyName ) : Guid.NewGuid( ).ToString( );

			builder.Append( body );

			// Add the postfix (if defined)
			if ( !String.IsNullOrEmpty( postfix ) )
			{
				builder.Append( '_' );
				builder.Append( postfix );
			}

			return builder.ToString( );
		}

		/// <summary>
		///     Gets a message queue with the specified path.
		/// </summary>
		/// <param name="path">
		///     A string containing the path of the queue.
		/// </param>
		/// <returns>
		///     An object that represents the message queue.
		/// </returns>
		public static MessageQueue GetQueue( string path )
		{
			if ( String.IsNullOrEmpty( path ) )
			{
				throw new ArgumentException( "The specified path parameter is invalid." );
			}

			// Checks if the specified queue already exists
			MessageQueue queue = !MessageQueue.Exists( path ) ? MessageQueue.Create( path ) : new MessageQueue( path );

			return queue;
		}

		/// <summary>
		///     Gets a safe version of the queue name (or queue name component)
		/// </summary>
		private static string GetSafeQueueName( string queueName )
		{
			string safeQueueName = string.Empty;

			if ( !String.IsNullOrEmpty( queueName ) )
			{
				safeQueueName = ( queueName.ToLower( ).IndexOf( "testappdomain:", StringComparison.Ordinal ) ) != -1 ? "TestAppDomain" : queueName;
			}

			return safeQueueName;
		}
	}
}