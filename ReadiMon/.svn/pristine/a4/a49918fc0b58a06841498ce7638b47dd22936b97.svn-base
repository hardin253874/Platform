// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Messages
{
	/// <summary>
	///     Restore UI message.
	/// </summary>
	[DataContract( Name = "RestoreUiMessage" )]
	public class RestoreUiMessage
	{
        /// <summary>
        /// The section name to open when restoring (Optional).
        /// </summary>
        [DataMember]
        public string Section { get; set; }

        /// <summary>
        /// The entry name to open when restoring (Optional).
        /// </summary>
        [DataMember]
        public string Entry { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RestoreUiMessage"/> class.
		/// </summary>
		/// <param name="showEntityBrowser">if set to <c>true</c> [show entity browser].</param>
		public RestoreUiMessage( bool showEntityBrowser )
		{
			ShowEntityBrowser = showEntityBrowser;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [show entity browser].
		/// </summary>
		/// <value>
		///   <c>true</c> if [show entity browser]; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool ShowEntityBrowser
		{
			get;
			set;
		}


		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return Serializer.SerializeObject( this );
		}
	}
}