// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Xml;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Interface for versioned xml application deserializer.
	/// </summary>
	internal interface IXmlApplicationDeserializer
	{
        /// <summary>
        ///     Deserializes the application using the specified XML text reader.
        /// </summary>
        /// <param name="xmlReader">The XML reader.</param>
        /// <param name="context">The context.</param>
        PackageData Deserialize( XmlReader xmlReader, IProcessingContext context = null );
	}
}