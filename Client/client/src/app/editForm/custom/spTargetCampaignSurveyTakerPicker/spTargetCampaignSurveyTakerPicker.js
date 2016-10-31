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
                           options: [],
                           selectedOption: null,
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
                       var personTypeIds = [];
                       var relationshipList = [];

                       function getPersonTypesPromise() {
                           var promise = $q.when(personTypeIds);

                           if (!personTypeIds || personTypeIds.length === 0) {
                               promise = spEntityService.getEntity('core:person', 'name, derivedTypes*.{name}').then(function (personType) {
                                   var types = spUtils.walkGraph(function (t) { return t.derivedTypes; }, personType);
                                   personTypeIds = _.map(types, 'idP');
                                   return personTypeIds;
                               });
                           }

                           return promise;
                       }

                       function getOptionsPromise(entities, pids) {
                           var promises = [];

                           if (!pids) {
                               promises.push(getPersonTypesPromise().then(function(result) {
                                   pids = result;
                               }));
                           }

                           var extra = {};

                           _.each(entities, function (e) {
                               if (e && !sp.result(e, 'toType.idP')) {
                                   promises.push(spEntityService.getEntity(e.idP, 'fromName, fromType.{name}, toName, toType.{name}').then(function (rel) {
                                       if (rel) {
                                           _.set(extra, e.idP, {
                                               fromName: rel.fromName,
                                               toName: rel.toName,
                                               from: sp.result(rel, 'fromType.idP'),
                                               to: sp.result(rel, 'toType.idP')
                                           });
                                       }
                                   }));
                               }
                           });

                           if (!promises.length) {
                               promises.push($q.when());
                           }

                           return $q.all(promises).then(function () {
                               return _.map(entities, function (e) {
                                   var to = sp.result(e, 'toType.idP') || _.get(extra, e.idP + '.to');
                                   var forward = _.includes(pids, to);
                                   var displayName = (forward ? (e.toName || _.get(extra, e.idP + '.toName')) : (e.fromName || _.get(extra, e.idP + '.fromName'))) || e.name;
                                   return {
                                       id: e.idP,
                                       name: displayName,
                                       from: sp.result(e, 'fromType.idP') || _.get(extra, e.idP + '.from'),
                                       to: to,
                                       forward: forward
                                   };
                               });
                           });
                       }

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
                                   if (scope.formData && scope.relSurveyTaker && scope.relSurveyTaker.id > 0) {
                                       getOptionsPromise(scope.formData.getRelationship(scope.relSurveyTaker)).then(function (selectedOptions) {
                                           scope.model.selectedOption = _.first(selectedOptions) || null;
                                       });
                                   }
                               });

                               /////
                               // When the related target type value changes, then update the contents of the available relationships.
                               /////
                               scope.$watch('model.targetTypeEntity', function () {
                                   scope.model.options.length = 0;

                                   if (scope.model.targetTypeEntity) {

                                       /////
                                       // Store and pass on the 'person' type's id.
                                       /////
                                       getPersonTypesPromise().then(function(pids) {
                                           spEntityService.getEntity(scope.model.targetTypeEntity.idP, 'name, inherits*.{name}').then(function(targetType) {
                                               var types = spUtils.walkGraph(function(t) { return t.inherits; }, targetType);
                                               var tids = _.map(types, 'idP');

                                               var contextExpression = '';
                                               _.forEach(tids, function (tid) {
                                                   _.forEach(pids, function (pid) {
                                                       contextExpression += '(id([From type])=' + tid + ' and id([To type])=' + pid + ') or (id([To type])=' + tid + ' and id([From type])=' + pid + ') or ';
                                                   });
                                               });

                                               contextExpression = _.trimEnd(contextExpression, ' or ');
                                               if (!contextExpression || !contextExpression.length) {
                                                   contextExpression = 'false';
                                               }
                                               
                                               spEntityService.getEntitiesOfType('core:relationship', 'name,fromType.{name},toType.{name},cardinality.{name,alias},toName,toScriptName,fromName,fromScriptName', { filter: contextExpression })
                                                   .then(function (results) {
                                                       relationshipList = results;
                                                       return getOptionsPromise(results, pids).then(function(options) {
                                                           scope.model.options = options;
                                                       });
                                                   });
                                           });
                                       });
                                   }
                               });

                               /////
                               // When a relationship is selected, update the form with the id and the direction.
                               /////
                               scope.$watch('model.selectedOption', function (value) {
                                   if (scope.formData && scope.relSurveyTaker && scope.relSurveyTaker.id > 0) {
                                       
                                       // set the accompanying direction on the selected relationship
                                       if (value) {
                                           
                                           var existingValue = scope.formData.getLookup(scope.relSurveyTaker);
                                           if (!existingValue || (existingValue.idP !== value.id)) {

                                               if (scope.model.targetTypeEntity && relSurveyTakerDirection && relSurveyTakerDirection.id > 0) {
                                                   scope.formData.setLookup(relSurveyTakerDirection, value.forward ? 'core:forward' : 'core:reverse');
                                               }

                                               scope.formData.setLookup(scope.relSurveyTaker, _.find(relationshipList, { 'idP': value.id }));
                                           }

                                       } else {
                                           scope.formData.setLookup(relSurveyTakerDirection, null);
                                           scope.formData.setLookup(scope.relSurveyTaker, null);
                                       }

                                       validate();
                                   }
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
                           if (scope.model.selectedOption && scope.model.selectedOption.id > 0) {
                               var params = {};
                               var formId = spEditForm.getControlConsoleFormId(scope.formControl);
                               if (formId && _.isNumber(formId)) {
                                   params.formId = formId;
                               }

                               scope.$emit('addOnReturnFromChildUpdate', function (formScope, formData) {
                                   spEditForm.updateLookupNameOnReturnFromChildUpdate(spNavService.getCurrentItem(), scope.relationshipToRender, false, formData);
                               });

                               spNavService.navigateToChildState('viewForm', scope.model.selectedOption.id, params);
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