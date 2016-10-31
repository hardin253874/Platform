// Copyright 2011-2016 Global Software Innovation Pty Ltd
var fieldPropertiesTestHelper;

(function (module) {
    module.stringField = spEntity.fromJSON(
                    {
                        id: 9991,
                        'name': 'nameField',
                        description: '',
                        dataState:0,
                        isRequired: false,
                        allowMultiLines: false,
                        maxLength: jsonInt(),
                        minLength: jsonInt(),
                        defaultValue:'',
                        pattern: {
                            id: 9992,
                            name: 'Resource Name',
                            regex: '^[^<>]+$',
                            regexDescription: 'The field cannot contain angled brackets.'
                        },
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

    module.dummyFormControl = spEntity.fromJSON(
                {
                    id: 'myControlEntity',
                    name: '',
                    description:'',
                    dataState: 0,
                    'console:mandatoryControl': false,
                    'console:showControlHelpText': false,
                    'console:readOnlyControl': false,
                    'console:isReversed': false,
                    isOfType: { id: 'k:singleLineTextControl' },
                    'k:renderingBackgroundColor': 'transparent',
                    'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                    fieldToRender: {
                        id: 9991,
                        name: 'nameField',
                        description: '',
                        isRequired: false,
                        allowMultiLines: false,
                        maxLength: jsonInt(),
                        minLength: jsonInt(),
                        defaultValue: '',
                        pattern: {
                            id: 9992,
                            name:'Resource Name',
                            regex: '^[^<>]+$',
                            regexDescription: 'The field cannot contain angled brackets.'
                        },
                        isOfType: [{
                            id: 'stringField',
                            'k:fieldDisplayName': { name: 'Text' },
                            'k:defaultRenderingControls': [{
                                alias: 'console:singleLineTextControl',
                                'k:context': { name: 'Html' }
                            }]
                        }]
                    }
                });

    module.stringPatterns = spEntity.fromJSON(
               [{
                    id: 9992,
                    name: 'Resource Name',
                    regex: '^[^<>]+$',
                    regexDescription: 'The field cannot contain angled brackets.'
               },
               {
                   id: 9993,
                   name: 'Host Name',
                   regex: '^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$',
                   regexDescription: 'A network address.'
               },
               {
                   id: 9994,
                   name: 'Web Address',
                   regex: '^(http(s)?://)?([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?$',
                   regexDescription: 'A valid web address, with or without the leading HTTP.'
               },
               {
                   id: 9995,
                   name: 'Phone Number',
                   regex: '^[+]?[0-9()*# -]+$',
                   regexDescription: 'The field can only contain digits and punctuation characters.'
               },
               {
                   id: 9996,
                   name: 'Email Address',
                   regex: '^[^@]+[@]([a-zA-Z][a-zA-Z0-9-]*)(\\.[a-zA-Z][a-zA-Z0-9-]*)*$',
                   regexDescription: 'The field must be in the form of an email address.'
               }
               ]);
    module.schemaFields = spEntity.fromJSON(
              [{
                  id:8001,
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
                  id: 8003,
                  alias: 'core:isRequired',
                  'name': 'isRequiredField',
                  isOfType: [{
                      id: 'boolField',
                      alias: 'core:boolField',
                      'k:fieldDisplayName': { name: 'Yes/No' },
                      'k:defaultRenderingControls': [{
                          alias: 'core:checkboxKFieldRenderControl',
                          'k:context': { name: 'Html' }
                      }]
                  }]
              },
              {
                  id: 8004,
                  alias: 'core:defaultValue',
                  'name': 'defaultValueField',
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
                  id: 8005,
                  alias: 'core:decimalPlaces',
                  'name': 'decimalPlacesField',
                  isRequired: false,
                  maxInt: jsonInt(),
                  minInt: jsonInt(),
                  defaultValue:3,
                  isOfType: [{
                      id: 'intField',
                      alias: 'core:intField',
                      'k:fieldDisplayName': { name: 'Int' },
                      'k:defaultRenderingControls': [{
                          alias: 'core:numericKFieldRenderControl',
                          'k:context': { name: 'Html' }
                      }]
                  }]
              },
                    {
                        id: 8006,
                        alias: 'core:autoNumberDisplayPattern',
                        'name': 'autoNumberDisplayPatternField',
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
                      id: 8007,
                      alias: 'core:autoNumberSeed',
                      'name': 'autoNumberSeedField',
                      isRequired: false,
                      maxInt: jsonInt(),
                      minInt: jsonInt(),
                      defaultValue: 1,
                      isOfType: [{
                          id: 'intField',
                          alias: 'core:intField',
                          'k:fieldDisplayName': { name: 'Int' },
                          'k:defaultRenderingControls': [{
                              alias: 'core:numericKFieldRenderControl',
                              'k:context': { name: 'Html' }
                          }]
                      }]
                  },
                  {
                      id: 8008,
                      alias: 'core:minLength',
                      'name': 'minLengthField',
                      isRequired: false,
                      maxInt: jsonInt(),
                      minInt: jsonInt(),
                      isOfType: [{
                          id: 'intField',
                          alias: 'core:intField',
                          'k:fieldDisplayName': { name: 'Int' },
                          'k:defaultRenderingControls': [{
                              alias: 'core:numericKFieldRenderControl',
                              'k:context': { name: 'Html' }
                          }]
                      }]
                  },
                  {
                      id: 8009,
                      alias: 'core:maxLength',
                      'name': 'core:maxLengthField',
                      isRequired: false,
                      maxInt: jsonInt(),
                      minInt: jsonInt(),
                      isOfType: [{
                          id: 'intField',
                          alias: 'core:intField',
                          'k:fieldDisplayName': { name: 'Int' },
                          'k:defaultRenderingControls': [{
                              alias: 'core:numericKFieldRenderControl',
                              'k:context': { name: 'Html' }
                          }]
                      }]
                  },
                  {
                      id: 8010,
                      alias: 'core:fieldScriptName',
                      'name': 'Script name',
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
                  }
              ]);
    
    module.schemaFieldTypes = spEntity.fromJSON(
             [{
                 id: 'core:stringField',
                 alias: 'core:stringField',
                 'k:fieldDisplayName': { name: 'Text' },
                 'k:defaultRenderingControls': [{
                     alias: 'console:singleLineTextControl',
                     'k:context': { name: 'Html' }
                 }]
             },
             {
                 id: 'core:intField',
                 alias: 'core:intField',
                 'k:fieldDisplayName': { name: 'Numeric' },
                 'k:defaultRenderingControls': [{
                     alias: 'core:numericKFieldRenderControl',
                     'k:context': { name: 'Html' }
                 }]
             },
             {
                 id: 'core:decimalField',
                 alias: 'core:decimalField',
                 'k:fieldDisplayName': { name: 'Decimal' },
                 'k:defaultRenderingControls': [{
                     alias: 'core:decimalKFieldRenderControl',
                     'k:context': { name: 'Html' }
                 }]
             },
             {
                 id: 'core:currencyField',
                 alias: 'core:currencyField',
                 'k:fieldDisplayName': { name: 'Currency' },
                 'k:defaultRenderingControls': [{
                     alias: 'core:currencyKFieldRenderControl',
                     'k:context': { name: 'Html' }
                 }]
             },
             {
                 id: 'core:dateField',
                 alias: 'core:dateField',
                 'k:fieldDisplayName': { name: 'Date Only' },
                 'k:defaultRenderingControls': [{
                     alias: 'core:dateKFieldRenderControl',
                     'k:context': { name: 'Html' }
                 }]
             },
                   {
                       id: 'core:timeField',
                       alias: 'core:timeField',
                       'k:fieldDisplayName': { name: 'Time Only' },
                       'k:defaultRenderingControls': [{
                           alias: 'core:timeKFieldRenderControl',
                           'k:context': { name: 'Html' }
                       }]
                   },
                 {
                     id: 'core:dateTimeField',
                     alias: 'core:dateTimeField',
                     'k:fieldDisplayName': { name: 'Date and Time' },
                     'k:defaultRenderingControls': [{
                         alias: 'core:dateAndTimeKFieldRenderControl',
                         'k:context': { name: 'Html' }
                     }]
                 },
                 {
                     id: 'boolField',
                     alias: 'core:boolField',
                     'k:fieldDisplayName': { name: 'Yes/No' },
                     'k:defaultRenderingControls': [{
                         alias: 'core:checkboxKFieldRenderControl',
                         'k:context': { name: 'Html' }
                     }]
                 },
                 {
                     id: 'autoNumberField',
                     alias: 'core:autoNumberField',
                     'k:fieldDisplayName': { name: 'Autonumber' },
                     'k:defaultRenderingControls': [{
                         alias: 'core:autoNumberFieldRenderControl',
                         'k:context': { name: 'Html' }
                     }]
                 }
             ]);
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
    


})(fieldPropertiesTestHelper || (fieldPropertiesTestHelper = {}));

