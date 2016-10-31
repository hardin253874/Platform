// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, Globalize, console, sp, rnBoard */

//todo - dnd reorder based on a rank field (or maybe also order or ordinal)

(function () {
    'use strict';

    angular.module('mod.app.board').directive('rnBoard', rnBoardDirective);

    /* @ngInject */
    function rnBoardDirective() {

        return {
            scope: {
                boardId: '=',
                analyser: '=',
				iconInfo: '='
            },
            controller: BoardViewController,
            controllerAs: 'board',
            bindToController: true,
            templateUrl: 'board/templates/rnBoard.tpl.html',
            link: link
        };

        function link(scope, elem, attrs) {
            scope.board.toggleFullScreen = () => {
                requestFullScreen(_.first(elem.get(0).getElementsByClassName('board')));
            };
        }

        function requestFullScreen(elem) {
            if (!elem) return;

            var f = elem.requestFullScreen ||
                elem.mozRequestFullScreen ||
                elem.webkitRequestFullscreen ||
                elem.msRequestFullscreen;

            f.call(elem);
        }
    }

    /* @ngInject */
    function BoardViewController($scope, $window, $element, $q, $templateCache, rnBoardService, spNavService, spNavigationBuilderProvider, spActionsService, spContextMenuService, spMobileContext, spAlertsService, consoleIconService, titleService, spState) {

        let navigationBuilderProvider = spNavigationBuilderProvider($scope);
        let makeId = sp.coerseToNumberOrLeaveAlone;
        let defaultCustomTemplate = $templateCache.get('board/cardTemplates/exampleCustomCard.tpl.html');
        let defaultTemplateName = 'story';
        let nullDimension = {values: [{value: ''}]};

        let vm = this;
        
        let applyFilter = _.debounce(function() {
            safeApply($scope, function() {
                vm.cards = getCardValues();
                vm.legend = getLegendValues();
            });
        }, 300);

        vm.showQuickAddInputId = _.uniqueId('showQuickAddInputId');

        vm.colWidthCss = null;

        vm.title = '';
        vm.overflowMessage = '';
        vm.model = null;
        vm.analyserParams = spState.params.q;
        vm.customTemplate = defaultCustomTemplate;
        vm.cardTemplates = [
            { key: 'all columns', displayName: 'All values & labels', template: $templateCache.get('board/cardTemplates/allColsCard.tpl.html') },
            { key: 'all columns - values only', displayName: 'All values only', template: $templateCache.get('board/cardTemplates/allColsValsOnlyCard.tpl.html') },
            { key: 'story - brief', displayName: 'Name only', template: $templateCache.get('board/cardTemplates/storyCardBrief.tpl.html') },
            { key: 'story', displayName: 'Name & description', template: $templateCache.get('board/cardTemplates/storyCard.tpl.html') },
            { key: 'approval task', displayName: 'Approvals', template: $templateCache.get('board/cardTemplates/approvalTaskCard.tpl.html') },
            { key: 'basic - no id', displayName: '', template: $templateCache.get('board/cardTemplates/taskCardNoId.tpl.html'), hidden: true },
            { key: 'timeline', displayName: '', template: $templateCache.get('board/cardTemplates/timelineCard.tpl.html'), hidden: true },
            { key: 'custom', displayName: 'Custom', template: vm.customTemplate, hidden: true }
        ];

        vm.cardTemplateName = 'story';

        vm.dropOptions = {
            supportTouchEvents: true,
            onAllowDrop: onAllowDrop,
            onDrop: onDrop
        };
        vm.dragOptions = {
            supportTouchEvents: true
        };

        vm.reportPickerOptions = {
            pickerReportId: 'console:reportsReport',
            entityTypeId: 'core:report',
            reportOptions: {}
        };
        vm.childReportPickerOptions = {
            pickerReportId: 'console:reportsReport',
            entityTypeId: 'core:report',
            reportOptions: {}
        };
        vm.childBoardPickerOptions = {
            pickerReportId: 'core:boardsReport',
            entityTypeId: 'core:board',
            reportOptions: {}
        };

        vm.isAdmin = () => rnBoardService.isAdmin();
        vm.getConfigStyleClass = () => rnBoardService.getConfigStyleClass();
        vm.viewRecordHref = id => spNavService.getChildHref('editForm', id, {});
        vm.getLegendStyle = getLegendStyle;
        vm.getColumnStyle = getColumnStyle;
        vm.showHeaders = showHeaders;
        vm.showLegend = showLegend;
        vm.onShowColDimChanged = onShowColDimChanged;
        vm.onShowRowDimChanged = onShowRowDimChanged;

        vm.addItem = addItem;
        vm.drilldown = drilldown;
        vm.refresh = () => requestModel(vm.boardId, vm.model);
        vm.save = saveBoard;
        vm.configContextMenu = {};
        vm.canShowConfigMenu = false;
        vm.cardContextMenu = {};
        vm.cardSelected = {};
        vm.cardRightClick = cardRightClick;
        vm.contextMenuIsOpen = false;
        vm.getContextMenuActions = getContextMenuActions;
        vm.getActionExecutionContext = getActionExecutionContext;
        vm.executeAction = executeAction;
        vm.search = {
            onSearchValueChanged: function() {
                vm.cardFilter = vm.search.value || '';
                applyFilter();
            },
            value: ''
        };
        vm.isMobile = spMobileContext.isMobile;
        vm.isTablet = spMobileContext.isTablet;
        vm.busyIndicator = {
            type: 'spinner',
            text: 'Loading...',
            placement: 'element',
            isBusy: true
        };

        let scroller;
        let headers;
        vm.setupScrolling = setupScrolling;

        vm.colValues = [];
        vm.rowValues = [];
        vm.styleValues = [];
        vm.cards = {};
        vm.legend = {};

        // changes due to user actions
        $scope.$watch('board.boardId', requestModel);
        $scope.$watch('board.colDimOrd', onColumnDimensionChanged);
        $scope.$watch('board.rowDimOrd', onRowDimensionChanged);
        $scope.$watch('board.styleDimOrd', onStyleDimensionChanged);
        $scope.$watch('board.cardTemplateName', onCardTemplateNameChanged);
        
        // changes due to model updates
        $scope.$watch('board.colValues.length', onColValuesChanged);

        // configure menu
        $scope.nav = spNavService;
        $scope.$watch('nav.isFullEditMode', function() {
            if (vm.model) {
                vm.configContextMenu = navigationBuilderProvider.buildConfigureContextMenu(vm.model.board);
                vm.canShowConfigMenu = spNavService.canShowConfigMenu(vm.model.board);                
            }
        });
        $scope.configMenuUpdateEntityProperties = function () { navigationBuilderProvider.configureNavItem(vm.model.board); };
        $scope.configMenuDeleteEntity = function () { navigationBuilderProvider.removeNavItem(vm.model.board); };

        init();

        // only functions after here
        let getCardValuesDebounced = _.debounce(function() {
            safeApply($scope, function() {
                vm.cards = getCardValues();
            });
        }, 100);

        let getLegendValuesDebounced = _.debounce(function() {
            safeApply($scope, function() {
                vm.legend = getLegendValues();
            });
        }, 100);

        function init() {
            vm.cardFilter = '';
            updateCardTemplateDetails();
            onDimensionsUpdated();
        }
        
        function getLegendStyle(s, board) {
            var style = {};

            if (!s && !board) {
                return style;
            }

            if (s){
                style['background-color'] = s.colour;
            }

            var fudge = 0;
            if (board) {
                var length = board.styleValues.length;
                var w = 100 / length;
                if (w > 10) {
                    w = 10;
                }
                if (length > 9) {
                    fudge = length / 10; // leave some leading space
                }
            
                style['min-width'] = (w - fudge) + '%';
            }  
            
            return style;
        }

        function getColumnStyle(col, board, isHeader) {
            var style = {};

            if (!col && !board) {
                return style;
            }

            var cols = _.filter(board.colValues, 'show');

            if (!vm.colWidthCss && vm.model && cols.length) {
                vm.colWidthCss = 100 / cols.length + '%';
            }

            var hasVerticalScroll = false;
            if (vm.hasVerticalScroll) {
                hasVerticalScroll = vm.hasVerticalScroll();
            }

            if (board && vm.colWidthCss) {
                if (!vm.isMobile && !vm.isTablet && (col === _.last(cols)) && (isHeader || !hasVerticalScroll)) {
                    style['width'] = 'calc(' + vm.colWidthCss + ' + 17px)'; // scrollbar allowance
                } else {
                    style['width'] = vm.colWidthCss;
                }
            }

            return style;
        }
        
        function drilldown(item) {
            var targetBoardId = sp.result(vm.model, 'board.drilldownTargetBoard.idP');
            var rootTypeId = rnBoardService.getRootTypeId(vm.model);
            if (targetBoardId && rootTypeId) {
                spNavService.navigateToSibling('board', targetBoardId, {q: rootTypeId + '|' + item.eid});
            }
        }
        
        function cardRightClick(event, item) {
            if (!vm.isMobile && !vm.isTablet) {
                vm.cardSelected = item;
            }
        }
        
        function getContextMenuActions() {
            return getCardActions().then(function(actions) {
                return spContextMenuService.getItemsFromActions(actions, 'contextmenu');
            }).finally(function() {
                vm.cardSelected = {};
            });
        }

        function getActionExecutionContext(action, ids) {
            return {
                scope: $scope,
                state: action.state,
                selectionEntityIds: ids,
                isEditMode: false, // board has no edit mode
                refreshDataCallback: vm && vm.refresh
            };
        }

        function getCardActions(item) {
            var entity = item;
            if (!entity || !entity.eid) {
                entity = vm.cardSelected;
            }

            if (entity && entity.eid) {
                let reportId = sp.result(vm, 'model.board.boardReport.idP');
                
                if (reportId && reportId > 0) {
                    let actionRequest = {
                        ids: [entity.eid],
                        lastId: entity.eid,
                        reportId: reportId,
                        hostIds: [],
                        hostTypeIds: [],
                        data: {},
                        display: 'contextmenu'
                    };

                    return spActionsService.getConsoleActions(actionRequest).then(function(response) {
                        if (response && response.actions && response.actions.length) {
                            return response.actions;
                        }
                        return [];
                    });
                }
            }

            return $q.when(null);
        }

        function executeAction(item) {
            getCardActions(item).then(function (actions) {

                if (actions && actions.length > 0) {
                    let action = _.first(actions);

                    if (action) {
                        let ctx = getActionExecutionContext(action, [item.eid]);
                        
                        spActionsService.executeAction(action, ctx);
                    }
                }
            });
        }
        
        function onCardTemplateNameChanged(name, prev) {
            if (name && name !== prev) {
                vm.cards = {};
                return rnBoardService.updateCardTemplate(vm.model, vm.cardTemplateName, vm.customTemplate)
                    .then(modelUpdated)
                    .finally(getCardValuesDebounced);
            }
            return $q.when();
        }

        // changed in the settings popup
        function onShowColDimChanged(col) {
            if (col) {
                if (col.show === false) {
                    _.unset(vm.cards, '[' + col.rid + ']');
                } else {
                    prepareCardValues(vm.cards, col);
                }

                onColValuesChanged();
            }
        }

        function onShowRowDimChanged(row) {
            if (row) {
                _.forEach(vm.colValues, function(col) {
                    if (!col || (col.show === false)) return;
                    let path = '[' + col.rid + '].count';
                    let count = sp.result(vm.cards, path) || 0;
                    if (row.show === false) {
                        let subpath = '[' + col.rid + '][' + row.rid + ']';
                        let subcount = sp.result(vm.cards, subpath + '.count') || 0;
                        count -= subcount;
                        _.unset(vm.cards, subpath);
                    } else {
                        count = prepareCardRowValues(vm.cards, col, count, row);
                    }
                    _.setWith(vm.cards, path, count);
                });
            }
        }

        function showLegend() {
            return vm.styleValues.length && _.some(vm.styleValues, 'rid');
        }

        function showHeaders() {
            return vm.colValues.length && _.some(vm.colValues, 'rid');
        }

        function saveBoard() {
            // shouldn't be doing this here....
            // assuming this isn't saving the board itself
            rnBoardService.updateCardTemplate(vm.model, vm.cardTemplateName, vm.customTemplate);

            return rnBoardService.saveBoard(vm.model)
                .then(model => {
                    spAlertsService.addAlert('Saved board: ' + vm.model.board.name, {expires: 3});
                    return model;
                }, error => {
                    spAlertsService.addAlert(formatSaveErrorMessage(error, 'Save failed'), { severity: spAlertsService.sev.Error });
                });
        }

        function addItem() {
            console.log(`create item for "${vm.newItemExpression}"`);

            if (!vm.newItemExpression) return;

            rnBoardService.quickAddEntity(vm.model, vm.newItemExpression).then(() => {
                vm.newItemExpression = '';
                requestModel(vm.boardId, vm.model);
            });
        }

        function dimensionForOrd(ord) {
            return vm.model && _.find(vm.model.dimensions, d => d.ord === ord);
        }

        function requestModel(boardId, oldModel) {
            console.assert(boardId);

            if (vm.requestPending) return;

            let options = {
                conds: parseConditionString(vm.analyser),
                oldModel: oldModel
            };

            vm.requestPending = true;
            vm.busyIndicator.isBusy = true;
            rnBoardService.requestModel(boardId, options).then(model => {
                try {
                    modelUpdated(model);
                    vm.colValues = getDimensionValues('colDimension');
                    vm.rowValues = getDimensionValues('rowDimension');
                    vm.styleValues = getDimensionValues('styleDimension');
                    getCardValuesDebounced();
                    getLegendValuesDebounced();
                } finally {
                    vm.requestPending = false;
                    vm.busyIndicator.isBusy = false;
                }
            });
        }

        function modelUpdated(model) {
            titleService.setTitle('Board');

            vm.model = model;
            vm.title = model.board.name;
            vm.reportPickerOptions.selectedEntities = _.compact([sp.result(vm, 'model.board.boardReport')]);
            vm.childReportPickerOptions.selectedEntities = _.compact([sp.result(vm, 'model.board.boardChildReport')]);
            vm.childBoardPickerOptions.selectedEntities = _.compact([sp.result(vm, 'model.board.drilldownTargetBoard')]);
            vm.configContextMenu = navigationBuilderProvider.buildConfigureContextMenu(model.board);
            vm.canShowConfigMenu = spNavService.canShowConfigMenu(model.board);

            updateCardTemplateDetails();
            onDimensionsUpdated();
			updateConsoleIconDetails();
            updateShowQuickAdd();
            updateOverflowMessage();

            return model;
        }

        function updateOverflowMessage() {
            vm.overflowMessage = '';
            const {cardCount, maxCards} = rnBoardService.getBoardStats(vm.model);
            if (cardCount >= maxCards) {
                vm.overflowMessage = `Warning: Not all cards may be shown as this board has reached the limit of ${maxCards} cards`;
            }
        }

        function updateShowQuickAdd() {
            vm.showQuickAdd = sp.result(vm, 'model.board.boardShowQuickAdd');
            if (_.isUndefined(vm.showQuickAdd)) vm.showQuickAdd = true;
        }

		function updateConsoleIconDetails() {
		    var board = sp.result(vm, 'model.board'); 
			var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(board);
            if (iconInfo) {
                var iconUrl = iconInfo.iconUrl;
                if (iconUrl) {
				    vm.iconInfo = vm.iconInfo || {};
                    vm.iconInfo.headerIconUrl = iconUrl;
                    vm.iconInfo.headerIconStyle = vm.iconInfo.headerIconStyle || {};
                    if (iconInfo.iconBackgroundColor) {
                        vm.iconInfo.headerIconStyle['background-color'] = sp.getCssColorFromARGBString(iconInfo.iconBackgroundColor);
                    }
                }
            }
        }

        function parseConditionString(conditionString) {
            // expect format typeId|id,...
            // only doing simple relationships right now
            return _((conditionString || '').split(','))
                .map(p => p && p.split('|'))
                .map(p => p && p.length > 1 ? {typeId: makeId(p[0]), id: makeId(p[1])} : null)
                .compact()
                .value();
        }

        function updateCardTemplateDetails() {
            vm.cardTemplateName = sp.result(vm, 'model.board.boardCardTemplateName') || defaultTemplateName;
            vm.customTemplate = sp.result(vm, 'model.board.boardCardCustomTemplate') || defaultCustomTemplate;

            // update the custom template
            var custom = _.find(vm.cardTemplates, { 'key': 'custom' });
            if (custom) {
                custom.template = vm.customTemplate;
            }
            
            // what we render
            var ct = _.find(vm.cardTemplates, { 'key': vm.cardTemplateName });
            if (ct) {
                vm.cardTemplate = ct.template;
            }
        }

        function onDimensionsUpdated() {
            if (!vm.model) return;

            vm.colDimOrd = sp.result(vm, 'model.colDimension.ord');
            vm.rowDimOrd = sp.result(vm, 'model.rowDimension.ord');
            vm.styleDimOrd = sp.result(vm, 'model.styleDimension.ord');
        }

        function onDimensionSelected(dimName, ord, prev) {
            if (_.isUndefined(ord) || (ord === prev)) {
                 return [];
            }

            // the user has changed the given dimension to a value or to no value
            vm.model[dimName] = dimensionForOrd(ord);
            
            onDimensionsUpdated();

            // the following is temp ... to change to use the entity directly
            var dimRels = {
                colDimension: 'boardColumnDimension',
                rowDimension: 'boardSwimlaneDimension',
                styleDimension: 'boardStyleDimension'
            };

            rnBoardService.updateBoardDimension(vm.model, dimRels[dimName], vm.model[dimName]);

            return getDimensionValues(dimName);
        }

        function onColumnDimensionChanged(ord, prev) {
            if (ord !== prev && !_.isUndefined(ord)) {
                vm.colValues = onDimensionSelected('colDimension', ord, prev);
                getCardValuesDebounced();
                getLegendValuesDebounced();
                onColValuesChanged();
            }
        }

        function onRowDimensionChanged(ord, prev) {
            if (ord !== prev && !_.isUndefined(ord)) {
                vm.rowValues = onDimensionSelected('rowDimension', ord, prev);
                getCardValuesDebounced();
            }
        }

        function onStyleDimensionChanged(ord, prev) {
            if (ord !== prev && !_.isUndefined(ord)) {
                vm.styleValues = onDimensionSelected('styleDimension', ord, prev);
                getLegendValuesDebounced();
            }
        }

        function onColValuesChanged() {
            vm.colWidthCss = null;
            let count = _.filter(vm.colValues, 'show').length || 1; // always at least one. otherwise what's the point?
            if (vm.model) {
                vm.colWidthCss = (100 / count) + '%';
            }
        }
        
        function onAllowDrop(source, target, dragData, dropData) {
            return canDrop(source, target, dragData,dropData);
        }

        function canDrop(source, target, dragData, dropData) {
            var draggingLegendItem = hasClass(source, 'board-legend-item');
            var droppingOnLegendItem = hasClass(target, 'board-legend-item');
            var draggingCardItem = hasClass(source, 'board-card-item');
            var droppingOnCardItem = hasClass(target, 'board-card-item');
            var droppingOnDimensionItem = hasClass(target, 'board-dimension-item');

            var legendOnToCard = draggingLegendItem && droppingOnCardItem;
            var cardOnToLegend = draggingCardItem && droppingOnLegendItem;
            var cardOnToDimension = draggingCardItem && (droppingOnCardItem || droppingOnDimensionItem);

            return legendOnToCard || cardOnToLegend || cardOnToDimension;
        }

        function onDrop(event, source, target, dragData, dropData) {
            if (!canDrop(source, target, dragData, dropData)) {
                return true;
            }

            // dragData is the item (i.e. the report row) being dragged
            // dropData is an array of [dim,value] pairs, and optionally
            // a pair of [null,item] if the card was dropped on another card...
            var item = dragData;
            var dimValuePairs = dropData;

            // ...OR... a legend is being dragged to a card
            if (hasClass(source, 'board-legend-item')) {
                item = sp.result(dropData, '2.1');
                dimValuePairs = dragData;
            }

            vm.busyIndicator.isBusy = true;

            rnBoardService.updateEntityDimensions(vm.model, item, dimValuePairs)
                .then(({error}) => {
                    if (error) {
                        let errorText = error.statusText || error.status || error;
                        spAlertsService.addAlert(`Failed to update the record: ${errorText}`, { expires: 5 });
                    } else {
                        getCardValuesDebounced();
                        getLegendValuesDebounced();
                    }
                })
                .finally(() => { vm.busyIndicator.isBusy = false; });

            return false;
        }

        function getDimensionValues(dimName) {
            // the nullDimension with a single empty value exists to help rendering logic
            // and using predefined object rather than a literal inline here stops digest cycle issues.
            let dim = sp.result(vm, ['model', dimName]) || nullDimension;
            return dim.values || [];
        }

        function prepareCardRowValues(cards, col, count, row) {
            if (!row || (row.show === false)) return 0;
            let items = rnBoardService.getFilteredItems(vm.model, { dimValPairs: [[vm.model.colDimension, col], [vm.model.rowDimension, row]], cardFilter: vm.cardFilter });
            count += items.length;
            let path = '[' + col.rid + '][' + row.rid + ']';
            _.setWith(cards, path, items);
            _.setWith(cards, path + '.count', items.length);
            return count;
        }

        function prepareCardValues(cards, col) {
            if (!col || (col.show === false)) return 0;
            let count = 0;
            let fn = _.partial(prepareCardRowValues, cards, col, count);
            count = _.sumBy(vm.rowValues, fn);
            _.setWith(cards, '[' + col.rid + '].count', count);
            return count;
        }
        
        function getCardValues() {
            let cards = {};
            let t0 = performance.now();
            let fn = _.partial(prepareCardValues, cards);
            try {
                _.each(vm.colValues, fn);
            } finally {
                let t1 = performance.now();
                console.log('rnBoard.directive.getCardValues took ' + (t1 - t0) + ' milliseconds.');
            }
            return cards;
        }
        
        function getLegendValues() {
            let legend = {};
            _.forEach(vm.styleValues, function(s) {
                if (!s || (s.show === false)) return;
                let l = rnBoardService.getFilteredItems(vm.model, { dimValPairs: [[vm.model.styleDimension, s]], cardFilter: vm.cardFilter });
                _.setWith(legend, '[' + s.rid + '].count', l.length);
            });
            return legend;
        }
        
        function formatSaveErrorMessage(error, unexpectedMessage) {
            var message = error.message || unexpectedMessage;
            if (error.data) {
                message += ': ' + (error.data.ExceptionMessage || error.data.Message);
            }
            return message;
        }

        function hasClass(node, className) {
            if (!node) {
                return false;
            }
            var n = node.length ? _.head(node) : node;
            if (!n || !n.classList) {
                return false;
            }
            return n.classList.contains(className);
        }

        function safeApply(scope, fn) {
            if (!scope.$root.$$phase) {
                scope.$apply(fn);
            } else {
                fn();
            }
        }

        // sync the scrolling of the headers with the columns
        function setupScrolling() {
            if (!headers) {
                headers = angular.element($element.find('.board-column-header-container')[0]);
            }

            if (!scroller) {
                scroller = angular.element($element.find('.board-rows')[0]);
                scroller.bind('scroll', function() {
                    headers[0].scrollLeft = scroller[0].scrollLeft;
                });

                vm.hasVerticalScroll = () => scroller[0].scrollHeight > scroller[0].clientHeight;
            }
        }
    }    
}());
