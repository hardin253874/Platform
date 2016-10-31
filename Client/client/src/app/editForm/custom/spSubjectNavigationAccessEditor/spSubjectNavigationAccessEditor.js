// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spSubjectNavigationAccessEditor', [
        'mod.common.spMobile',
        'mod.common.spEntityService',
        'mod.app.editFormServices',
        'mod.services.promiseService',
        'mod.common.spCachingCompile',
        'sp.navService'])
           .directive('spSubjectNavigationAccessEditor', function ($q, spMobileContext, spEntityService, spEditForm, spPromiseService, spCachingCompile, spNavService) {
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
                           allowDisplayOptions: {
                               mode: 'view',
                               subject: null,
                               hideSubjectSelection: true,
                               isInDesign: scope.isInDesign,
                               isReadOnly: scope.isReadOnly
                           }
                       };

                       scope.relationshipToRender = null;

                       scope.$watch("formControl", function() {
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

                               scope.formData.registerRelationship('core:allowDisplay');

                               scope.model.allowDisplayOptions.subject = scope.formData;

                               if (scope.formMode === spEditForm.formModes.edit) {
                                   scope.model.allowDisplayOptions.mode = 'edit';
                               }
                           }
                       });

                       scope.$watch("formMode", function() {
                           if (scope.formMode === spEditForm.formModes.edit) {
                               scope.model.allowDisplayOptions.mode = 'edit';
                               scope.$broadcast('allowDisplayAction', { action: 'edit' });
                           } else {
                               scope.model.allowDisplayOptions.mode = 'view';
                               scope.$broadcast('allowDisplayAction', { action: 'cancel' });
                           }
                       });

                       function handlePreSave(entity) {
                           spNavService.middleLayoutBusy = true;

                           scope.$broadcast('allowDisplayAction', { action: 'save' });

                           return spPromiseService.poll(function () { return $q.when(scope.model.allowDisplayOptions.mode); }, function (result) { return result === 'view'; }, 100, 200).finally(function () {
                               spNavService.middleLayoutBusy = false;

                               if (scope.model.allowDisplayOptions.mode === 'edit') {
                                   console.error('spSubjectNavigationAccessEditor.handlePreSave: allowDisplayAction did not resolve in time.');

                                   throw new Error('Unexpected error saving navigation access.');
                               }
                           });
                       }
                       
                       /////
                       // Control sizing and placement.
                       /////
                       scope.$on('gather', function (event, callback) {
                           callback(scope.formControl, scope.parentControl, element);
                       });

                       var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spSubjectNavigationAccessEditor/spSubjectNavigationAccessEditor.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());