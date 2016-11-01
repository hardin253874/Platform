// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;
using EDC.Security;
using System.Linq;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Represents all data being passed into a request.
    /// </summary>
    [DataContract]
    public class ActionRequest
    {
        /// <summary>
        /// The resource ids for which data is being requested.
        /// </summary>
        [DataMember(Name = "ids")]
        public long[] SelectedResourceIds { get; set; }

        /// <summary>
        /// The screen(s) or form(s) that are acting as containers around where the request initiated.
        /// </summary>
        [DataMember(Name = "hostIds")]
        public long[] HostResourceIds { get; set; }

        /// <summary>
        /// The ids of the entity types that the host resource implements.
        /// </summary>
        [DataMember(Name = "hostTypeIds")]
        public List<long> HostTypeIds { get; set; }

        /// <summary>
        /// The id of the report that initiated the request. May be null if none was involved.
        /// </summary>
        [DataMember(Name = "reportId")]
        public long? ReportId { get; set; }

        /// <summary>
        /// The id of the entity being edited/viewed (this report is displayed in a form control/tab control which is used to view/edit a relationship of the entity). May be -1 if none was involved.
        /// </summary>
        [DataMember(Name = "formDataEntityId")]
        [DefaultValue (-1)]
        public long? FormDataEntityId { get; set; }

        /// <summary>
        /// The id of the entity type for the root report resource type. Used to override that in the root node of the report.
        /// </summary>
        [DataMember(Name = "entityTypeId")]
        public long? EntityTypeId { get; set; }

        /// <summary>
        /// The id of the resource that was last selected before the action request. May or may not be in <see cref="SelectedResourceIds"/>
        /// and may also be null.
        /// </summary>
        [DataMember(Name = "lastId")]
        public long? LastSelectedResourceId { get; set; }

        /// <summary>
        /// The id of a related resource that may be selected as part of a cell location click.
        /// </summary>
        [DataMember(Name = "cellId")]
        public long? CellSelectedResourceId { get; set; }

        /// <summary>
        /// Additional data provided to generate menu items.
        /// </summary>
        [DataMember(Name = "data")]
        public Dictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// The context under which action items are being presented in the console.
        /// </summary>
        [DataMember(Name = "display")]
        public ActionContext ActionDisplayContext { get; set; }

        /// <summary>
        /// The id of the form that initiated the request. May be null if none was involved. Used in form actions.
        /// </summary>
        [DataMember(Name = "formId")]
        public long? FormId { get; set; }
    }

	/// <summary>
	/// Action Request Padded
	/// </summary>
	public class ActionRequestExtended : ActionRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ActionRequestExtended"/> class.
		/// </summary>
		public ActionRequestExtended()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionRequestExtended"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		public ActionRequestExtended(ActionRequest request)
			: this( )
		{
			ActionDisplayContext = request.ActionDisplayContext;
			AdditionalData = request.AdditionalData;
			CellSelectedResourceId = request.CellSelectedResourceId;
			EntityTypeId = request.EntityTypeId;
			HostResourceIds = request.HostResourceIds;
			HostTypeIds = request.HostTypeIds;
			LastSelectedResourceId = request.LastSelectedResourceId;
			ReportId = request.ReportId;
            FormDataEntityId = request.FormDataEntityId ?? -1;
			SelectedResourceIds = request.SelectedResourceIds;
            FormId = request.FormId;
        }


        /// <summary>
        /// Checks the request state to see if there is any selection involved, and if it is for a single entity.
        /// </summary>
        public bool IsSingleSelection
		{
			get
			{
				return LastSelectedResourceId != null && SelectedResources != null && SelectedResources.Count == 1;
			}
		}

		/// <summary>
		/// Checks the request state to see if there is any selection involved, and if it is for multiple entities.
		/// </summary>
		public bool IsMultipleSelection
		{
			get
			{
				return SelectedResourceIds != null && SelectedResourceIds.Length > 1;
			}
		}

		/// <summary>
		/// If set to true, designates that the "New" menu should appear on the tool bar.
		/// </summary>
		public bool? ShowNewActionsButton
		{
			get;
			set;
		}

		/// <summary>
		/// If set to true, designates that the "Export" menu should appear on the tool bar.
		/// </summary>
		public bool? ShowExportActionsButton
		{
			get;
			set;
		}

        /// <summary>
		/// If set to true, designates that the "Edit Inline" button should appear on the toolbar.
		/// </summary>
        public bool? ShowEditInlineActionsButton { get; set; }

        /// <summary>
        /// Gets or sets the report.
        /// </summary>
        /// <value>
        /// The report.
        /// </value>
        public Report Report
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the last selected resource.
		/// </summary>
		/// <value>
		/// The last selected resource.
		/// </value>
		public Resource LastSelectedResource
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the host resources.
		/// </summary>
		/// <value>
		/// The host resources.
		/// </value>
		public IList<Resource> HostResources
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the selected resources.
		/// </summary>
		/// <value>
		/// The selected resources.
		/// </value>
		public IList<Resource> SelectedResources
		{
			get;
			set;
		}

		/// <summary>
		/// The entity types applicable to <see cref="Report"/>.
		/// </summary>
		public IList<EntityType> ReportTypes
		{
			get;
			set;
		}

		/// <summary>
		/// The entity types applicable to those resources in <see cref="HostResources"/>.
		/// </summary>
		public IList<EntityType> HostResourceTypes
		{
			get;
			set;
		}

		/// <summary>
		/// The entity types applicable to those resources in <see cref="SelectedResources"/>.
		/// </summary>
		public IList<EntityType> SelectedResourceTypes
		{
			get;
			set;
		}

		/// <summary>
		/// The entity type at the heart of the report's query expression.
		/// </summary>
		public EntityType ReportBaseType
		{
			get;
			set;
		}

		/// <summary>
		/// All the entity types referenced in the query of the report.
		/// </summary>
		public IList<EntityType> ReportUsedTypes
		{
			get;
			set;
		}

		/// <summary>
		/// The action items suppressed by any behaviors that are referenced.
		/// </summary>
		public IList<ActionMenuItem> SuppressedActionItems
		{
			get;
			set;
		}

		/// <summary>
		/// The create action items suppressed on this menu.
		/// </summary>
		public IList<EntityType> SuppressedNewActionTypes
		{
			get;
			set;
		}

		/// <summary>
		/// The action items meant to be shown as buttons by referenced behaviors.
		/// </summary>
		public IList<ActionMenuItem> IncludedAsButtonActionItems
		{
			get;
			set;
		}

		/// <summary>
		/// The create action items included as a button on this menu.
		/// </summary>
		public IList<EntityType> IncludedAsButtonNewActionTypes
		{
			get;
			set;
		}

        /// <summary>
		/// Gets or sets the form.
		/// </summary>
		/// <value>
		/// The form that has actions associated with.
		/// </value>
		public CustomEditForm Form
        {
            get;
            set;
        }

        /// <summary>
		/// The entity type that can by edited by the form.
		/// </summary>
		public EntityType TypeToEditWithForm
        {
            get;
            set;
        }

        /// <summary>
        /// The timezone that the request originated in.
        /// </summary>
	    public string TimeZone
	    {
	        get;
            set;
	    }

        /// <summary>
        /// Get the data hash for the request
        /// </summary>
        /// <returns></returns>
        public int GetRequestHash()
        {
            return CryptoHelper.HashObjects(new List<object>{
                ActionDisplayContext,
                HashAdditionalData(AdditionalData),
                CellSelectedResourceId,
                EntityTypeId,
                CryptoHelper.HashValues(HostResourceIds),
                CryptoHelper.HashValues(HostTypeIds),
                LastSelectedResourceId,
                ReportId,
                FormDataEntityId,
                CryptoHelper.HashValues(SelectedResourceIds),
                FormId
            });
        } 

        int HashAdditionalData(Dictionary<string, object> data)
        {
            if (data != null)
            {
                // Probably not the most efficent way to hash a dictionary, but it's probably good enough
                return CryptoHelper.HashHashes(
                    data.Select(kvp =>
                        CryptoHelper.HashObjects(new List<object> { kvp.Key, kvp.Value })));
            }
            else
                return 0;
        }
	}
}