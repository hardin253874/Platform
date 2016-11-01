// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     Hash settings
	/// </summary>
	public class HashSettings
	{
		#region Public Methods

		/// <summary>
		///     Gets the hash settings given a specified version.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentOutOfRangeException">version</exception>
		public static HashSettings GetHashSettings( int version = HashVersion11 )
		{
			HashSettings settings;

			switch ( version )
			{
				case HashVersion10:
					settings = new HashSettings
					{
						Version = version,
						SaltBytesCount = SaltSizeV10,
						HashBytesCount = HashSizeV10,
						IterationsCount = IterationsCountV10
					};
					break;

				case HashVersion11:
					settings = new HashSettings
					{
						Version = version,
						SaltBytesCount = SaltSizeV11,
						HashBytesCount = HashSizeV11,
						IterationsCount = IterationsCountV11
					};
					break;

				default:
					settings = null;
					break;
			}

			if ( settings == null )
			{
				throw new ArgumentOutOfRangeException( nameof( version ) );
			}

			return settings;
		}

		#endregion

		#region Constants

		/// <summary>
		///     The hash size V10
		/// </summary>
		private const int HashSizeV10 = 24;

		/// <summary>
		///     The hash size V11
		/// </summary>
		private const int HashSizeV11 = 24;

		/// <summary>
		///     Settings for pre release
		/// </summary>
		private const int HashVersion10 = 1;

		/// <summary>
		///     Settings for release
		/// </summary>
		private const int HashVersion11 = 2;

		/// <summary>
		///     The iterations count V10
		/// </summary>
		private const int IterationsCountV10 = 1000;

		/// <summary>
		///     The iterations count V11
		/// </summary>
		private const int IterationsCountV11 = 20000;

		/// <summary>
		///     The salt size V10
		/// </summary>
		private const int SaltSizeV10 = 24;

		/// <summary>
		///     The salt size V11
		/// </summary>
		private const int SaltSizeV11 = 24;

		#endregion

		#region Properties

		/// <summary>
		///     Gets or sets the hash bytes count.
		/// </summary>
		/// <value>
		///     The hash bytes count.
		/// </value>
		public int HashBytesCount
		{
			get;
			private set;
		}


		/// <summary>
		///     Gets or sets the iterations count.
		/// </summary>
		/// <value>
		///     The iterations count.
		/// </value>
		public int IterationsCount
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the salt bytes count.
		/// </summary>
		/// <value>
		///     The salt bytes count.
		/// </value>
		public int SaltBytesCount
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public int Version
		{
			get;
			private set;
		}

		#endregion Properties
	}
}