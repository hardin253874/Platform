// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spSubjectRecordAccessEditor', [
        'mod.common.spMobile',
        'mod.common.spEntityService',
        'mod.app.editFormServices',
        'mod.services.promiseService',
        'mod.common.spCachingCompile',
        'sp.navService'])
           .directive('spSubjectRecordAccessEditor', function ($q, spMobileContext, spEntityService, spEditForm, spPromiseService, spCachingCompile, spNavService) {
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

                       var loaded = false;

                       /////
                       // Control setup.
                       /////
                       scope.model = {};

                       scope.relationshipToRender = null;

                       scope.$watch("formControl", function () {
                           if (scope.formControl) {
                               scope.relationshipToRender = scope.formControl.relationshipToRender;

                               if (!scope.formControl.handlePreSave) {
                                   scope.formControl.handlePreSave = handlePreSave;
                               }
                           }
                       });

                       scope.$watch("formData", function () {
                           if (scope.formData) {
                               scope.canModify = scope.formData.canModify || false;
                               scope.canDelete = scope.formData.canDelete || false;

                               scope.formData.registerRelationship(scope.relationshipToRender.eid());
                               
                               scope.model.accessOptions = {
                                   mode: scope.formMode === spEditForm.formModes.edit ? 'edit' : 'view',
                                   subject: scope.formData,
                                   isInDesign: scope.isInDesign,
                                   containerLoaded: loaded,
                                   removeDeleted: removeDeleted,
                                   insertCreated: insertCreated
                               };
                           }
                       });

                       scope.$watch("formMode", function () {
                           if (scope.model.accessOptions) {
                               if (scope.formMode === spEditForm.formModes.edit) {
                                   scope.model.accessOptions.mode = 'edit';
                                   scope.$broadcast('accessAction', { action: 'edit' });
                               } else {
                                   scope.model.accessOptions.mode = 'view';
                                   scope.$broadcast('accessAction', { action: 'cancel' });
                               }
                           }
                       });

                       function handlePreSave(entity) {
                           spNavService.middleLayoutBusy = true;

                           scope.$broadcast('accessAction', { action: 'save' });
                           
                           return spPromiseService.poll(function () { return $q.when(scope.model.accessOptions.mode); }, function (result) { return result === 'view'; }, 100, 200).finally(function () {
                               spNavService.middleLayoutBusy = false;

                               if (scope.model.accessOptions.mode === 'edit') {
                                   console.error('spSubjectRecordAccessEditor.handlePreSave: accessOptions did not resolve in time.');

                                   throw new Error('Unexpected error saving record access.');
                               }
                           });
                       }

                       function removeDeleted(entity) {
                           var relId = { id: scope.relationshipToRender.eid(), isReverse: false };
                           var rels = scope.formData.getRelationship(relId);

                           if (rels) {
                               if (scope.formMode !== spEditForm.formModes.edit) {
                                   spNavService.middleLayoutBusy = true;

                                   _.remove(rels.getRelationshipContainer().instances, function(ri) {
                                       if (!ri || !ri.entity) {
                                           return false;
                                       }

                                       return entity && (entity.idP === ri.entity.idP);
                                   });

                                   return spEditForm.saveFormData(scope.formData).finally(function() {
                                       spNavService.middleLayoutBusy = false;
                                   });
                               } else {
                                   rels.getRelationshipContainer().deleteEntity(entity);
                               }
                           }

                           return $q.when();
                       }

                       function insertCreated(entity) {
                           var relId = { id: scope.relationshipToRender.eid(), isReverse: false };
                           var rels = scope.formData.getRelationship(relId);

                           if (rels) {
                               rels.getRelationshipContainer().add(entity);
                           }

                           return $q.when();
                       }

                       /////
                       // Control sizing and placement.
                       /////
                       scope.$on('gather', function (event, callback) {
                           callback(scope.formControl, scope.parentControl, element);
                       });

                       scope.$on('measureArrangeComplete', function(event) {
                           if (scope.formControl) {
                               loaded = true;
                               if (scope.model.accessOptions) {
                                   scope.model.accessOptions.containerLoaded = loaded;
                               }
                           }
                       });

                       var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spSubjectRecordAccessEditor/spSubjectRecordAccessEditor.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());