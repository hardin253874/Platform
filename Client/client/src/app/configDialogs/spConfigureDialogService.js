// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mod.app.configureDialog.service', ['mod.common.spEntityService'])

    /**
    * Module implementing the configure dialog service.
    * configureDialogService provides methods to interact with the configure dialog to set field and relationship properties.
    
    */

    .service('configureDialogService', function($q, $rootScope, spEntityService) {
        'use strict';
        var exports = {};
        var fieldBuildRequest = 'id,name,description,alias,{isOfType, isOfType.inherits*}.{id, alias, name,{k:renderingControl, k:defaultRenderingControls}.{name,alias,k:designControl,k:control,k:context.name}}, isRequired, allowMultiLines, pattern.{regex, regexDescription}, ' +
                      'minLength, maxLength, minInt, maxInt, minDecimal, maxDecimal, minDate, maxDate, minTime, maxTime, minDateTime, maxDateTime,decimalPlaces,fieldRepresents.{id, alias},defaultValue,fieldWatermark,fieldScriptName,isCalculatedField,fieldCalculation';
        var fieldQueryFragment = fieldBuildRequest + ',fieldIsOnType.{id, alias}, autoNumberDisplayPattern,autoNumberSeed';
        var fieldControlQueryFragment = 'name, description, alias,isOfType.{id, alias}, k:renderingHeight, k:renderingWidth, k:renderingBackgroundColor,k:mandatoryControl, k:readOnlyControl,k:visibilityCalculation,k:fieldToRender.{' + fieldQueryFragment + '}';
     // exports.fieldBuildRequest = fieldBuildRequest;
        
       /**
      * Retrieves the schema information required to implement field properties dialog.
      * 
      */
        function getSchemaInfoForFields() {
            var batch = new spEntityService.BatchRequest();
            var validateFields = ['core:name', 'core:description', 'core:isRequired', 'core:defaultValue', 'core:decimalPlaces', 'core:autoNumberDisplayPattern', 'core:autoNumberSeed', 'core:minLength', 'core:maxLength', 'core:minInt', 'core:maxInt', 'core:minDecimal', 'core:maxDecimal', 'core:minDate', 'core:maxDate', 'core:minTime', 'core:maxTime', 'core:minDateTime', 'core:maxDateTime', 'core:fieldScriptName'];


            var fieldTypeRequest = 'id, alias,name,k:fieldDisplayName.name,{k:renderingControl, k:defaultRenderingControls}.{name,alias,k:designControl,k:control,k:context.name}';


            var r1 = spEntityService.getEntities(validateFields, fieldBuildRequest, { batch: batch });
          
            var r2 = spEntityService.getEntitiesOfType('core:stringPattern', 'id,alias,name,regex,regexDescription', { batch: batch });

            var r3 = spEntityService.getEntities(['core:stringField', 'core:intField', 'core:decimalField', 'core:currencyField', 'core:dateField', 'core:timeField', 'core:dateTimeField', 'boolField', 'autoNumberField'], fieldTypeRequest, { batch: batch });
            
            var r4 = spEntityService.getEntitiesOfType('k:resizeEnum', 'name,alias,enumOrder', { batch: batch });

            var r5 = spEntityService.getEntitiesOfType('core:fieldRepresentsEnum', 'name,alias', { batch: batch });
         
            var promise= $q.all({
                fields: r1,
                stringPatterns: r2,
                fieldTypes: r3,
                resizeModes: r4,
                fieldRepresentsEnums: r5
            });
            
            batch.runBatch();
            return promise;
        }

        exports.getSchemaInfoForFields = getSchemaInfoForFields;

        /**
        *returns bool value. True if type has instance/false if type has no instances.
        *@param {number | string} id - the id/alias of the type to check for the instances.
        **/
        exports.isTypeHasInstances = function(type) {
            var queryRequest = 'instancesOfType.{name,id}';
            return spEntityService.getEntity(type, queryRequest).then(function (result) {
                return (result.getInstancesOfType() && result.getInstancesOfType().length > 0);
            });
            
        };
        /**
      *returns the promise for the field control entity request
      *@param {number | string} id - the id/alias of the field control.
      *
      **/
        function  getFormControlEntity(id) {
            return spEntityService.getEntity(id, fieldControlQueryFragment);
        }

        exports.getFormControlEntity = getFormControlEntity;
        
        /**
    *returns the promise for the field entity request
    *@param {number | string} id - the id/alias of the field.
    *
    **/
        function getFieldEntity(id) {
            return spEntityService.getEntity(id, fieldQueryFragment);
        } 
        exports.getFieldEntity = getFieldEntity;
        /**
        * Retrieves the schema information required to implement image field properties dialog.
        * 
        */
        /**
        * Retrieves the schema information required to implement image field properties dialog.
        * 
        */
        function getSchemaInfoForImageField () {
            
            var batch = new spEntityService.BatchRequest();
            var validateFields = ['core:name', 'core:description'];

            var r1 = spEntityService.getEntities(validateFields, fieldBuildRequest, { batch: batch });

            var r2 = spEntityService.getEntitiesOfType('console:thumbnailSizeEnum', 'name,alias,k:thumbnailWidth, k:thumbnailHeight,k:isSystemThumbnail', { batch: batch });
          
            var r3 = spEntityService.getEntitiesOfType('imageScaleEnum', 'name,alias,description,enumOrder', { batch: batch });
            
            var r4 = spEntityService.getEntitiesOfType('k:resizeEnum', 'name,alias,enumOrder', { batch: batch });
            
            var r5 = spEntityService.getEntity('core:photoFileType', 'name,alias,defaultPickerReport.name', { batch: batch });
            //templateReport
            var r6 = spEntityService.getEntity('core:templateReport', 'name,alias,description', { batch: batch });

            var promise = $q.all({
                fields: r1,
                thumbNailSize: r2,
                thumbNailScaling: r3,
                resizeModes: r4,
                toType: r5,
                templateReport:r6
            });

            batch.runBatch();
            return promise;
        }
        exports.getSchemaInfoForImageField = getSchemaInfoForImageField;
        
        /**
        *returns the promise for the image control entity request
        *@param {number | string} id - the id/alias of the image field control.
        *
        **/
        var imageRelationshipBuildRequest = 'name, description,alias,toName,fromName,fromType.{ name,defaultPickerReport.name},fromType.inherits*.{ name,defaultPickerReport.name },toType.{ name,defaultPickerReport.name},toType.inherits*.{ name,defaultPickerReport.name},cardinality.name,relationshipIsMandatory,revRelationshipIsMandatory,toTypeDefaultValue.name,fromTypeDefaultValue.name,isRelationshipReadOnly,relType.alias,cascadeDelete,cascadeDeleteTo,cloneAction.alias,reverseCloneAction.alias,implicitInSolution,reverseImplicitInSolution';
        function getImageControlEntity(id) {
            var imageControlBuildRequest = 'name, description, alias,isOfType.{id, alias}, k:renderingBackgroundColor,k:mandatoryControl, k:readOnlyControl, k:thumbnailScalingSetting.{ alias },k:thumbnailSizeSetting.{ alias, k:thumbnailWidth, k:thumbnailHeight,k:pickerReport.{name, alias, description}},k:visibilityCalculation,' +
                'k:relationshipToRender.{' + imageRelationshipBuildRequest + '}';
            return spEntityService.getEntity(id, imageControlBuildRequest);
        }
        exports.getImageControlEntity = getImageControlEntity;
        
        /**
        *returns the promise for the image relationship entity request
        *@param {number | string} id - the id/alias of the image relationship entity.
        *
        **/
        function getImageRelationshipEntity(id) {
            return spEntityService.getEntity(id, imageRelationshipBuildRequest);
        }
        exports.getImageRelationshipEntity = getImageRelationshipEntity;

       /**
       * Retrieves the schema information required to implement choice field properties dialog.
       * 
       */
        function getSchemaInfoForChoiceField() {

            var batch = new spEntityService.BatchRequest();
            var validateFields = ['core:name', 'core:description', 'core:toScriptName'];
          

            var r1 = spEntityService.getEntities(validateFields, fieldBuildRequest, { batch: batch });

            var r2 = spEntityService.getEntity('console:enumReport', 'name, description', { batch: batch });
            
            var r3 = spEntityService.getEntity('core:enumType', 'name,alias,description,defaultPickerReport.{name,alias,description}', { batch: batch });
            
            var r4 = spEntityService.getEntitiesOfType('k:resizeEnum', 'name,alias,enumOrder', { batch: batch });

            var promise = $q.all({
                fields: r1,
                enumTypeReport: r2,
                enumType: r3,
                resizeModes:r4
            });

            batch.runBatch();
            return promise;
        }

        exports.getSchemaInfoForChoiceField = getSchemaInfoForChoiceField;
        
        var choiceRelationshipBuildRequest = 'name, description,alias,toName,fromName,fromType.{ name,defaultPickerReport.name},fromType.inherits*.{ name,defaultPickerReport.name },toType.{ name,defaultPickerReport.name,instancesOfType.{name,description,enumOrder}},toType.inherits*.{ name,defaultPickerReport.name,instancesOfType.{name,description,enumOrder}},cardinality.name,relationshipIsMandatory,revRelationshipIsMandatory,toTypeDefaultValue.name,fromTypeDefaultValue.name,isRelationshipReadOnly,relType.alias,cascadeDelete,cascadeDeleteTo,cloneAction.alias,reverseCloneAction.alias,implicitInSolution,reverseImplicitInSolution';

        /**
       *returns the promise for the image control entity request
       *@param {number | string} id - the id/alias of the choice field control.
       *
       **/
       function getChoiceFieldControlEntity(id) {
           var choiceFieldControlBuildRequest = 'name, description, alias,isOfType.{id, alias}, k:renderingBackgroundColor,k:renderingHorizontalResizeMode.{id,name,alias}, k:renderingVerticalResizeMode.{id,name,alias},k:mandatoryControl, k:readOnlyControl,k:visibilityCalculation,' +
                'k:relationshipToRender.{' + choiceRelationshipBuildRequest + '}, k:relationshipControlFilters.{ k:relationshipControlFilterOrdinal, k:relationshipControlFilter.{ name, k:isReversed, k:relationshipToRender.{ name, fromName, toName } }, k:relationshipFilter.id, k:relationshipDirectionFilter.alias }';
            return spEntityService.getEntity(id, choiceFieldControlBuildRequest);
        }
       exports.getChoiceFieldControlEntity = getChoiceFieldControlEntity;
        

        /**
        *returns the promise for the choice relationship entity request
        *@param {number | string} id - the id/alias of the choice relationship.
        *
        **/
       function getChoiceRelationshipEntity(id) {
           return spEntityService.getEntity(id, choiceRelationshipBuildRequest);
       }
       exports.getChoiceRelationshipEntity = getChoiceRelationshipEntity;
        /**
       *returns the promise for the instances of enum type entity request
       *@param {number | string} id - the id/alias of the  enum type entity.
       *
       **/
        function getChoiceValuesOfType(id) {
            //return spEntityService.getEntitiesOfType(id, 'name,alias,description,enumOrder');

            var query = 'name,alias,description,defaultPickerReport.name,' +
                        'enumValueFormattingType.{name,alias},' +
                        'instancesOfType.{name,description,enumOrder,canModify,canDelete,' +
                                            'enumFormattingRule.{' +
                                                'name,alias,' +
                                                'isOfType.{ name, alias },' +
                                                'iconRules.{iconRuleImage.{name, alias}},' +
                                                'colorRules.{' +
                                                    'colorRuleForeground,' +
                                                    'colorRuleBackground' +
                                                    '}' +
                                            '}' +
                                          '},' +
                        'inherits*.{name,alias,description,defaultPickerReport.name}';


            return spEntityService.getEntity(id, query);
        }
        exports.getChoiceValuesOfType = getChoiceValuesOfType;
        
       /**
       * Retrieves the schema information required to implement container properties dialog.
       * 
       */
        function getSchemaInfoForContainer() {
            var batch = new spEntityService.BatchRequest();

            var r1 = spEntityService.getEntity('core:name', fieldBuildRequest, { batch: batch });            
            var r2 = spEntityService.getEntitiesOfType('k:resizeEnum', 'name,alias,enumOrder', { batch: batch });
            var r3 = spEntityService.getEntity('core:description', fieldBuildRequest, { batch: batch });

            var promise = $q.all({
                field: r1,
                description: r3,
                resizeModes: r2
            });

            batch.runBatch();
            return promise;
            
        }
        exports.getSchemaInfoForContainer = getSchemaInfoForContainer;

        /**
       *returns the promise for the container control entity request
       *@param {number | string} id - the id/alias of the container control.
       *
       **/
        function getContainerControlEntity(id) {
            var containerControlBuildRequest = 'name, description, alias,k:renderingBackgroundColor,k:renderingHorizontalResizeMode.{id,name,alias,enumOrder},k:renderingVerticalResizeMode.{id,name,alias,enumOrder},k:hideLabel,k:visibilityCalculation';
            return spEntityService.getEntity(id, containerControlBuildRequest);
        }
        exports.getContainerControlEntity = getContainerControlEntity;
        
        /**
        * Retrieves the schema information required to implement lookup and relationship properties dialog.
        */
        exports.getSchemaInfo = function() {

            var batch = new spEntityService.BatchRequest();
            var fields = ['core:name', 'core:description', 'core:toName', 'core:fromName', 'core:toScriptName', 'core:fromScriptName'];
            var entities = ['core:testSolution', 'core:definition', 'console:inlineRelationshipRenderControl', 'console:tabRelationshipRenderControl', 'console:dropDownRelationshipRenderControl', 'core:drop', 'core:cloneReferences', 'core:cloneEntities', 'core:manyToOne', 'core:oneToMany', 'core:oneToOne', 'core:manyToMany'];
            
            var r1 = spEntityService.getEntities(fields, fieldBuildRequest, { batch: batch });
            var r2 = spEntityService.getEntities(entities, 'alias, name, description', { batch: batch });
            var r3 = spEntityService.getEntitiesOfType('k:resizeEnum', 'name,alias,enumOrder', { batch: batch });

            var promise = $q.all({
                fields: r1,
                entities: r2,
                resizeModes:r3
            });

            batch.runBatch();
            return promise;
        };
        
        ///
        // Get dummy fieldControlOnForm from json.
        ///
        exports.getDummyFieldControlOnForm = function(field, fieldTitle) {
            var fieldType = field.getIsOfType()[0];

            var defaultRenderingControl = _.find(fieldType.getDefaultRenderingControls(), function(control) {
                return control.getContext().getName() === 'Html';
            });
            if (!defaultRenderingControl) {
                defaultRenderingControl = _.find(fieldType.getRenderingControl(), function(control) {
                    return control.getContext().getName() === 'Html';
                });
            }
            var dummyFormControl = spEntity.fromJSON({
                typeId: defaultRenderingControl.nsAlias,
                'name': fieldTitle,
                'description': '',
                'console:fieldToRender': field,
                'console:mandatoryControl': false,
                'console:showControlHelpText': false,
                'console:readOnlyControl': false,
                'console:isReversed': false,
                'console:visibilityCalculation': ''
            });
            return dummyFormControl;
        };
        
        
        /// lookup/ relationship section
        var baseRelationshipRequestString = 'name, description, alias, toName, fromName, toScriptName, fromScriptName, securesTo, securesFrom, defaultFromUseCurrent, defaultToUseCurrent, hideOnFromType, hideOnFromTypeDefaultForm, hideOnToType, hideOnToTypeDefaultForm, fromType.{ name,defaultPickerReport.name},fromType.inherits*.{ name,defaultPickerReport.name },toType.{ name,defaultPickerReport.name},toType.inherits*.{ name,defaultPickerReport.name},cardinality.name,relationshipIsMandatory,revRelationshipIsMandatory,toTypeDefaultValue.name,fromTypeDefaultValue.name,isRelationshipReadOnly,relType.alias,cascadeDelete,cascadeDeleteTo,cloneAction.alias,reverseCloneAction.alias,implicitInSolution,reverseImplicitInSolution';
        var baseRelControlOnFormRequestString = 'name, description, alias, k:isReversed, canCreate, canCreateDerivedTypes, resourceViewerConsoleForm.name, resourceViewerTabletForm.name,isOfType.{id, alias, k:designControl }, k:renderingBackgroundColor,k:mandatoryControl, k:readOnlyControl,k:visibilityCalculation,' +
                                                'k:relationshipToRender.{' + baseRelationshipRequestString + '}, k:relationshipControlFilters.{ k:relationshipControlFilterOrdinal, k:relationshipControlFilter.{ name, k:isReversed, k:relationshipToRender.{ name, fromName, toName } }, k:relationshipFilter.id, k:relationshipDirectionFilter.alias }';
        
        exports.getBaseRelControlOnFormEntity = function (id) {
            return spEntityService.getEntity(id, baseRelControlOnFormRequestString);
        };
        
        exports.getBaseRelationshipEntity = function (id) {
            return spEntityService.getEntity(id, baseRelationshipRequestString);
        };
        
        exports.getFormsForTypeAndInheritedTypes = function (typeId) {
            var buildString = 'alias, k:formsToEditType.{ alias, name }, inherits*.{ alias, k:formsToEditType.{ alias, name } }';
            return spEntityService.getEntity(typeId, buildString);
        };
        
        exports.getReportsForType = function (typeId) {
            var buildString = 'alias, definitionUsedByReport.{ alias, name, isOfType.alias, reportForAccessRule.name }';
            return spEntityService.getEntity(typeId, buildString);
        };
        
        exports.getTypeAndInheritedTypes = function (typeId) {
            var buildString = 'alias, name, inherits*.alias';
            return spEntityService.getEntity(typeId, buildString);
        };
        
        return exports;
    });