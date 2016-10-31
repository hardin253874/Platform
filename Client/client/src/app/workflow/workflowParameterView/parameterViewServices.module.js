// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

// TODO: have an issue with promises and the apply/cancel functions being callable multiple times
// TODO: don't use the name "apply" for the callback.... is confusing given function's apply function

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewServices', ['mod.services.workflowService', 'mod.common.viewRegion']);
}());