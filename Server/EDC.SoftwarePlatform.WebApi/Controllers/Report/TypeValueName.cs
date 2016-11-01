// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Type Value Name class.
	/// </summary>
	[DataContract]
	public class TypeValueName
	{
		/// <summary>
		///     Gets or sets the type enumeration.
		/// </summary>
		/// <value>
		///     The type enumeration.
		/// </value>
		[DataMember( Name = "enum", EmitDefaultValue = false, IsRequired = true )]
		public string TypeEnumeration
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type enumeration be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTypeEnumeration( )
		{
			return TypeEnumeration != null;
		}

		/// <summary>
		///     Gets or sets the display name of the type.
		/// </summary>
		/// <value>
		///     The display name of the type.
		/// </value>
		[DataMember( Name = "name", EmitDefaultValue = false, IsRequired = true )]
		public string TypeDisplayName
		{
			get;
			set;
		}

		/// <summary>
		///		Should the display name of the type be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTypeDisplayName( )
		{
			return TypeDisplayName != null;
		}
	}
}