// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Services.Console
{
	/// <summary>
	///     Specifies information enough to represent an <see cref="ActionMenuItem" /> on the client.
	/// </summary>
	[DataContract]
	public class ActionMenuItemInfo
	{
		public const string AdditionalDataRelatedResourceArgKey = "RelatedResourceArg";
		public const string AdditionalDataReportTemplateIdKey = "ReportTemplateId";
		public const string AdditionalDataResourceReplaceKey = "%Resource%";
		public const string AdditionalDataTypeReplaceKey = "%Type%";
		public const string AdditionalDataWorkflowKey = "Workflow";

		/// <summary>
		///     Basic constructor.
		/// </summary>
		public ActionMenuItemInfo( )
		{
			IsEnabled = true;
		}

		/// <summary>
		///     Additional data that can be passed back, and accessible by the Class.
		/// </summary>
		[DataMember( Name = "data", EmitDefaultValue = false )]
		public Dictionary<string, object> AdditionalData
		{
			get;
			set;
		}

		/// <summary>
		///		Should the additional data be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAdditionalData( )
	    {
		    return AdditionalData != null;
	    }

		/// <summary>
		///     This action operates when more than one selection is made.
		/// </summary>
		[DataMember( Name = "ismultiselect" )]
		public bool AppliesToMultipleSelection
		{
			get;
			set;
		}

		/// <summary>
		///     This action operates when a selection is made.
		/// </summary>
		[DataMember( Name = "isselect" )]
		public bool AppliesToSelection
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates if, in the current context, the IsButton state may be changed.
		/// </summary>
		[DataMember( Name = "canEditIsButton" )]
		public bool? CanEditIsButton
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates if, in the current context, the IsEnabled state may be changed.
		/// </summary>
		[DataMember( Name = "canEditIsEnabled" )]
		public bool? CanEditIsEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Child menu items.
		/// </summary>
		[DataMember( Name = "children", EmitDefaultValue = false )]
		public List<ActionMenuItemInfo> Children
		{
			get;
			set;
		}

		/// <summary>
		///		Should the children be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeChildren( )
	    {
		    return Children != null;
	    }

		/// <summary>
		///     The description of the item.
		/// </summary>
		[DataMember( Name = "description" )]
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     The action label to present when displayed.
		/// </summary>
		[DataMember( Name = "displayname" )]
		public string DisplayName
		{
			get;
			set;
		}

		/// <summary>
		///     The action label to present when disabled or selection information is not to be included.
		/// </summary>
		[DataMember( Name = "emptyname" )]
		public string EmptySelectName
		{
			get;
			set;
		}

		/// <summary>
		///     The related entity identifier.
		/// </summary>
		[DataMember( Name = "eid" )]
		public long EntityId
		{
			get;
			set;
		}

		/// <summary>
		///     Identifies the client action to perform when executed.
		/// </summary>
		[DataMember( Name = "method" )]
		public string HtmlActionMethod
		{
			get;
			set;
		}

		/// <summary>
		///     The control action passed to the client side class. I.e. verb.
		/// </summary>
		[DataMember( Name = "state" )]
		public string HtmlActionState
		{
			get;
			set;
		}

		/// <summary>
		///     The icon to present with the item label.
		/// </summary>
		[DataMember( Name = "icon" )]
		public string Icon
		{
			get;
			set;
		}

		/// <summary>
		///     The action menu item identifier.
		/// </summary>
		[DataMember( Name = "id" )]
		public long Id
		{
			get;
			set;
		}

        /// <summary>
        ///     The action menu item alias.
        /// </summary>
        [DataMember(Name = "alias")]
        public string Alias
        {
            get;
            set;
        }

		/// <summary>
		///     Indicates that this action is to be presented as a button outside of the menu.
		/// </summary>
		[DataMember( Name = "isbutton" )]
		public bool IsButton
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates that this action is to be presented on a right-click menu.
		/// </summary>
		[DataMember( Name = "iscontext" )]
		public bool IsContextMenu
		{
			get;
			set;
		}

		/// <summary>
		///     True if this action is included in the menu.
		/// </summary>
		[DataMember( Name = "isenabled" )]
		public bool IsEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates that this action is to be presented on a the actions menu.
		/// </summary>
		[DataMember( Name = "ismenu" )]
		public bool IsMenu
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates that this item is to be considered as an item divider.
		/// </summary>
		[DataMember( Name = "isseparator" )]
		public bool IsSeparator
		{
			get;
			set;
		}

		/// <summary>
		///     Indicates that this action is owned by the system and can not be modified.
		/// </summary>
		[DataMember( Name = "issystem" )]
		public bool IsSystem
		{
			get;
			set;
        }

        /// <summary>
        ///     The action represents an instruction to create a 'new instance'
        /// </summary>
        [DataMember( Name = "isnew", EmitDefaultValue = false )]
        public bool IsNew
        {
            get;
            set;
        }

        /// <summary>
        ///     The action label to present when there a multiple selected items.
        /// </summary>
        [DataMember( Name = "multiname" )]
		public string MultiSelectName
		{
			get;
			set;
		}

		/// <summary>
		///     The name or label of the item.
		/// </summary>
		[DataMember( Name = "name" )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     The ordinal of the item in a menu.
		/// </summary>
		[DataMember( Name = "order" )]
		public int Order
		{
			get;
			set;
		}

        /// <summary>
        ///     An expression that may affect the <see cref="IsEnabled"/> state of this item.
        /// </summary>
	    [DataMember(Name = "expression")]
	    public string Expression
	    {
	        get;
            set;
	    }
        
        /// <summary>
        ///     A list of name / id pairs expected to be used in the expression.
        /// </summary>
        [DataMember(Name = "expressionEntities")]
	    public Dictionary<string, ActionEntityReference> ExpressionEntities
	    {
	        get;
            set;
	    }

        #region Non Contract

        /// <summary>
	    /// Security information specific to this action.
	    /// </summary>
        [IgnoreDataMember]
        public List<Permission> RequiresPermissions { get; set; }

        /// <summary>
        /// Security information specific to the parent context of this action.
        /// </summary>
        [IgnoreDataMember]
        public List<Permission> RequiresParentPermissions { get; set; }

        /// <summary>
        ///  Specifies an alternate target to the action then that inherited from the behavior.
        /// </summary>
        [IgnoreDataMember]
        public string HtmlActionTarget { get; set; }

		/// <summary>
		///     A human-readable representation.
		/// </summary>
		/// <returns></returns>
		public override string ToString( )
		{
			return string.Format( "{0} ({1})", Name, Id );
        }

        #endregion
    }

	/// <summary>
	///     Assorted helper methods for use with ActionMenuItems.
	/// </summary>
	public static class ActionMenuItemHelper
	{
		/// <summary>
		///     Combines a template and additional data to concatenate a suitable label for the action.
		/// </summary>
		/// <param name="label">The label template.</param>
		/// <param name="target">Target of the action.</param>
		/// <param name="additionalData">Additional data as a dictionary.</param>
		/// <returns>The formatted label.</returns>
		private static string GetDisplayName( string label, ActionTargetInfo target, Dictionary<string, object> additionalData )
		{
			var displayName = label ?? "";
			var resourceName = "";
			var typeName = "";

			if ( target != null && target.Entity != null )
			{
				if ( target.Entity.Is<EntityType>( ) )
				{
					var type = target.Entity.As<EntityType>( );
					resourceName = type.Name;
					typeName = type.Name;
				}
				else
				{
					if ( target.Entity.Is<Resource>( ) )
					{
						var resource = target.Entity.As<Resource>( );
						typeName = resource.IsOfType.First( ).Name;
						resourceName = !string.IsNullOrEmpty( resource.Name ) ? resource.Name : typeName;
					}
				}

				additionalData.Add( ActionMenuItemInfo.AdditionalDataResourceReplaceKey, resourceName ?? "" );
				additionalData.Add( ActionMenuItemInfo.AdditionalDataTypeReplaceKey, typeName ?? "" );

				// Workflow
				if ( target.Entity.Is<Workflow>( ) )
				{
					additionalData.Add( ActionMenuItemInfo.AdditionalDataWorkflowKey, target.Entity.Id );
				}

				// Report Template
				if ( target.Entity.Is<ReportTemplate>( ) )
				{
					additionalData.Add( ActionMenuItemInfo.AdditionalDataReportTemplateIdKey, target.Entity.Id );
				}
			}

			foreach ( var data in additionalData.Where( d => d.Key.StartsWith( "%" ) ) )
			{
				displayName = displayName.Replace( data.Key, data.Value.ToString( ) );
			}

			return displayName;
		}

		/// <summary>
		///     Converts an <see cref="ActionMenuItem" /> to a serializable <see cref="ActionMenuItemInfo" /> for transport as part
		///     of a data contract.
		/// </summary>
		/// <param name="item">The action menu item.</param>
		/// <param name="request">The action request.</param>
		/// <param name="targetGetter">A function that gets the info for the action target.</param>
		/// <param name="defaultTargetGetter">A default function that gets the info for the action target.</param>
		/// <returns>The final action menu item info.</returns>
		public static ActionMenuItemInfo ToInfo( this ActionMenuItem item, ActionRequestExtended request, Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> targetGetter, Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> defaultTargetGetter )
		{
			if ( item == null )
				throw new ArgumentNullException( nameof(item) );

			if ( request == null )
				throw new ArgumentNullException( nameof(request) );

			var label = item.Name;

            if ( request.IsMultipleSelection &&
			     item.AppliesToMultiSelection == true &&
			     !string.IsNullOrEmpty( item.MultiSelectName ) )
			{
				// show the "multi selection" label when appropriate
				label = item.MultiSelectName;
			}

            if ( request.ActionDisplayContext != ActionContext.ContextMenu &&
			     request.ActionDisplayContext != ActionContext.All &&
			     !string.IsNullOrEmpty( item.EmptySelectName ) &&
			     ( ( !request.IsMultipleSelection && request.LastSelectedResource == null ) ||
			       ( request.IsMultipleSelection && item.AppliesToMultiSelection != true && item.AppliesToSelection == true ) ) )
			{
				// show the "empty selection" label in appropriate contexts.
				label = item.EmptySelectName;
			}

            var targetInfo = targetGetter == null ? defaultTargetGetter( request, item ) : targetGetter( request, item );
            var additionalData = request.AdditionalData == null ? new Dictionary<string, object>() : 
                new Dictionary<string, object>( request.AdditionalData );
			var displayName = GetDisplayName( label, targetInfo, additionalData );

            var wfActionMenuItem = item.As<WorkflowActionMenuItem>( );
            if ( wfActionMenuItem?.ActionMenuItemToWorkflow != null )
			{
				// align the workflow name
				var wf = wfActionMenuItem.ActionMenuItemToWorkflow;
				displayName = wf.Name;

				// add the related resource name (if necessary)
				if ( wf.InputArgumentForRelatedResource != null )
					additionalData.Add( ActionMenuItemInfo.AdditionalDataRelatedResourceArgKey, wf.InputArgumentForRelatedResource.Name );
			}

			var gd = item.As<GenerateDocumentActionMenuItem>( );
			if ( gd?.ActionMenuItemToReportTemplate != null )
			{
				// align the report template name
				displayName = gd.ActionMenuItemToReportTemplate.Name;
			}

            string expression = null;
            Dictionary<string, ActionEntityReference> expressionEntities = null;

            if ( item.ActionRequiresExpression != null )
		    {
                // disable by expression; should be evaluated on server but here for cache
		        expression = item.ActionRequiresExpression.ActionExpressionString;

		        if ( item.ActionRequiresExpression.ActionExpressionEntities != null )
		        {
                    expressionEntities = new Dictionary<string, ActionEntityReference>();

		            var refs = item.ActionRequiresExpression.ActionExpressionEntities
                        .Select(e => new {e.Name, e.ReferencedEntity})
		                .GroupBy(e => e.Name)
		                .Select(g => g.First()).ToList();

                    refs.ForEach(r => expressionEntities.Add(r.Name, new ActionEntityReference { Id = r.ReferencedEntity.Id, TypeId = r.ReferencedEntity.IsOfType.First().Id }));
		        }
		    }

		    List<Permission> permissions = null;
		    List<Permission> parentPermissions = null;
		    if (item.ActionRequiresPermission.Any())
		    {
		        permissions = item.ActionRequiresPermission.ToList();
		    }
		    if (item.ActionRequiresParentPermission.Any())
		    {
		        parentPermissions = item.ActionRequiresParentPermission.ToList();
		    }
            
            return new ActionMenuItemInfo
			{
				Id = item.Id,
                Alias = item.Alias,
				EntityId = targetInfo?.Entity?.Id ?? 0,
				Order = item.MenuOrder ?? 0,
				Name = displayName,
				Description = item.Description,
				MultiSelectName = item.MultiSelectName,
				EmptySelectName = item.EmptySelectName,
				Icon = item.MenuIconUrl,
				IsSeparator = item.IsMenuSeparator == true,
				IsButton = item.IsActionButton == true,
				IsMenu = item.IsActionItem == true,
				IsContextMenu = item.IsContextMenu == true,
				IsSystem = item.IsSystem == true || item.TenantId == 0, // System Tenant(?)
				AppliesToSelection = item.AppliesToSelection == true,
				AppliesToMultipleSelection = item.AppliesToMultiSelection == true,
				HtmlActionMethod = item.HtmlActionMethod,
				HtmlActionState = item.HtmlActionState,
                HtmlActionTarget = item.HtmlActionTarget,
                RequiresPermissions = permissions,
                RequiresParentPermissions = parentPermissions,
				AdditionalData = additionalData,
                Expression = expression,
                ExpressionEntities = expressionEntities
			};
		}
	}
}