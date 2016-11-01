// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Login Result
	/// </summary>
	[DataContract]
	public class LoginResult : RedirectResult
	{
		/// <summary>
		///     Gets or sets the initial settings.
		/// </summary>
		/// <value>
		///     The initial settings.
		/// </value>
		[DataMember( Name = "initialSettings", EmitDefaultValue = false, IsRequired = false )]
		public InitialSettings InitialSettings { get; set; }

		/// <summary>
		///		Should the initial settings value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeInitialSettings( )
	    {
			return InitialSettings != null;
	    }

		/// <summary>
		///     Gets or sets the active account info.
		/// </summary>
		/// <value>
		///     The active account info.
		/// </value>
		[DataMember( Name = "activeAccountInfo", EmitDefaultValue = false, IsRequired = false )]
		public ActiveAccountInfo ActiveAccountInfo { get; set; }

		/// <summary>
		///		Should the active account information value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeActiveAccountInfo( )
	    {
			return ActiveAccountInfo != null;
	    }

		/// <summary>
		///     Gets or sets the test authorization token. This value is only provided during integration testing.
		/// </summary>
		/// <value>
		///     The active account info.
		/// </value>
		[DataMember( Name = "testToken", EmitDefaultValue = false, IsRequired = false )]
		public string TestToken { get; set; }

		/// <summary>
		///		Should the test token value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTestToken( )
	    {
			return TestToken != null;
	    }


        /// <summary>
        ///     Gets or sets the console timeout setting
        /// </summary>
        /// <value>
        ///     The console timeout info.
        /// </value>
        [DataMember(Name = "consoleTimeoutMinutes", EmitDefaultValue = false, IsRequired = false)]
        public double ConsoleTimeoutMinutes { get; set; }

		/// <summary>
		///		Should the console timeout minutes value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeConsoleTimeoutMinutes( )
	    {
			return ConsoleTimeoutMinutes != 0d;
	    }
	}
}