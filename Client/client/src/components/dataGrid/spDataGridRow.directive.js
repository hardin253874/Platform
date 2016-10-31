// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global $, _, console, angular, sp, spUtils, spEntity */

(function () {
    'use strict';

    angular.module('mod.common.ui.spDataGrid')
        .directive('spDataGridRow', spDataGridRowDirective);

    /* @ngInject */
    function spDataGridRowDirective($compile) {
        return {
            restrict: 'E',
            scope: false,
            link: link
        };

        function link(scope, iElement) {
            var model = scope.model;

            function buildRowLinkFunc(columns) {
                if (!columns) {
                    return;
                }

                // The row link function is up to date
                if (model.rowLinkFunc &&
                    model.rowLinkFunc.__columnChangeId &&
                    model.rowLinkFunc.__columnChangeId === columns.__columnChangeId) {
                    return;
                }

                var htmlView = ['<div ng-if="!getIsInlineEditing(row)" class="rn-row-wrapper rn-inline-{{dataGridOptions.getInlineEditingState(row.entity.rowIndex)}}">'];
                var htmlEdit = ['<div ng-if="getIsInlineEditing(row)" class="rn-row-wrapper rn-inline-{{dataGridOptions.getInlineEditingState(row.entity.rowIndex)}}">'];                

                for (var i = 0; i < columns.length; i++) {
                    var col = columns[i];
                    var inlineEditingCellTemplate = (col.colDef && col.colDef.inlineEditingCellTemplate) || col.cellTemplate;

                    htmlView.push(
                        '<div sp-right-click="rowRightClick($event, row)" class="ngCell ' + col.colIndex() + '" ng-click="dataGridCellClick(col)"',
                        ' sp-data-grid-row-col-scope="' + i + '">',
                        col.cellTemplate,
                        '</div>'
                    );
                    
                    htmlEdit.push(
                        '<div sp-right-click="rowRightClick($event, row)" class="ngCell ' + col.colIndex() + '"',
                        ' sp-data-grid-row-col-scope="' + i + '">',
                        inlineEditingCellTemplate,
                        '</div>');                    

                }
                htmlView.push('</div>');
                htmlEdit.push('</div>');

                var html = htmlView.join('') + htmlEdit.join('');

                model.rowLinkFunc = $compile(html);
                model.rowLinkFunc.__columnChangeId = columns.__columnChangeId;
            }

            scope.$watchCollection('renderedColumns', function (columns) {
                // The columns have changed. Clear the html and regenerate the columns
                if (!columns || !columns.length) {
                    return;
                }

                if (!columns.__columnChangeId) {
                    columns.__columnChangeId = _.uniqueId();
                }
                buildRowLinkFunc(columns);
                iElement.empty();
                model.rowLinkFunc(scope, function (clone) {
                    iElement.append(clone);
                });
            });
        }
    }
}());