// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

//TODO - simplify that results data structure - is a left over from the old report interface

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewControllers', [
        'mod.services.workflowService',
        'sp.workflow.parameterViewServices',
        'mod.common.viewRegion',
        'mod.common.ui.spContextMenu',
        'app.controls.spSearchControl',
        'sp.common.ui.expressionEditor',
        'mod.common.alerts'
    ]);

}());