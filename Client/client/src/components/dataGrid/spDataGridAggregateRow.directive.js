// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global $, _, console, angular, sp, spUtils, spEntity */

(function () {
    'use strict';

    angular.module('mod.common.ui.spDataGrid')
        .directive('spDataGridAggregateRow', spDataGridAggregateRowDirective);

    /* @ngInject */
    function spDataGridAggregateRowDirective($compile) {
        return {
            restrict: 'E',
            scope: false,            
            link: link
        };

        function link(scope, iElement) {
            var model = scope.model;            

            function buildAggRowLinkFunc(columns) {
                if (!columns) {
                    return;
                }

                // The row link function is up to date
                if (model.aggregateRowLinkFunc &&
                    model.aggregateRowLinkFunc.__aggColumnChangeId &&
                    model.aggregateRowLinkFunc.__aggColumnChangeId === columns.__aggColumnChangeId) {
                    return;
                }

                var html = '';

                for (var i = 0; i < columns.length; i++) {
                    var moreDatahtml = '';
                    if (i === columns.length - 1) {
                        moreDatahtml = ' <div class="aggregateRowCellTotalCell aggregateLoadMoreDataCell" style="{{getAggregateCellStyle(col)}}" ng-show="isMoreGroupDataAvailable(row)">' +
                            '<a ng-click="aggregateRowLoadDataClick($event, row)"><img src="assets/images/16x16/LoadData.png" />Load more data</a>' +
                        '</div>';
                    }
                    html = html + '<div class="ngCell aggregateRowCell" ng-style="getAggregateColumnStyle(columns, col, row)" sp-data-grid-row-col-scope="' + i + '">' +                         
                        moreDatahtml +
                        '<div ng-if="model.aggregateInfo.showSubTotals">' +
                        '<div class="aggregateRowCellTotalCell" ng-repeat="t in col.colDef.spColumnDefinition.totals | orderBy:\'order\'" style="{{getAggregateCellStyle(col)}}">' +
                        '{{getAggregateOptionLabel(t)}}&nbsp;{{getAggregateCellSubTotalValue(row, col, t)}}</div></div>' +
                        
                        '</div>';
                }

                model.aggregateRowLinkFunc = $compile(html);
                model.aggregateRowLinkFunc.__aggColumnChangeId = columns.__aggColumnChangeId;
            }

            scope.$watchCollection('renderedColumns', function (columns) {
                // The columns have changed. Clear the html and regenerate the columns
                if (!columns || !columns.length) {
                    return;
                }
                
                if (!columns.__aggColumnChangeId) {
                    columns.__aggColumnChangeId = _.uniqueId();
                }
                buildAggRowLinkFunc(columns);
                iElement.empty();
                model.aggregateRowLinkFunc(scope, function (clone) {
                    iElement.append(clone);
                });                
            });
        }
    }
}());