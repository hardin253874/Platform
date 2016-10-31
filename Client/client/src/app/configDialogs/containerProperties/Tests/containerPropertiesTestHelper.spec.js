// Copyright 2011-2016 Global Software Innovation Pty Ltd
var containerPropertiesTestHelper;

(function (module) {
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
    
    //Define all the schema info.
    module.schemaFields = spEntity.fromJSON(
       {
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
       });
    
    //Define object for existing form control
    var containerControl = {
        id: 9992,
        alias: 'myControlEntity',
        name: 'container1',
        description: '',
        dataState: 0,
        typeId: 'console:verticalStackContainerControl',
        'k:renderingBackgroundColor': 'transparent',
        'k:showControlHelpText' : false,
        'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
        'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
    };
    module.containerControl = spEntity.fromJSON(containerControl);
    
})(containerPropertiesTestHelper || (containerPropertiesTestHelper = {}));