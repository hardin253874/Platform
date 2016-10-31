// Copyright 2011-2016 Global Software Innovation Pty Ltd
var choiceFieldPropertiesTestHelper;

(function (module) {
    //Define all the schema info.
    module.schemaFields = spEntity.fromJSON(
       [{
           id: 8001,
           alias: 'core:name',
           'name': 'nameField',
           isRequired: false,
           allowMultiLines: false,
           maxLength: jsonInt(),
           minLength: jsonInt(),
           isOfType: [{
               id: 'stringField',
               alias: 'core:stringField',
               'k:fieldDisplayName': { name: 'Text' },
               'k:defaultRenderingControls': [{
                   alias: 'console:singleLineTextControl',
                   'k:context': { name: 'Html' }
               }]
           }]
       },
           {
               id: 8002,
               alias: 'core:description',
               'name': 'descriptionField',
               isRequired: false,
               allowMultiLines: true,
               maxLength: jsonInt(),
               minLength: jsonInt(),
               isOfType: [{
                   id: 'stringField',
                   alias: 'core:stringField',
                   'k:fieldDisplayName': { name: 'Text' },
                   'k:defaultRenderingControls': [{
                       alias: 'console:singleLineTextControl',
                       'k:context': { name: 'Html' }
                   }]
               }]
           },
           {
               id: 8002,
               alias: 'core:toScriptName',
               'name': 'To Script Name',
               isRequired: false,
               allowMultiLines: false,
               maxLength: jsonInt(),
               minLength: jsonInt(),
               isOfType: [{
                   id: 'stringField',
                   alias: 'core:stringField',
                   'k:fieldDisplayName': { name: 'Text' },
                   'k:defaultRenderingControls': [{
                       alias: 'console:singleLineTextControl',
                       'k:context': { name: 'Html' }
                   }]
               }]
           }]);

    module.enumTypeReport = spEntity.fromJSON(
        {
            id: 8003,
            name:'Choice Fields',
            alias:'console:enumReport',
            description:'A list of all choice fields types in the system.'
        });

    module.enumType = spEntity.fromJSON({
        id: 8004,
        name:'Choice Field',
        alias:'core:enumType',
        description: 'All choice-field type definitions are of this type.',
        defaultPickerReport: {
            id: 8003,
            name: 'Choice Fields',
            alias: 'console:enumReport'
        }
    });
    module.resizeModes = spEntity.fromJSON([
        {
            id: 8005,
            name: 'Manual',
            alias:'console:resizeManual'
        },
        {
            id: 8006,
            name: 'Automatic',
            alias: 'console:resizeAutomatic'
        }
    ]);
   var instancesOfType = [
        {
            id: 8010,
            name: 'Sunday',
            description: '',
            enumOrder: 1,
            canModify: true
        },
        {
            id: 8011,
            name: 'Monday',
            description: '',
            enumOrder: 2,
            canModify: true
        },
        {
            id: 8012,
            name: 'Tuesday',
            description: '',
            enumOrder: 3,
            canModify: true
        },
        {
            id: 8013,
            name: 'Wednesday',
            description: '',
            enumOrder: 4,
            canModify: true
        },
        {
            id: 8014,
            name: 'Thursday',
            description: '',
            enumOrder: 5,
            canModify: true
        },
        {
            id: 8015,
            name: 'Friday',
            description: '',
            enumOrder: 6,
            canModify: true
        },
        {
            id: 8016,
            name: 'Saturday',
            description: '',
            enumOrder: 7,
            canModify: true
        }
    ];

   module.instancesOfType = spEntity.fromJSON(instancesOfType);

    module.getChoiceValuesOfType = spEntity.fromJSON({
        id: 9992,
        name: 'Weekdays',
        alias: 'core:Weekdays',
        typeId: 'core:enumType',
        inherits: [{ id: 'enumValue' }],
        defaultPickerReport: {
            id: 8003,
            name: 'Choice Fields',
            alias: 'console:enumReport'
        },
        instancesOfType: instancesOfType
    });

    module.putEntity = function(entity) {
      //  module.choiceField.setToType(entity);
        if (entity.instancesOfType &&  entity.instancesOfType.length>0)
            module.instancesOfType = entity.instancesOfType;
    };
    
    module.getDummyFieldControlOnForm = function (field, fieldTitle) {
        var fieldType = field.getIsOfType()[0];

        var defaultRenderingControl = _.find(fieldType.getDefaultRenderingControls(), function (control) {
            return control.getContext().getName() === 'Html';
        });
        if (!defaultRenderingControl) {
            defaultRenderingControl = _.find(fieldType.getRenderingControl(), function (control) {
                return control.getContext().getName() === 'Html';
            });
        }
        var dummyFormControl = spEntity.fromJSON({
            typeId: defaultRenderingControl.nsAlias,
            'name': fieldTitle,
            'description': '',
            'console:fieldToRender': field,
            'console:mandatoryControl': false,
            'console:readOnlyControl': false,
            'console:showControlHelpText': false,
            'console:isReversed': false
        });
        return dummyFormControl;
    };
    
    //Define the object for existing relationship
    var choiceField ={
                       id: 9991,
                       name: 'Choice Field Test',
                       description: '',
                       dataState:0,
                       typeId:'core:relationship',
                       toName: 'Choice Field Test',
                       fromName: '',
                       fromType: { name: 'test:testEntity' },
                       toType: {
                           id: 9992,
                           name: 'Weekdays',
                           alias: 'core:Weekdays',
                           typeId:'core:enumType',
                           inherits: [{ id: 'enumValue' }],
                           defaultPickerReport: {
                               id: 8003,
                               name: 'Choice Fields',
                               alias: 'console:enumReport'
                           },
                           instancesOfType: instancesOfType
                       },
                       toTypeDefaultValue: { name: jsonString('') },
                       fromTypeDefaultValue: { name: jsonString('') },
                       cardinality: { alias: 'manyToOne' },
                       relationshipIsMandatory: false,
                       revRelationshipIsMandatory: false,
                       isRelationshipReadOnly: false,
                       relType: { alias: 'relChoiceField' },
                       cascadeDelete: false,
                       cascadeDeleteTo: false,
                       cloneAction: { alias: 'cloneReferences' },
                       reverseCloneAction: { alias: 'drop' },
                       implicitInSolution: false,
                       reverseImplicitInSolution: false
                   };

    module.choiceField = spEntity.fromJSON(choiceField);
    //Define object for existing form control
    var choiceFieldControl = {
        id: 9992,
        alias: 'myControlEntity',
        name: '',
        description: '',
        dataState: 0,
        typeId:'console:choiceRelationshipRenderControl',
        'console:mandatoryControl': false,
        'console:readOnlyControl': false,
        'console:showControlHelpText': false,
        'console:isReversed': false,
        isOfType: { id: 'console:choiceRelationshipRenderControl' },
        'k:renderingBackgroundColor': 'transparent',
        'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
        'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
        relationshipToRender: choiceField
    };
    module.choiceFieldControl = spEntity.fromJSON(choiceFieldControl);
    //define the object for new relationship
   
    //Define object for new form control
    
    

})(choiceFieldPropertiesTestHelper || (choiceFieldPropertiesTestHelper = {}));