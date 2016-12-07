// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spTargetCampaignSurveyTakerPicker', [
        'mod.common.spEntityService',
        'mod.app.editFormServices',
        'sp.common.fieldValidator',
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.navigationProviders',
        'mod.common.spCachingCompile'])
           .directive('spTargetCampaignSurveyTakerPicker', function ($q, $parse, $timeout, spEntityService, spEditForm, spFieldValidator, spFormBuilderService, spNavService, spCachingCompile) {
               return {
                   restrict: 'AE',
                   replace: false,
                   transclude: false,
                   scope: {
                       formControl: '=',
                       parentControl: '=?',
                       formData: '=',
                       formMode: '=?',
                       isInTestMode: '=?',
                       isReadOnly: '=?',
                       isInDesign: '=?'
                   },
                   link: function (scope, element) {

                       /////
                       // Control setup.
                       /////
                       scope.model = {
                           targetTypeEntity: null
                       };
                       scope.isMandatory = true;
                       scope.titleModel = {
                           name: 'Survey taker',
                           description: 'The relationship between the recipients and the object of the survey.',
                           hasName: true,
                           readonly: true
                       };
                       scope.customValidationMessages = [];
                       scope.relationshipToRender = null;
                       scope.relTarget = { id: -1, isReverse: false };
                       scope.relSurveyTaker = { id: -1, isReverse: false };
                       
                       var relSurveyTakerDirection = { id: -1, isReverse: false };

                       scope.$watch("formControl", function () {

                           if (scope.formControl) {

                               scope.formControl.spValidateControl = function () {
                                   validate();
                                   return scope.customValidationMessages.length === 0;
                               };

                               /////
                               // Hold the relationship that gets/sets the campaign target.
                               /////
                               if (!scope.relTarget || scope.relTarget.id < 0) {
                                   spEntityService.getEntity('core:campaignTarget', 'name').then(function (rel) {
                                       if (rel) {
                                           scope.relTarget = { id: rel.idP, isReverse: false };
                                       }
                                   });
                               }

                               /////
                               // Hold the relationship that gets/sets the relationship from target to survey taker.
                               /////
                               if (!scope.relSurveyTaker || scope.relSurveyTaker.id < 0) {
                                   spEntityService.getEntity('core:campaignTargetRelationship', 'name').then(function (rel) {
                                       if (rel) {
                                           scope.relationshipToRender = rel;
                                           scope.relSurveyTaker = { id: rel.idP, isReverse: false };
                                       }
                                   });
                               }

                               /////
                               // Hold the relationship that gets/sets the direction of the relationship from target to survey taker.
                               /////
                               if (!relSurveyTakerDirection || relSurveyTakerDirection.id < 0) {
                                   spEntityService.getEntity('core:campaignTargetRelationshipDirection', 'name').then(function (rel) {
                                       if (rel) {
                                           relSurveyTakerDirection = { id: rel.idP, isReverse: false };
                                       }
                                   });
                               }

                               /////
                               // Watch form data for changes to the campaign target.
                               /////
                               scope.$watch('formData && relTarget && (formData.getRelationshipContainer(relTarget).changeId + "|" + formData.id())', function () {
                                   var selectedEntities = [];
                                   if (scope.formData && scope.relTarget && scope.relTarget.id > 0) {
                                       selectedEntities = scope.formData.getRelationship(scope.relTarget);
                                   }
                                   scope.model.targetTypeEntity = _.first(selectedEntities) || null;
                               });

                               /////
                               // Watch form data for changes to the campaign survey taker relationship.
                               /////
                               scope.$watch('formData && relSurveyTaker && (formData.getRelationshipContainer(relSurveyTaker).changeId + "|" + formData.id())', function () {
                                   var selectedEntities = [];
                                   if (scope.formData && scope.relSurveyTaker && scope.relSurveyTaker.id > 0) {
                                       selectedEntities = scope.formData.getRelationship(scope.relSurveyTaker);
                                   }
                                   scope.model.relEntity = _.first(selectedEntities) || null;
                               });
                           }
                       });

                       /////
                       // Validation.
                       /////
                       scope.$on('validateForm', function () {
                           validate();
                       });

                       scope.$on('validateRelationship', function () {
                           validate();
                       });

                       function validate() {
                           if (scope.formData && scope.relSurveyTaker && scope.relSurveyTaker.id > 0) {
                               var relationships = scope.formData.getRelationship(scope.relSurveyTaker);
                               var errorMessages = spFieldValidator.validateFormRelationshipControl(scope.relationshipToRender, scope.isMandatory, relationships);
                               if (errorMessages.length > 0) {
                                   spFieldValidator.raiseValidationErrors(scope, errorMessages);
                               } else {
                                   spFieldValidator.clearValidationErrors(scope);
                               }
                           }
                       }

                       /////
                       // Navigation.
                       /////
                       scope.handleLinkClick = function () {
                           if (scope.model.relEntity) {
                               var params = {};
                               var formId = spEditForm.getControlConsoleFormId(scope.formControl);
                               if (formId && _.isNumber(formId)) {
                                   params.formId = formId;
                               }

                               scope.$emit('addOnReturnFromChildUpdate', function (formScope, formData) {
                                   spEditForm.updateLookupNameOnReturnFromChildUpdate(spNavService.getCurrentItem(), scope.relationshipToRender, false, formData);
                               });

                               spNavService.navigateToChildState('viewForm', scope.model.relEntity.idP, params);
                           }
                       };

                       /////
                       // Selection.
                       /////
                       scope.handleSelect = function(rel) {
                           if (scope.formData && scope.relSurveyTaker && scope.relSurveyTaker.id > 0) {

                               // set the accompanying direction on the selected relationship
                               if (rel) {
                                   var relEntity = rel.getEntity();

                                   if (relEntity) {
                                       var existingValue = scope.formData.getLookup(scope.relSurveyTaker);
                                       if (!existingValue || (existingValue.idP !== relEntity.idP)) {

                                           if (scope.model.targetTypeEntity && relSurveyTakerDirection && relSurveyTakerDirection.id > 0) {
                                               scope.formData.setLookup(relSurveyTakerDirection, !rel.isReverse() ? 'core:forward' : 'core:reverse');
                                           }

                                           scope.formData.setLookup(scope.relSurveyTaker, relEntity);
                                       }
                                   }
                               } else {
                                   scope.formData.setLookup(relSurveyTakerDirection, null);
                                   scope.formData.setLookup(scope.relSurveyTaker, null);
                               }

                               validate();
                           }
                       };

                       /////
                       // Control sizing and placement.
                       /////
                       scope.$on('gather', function (event, callback) {
                           callback(scope.formControl, scope.parentControl, element);
                       });

                       var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spTargetCampaignSurveyTakerPicker/spTargetCampaignSurveyTakerPicker.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());