// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Choice Item Definition class.
	/// </summary>
	[DataContract]
	public class ChoiceItemDefinition
	{
		/// <summary>
		///     Gets or sets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		[DataMember( Name = "id", EmitDefaultValue = false, IsRequired = true )]
		public long EntityIdentifier
		{
			get;
			set;
		}

		/// <summary>
		///		Should the entity identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEntityIdentifier( )
		{
			return EntityIdentifier != 0;
		}

		/// <summary>
		///     Gets or sets the display name.
		/// </summary>
		/// <value>
		///     The display name.
		/// </value>
		[DataMember( Name = "name", EmitDefaultValue = false, IsRequired = false )]
		public string DisplayName
		{
			get;
			set;
		}

        [DataMember(Name = "fmt", EmitDefaultValue = false, IsRequired = false)]
        public ReportConditionalFormatRule FormatRule
        {
            get;
            set;
        }

        /// <summary>
        ///		Should the display name beserialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
		private bool ShouldSerializeDisplayName( )
		{
			return DisplayName != null;
		}
	}
}