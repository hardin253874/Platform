// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular*/
angular.module('mod.app.workflow', [
    'sp.workflow.listController',
    'sp.workflow.runController',
    'sp.workflow.input',
    'sp.workflow.entityExplorer',

    'sp.workflow.parameterViewControllers',
    'sp.workflow.parameterViewServices',

    'sp.workflow.directives',

    // activity type specific
    'sp.workflow.activities.workflow',
    'sp.workflow.activities',

]);
