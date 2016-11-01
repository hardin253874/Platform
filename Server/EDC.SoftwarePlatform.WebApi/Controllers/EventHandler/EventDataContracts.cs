// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EventHandler
{
	/// <summary>
	///     The software platform event object
	/// </summary>
	[DataContract]
	public class EventData
	{
		/// <summary>
		///     Gets or sets the message.
		/// </summary>
		/// <value>The message.</value>
		[DataMember( Name = "message" )]
		public string Message
		{
			get;
			set;
		}
	}
}