// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing report constants.    
    * It contains the following constants:
    * <ul>
    *   <li>groupValueColumnIdPrefix - the group value column id prefix</li>
    *   <li>reportPageSize - default is 1000</li>    
    *   <li>reportViews - available report views</li>    
    *   <li>reportRollupSize - report rollup data limit size</li>
    *   <li>matrixHeaders - available matrix headers</li>    
    *   <li>chartTypes - available chart tyoes</li>    
    *   <li>contextMenu - context menu definition</li>    
    *   <li>aggregateRowContextMenu - context menu definition for aggregate rows</li>    
    * </ul>

    * @module spReportConstants                    
    */
    angular.module('mod.ui.spReportConstants', [])
        .constant('groupValueColumnIdPrefix', 'GroupValue_')
        .constant('reportPageSize', {
            value: 200
        })
        .constant('reportMobilePageSize', {
            value: 20
        })
        .constant('reportMobileMaxCols', {
            value: 3
        })
        .constant('screenReportMobilePageSize', {
            value: 7    // 6 rows (to display) + 1 (to check if more data is available)
        })
        .constant('reportRollupSize', {
            value: 1000
        })
        .constant('contextMenu', {
            menuItems: [
                {
                    text: 'Sort A-Z',
                    icon: 'assets/images/16x16/SortAscending.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'sortAscending\', col)'                    
                },
                {
                    text: 'Sort Z-A',
                    icon: 'assets/images/16x16/SortDescending.png',                    
                    type: 'click',
                    click: 'onContextMenuAction(\'sortDescending\', col)'
                },
                {
                    text: 'Cancel Sort',
                    icon: 'assets/images/16x16/SortCanceling.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'cancelSort\', col)',
                    disabled: 'isContextMenuActionDisabled(\'cancelSort\', col)'
                },
                {
                    text: 'Sort Options',
                    icon: 'assets/images/16x16/SortOption.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'sortOptions\', col)'
                },
                {
                    type: 'divider',
                    hidden: 'isContextMenuActionHidden(\'groupByDivider\', col)'
                },
                {
                    text: 'Group By',
                    icon: 'assets/images/16x16/GroupBy.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'groupBy\', col)',
                    hidden: 'isContextMenuActionHidden(\'groupBy\', col)'
                },
                {
                    text: 'Summarise',
                    icon: 'assets/images/16x16/SummariseDataGrid.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'summarise\', col)',
                    hidden: 'isContextMenuActionHidden(\'summarise\', col)'
                },
                {
                    type: 'divider',
                    hidden: 'isContextMenuActionHidden(\'aggregateDivider\', col)'
                },
                {
                    text: 'Rename Column',
                    icon: 'assets/images/16x16/RenameColumn.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'renameColumn\', col)',
                    hidden: 'isContextMenuActionHidden(\'renameColumn\', col)'
                },
                {
                    text: 'Format Column',
                    icon: 'assets/images/16x16/Format.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'formatColumn\', col)',
                    hidden: 'isContextMenuActionHidden(\'formatColumn\', col)'
                },
                {
                    text: 'Remove Column',
                    icon: 'assets/images/16x16/Delete.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'removeColumn\', col)',
                    hidden: 'isContextMenuActionHidden(\'removeColumn\', col)'
                },
                {
                    text: 'Show Totals',
                    icon: 'assets/images/16x16/ShowTotal.png',
                    type: 'click',
                    click: 'onContextMenuAction(\'showTotals\', col)',
                    hidden: 'isContextMenuActionHidden(\'showTotals\', col)'
                },
                {
                    text: 'Edit Calculation',
                    type: 'click',
                    click: 'onContextMenuAction(\'editCalculation\', col)',
                    hidden: 'isContextMenuActionHidden(\'editCalculation\', col)'
                }
            ]
        })
        .constant('aggregateRowContextMenu', {
            menuItems: [
            {
                text: 'Sort A-Z',
                icon: 'assets/images/16x16/SortAscending.png',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'sortAscending\', row)'
            },
            {
                text: 'Sort Z-A',
                icon: 'assets/images/16x16/SortDescending.png',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'sortDescending\', row)'
            },            
            {
                text: 'Sort Options',
                icon: 'assets/images/16x16/SortOption.png',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'sortOptions\', row)'
            },
            {
                type: 'divider',
                hidden: 'isAggregateContextMenuActionHidden(\'groupByDivider\')'
            },
            {
                text: 'Cancel Group By',
                icon: 'assets/images/16x16/GroupBy.png',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'cancelGroupBy\', row)',
                hidden: 'isAggregateContextMenuActionHidden(\'cancelGroupBy\')'
            },
            {
                type: 'divider',
                hidden: 'isAggregateContextMenuActionHidden(\'groupByDivider\')'
            },            
            {
                text: 'Hide Label',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'hideLabel\')',
                hidden: 'isAggregateContextMenuActionHidden(\'hideLabel\')'
            },
            {
                text: 'Show Label',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'showLabel\')',
                hidden: 'isAggregateContextMenuActionHidden(\'showLabel\')'
            },
            {
                text: 'Hide Count',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'hideCount\')',
                hidden: 'isAggregateContextMenuActionHidden(\'hideCount\')'
            },
            {
                text: 'Show Count',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'showCount\')',
                hidden: 'isAggregateContextMenuActionHidden(\'showCount\')'
            },
            {
                type: 'divider'              
            },
            {
                text: 'Expand All',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'expandAll\', row)'
            },
            {
                text: 'Collapse All',
                type: 'click',
                click: 'onAggregateContextMenuAction(\'collapseAll\', row)'
            }
        ]
    });
}());