// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Xml;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Interface for versioned xml application deserializer.
	/// </summary>
	internal interface IXmlApplicationSerializer
	{
		/// <summary>
        ///     Sets the package data.
        /// </summary>
        /// <value>
        ///     The package data.
        /// </value>
        PackageData PackageData
        {
			set;
		}

		/// <summary>
		///     Serializes the application using the specified XML writer.
		/// </summary>
		/// <param name="xmlWriter">The XML writer.</param>
		/// <param name="context">The context.</param>
		void Serialize( XmlWriter xmlWriter, IProcessingContext context = null );
	}
}