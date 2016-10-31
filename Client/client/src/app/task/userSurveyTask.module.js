// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask', [
        'mod.common.spEntityService', 'sp.navService', 'mod.common.alerts',
        'app.controls.inputFilters.spIntegerInput', 'app.controls.inputFilters.spFloatingPointInput',
        'sp.common.directives', 'mod.common.ui.spDialogService', 'sp.common.fileUpload', 'mod.common.spXsrf',
        'mod.common.ui.spEntityRadioPicker', 'app.editForm.spInlineRelationshipPicker', 'mod.common.ui.spEntityCheckBoxPicker']);

    angular.module('mod.app.userSurveyTask')
        .config(function ($stateProvider) {

            $stateProvider.state('userSurveyTask', {
                url: '/{tenant}/{eid}/userSurveyTask?path',
                templateUrl: 'task/userSurveyTask.tpl.html',
                data: { showBreadcrumb: false }
            });
        });

}());