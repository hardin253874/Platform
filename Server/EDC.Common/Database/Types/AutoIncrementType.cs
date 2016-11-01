// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.Database.Types
{
	/// <summary>
	///     An AutoNumber type
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	public class AutoIncrementType : Int32Type
	{
		/// <summary>
		///     Gets the display name.
		/// </summary>
		public new static string DisplayName
		{
			get
			{
				return "AutoIncrement";
			}
		}
	}
}