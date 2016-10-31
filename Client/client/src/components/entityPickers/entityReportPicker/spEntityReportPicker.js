// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing an entity report picker.
    * Displays a given report. This directive uses 'spReport' directive.
    * 
    * @module spEntityReportPicker
    * @example

    Using the spEntityReportPicker:
    
    This directive uses 'spReport' directive. Pls refer to 'spReport' directive for the valid options.
           
    Options currently used by this directive are:
        - reportId - {number} - the report id.
        - multiSelect - {boolean}. True to select many items, false otherwise.
        - selectedItems - {array of object} - the array of objects selected from report.
        - entityTypeId - {number} - the report will be constrained by the given entityTypeId. e.g. the report could be a 'template report' but it can be constrained to display instances of only 'employee' type.
    * 
    */
    angular.module('mod.common.ui.spEntityReportPicker', ['mod.common.spEntityService', 'mod.common.spMobile'])
        .directive('spEntityReportPicker', function ($rootScope, $timeout, spEntityService, spNgUtils,
                                                     spMobileContext, spInlineEditService) {

            var extension = spMobileContext.isMobile ? 'Mobile' : '';
              
            return {
                restrict: 'E',
                templateUrl: 'entityPickers/entityReportPicker/spEntityReportPicker' + extension + '.tpl.html',
                scope: {
                    options: '='
                },
                link: function (scope, iElement, iAttrs) {

                    scope.isMobile = spMobileContext.isMobile;
                    scope.isTablet = spMobileContext.isTablet;

                    scope.showButtons = function () {
                        // We don't show picker buttons if there are any inline editing sessions
                        // anywhere. The getActiveSessions method will always return an array.
                        return spInlineEditService.getActiveSessions().length === 0;
                    };

                    scope.$watch('options.entityTypeId', function (newVal) {

                        $timeout(function () {
                            // set focus on first visible input if its not the mobile/tablet device
                            if (!scope.isMobile && !scope.isTablet) {
                                spNgUtils.setFocusOnFirstVisibleInputInElement(iElement);
                            }
                        }, 0);

                        if (scope.options.objectName)
                            return;

                        if(newVal && _.isNumber(newVal) && newVal > 0) {
                            spEntityService.getEntity(newVal, 'name').then(function (entity) {
                                if (entity) {

                                    scope.options.objectName = entity.name === 'Definition' ? 'Object' : entity.name;
                                }
                            });
                        }
                    });

                    // ** Note: the following code is a temp implementation of new buttons untill the context menu/action item functionality is finalized.
                    // this code is currently used in 'spInlineRelationshipController.js' to handle create from the report picker
                    scope.handleCreate = function (entityId) {
                        // keep modal open if can't create related resource else close the modal 
                        if (scope.options.cancelDialog && !scope.options.disallowCreateRelatedEntityInNewMode) {
                            scope.options.cancelDialog();
                        }
                        
                        if (scope.options.newButtonInfo && scope.options.newButtonInfo.handleCreate) {
                            scope.options.newButtonInfo.handleCreate(entityId);
                        }
                    };
                    
                    scope.singleOptionClick = function () {
                        // keep modal open if can't create related resource else close the modal 
                        if (scope.options.cancelDialog && !scope.options.disallowCreateRelatedEntityInNewMode) {
                            scope.options.cancelDialog();
                        }
                        
                        if (scope.options.newButtonInfo && scope.options.newButtonInfo.singleOptionClick) {
                            scope.options.newButtonInfo.singleOptionClick();
                        }
                    };
                }
            };
        });
}());