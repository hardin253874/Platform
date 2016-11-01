// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Model.PartialClasses
{
	/// <summary>
	///     Dependency failure class.
	/// </summary>
	public class DependencyFailure
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DependencyFailure" /> class.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="dependencyName">Name of the dependency.</param>
		/// <param name="minVersion">The minimum version.</param>
		/// <param name="maxVersion">The maximum version.</param>
		/// <param name="reason">The reason.</param>
		public DependencyFailure( Guid applicationId, string name, string dependencyName, Version minVersion, Version maxVersion, DependencyFailureReason reason )
		{
			ApplicationId = applicationId;
			Name = name;
			DependencyName = dependencyName;
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			Reason = reason;
		}

		/// <summary>
		///     Gets the application identifier.
		/// </summary>
		/// <value>
		///     The application identifier.
		/// </value>
		public Guid ApplicationId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the current version.
		/// </summary>
		/// <value>
		///     The current version.
		/// </value>
		public Version CurrentVersion
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the name of the dependency.
		/// </summary>
		/// <value>
		///     The name of the dependency.
		/// </value>
		public string DependencyName
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the name of the dependent.
		/// </summary>
		/// <value>
		///     The name of the dependent.
		/// </value>
		public string DependentName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the maximum version.
		/// </summary>
		/// <value>
		///     The maximum version.
		/// </value>
		public Version MaxVersion
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the minimum version.
		/// </summary>
		/// <value>
		///     The minimum version.
		/// </value>
		public Version MinVersion
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the reason.
		/// </summary>
		/// <value>
		///     The reason.
		/// </value>
		public DependencyFailureReason Reason
		{
			get;
			set;
		}
	}
}