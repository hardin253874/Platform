// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Net;
using System.Security;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Contains the identifying values of a redis client.
	/// </summary>
	public static class Identity
	{
		/// <summary>
		///     The app domain identifier
		/// </summary>
		private static readonly Lazy<int> AppDomainIdField = new Lazy<int>( ( ) => AppDomain.CurrentDomain.Id, true );

		/// <summary>
		///     The app domain name escaped
		/// </summary>
		private static readonly Lazy<string> AppDomainNameEscapedField = new Lazy<string>( ( ) => SecurityElement.Escape( AppDomainName ), true );

		/// <summary>
		///     The app domain name
		/// </summary>
		private static readonly Lazy<string> AppDomainNameField = new Lazy<string>( ( ) => AppDomain.CurrentDomain.FriendlyName, true );

		/// <summary>
		///     The machine name escaped
		/// </summary>
		private static readonly Lazy<string> MachineNameEscapedField = new Lazy<string>( ( ) => SecurityElement.Escape( MachineName ), true );

		/// <summary>
		///     The machine name
		/// </summary>
		private static readonly Lazy<string> MachineNameField = new Lazy<string>( ( ) => Dns.GetHostEntry( "localhost" ).HostName, true );

		/// <summary>
		///     The Process identifier
		/// </summary>
		private static readonly Lazy<int> ProcessIdField = new Lazy<int>( ( ) => Process.GetCurrentProcess( ).Id, true );

		/// <summary>
		///     The process name escaped
		/// </summary>
		private static readonly Lazy<string> ProcessNameEscapedField = new Lazy<string>( ( ) => SecurityElement.Escape( ProcessName ), true );

		/// <summary>
		///     The process name
		/// </summary>
		private static readonly Lazy<string> ProcessNameField = new Lazy<string>( ( ) => Process.GetCurrentProcess( ).MainModule.ModuleName, true );

		/// <summary>
		///     Gets the application domain identifier.
		/// </summary>
		/// <value>
		///     The application domain identifier.
		/// </value>
		public static int AppDomainId
		{
			get
			{
				return AppDomainIdField.Value;
			}
		}


		/// <summary>
		///     Gets the name of the application domain.
		/// </summary>
		/// <value>
		///     The name of the application domain.
		/// </value>
		public static string AppDomainName
		{
			get
			{
				return AppDomainNameField.Value;
			}
		}

		/// <summary>
		///     Gets the name of the application domain (escaped).
		/// </summary>
		/// <value>
		///     The name of the application domain (escaped).
		/// </value>
		public static string AppDomainNameEscaped
		{
			get
			{
				return AppDomainNameEscapedField.Value;
			}
		}

		/// <summary>
		///     Gets the name of the machine.
		/// </summary>
		/// <value>
		///     The name of the machine.
		/// </value>
		public static string MachineName
		{
			get
			{
				return MachineNameField.Value;
			}
		}

		/// <summary>
		///     Gets the name of the machine (escaped).
		/// </summary>
		/// <value>
		///     The name of the machine (escaped).
		/// </value>
		public static string MachineNameEscaped
		{
			get
			{
				return MachineNameEscapedField.Value;
			}
		}

		/// <summary>
		///     Gets the process identifier.
		/// </summary>
		/// <value>
		///     The process identifier.
		/// </value>
		public static int ProcessId
		{
			get
			{
				return ProcessIdField.Value;
			}
		}

		/// <summary>
		///     Gets the name of the process.
		/// </summary>
		/// <value>
		///     The name of the process.
		/// </value>
		public static string ProcessName
		{
			get
			{
				return ProcessNameField.Value;
			}
		}

		/// <summary>
		///     Gets the name of the process  (escaped).
		/// </summary>
		/// <value>
		///     The name of the process (escaped).
		/// </value>
		public static string ProcessNameEscaped
		{
			get
			{
				return ProcessNameEscapedField.Value;
			}
		}
	}
}