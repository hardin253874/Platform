// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.Database.Types
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	public abstract class NumericType<T> : DatabaseType
		where T : struct
	{
	}
}