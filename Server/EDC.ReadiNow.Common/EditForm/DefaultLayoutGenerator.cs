// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests;

namespace EDC.ReadiNow.EditForm
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class DefaultLayoutGenerator : IDisposable
    {
        /// <summary>
        /// Query used to prepare a type for form generation.
        /// </summary>
        public static readonly string FormGenerationPreloaderQuery = @"
            let @ID = { isOfType.id }
            let @FIELDGROUP = {
                isOfType.id,
                name
            }
            let @FIELD = {
                description,
                fieldWatermark,
                hideField,
                hideFieldDefaultForm,
                autoNumberDisplayPattern,
                isOfType.k:defaultRenderingControls.@ID,
                k:fieldInstanceRenderingControl.@ID,
                fieldInGroup.@FIELDGROUP
            }
            let @REL = {                
                description,
                hideOnFromType, hideOnToType, 
                hideOnFromTypeDefaultForm, hideOnToTypeDefaultForm,
                relationshipIsMandatory, revRelationshipIsMandatory,
                defaultFromUseCurrent, defaultToUseCurrent,
                {relationshipInFromTypeGroup, relationshipInToTypeGroup}.@FIELDGROUP,
                {fromTypeDefaultValue, toTypeDefaultValue}.@ID,
                relType.@ID
            }
            let @TYPE = {
                isOfType.id,
                canCreateType,
                inherits.@TYPE,
                fields.@FIELD,
                { relationships, reverseRelationships }.@REL,
                fieldGroups.@FIELDGROUP,
                k:formsToEditType.name
            }
            @TYPE";

        #region Cached Entities
        private Dictionary<long, IEntity> fieldTypeToRenderControlCache = new Dictionary<long, IEntity>();


        /// <summary>
        /// Gets the header column container control.
        /// </summary>
        /// <value>
        /// The header column container control.
        /// </value>
        private RenderControlType HeaderColumnContainerControl
        {
            get
            {
                if (headerColumnContainerControl == null)
                {
                    headerColumnContainerControl = Entity.Get<RenderControlType>(new EntityRef("console", "headerColumnContainerControl"));
                }

                return headerColumnContainerControl;
            }
        }
        private RenderControlType headerColumnContainerControl;


        /// <summary>
        /// Gets the horizontal stack container control.
        /// </summary>
        /// <value>
        /// The horizontal stack container control.
        /// </value>
        private RenderControlType HorizontalStackContainerControl
        {
            get
            {
                if (horizontalStackContainerControl == null)
                {
                    horizontalStackContainerControl = Entity.Get<RenderControlType>(new EntityRef("console", "horizontalStackContainerControl"));
                }

                return horizontalStackContainerControl;
            }
        }
        private RenderControlType horizontalStackContainerControl;


        /// <summary>
        /// Gets the vertical stack container control.
        /// </summary>
        /// <value>
        /// The vertical stack container control.
        /// </value>
        private RenderControlType VerticalStackContainerControl
        {
            get
            {
                if (verticalStackContainerControl == null)
                {
                    verticalStackContainerControl = Entity.Get<RenderControlType>(new EntityRef("console", "verticalStackContainerControl"));
                }

                return verticalStackContainerControl;
            }
        }
        private RenderControlType verticalStackContainerControl;


        /// <summary>
        /// Gets the tab container control.
        /// </summary>
        /// <value>
        /// The tab container control.
        /// </value>
        private RenderControlType TabContainerControl
        {
            get
            {
                if (tabContainerControl == null)
                {
                    tabContainerControl = Entity.Get<RenderControlType>(new EntityRef("console", "tabContainerControl"));
                }

                return tabContainerControl;
            }
        }
        private RenderControlType tabContainerControl;


        /// <summary>
        /// Gets the control on form.
        /// </summary>
        /// <value>
        /// The control on form.
        /// </value>
        private EntityType ControlOnForm
        {
            get
            {
                if (controlOnForm == null)
                {
                    controlOnForm = Entity.Get<EntityType>(new EntityRef("console", "controlOnForm"));
                }

                return controlOnForm;
            }
        }
        private EntityType controlOnForm;


        /// <summary>
        /// Gets the field control on form.
        /// </summary>
        /// <value>
        /// The field control on form.
        /// </value>
        private EntityType FieldControlOnForm
        {
            get
            {
                if (fieldControlOnForm == null)
                {
                    fieldControlOnForm = Entity.Get<EntityType>(new EntityRef("console", "fieldControlOnForm"));
                }

                return fieldControlOnForm;
            }
        }
        private EntityType fieldControlOnForm;


        /// <summary>
        /// Gets the default picker report.
        /// </summary>
        /// <value>
        /// The default picker report.
        /// </value>
        private ResourcePicker DefaultPickerReport
        {
            get
            {
                if (defaultPickerReport == null)
                {
                    defaultPickerReport = Entity.Get<ResourcePicker>(new EntityRef("core", "templateReport"));
                }

                return defaultPickerReport;
            }
        }
        private ResourcePicker defaultPickerReport;


        /// <summary>
        /// Gets the relationship control on form.
        /// </summary>
        /// <value>
        /// The relationship control on form.
        /// </value>
        private EntityType RelationshipControlOnForm
        {
            get
            {
                if (relationshipControlOnForm == null)
                {
                    relationshipControlOnForm = Entity.Get<EntityType>(new EntityRef("console", "relationshipControlOnForm"));
                }

                return relationshipControlOnForm;
            }
        }
        private EntityType relationshipControlOnForm;


        /// <summary>
        /// Gets the choice relationship render control.
        /// </summary>
        /// <value>
        /// The choice relationship render control.
        /// </value>
        private RenderControlType ChoiceRelationshipRenderControl
        {
            get
            {
                if (choiceRelationshipRenderControl == null)
                {
                    choiceRelationshipRenderControl = Entity.Get<RenderControlType>(new EntityRef("console", "choiceRelationshipRenderControl"));
                }

                return choiceRelationshipRenderControl;
            }
        }
        private RenderControlType choiceRelationshipRenderControl;


        /// <summary>
        /// Gets the calculated render control.
        /// </summary>
        /// <value>
        /// The calculated render control.
        /// </value>
        private RenderControlType CalculatedRenderControl
        {
            get
            {
                if (calculatedRenderControl == null)
                {
                    calculatedRenderControl = Entity.Get<RenderControlType>(new EntityRef("console", "calculatedRenderControl"));
                }

                return calculatedRenderControl;
            }
        }
        private RenderControlType calculatedRenderControl;


        /// <summary>
        /// Gets the structure view relationship control.
        /// </summary>
        /// <value>
        /// The structure view relationship control.
        /// </value>
        private RenderControlType StructureViewRelationshipControl
        {
            get
            {
                if (structureViewRelationshipControl == null)
                {
                    structureViewRelationshipControl = Entity.Get<RenderControlType>(new EntityRef("console", "structureViewRelationshipControl"));
                }

                return structureViewRelationshipControl;
            }
        }
        private RenderControlType structureViewRelationshipControl;


        /// <summary>
        /// Gets the inline relationship render control.
        /// </summary>
        /// <value>
        /// The inline relationship render control.
        /// </value>
        private RenderControlType InlineRelationshipRenderControl
        {
            get
            {
                if (inlineRelationshipRenderControl == null)
                {
                    inlineRelationshipRenderControl = Entity.Get<RenderControlType>(new EntityRef("console", "inlineRelationshipRenderControl"));
                }

                return inlineRelationshipRenderControl;
            }
        }
        private RenderControlType inlineRelationshipRenderControl;


        /// <summary>
        /// Gets the tab relationship render control.
        /// </summary>
        /// <value>
        /// The tab relationship render control.
        /// </value>
        private RenderControlType TabRelationshipRenderControl
        {
            get
            {
                if (tabRelationshipRenderControl == null)
                {
                    tabRelationshipRenderControl = Entity.Get<RenderControlType>(new EntityRef("console", "tabRelationshipRenderControl"));
                }

                return tabRelationshipRenderControl;
            }
        }
        private RenderControlType tabRelationshipRenderControl;


        /// <summary>
        /// Gets the default relationship behavior.
        /// </summary>
        /// <value>
        /// The default relationship behavior.
        /// </value>
        private MultiRelationshipControlBehavior DefaultRelationshipBehavior
        {
            get
            {
                if (defaultRelationshipBehavior == null)
                {
                    defaultRelationshipBehavior = Entity.Get<MultiRelationshipControlBehavior>(new EntityRef("console", "defaultRelationshipBehavior"));
                }

                return defaultRelationshipBehavior;
            }
        }
        private MultiRelationshipControlBehavior defaultRelationshipBehavior;


        /// <summary>
        /// Gets the structure view.
        /// </summary>
        /// <value>
        /// The structure view.
        /// </value>
        private EntityType StructureView
        {
            get
            {
                if (structureView == null)
                {
                    structureView = Entity.Get<EntityType>(new EntityRef("core", "structureView"));
                }

                return structureView;
            }
        }
        private EntityType structureView;


        /// <summary>
        /// Gets the name field.
        /// </summary>
        /// <value>
        /// The name field.
        /// </value>
        private StringField NameField
        {
            get
            {
                if (nameField == null)
                {
                    nameField = Entity.Get<StringField>(new EntityRef("core", "name"));
                }

                return nameField;
            }
        }
        private StringField nameField;


        /// <summary>
        /// Gets the description field.
        /// </summary>
        /// <value>
        /// The description field.
        /// </value>
        private StringField DescriptionField
        {
            get
            {
                if (descriptionField == null)
                {
                    descriptionField = Entity.Get<StringField>(new EntityRef("core", "description"));
                }

                return descriptionField;
            }
        }
        private StringField descriptionField;


        /// <summary>
        /// Gets the small thumbnail.
        /// </summary>
        /// <value>
        /// The small thumbnail.
        /// </value>
        private ThumbnailSizeEnum SmallThumbnail
        {
            get { return _smallThumbnail ?? (_smallThumbnail = Entity.Get<ThumbnailSizeEnum>(new EntityRef("console", "smallThumbnail"))); }
        }
        private ThumbnailSizeEnum _smallThumbnail;


        /// <summary>
        /// Gets the scale image proportionally enum.
        /// </summary>
        /// <value>
        /// The scale image proportionally.
        /// </value>
        private ImageScaleEnum ScaleImageProportionally
        {
            get { return _scaleImageProportionally ?? (_scaleImageProportionally = Entity.Get<ImageScaleEnum>(new EntityRef("core", "scaleImageProportionally"))); }
        }
        private ImageScaleEnum _scaleImageProportionally;

        #endregion

		EntityRef htmlUiContext = new EntityRef( "console", "uiContextHtml" );
        EntityRef enumValue = new EntityRef("core", "enumValue");
        private EntityRef _imageFileTypeType = new EntityRef("core", "imageFileType");
        EntityRef calculateResultValue = new EntityRef("core", "calculatedResult");
        EntityRef structureViewValue = new EntityRef("core", "structureLevel");


        private bool _disposed = false;
        private bool _formReqdForDesignMode = false;

        List<IEntity> tempEntitiesInCache = new List<IEntity>();  // Temporary entities that must be explicitly disposed to remove them from the cache.


        /// <summary>
        /// The context we are generating the form for
        /// </summary>
        public CurrentUiContext curentUiContext { get; private set; }

        /// <summary>
        /// Get the entity that represents the current UI context
        /// </summary>
        public EntityRef CurrentUiContextEntity
        {
            get
            {
                switch (curentUiContext)
                {
                    case CurrentUiContext.Html: return htmlUiContext;
                    default: throw new Exception("Enexpected UI context");
                }
            }
        }


        /// <summary>
        /// The type we are generating the form for
        /// </summary>
        public EntityRef TypeRef { get; private set; }


        /// <summary>
        /// Gets the type ref entity.
        /// </summary>
        /// <value>
        /// The type ref entity.
        /// </value>
        public EntityType TypeRefEntity { get; private set; }


        /// <summary>
        /// The generated layout
        /// </summary>
        public CustomEditForm Layout { get; private set; }

        /// <summary>
        /// The generated layout
        /// </summary>
        public EntityData GetLayoutAsEntityData( string requestQuery )
        {
            // turn the custom form into EntityData
            var requestEntities = new List<IEntity>( );
            requestEntities.Add( Layout );

#pragma warning disable 618
            var result = EntityInfoService.ToEntityData( requestEntities, requestQuery ).First( );
#pragma warning restore 618

            // ensure that the newly creates EntityData objects have the correct state 
            UpdateDataStateOfTempEntities( result, tempEntitiesInCache.Select( e => e.Id ).ToList( ) );

            return result;
        } 

        public DefaultLayoutGenerator(CurrentUiContext uiContext, EntityRef _typeRef, bool formReqdForDesignMode)
        {
            _formReqdForDesignMode = formReqdForDesignMode;
            curentUiContext = uiContext;
            TypeRef = _typeRef;
            TypeRefEntity = Entity.Get<EntityType>(TypeRef);

            if (curentUiContext == CurrentUiContext.Html)
                Layout = GetGeneratedLayoutHtml(TypeRefEntity);
            else
                throw new InvalidOperationException("Unknown UI context");

            Layout.TypeToEditWithForm = TypeRefEntity;            
        }

        /// <summary>
        /// Gets the proposed name of the form.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public string GetProposedFormName(EntityType entityType)
        {
            return EditFormHelper.GetProposedFormName(entityType);                        
        }

		/// <summary>
		/// Pre-load report entities.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
        private static void PreloadFormGeneration(long typeId)
        {
            var rq = new EntityRequest(typeId, FormGenerationPreloaderQuery, "Preload generated form for type " + typeId.ToString());
            BulkPreloader.Preload(rq);
        }

        /// <summary>
        /// Given a resource definition, return the generated Layout
        /// </summary>
        CustomEditForm GetGeneratedLayoutHtml(EntityType entityType)
        {
            PreloadFormGeneration(entityType.Id);

            CustomEditForm form = new CustomEditForm();
            EntityRelationshipCollection<IEntity> formContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();

            tempEntitiesInCache.Add(form); // track the temp object

			form.UiContextForForm = Entity.Get<UiContext>( new EntityRef( "console", "uiContextHtml" ), new EntityRef( "core", "name" ) );

            // Setup the name of the form
            form.Name = GetProposedFormName(entityType);

            //
            // set up the layout
            //

            var header = new HeaderColumnContainerControl();
            EntityRelationshipCollection<IEntity> headerContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();
            tempEntitiesInCache.Add(header); // track the temp object


            header.RenderingOrdinal = 0;
            formContainedControlsOnForm.Add(header);

            var twoColumns = new HorizontalStackContainerControl();
            EntityRelationshipCollection<IEntity> twoColumnsContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();
            twoColumns.RenderingOrdinal = 10;
            tempEntitiesInCache.Add(twoColumns); // track the temp object

            formContainedControlsOnForm.Add(twoColumns);

            var leftColumn = new VerticalStackContainerControl();
            EntityRelationshipCollection<IEntity> leftColumnContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();
            leftColumn.RenderingOrdinal = 0;
            tempEntitiesInCache.Add(leftColumn); // track the temp object


            var rightColumn = new VerticalStackContainerControl();
            EntityRelationshipCollection<IEntity> rightColumnContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();
            tempEntitiesInCache.Add(rightColumn); // track the temp object

            rightColumn.RenderingOrdinal = 10;

            //TODO: Tabs as tabs
            var tabs = new TabContainerControl();
            EntityRelationshipCollection<IEntity> tabsContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();
            tabs.RenderingOrdinal = 20;
            tempEntitiesInCache.Add(tabs); // track the temp object


            //
            // Get the type and start going through all the fields in alhabetical order
            // For each new group encoundered create a FormFieldContainer and populating it with the fields
            // fields not in a group are put in their own container
            //            

            // This list holds field entities and singular relationships to controls
            var fieldControlsList = new List<Tuple<IEntity, INamedControl>>();

            string formTypeAlias = entityType.Alias;
            bool isImageFileType = entityType.GetAncestorsAndSelf().Any(t => t.Id == _imageFileTypeType.Id);

            var typeFields = from f in entityType.GetAllFields()
                             where !IsHiddenField(f)
                             select new Tuple<IEntity, INamedControl>(f, CreateField(f, formTypeAlias, isImageFileType));

            var fieldGroupContainers = new Dictionary<long, StructureControlOnForm>();
            Dictionary<long, EntityRelationshipCollection<IEntity>> fieldGroupControlsOnForm = new Dictionary<long, EntityRelationshipCollection<IEntity>>();
            var leftOverFields = new VerticalStackContainerControl().Cast<StructureControlOnForm>();
            EntityRelationshipCollection<IEntity> leftOverFieldsContainedControlsOnForm = new EntityRelationshipCollection<IEntity>();
            tempEntitiesInCache.Add(leftOverFields); // track the temp object
            Dictionary<long, string> idToGroupName = new Dictionary<long, string>();

            // Add the type fields to the field controls list
            fieldControlsList.AddRange(typeFields);

            // Get the relationships
            IEnumerable<EntityType> ancestorTypes = entityType.GetAncestorsAndSelf();
            RelationshipControls relationshipControls = GetAllRelationshipsControls(ancestorTypes);

            //
            // Relationships
            //
            var forwardRelControls = relationshipControls.ForwardRelationshipControls;
            var reverseRelControls = relationshipControls.ReverseRelationshipControls;

            // Find all the relationships that are in field groups and add them to the field control list
            foreach (NamedRelationshipControl namedControl in forwardRelControls.Concat(reverseRelControls).OrderBy(r => r.Name))
            {
                FieldGroup fieldGroup = null;                
                var relationshipControl = namedControl.Control as RelationshipControlOnForm;
                if (relationshipControl != null)
                {
                    bool isForward = forwardRelControls.Contains(namedControl);
                    fieldGroup = isForward ? relationshipControl.RelationshipToRender.RelationshipInFromTypeGroup : relationshipControl.RelationshipToRender.RelationshipInToTypeGroup;
                }

                if (fieldGroup != null)
                {
                    // The relationship is in a group. So we need to add it to its group                    
                    fieldControlsList.Add(new Tuple<IEntity, INamedControl>(fieldGroup, namedControl));
                }
                else
                {
                    // Add the relationship to the default list
                    leftOverFieldsContainedControlsOnForm.Add(namedControl.Control);
                }
            }            

            int renderingOrdinal = 0;
            foreach (var selectedField in fieldControlsList.OrderBy(f => f.Item2 != null ? f.Item2.Name : null))
            {
                if (selectedField.Item1 != null &&
                    selectedField.Item2 != null)
                {
                    FieldGroup fieldGroup = null;
                    var field = selectedField.Item1 as Field;

                    if (field != null)
                    {                    
                        // Get the field group from the field
                        fieldGroup = field.FieldInGroup;
                    }
                    else if (selectedField.Item1 is FieldGroup)
                    {
                        fieldGroup = selectedField.Item1 as FieldGroup;
                    }

                    if (IsHeaderField(field) || fieldGroup == null)
                    {
                        headerContainedControlsOnForm.Add(selectedField.Item2.Control);
                    }
                    else
                    {
                        long groupId = fieldGroup.Id;
                        //TODO: Add the name to groups
                        if (!fieldGroupContainers.ContainsKey(groupId))
                        {
                            var tempContainer = new VerticalStackContainerControl();
                            tempEntitiesInCache.Add(tempContainer); // track the temp object

                            string groupName = string.Empty;
                            if (!idToGroupName.TryGetValue(fieldGroup.Id, out groupName))
                            {
                                groupName = fieldGroup.Name;
                                idToGroupName[fieldGroup.Id] = groupName;
                            }

                            tempContainer.Name = groupName;
                            tempContainer.RenderingOrdinal = renderingOrdinal;
                            renderingOrdinal += 10;

                            fieldGroupContainers.Add(groupId, tempContainer.Cast<StructureControlOnForm>());
                            fieldGroupControlsOnForm[groupId] = new EntityRelationshipCollection<IEntity>();
                        }

                        fieldGroupControlsOnForm[groupId].Add(selectedField.Item2.Control);
                    }
                }
            }            

            // Tabs Relationships

            var tabForwardRelControls = relationshipControls.ForwardTabRelationshipControls;

            var tabForwardRelControlsWithoutStructureLevels = from t in tabForwardRelControls
                                                              where t.Name != "Structure Levels"
                                                              select t;

            var tabReverseRelControls = relationshipControls.ReverseTabRelationshipControls;

            renderingOrdinal = 0;
            foreach (var relationshipTab in tabForwardRelControlsWithoutStructureLevels.Concat(tabReverseRelControls).OrderBy(r => r.Name))
            {
                relationshipTab.Control.SetField(new EntityRef("console", "renderingOrdinal"), renderingOrdinal);
                renderingOrdinal += 10;
                tabsContainedControlsOnForm.Add(relationshipTab.Control);
            }

            //
            // Take the non empty field containers and add them to the left and right columns
            //
            IEnumerable<StructureControlOnForm> fieldContainers = fieldGroupContainers.Values;

            if (leftOverFieldsContainedControlsOnForm.Count > 0)
                fieldContainers = fieldContainers.Concat(leftOverFields.ToEnumerable());

            double totalLength = 0;
            Dictionary<long, int> countFieldControlPerContainer = new Dictionary<long, int>();
            foreach (var table in fieldContainers)
            {
                int fieldsInTable = StructureControlOnFormHelper.GetAllFieldControlOnForms(table).Count();
                countFieldControlPerContainer[table.Id] = fieldsInTable;
                totalLength += fieldsInTable;
            }

            totalLength += fieldGroupContainers.Count * 0.5;

            double runningSum = 0;
            renderingOrdinal = 0;

            foreach (var table in fieldContainers)
            {
                int fieldsInTable = 0;
                countFieldControlPerContainer.TryGetValue(table.Id, out fieldsInTable);

                table.RenderingOrdinal = renderingOrdinal;
                renderingOrdinal += 10;

                if (2.0 * runningSum < totalLength)  // keep adding until we go pas the halfway point. This means the left column is always more filled.
                    leftColumnContainedControlsOnForm.Add(table);
                else
                    rightColumnContainedControlsOnForm.Add(table);

                runningSum += fieldsInTable + 0.5;
            }

            //
            // Add only the containers with controls
            //
            if (leftColumnContainedControlsOnForm.Count > 0)
                twoColumnsContainedControlsOnForm.Add(leftColumn);

            if (rightColumnContainedControlsOnForm.Count > 0)
                twoColumnsContainedControlsOnForm.Add(rightColumn);

            if (tabsContainedControlsOnForm.Count > 0)
                formContainedControlsOnForm.Add(tabs);

            //
            // Put them all together
            //
	        EntityRef containedControlsOnForm = new EntityRef( "console:containedControlsOnForm" );

			form.SetRelationships( containedControlsOnForm, formContainedControlsOnForm, Direction.Forward );
			header.SetRelationships( containedControlsOnForm, headerContainedControlsOnForm, Direction.Forward );
			twoColumns.SetRelationships( containedControlsOnForm, twoColumnsContainedControlsOnForm, Direction.Forward );
			leftColumn.SetRelationships( containedControlsOnForm, leftColumnContainedControlsOnForm, Direction.Forward );
			rightColumn.SetRelationships( containedControlsOnForm, rightColumnContainedControlsOnForm, Direction.Forward );
			tabs.SetRelationships( containedControlsOnForm, tabsContainedControlsOnForm, Direction.Forward );
			leftOverFields.SetRelationships( containedControlsOnForm, leftOverFieldsContainedControlsOnForm, Direction.Forward );

            foreach (KeyValuePair<long, StructureControlOnForm> kvp in fieldGroupContainers)
            {
				kvp.Value.SetRelationships( containedControlsOnForm, fieldGroupControlsOnForm [ kvp.Key ], Direction.Forward );
            }

            return form;
        }

        /// <summary>
        /// Is the provided field meant to go into the header?
        /// </summary>
        private bool IsHeaderField(Field field)
        {
            return (field != null && (field.Id == NameField.Id || field.Id == DescriptionField.Id)); // hack
        }

        /// <summary>
        /// Is the provided field to be hidden?
        /// </summary>
        private bool IsHiddenField(Field field)
        {
            bool hidden = (field.HideField ?? false) || (field.HideFieldDefaultForm ?? false);
            return hidden;
        }


        private NamedFieldControl CreateField(Field field, string formTypeAlias, bool isImageFileType)
        {
            NamedFieldControl namedFieldControl = new NamedFieldControl();

            FieldControlOnForm fieldControl = new FieldControlOnForm();

            tempEntitiesInCache.Add(fieldControl); // track the temp object

            string name = field.Name ?? field.Alias ?? string.Empty;
            fieldControl.FieldToRender = field;
            fieldControl.Name = name;
            fieldControl.Description = field.Description ?? string.Empty;

            IEntity controlToRender = GetRenderControlForFieldInstance(field);

            if (controlToRender == null)
            {
                // Hack to deal with Document library
                if (formTypeAlias == "core:document" && field.Id == NameField.Id)
                {
                    long fileNameUploadControl = Entity.GetId("console", "fileNameUploadControl");
                    controlToRender = Entity.Get<IEntity>(fileNameUploadControl);
                }
                else if (isImageFileType && field.Id == NameField.Id)
                {
                    long imageFileNameUploadControl = Entity.GetId("console", "imageFileNameUploadControl");
                    controlToRender = Entity.Get<IEntity>(imageFileNameUploadControl);
                }
                else
                {
                    controlToRender = GetRenderControlForType(field.GetFieldType());
                }
            }

            if (controlToRender != null)
            {
                // change the controls type to be the correct for the field type
                fieldControl.IsOfType.Clear();
                fieldControl.IsOfType.Add(controlToRender.As<EntityType>());

                if (field.Id == NameField.Id)
                    fieldControl.RenderingOrdinal = 50;
                else if (field.Id == DescriptionField.Id)
                    fieldControl.RenderingOrdinal = 55;
                else
                    fieldControl.RenderingOrdinal = 100;

                namedFieldControl.Name = name;
                namedFieldControl.Control = fieldControl;

                return namedFieldControl;
            }
            else
                return null;
        }


        /// <summary>
        /// Gets the render control for field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private IEntity GetRenderControlForFieldInstance(Field field)
        {
            IEntity controlToRender = null;

            if (field != null)
            {
                IEntityCollection<FieldRenderControlType> fieldInstanceRenderingControl = field.FieldInstanceRenderingControl;
                if (fieldInstanceRenderingControl != null)
                {
                    FieldRenderControlType fieldRenderControlType = fieldInstanceRenderingControl.FirstOrDefault(rc => rc.Context.Id == CurrentUiContextEntity.Id);
                    if (fieldRenderControlType != null)
                    {
                        controlToRender = fieldRenderControlType;
                    }
                }
            }

            return controlToRender;
        }


        private NamedRelationshipControl CreateRelationship(Relationship rel, EntityType toType, EntityType fromType, bool isReversed, bool isTabRelationship, bool isCalculatedRelationship)
        {
            NamedRelationshipControl namedRelationshipControl = new NamedRelationshipControl();

            bool isChoiceRelationship = IsChoiceRelationship(toType, isReversed);

            //relationship is mandatory
            bool isMandatory = isReversed ? rel.RevRelationshipIsMandatory == true : rel.RelationshipIsMandatory == true;
            RelationshipControlOnForm relControl = null;

            if (isChoiceRelationship)
            {
                if (rel.Cardinality_Enum == CardinalityEnum_Enumeration.ManyToMany)
                {
                    relControl = new MultiChoiceRelationshipRenderControl().Cast<RelationshipControlOnForm>();                    
                }
                else
                {
                    relControl = new ChoiceRelationshipRenderControl().Cast<RelationshipControlOnForm>();
                    Resource toTypeDefaultValue = rel.ToTypeDefaultValue;
                    if (toTypeDefaultValue != null)
                    {
                        relControl.RelationshipDefaultValue = toTypeDefaultValue;
                    }
                }
            }
            else if (isCalculatedRelationship)
            {
                //TODO calculated
                relControl = new CalculatedRenderControl().Cast<RelationshipControlOnForm>();
            }
            else
            {
                //structure view relationship                
                if (IsStrcutureViewRelationship(toType, isReversed))
                {
                    relControl = new StructureViewRelationshipControl().Cast<RelationshipControlOnForm>();
                }

                if (relControl == null)
                {
                    if (IsImageRelationship(rel, isReversed, isTabRelationship))
                    {
                        var imageRelationshipControl = new ImageRelationshipRenderControl();
                        imageRelationshipControl.ThumbnailScalingSetting = ScaleImageProportionally;
                        imageRelationshipControl.ThumbnailSizeSetting = SmallThumbnail;

                        relControl = imageRelationshipControl.Cast<RelationshipControlOnForm>();                        
                    }
                }

                if (relControl == null)
                {
                    if (!isTabRelationship)
                    {
                        relControl = new InlineRelationshipRenderControl().Cast<RelationshipControlOnForm>();
                        Resource toTypeDefaultValue = rel.ToTypeDefaultValue;
                        if (toTypeDefaultValue != null)
                        {
                            relControl.RelationshipDefaultValue = toTypeDefaultValue;
                        }
                    }
                    else
                    {
                        relControl = new TabRelationshipRenderControl().Cast<RelationshipControlOnForm>();
                        relControl.HasRelationshipControlBehavior = DefaultRelationshipBehavior;

                        // set default resize modes
                        relControl.SetRenderingVerticalResizeMode("resizeSpring");
                        relControl.SetRenderingHorizontalResizeMode("resizeSpring");
                    }
                }
            }            

            string name = NameOrDefault(isReversed ? rel.FromName : rel.ToName, rel.Name);
            relControl.Name = name;
            relControl.Description = rel.Description;
            relControl.MandatoryControl = isMandatory;
            relControl.RelationshipToRender = rel;
            relControl.RenderingOrdinal = 100;
            relControl.IsReversed = isReversed;

            // set defaultDisplayReport and defaultPickerReport of relationship control
            if (!_formReqdForDesignMode)
            {
                Report displayReport = null;
                ResourcePicker pickerReport = null;
                if (!isReversed)
                {
                    displayReport = EditFormHelper.GetDefaultDisplayReportForType(Enumerable.Repeat(toType, 1));
                    relControl.RelationshipDisplayReport = displayReport;

                    pickerReport = EditFormHelper.GetDefaultPickerReportForType(Enumerable.Repeat(toType, 1));
                    relControl.PickerReport = pickerReport ?? DefaultPickerReport;
                }
                else
                {
                    displayReport = EditFormHelper.GetDefaultDisplayReportForType(Enumerable.Repeat(fromType, 1));
                    relControl.RelationshipDisplayReport = displayReport;

                    pickerReport = EditFormHelper.GetDefaultPickerReportForType(Enumerable.Repeat(fromType, 1));
                    relControl.PickerReport = pickerReport ?? DefaultPickerReport;
                }
            }

            // Can the relationship have fields
            //relControl.CanHaveFields = rel.Fields.Any();

            // hide 'New'/'Add' buttons on this rel control
            relControl.HideAddButton = false;
            relControl.HideNewButton = false;
            relControl.HideRemoveButton = false;

            tempEntitiesInCache.Add(relControl); // track the temp object

            namedRelationshipControl.Control = relControl;
            namedRelationshipControl.Name = name;

            return namedRelationshipControl;
        }


        private List<NamedRelationshipControl> CreateStructureLevelInstanceControls(Relationship rel, bool isReversed)
        {
            List<NamedRelationshipControl> namedRelationshipControls = new List<NamedRelationshipControl>();
            EntityType structureView = StructureView;

            foreach (Resource r in structureView.InstancesOfType)
            {
                EntityRef resultId = new EntityRef(r.Id);
                StructureView result = Entity.Get<StructureView>(resultId);
                if (result != null)
                {
                    NamedRelationshipControl namedControl = new NamedRelationshipControl();
                    RelationshipControlOnForm relControl = new RelationshipControlOnForm();

                    relControl = new StructureViewRelationshipControl().Cast<RelationshipControlOnForm>(); ;

                    string name = result.Name;

                    relControl.Name = name;
                    relControl.Description = rel.Description;
                    relControl.PickerStructureView = result;
                    relControl.RelationshipToRender = rel;

                    relControl.RenderingOrdinal = 100;
                    relControl.IsReversed = isReversed;

                    relControl.CanHaveFields = false;
                    tempEntitiesInCache.Add(relControl); // track the temp object

                    namedControl.Name = name;
                    namedControl.Control = relControl;

                    namedRelationshipControls.Add(namedControl);

                }
            }


            return namedRelationshipControls;
        }

        private List<NamedRelationshipControl> CreateCalculatedControls(Relationship rel, EntityType toType, EntityType fromType, bool isReversed)
        {
            List<NamedRelationshipControl> namedRelationshipControls = new List<NamedRelationshipControl>();

            foreach (Resource r in toType.InstancesOfType)
            {
                EntityRef resultId = new EntityRef(r.Id);
                CalculatedResult result = Entity.Get<CalculatedResult>(resultId);
                if (result != null)
                {
                    EntityType calculatedType = result.CalculatedResultType;
                    Report calculatedReport = result.CalculatedResultReport;
                    if (calculatedReport != null && calculatedType != null && calculatedType == fromType)
                    {
                        NamedRelationshipControl namedControl = new NamedRelationshipControl();
                        RelationshipControlOnForm relControl = new RelationshipControlOnForm();

                        relControl = new CalculatedRenderControl().Cast<RelationshipControlOnForm>();

                        string name = result.Name;

                        relControl.Name = name;
                        relControl.Description = rel.Description;

                        relControl.RelationshipToRender = rel;
                        relControl.RenderingOrdinal = 100;
                        relControl.IsReversed = isReversed;

                        relControl.PickerReport = (ResourcePicker) calculatedReport;

                        relControl.CanHaveFields = false;
                        tempEntitiesInCache.Add(relControl); // track the temp object

                        namedControl.Name = name;
                        namedControl.Control = relControl;

                        namedRelationshipControls.Add(namedControl);
                    }
                }
            }


            return namedRelationshipControls;
        }
        /// <summary>
        /// Convenient helper for selecting a 'name' string.
        /// </summary>
        private string NameOrDefault(string preferredName, string defaultName)
        {
            if (string.IsNullOrEmpty(preferredName))
                return defaultName;
            return preferredName;
        }

		/// <summary>
		/// Is the given relationship one that should be represented by a choice field
		/// </summary>
		/// <param name="toType">To type.</param>
		/// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
		/// <returns></returns>
        bool IsChoiceRelationship(EntityType toType, bool isReversed)
        {
            // If it is forward and the to type is a EnumValue, it's a choice
            return !isReversed && (toType.Inherits.FirstOrDefault(t => t.Id == enumValue.Id) != null);
        }

		/// <summary>
		/// Is the given relationship one that should be represented by a calculated field
		/// </summary>
		/// <param name="toType">To type.</param>
		/// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
		/// <returns></returns>
        private bool IsCalculatedRelationship(EntityType toType, bool isReversed)
        {

            // If it is forward and the to type is a EnumValue, it's a choice
            return !isReversed && (toType.Id == calculateResultValue.Id);
        }

        /// <summary>
        /// Is the given relationship one that should be represented by an image field
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="isReversed">if set to <c>true</c> if reversed.</param>
        /// <param name="isMultiRelationship">if set to <c>true</c> is a multi relationship.</param>
        /// <returns>
        ///   <c>true</c> if the relationship is an image relationship; otherwise, <c>false</c>.
        /// </returns>
        private bool IsImageRelationship(Relationship relationship, bool isReversed, bool isMultiRelationship)
        {
            if (isMultiRelationship)
            {
                return false;
            }

            bool isImageRelationship = !isReversed ? 
                (relationship.ToType.GetAncestorsAndSelf().Any(t => t.Id == _imageFileTypeType.Id)) : 
                (relationship.FromType.GetAncestorsAndSelf().Any(t => t.Id == _imageFileTypeType.Id));

            return isImageRelationship;
        }

		/// <summary>
		/// Is the given relationship one that should be represented by a StrcutureView
		/// </summary>
		/// <param name="toType">To type.</param>
		/// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
		/// <returns></returns>
        private bool IsStrcutureViewRelationship(EntityType toType, bool isReversed)
        {
            return !isReversed && (toType.Id == structureViewValue.Id);
        }
		/// <summary>
		/// Is the given relationship one that should be shown in a tab
		/// </summary>
		/// <param name="rel">The relative.</param>
		/// <param name="toType">To type.</param>
		/// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">rel</exception>
        bool IsTabRelationship(Relationship rel, EntityType toType, bool isReversed)
		{
		    WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

            if (rel == null)
                throw new ArgumentNullException("rel");

            if (IsChoiceRelationship(toType, isReversed) || 
                IsCalculatedRelationship(toType, isReversed))
                return false;

            long cardinalityId = rel.Cardinality.Id;

            if ((!isReversed && ((cardinalityId == aliases.OneToOne ) || (cardinalityId == aliases.ManyToOne )))
                   || (isReversed && ((cardinalityId == aliases.OneToOne ) || (cardinalityId == aliases.OneToMany ))))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Is the given relationship one that should be shown at all?
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="isReversed"></param>
        /// <returns></returns>
        bool IsHiddenRelationship(Relationship rel, bool isReversed)
        {
            if (isReversed)
                return (rel.HideOnToType == true || rel.HideOnToTypeDefaultForm == true);
            else
                return (rel.HideOnFromType == true || rel.HideOnFromTypeDefaultForm == true);
        }

        /// <summary>
        /// Get the render control that is most appropriate for rendering the given type including looking at
        /// case types. (Note multiple inheritance is ignored.)
        /// Return null if non is found.
        /// </summary>
        /// <returns></returns>
        IEntity GetRenderControlForType(FieldType fieldType)
        {
            IEntity controlForType;
            if (fieldTypeToRenderControlCache.TryGetValue(fieldType.Id, out controlForType))
            {
                return controlForType;
            }

            FieldRenderControlType fieldControl;

            // Determine if a default control has been specified for this fieldType by checking the defaultRenderingControls relationship
            var defaultControlForContext =
                (
                    from dc in fieldType.DefaultRenderingControls
                    where dc.Context.Id == CurrentUiContextEntity.Id
                    select dc
                ).FirstOrDefault();

            if (defaultControlForContext != null)
            {
                // yes, use the default
                fieldControl = defaultControlForContext;
            }
            else
            {
                // No default specified, just select the first applicable control we find
                var renderControlForContext = (from dc in fieldType.RenderingControl where dc.Context.Id == CurrentUiContextEntity.Id select dc).FirstOrDefault();

                if (renderControlForContext != null)
                {
                    // found a suitable control, return that
                    fieldControl = renderControlForContext;
                }
                else
                {
                    //Warning: this will fail if a fieldType inherits from multipe types
                    FieldType parent = fieldType.Inherits[0].As<FieldType>();
                    if (parent != null)
                        return GetRenderControlForType(parent);
                    else
                        fieldControl = null;
                }
            }

            controlForType = fieldControl;

            fieldTypeToRenderControlCache[fieldType.Id] = controlForType;

            return controlForType;
        }

        /// <summary>
        /// Go through the entityData tree and mark any which are temp entites as "Created".
        /// Do the same for relationships
        /// </summary>        
        void UpdateDataStateOfTempEntities(EntityData entityData, List<long> tempEntityIds)
        {
            UpdateDataStateOfTempEntities_internal(entityData, tempEntityIds, new List<long>());

        }


        void UpdateDataStateOfTempEntities_internal(EntityData entityData, List<long> tempEntityIds, List<long> passedObjects)
        {
            // prevent loops
            if (passedObjects.Contains(entityData.Id.Id))
                return;

            passedObjects.Add(entityData.Id.Id);

            // check if this is a temporary entity and update the state and relations
            if (tempEntityIds.Contains(entityData.Id.Id))
            {
                // mark this entity and all relationship instances DataState as Create
                entityData.DataState = DataState.Create;

                var isOfTypeRel = new EntityRef("core", "isOfType");

                foreach (var relationship in entityData.Relationships)
                    if (relationship.RelationshipTypeId.Id != isOfTypeRel.Id)   // isOfType is special because is is dealt with using TypeIds
                        foreach (var ri in relationship.Instances)
                            if (ri.DataState != DataState.Create)
                                ri.DataState = DataState.Create;
            }

            // Walk the rest of the tree
            foreach (var relationship in entityData.Relationships)
                foreach (var relationshipInstance in relationship.Instances)
                    UpdateDataStateOfTempEntities_internal(relationshipInstance.Entity, tempEntityIds, passedObjects);
        }


        #region IDisposable Methods
        /// <summary>
        /// Dispose the Generator and temporary entities.
        /// </summary>
        /// <param name="disposing">True if Dispose is called from user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                    foreach (var entity in tempEntitiesInCache)
                    {
                        entity.Dispose();
                    }

                }
                _disposed = true;
            }
        }


        /// <summary>
        /// Dispose the mutex object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        /// <summary>
        /// Gets all the relationship controls for the specified entity type
        /// </summary>
        private RelationshipControls GetAllRelationshipsControls(IEnumerable<EntityType> types)
        {
            RelationshipControls controls = new RelationshipControls();

            foreach (EntityType curType in types)
            {
                foreach (Relationship r in curType.Relationships)
                {
                    EntityType fromType = r.FromType;
                    EntityType toType = r.ToType;

                    if (fromType != null && toType != null)
                    {
                        if (IsHiddenRelationship(r, false))
                        {
                            continue;
                        }

                        bool isTabRelationship = IsTabRelationship(r, toType, false);
                        bool isCalculatedRelationship = IsCalculatedRelationship(toType, false);

                        if (!isCalculatedRelationship)
                        {
                            if (isTabRelationship)
                            {
                                if (r.ToName == "Structure Levels")
                                {
                                    controls.StructureViewRelationshipControls.AddRange(CreateStructureLevelInstanceControls(r, false));
                                }
                                controls.ForwardTabRelationshipControls.Add(CreateRelationship(r, toType, fromType, false, isTabRelationship, isCalculatedRelationship));
                            }
                            else
                            {
                                controls.ForwardRelationshipControls.Add(CreateRelationship(r, toType, fromType, false, isTabRelationship, isCalculatedRelationship));
                            }
                        }
                        else
                        {
                            controls.ForwardRelationshipControls.AddRange(CreateCalculatedControls(r, toType, fromType, false));
                        }
                    }
                }

                foreach (Relationship r in curType.ReverseRelationships)
                {
                    EntityType fromType = r.FromType;
                    EntityType toType = r.ToType;

                    if (fromType != null && toType != null)
                    {
                        if (IsHiddenRelationship(r, true))
                        {
                            continue;
                        }

                        bool isTabRelationship = IsTabRelationship(r, toType, true);
                        bool isCalculatedRelationship = IsCalculatedRelationship(toType, true);

                        if (!isCalculatedRelationship)
                        {
                            if (isTabRelationship)
                            {
                                controls.ReverseTabRelationshipControls.Add(CreateRelationship(r, toType, fromType, true, isTabRelationship, isCalculatedRelationship));
                            }
                            else
                            {
                                controls.ReverseRelationshipControls.Add(CreateRelationship(r, toType, fromType, true, isTabRelationship, isCalculatedRelationship));
                            }
                        }
                        else
                        {
                            controls.ReverseRelationshipControls.AddRange(CreateCalculatedControls(r, toType, fromType, true));
                        }
                    }
                }
            }

            return controls;
        }
    }
}

