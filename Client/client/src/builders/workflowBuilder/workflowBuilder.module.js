// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, sp, spWorkflowConfiguration */

(function () {
    'use strict';

    angular.module('sp.workflow.builder', [
        'ui.router',
        'titleService',
        'mod.common.alerts',
        'sp.navService',
        'mod.services.workflowService',
        'mod.services.workflowConfiguration',
        'mod.common.ui.spEditFormDialog',
        'sp.common.workflow.diagramDirectives',
        'sp.workflow.parameterViewServices'
    ]);
}());
