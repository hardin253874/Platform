// Copyright 2011-2016 Global Software Innovation Pty Ltd
var imageFieldPropertiesTestHelper;

(function (module) {
    module.imageField = spEntity.fromJSON(
                    {
                        id: 9991,
                        name: 'Image Field Test',
                        description: '',
                        toName: 'Image Field Test',
                        fromName: '',
                        fromType: { name: 'test:testEntity' },
                        toType: {
                            name: 'Photo File Type',
                            alias: 'core:photoFileType',
                            defaultPickerReport: jsonLookup()
                        },
                        toTypeDefaultValue: { name: jsonString('') },
                        fromTypeDefaultValue: { name: jsonString('') },
                       cardinality: { alias: 'manyToOne' },
                       relationshipIsMandatory: false,
                       revRelationshipIsMandatory: false,
                       isRelationshipReadOnly: false,
                       relType:{alias:'relLookup'},
                       cascadeDelete:false,
                       cascadeDeleteTo:false,
                       cloneAction: { alias: 'cloneReferences' },
                       reverseCloneAction: { alias: 'drop' },
                       implicitInSolution:false,
                       reverseImplicitInSolution:false
                    });
    
    module.dummyFormControl = spEntity.fromJSON(
             {
                 id: 9993,
                 alias: 'myControlEntity',
                 name: '',
                 typeId:'console:imageRelationshipRenderControl',
                 description: '',
                 dataState: 0,
                 'console:mandatoryControl': false,
                 'console:readOnlyControl': false,
                 'console:showControlHelpText': false,
                 'console:isReversed': false,
                 'console:renderingBackgroundColor': 'white',
                 'console:thumbnailScalingSetting': jsonLookup(),
                 'console:thumbnailSizeSetting': jsonLookup(),
                 'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                 'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                 relationshipToRender: {
                     id: 9991,
                     name: 'Image Field Test',
                     description: '',
                     toName: 'Image Field Test',
                     fromName: '',
                     fromType: { name: 'test:testEntity' },
                     toType: {
                         name: 'Photo File Type',
                         alias: 'core:photoFileType',
                         defaultPickerReport:jsonLookup()
                     },
                     toTypeDefaultValue:jsonString(''),
                     cardinality: { alias: 'manyToOne' },
                     relationshipIsMandatory: false,
                     revRelationshipIsMandatory: false,
                     isRelationshipReadOnly: false,
                     relType: { alias: 'relLookup' },
                     cascadeDelete: false,
                     cascadeDeleteTo: false,
                     cloneAction: { alias: 'cloneReferences' },
                     reverseCloneAction: { alias: 'drop' },
                     implicitInSolution: false,
                     reverseImplicitInSolution: false
                 }
             });

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
            }]);
    
    module.thumbNailSize = spEntity.fromJSON(
        [{
            name: 'Small',
            alias: 'console:smallThumbnail',
            'k:thumbnailWidth': 150,
            'k:thumbnailHeight': 150,
            'k:isSystemThumbnail':false
            
        },
        {
            name: 'Large',
            alias: 'console:largeThumbnail',
            'k:thumbnailWidth': 300,
            'k:thumbnailHeight': 300,
            'k:isSystemThumbnail': false

        },
        {
            name: 'Icon',
            alias: 'console:iconThumbnailSize',
            'k:thumbnailWidth': 16,
            'k:thumbnailHeight': 16,
            'k:isSystemThumbnail': true

        }]);
    module.thumbNailScaling = spEntity.fromJSON(
      [{
          name: 'Fit proportional',
          alias: 'core:scaleImageProportionally',
          description: 'Resize image to fit inside the item while maintaining the aspect ratio',
          enumOrder: 1
      },
      {
          name: 'Fit to size',
          alias: 'core:scaleImageToFit',
          description: 'Resize image to fit inside the item',
          enumOrder: 2

      },
      {
          name: 'Crop',
          alias: 'core:cropImage',
          description: 'Crop image to fit inside the item',
          enumOrder: 3

      }]);
    module.resizeModes = spEntity.fromJSON([
       {
           id: 8005,
           name: 'Manual',
           alias: 'console:resizeManual'
       },
       {
           id: 8006,
           name: 'Automatic',
           alias: 'console:resizeAutomatic'
       }
    ]);
    module.toType = spEntity.fromJSON({
        id: 2100,
        name: 'Photo File Type',
        alias: 'core:photoFileType',
        defaultPickerReport: jsonLookup()
    });

    module.templateReport = spEntity.fromJSON({
        id: 1234,
        name: 'Template',
        alias: 'core:templateReport'
    });

})(imageFieldPropertiesTestHelper || (imageFieldPropertiesTestHelper = {}));