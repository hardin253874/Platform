// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, rnBoard, jsonLookup, jsonString, jsonBool */

//todo: fix up inconsistent pushing vs concat on the dimension values...


// notes ... more data model to get and save
// a board has a boardReport lookup to a report
// a board has a boardChildReport lookup to a report
// a board has a boardRankColumn lookup to a reportColumn
// a board has a boardCardCustomTemplate string field
// a board has a boardCardTemplateName string as the name of a built in template
// a board has boardColumnDimension, boardSwimlaneDimension, boardStyleDimension lookups to boardDimension
// a board has a drilldownTargetBoard to a board
// a boardDimension has a lookup boardDimensionReportColumn to a reportColumn
// a boardDimension has a many to many boardDimensionValue to a resource
// a boardDimension has a boardDimensionShowUndefined bool field

(function () {
    'use strict';

    angular.module('mod.app.board').factory('rnBoardService', rnBoardService);

    /* @ngInject */
    function rnBoardService($q, spEntityService, spReportService, spState) {

        // We have the concept of a BoardModel object that has the report metadata
        // and results for a given report.
        // It also has available Dimensions for the report.
        // A BoardDimension is the identifier relating to the field, lookup. etc.
        // that the dimension is based on, and also a list of possible values, each
        // being some scalar value and optionally some id, the latter for example required
        // for relationship based dimensions.

        //todo - add report paging support... how to limit data and get more....

        var REPORT_PAGE_SIZE = 200;
        var CHILD_REPORT_PAGE_SIZE = 200;
        var RELATED_PAGE_SIZE = 100;

        var RELATED_LIMIT = 50;
        var CHOICE_LIMIT = 50;

        var exports = {
            getBoardStats,
            getItemValue,
            getItemValueId,
            getFilteredItems,
            getRootTypeId,
            isAdmin,
            getConfigStyleClass,
            quickAddEntity,
            requestModel,
            saveBoard,
            sendEntityUpdate,
            structureViewToString,
            updateBoardDimension,
            updateBoardReport,
            updateCardTemplate,
            updateChildBoard,
            updateChildBoardReport,
            updateEntityDimensions,
            updateItemValue,
            updateShowQuickAdd,

            internal: { // exposed for unit testing
                getDimensionUpdateActions,
                getRankUpdateActions,
                updateEntity
            }
        };

        activate();

        return exports;

        /**
         * Do any once off service initialisation, if any.
         */
        function activate() {

        }

        /**
         * Get some useful stats about the board.
         */
        function getBoardStats(model) {
            return {
                maxCards: REPORT_PAGE_SIZE,
                cardCount: model.data && model.data.length || 0
            };
        }

        /**
         * Is the current user an admin type.
         */
        function isAdmin() {
            // yuk...
            return spState.data && _.some(spState.data.appTools, {_id: 'NavAdminToolboxButton'});
        }

        function getConfigStyleClass() {
            //bug 27674 Board View: Picker report in the Task properties is playing up in IE
            //and here is the issue: In IE10 and IE11, containers with display: flex and flex-direction: column will not properly calculate their flexed childrens' 
            //sizes if the container has min-height but no explicit height property.
            //the keyword for IE 10 is MSIE, and for IE 11 is Trident
            if (navigator.userAgent.indexOf('MSIE') !== -1 || navigator.userAgent.indexOf('Trident') !== -1 )
                return 'config-ie';
            else
                return 'config';
        }

        /**
         * Return a promise for a model based on a run of the report related to the given board.
         */
        function requestModel(boardId, {conds, oldModel} = {}) {
            var query = 'name, isOfType.{alias,name, ' +
                'k:typeConsoleBehavior.{ k:treeIcon.{ name, imageBackgroundColor}, k:treeIconUrl} }, ' +
                '  k:navigationElementIcon.{ alias, name, imageBackgroundColor}, ' +
                '  boardCardTemplateName, boardCardCustomTemplate, boardShowQuickAdd, ' +
                '{drilldownTargetBoard, boardRankColumn}.name,' +
                '{boardColumnDimension,boardSwimlaneDimension,boardStyleDimension}.' +
                '  {boardDimensionShowUndefined, boardDimensionShowAll, ' +
                '    {boardDimensionReportColumn, boardDimensionValue}.name}, ' +
                '{boardReport,boardChildReport}.{name, reportUsesDefinition.{ name,  ' +
                '  k:typeConsoleBehavior.{ k:treeIcon.{ name, imageBackgroundColor}, k:treeIconUrl} }, ' +
                '  rootNode.resourceReportNodeType.' +
                '  {name,k:defaultEditForm.id,relationships.toTypeDefaultValue.id}}';

            var model = {boardId, conds, lastAddedId: null};

            if (oldModel) {
                model.lastAddedId = oldModel.lastAddedId;
            }

            return spEntityService.getEntity(boardId, query)
                .then(entity => loadBoardEntity(model, entity))
                .then(model => syncDimensions(model))
                .catch(err => {
                    console.error('Failed to request a board with id: ' + boardId + '. Err: ' + err);
                    // todo - return some error indication in the model, or throw
                    return {};
                });
        }

        function loadBoardEntity(model, entity) {

            // console.log('DEBUG board entity: ' + JSON.stringify(spEntity.toJSON(entity, {skipDataState:true})));

            model.board = entity;
            model.formId = getFormId(model);

            // loading a child report, if one, can occur async
            // this is yuk.. now we are assuming we are mutating the model
            // and not replacing it in any part of our processing pipeline
            requestChildReportData(model);

            // May have conds that is a list of pairs of type and id
            // If so then we need to find a report col that is that type
            // and set up a report filter cond on the col.

            return $q.when(buildConditions(model))
                .then(model => requestReportData(model));
        }

        // always return the model
        function buildConditions(model) {

            // if there are any then we request meta data
            // to then find ...

            var reportId = sp.result(model.board, 'boardReport.idP');

            if (_.isEmpty(model.conds) || !reportId) return model;

            return spReportService.getReportData(reportId, {metadata: 'full', pageSize: 0})
                .then(data => {

                    // add the col eid into the col object to make it easier to work with
                    _.forEach(data.meta.rcols, (col, eid) => _.extend(col, {eid: sp.coerseToNumberOrLeaveAlone(eid)}));

                    model.reportConds = _(model.conds)
                        .map(({typeId, id}) => {

                            var col = _.find(data.meta.rcols, c => c.tid === typeId);
                            if (!col) return null;

                            return {
                                expid: col.eid + '',
                                oper: 'AnyOf',
                                type: 'InlineRelationship',
                                values: _.zipObject([id], [''])
                            };
                        })
                        .compact()
                        .value();
                    return model;
                })
                .catch(() => model);
        }

        function requestChildReportData(model) {
            var reportRequestOptions = {
                startIndex: 0,
                pageSize: CHILD_REPORT_PAGE_SIZE,
                metadata: 'full'
            };
            var reportId = sp.result(model.board, 'boardChildReport.idP');
            if (!reportId) {
                return model;
            }

            return spReportService.getReportData(reportId, reportRequestOptions)
                .then(results => {
                    model.childReportResults = results;

                    // if find an col in results.rcols that has tid == root type of the main report
                    // which we should know by now

                    var rootTypeId = getRootTypeId(model);
                    var parentCol = _.find(results.meta.rcols, c => c.tid === rootTypeId);
                    if (parentCol) {
                        //console.log('usable child report on col ' + parentCol.title);
                        model.childReportParentCol = parentCol;
                        model.childReportCols = rcolsToCols(results);
                    }

                    return model;
                })
                .catch(err => {
                    console.error('Failed to request a report: ' + reportId);
                    return {};
                });
        }

        function requestReportData(model) {

            var reportRequestOptions = {
                startIndex: 0,
                pageSize: REPORT_PAGE_SIZE,
                metadata: 'full',
                conds: model.reportConds
            };
            var reportId = sp.result(model.board, 'boardReport.idP');

            if (!reportId) {
                console.warn('Board does not have a related report');
                return model;
            }

            return spReportService.getReportData(reportId, reportRequestOptions)
                .then(_.partial(loadReportData, model))
                .catch(err => {
                    console.error('Failed to request a report: ' + reportId);
                    return {};
                });
        }

        function getRootTypeId(model) {
            return sp.result(model, 'board.boardReport.rootNode.resourceReportNodeType.idP');
        }

        function getFormId(model) {
            return sp.result(model, 'board.boardReport.rootNode.resourceReportNodeType.defaultEditForm.id');
        }

        function getRootTypeRels(model) {
            return sp.result(model, 'board.boardReport.rootNode.resourceReportNodeType.relationships');
        }

        function syncDimensions(model) {
            // must reset dimensions as we have new results, even if the same report
            // so match previous report columns if able otherwise default suitably
            model.colDimension = findMatching(model.dimensions, model.board.boardColumnDimension, model.colDimension);
            model.rowDimension = findMatching(model.dimensions, model.board.boardSwimlaneDimension, model.rowDimension);

            if (!model.colDimension && !model.rowDimension) {
                model.colDimension = _.first(model.dimensions);
            }

            model.styleDimension = findMatching(model.dimensions, model.board.boardStyleDimension, model.styleDimension) || _.first(_.tail(model.dimensions));

            return model;

        }

        // do a best guess at a matching dimension for the given dimEntity or dim
        function findMatching(dimensions, dimEntity, dim) {

            var colId = sp.result(dimEntity, 'boardDimensionReportColumn.idP');

            return (colId && _.find(dimensions, d => d.col.eid === colId)) ||
                (dim && dim.col && (
                    _.find(dimensions, d => d.col.eid === dim.col.eid) ||
                    _.find(dimensions, d => d.col.title === dim.col.title))
                );
        }

        /**
         * Set the report for the given board and saves the board.
         * Returns a promise for the new model.
         */
        function updateBoardReport(model, reportId) {
            console.assert(model && model.board);

            if (reportId === sp.result(model.board, 'boardReport.idP')) {
                return $q.when(model);
            }

            model.board.boardReport = reportId;
            return saveBoard(model).then(requestModel);
        }

        function updateChildBoardReport(model, reportId) {
            console.assert(model && model.board);

            if (reportId === sp.result(model.board, 'boardChildReport.idP')) {
                return $q.when(model);
            }

            model.board.boardChildReport = reportId;
            return saveBoard(model).then(requestModel);
        }

        function updateChildBoard(model, id) {
            console.assert(model && model.board);

            if (id === sp.result(model.board, 'drilldownTargetBoard.idP')) {
                return $q.when(model);
            }

            model.board.drilldownTargetBoard = id;
            return saveBoard(model).then(requestModel);
        }

        function updateCardTemplate(model, name, customTemplate) {
            console.assert(model && model.board);

            var board = model.board;
            var changed = board.boardCardTemplateName !== name || board.boardCardCustomTemplate !== customTemplate;

            if (changed) {
                board.boardCardTemplateName = name;
                board.boardCardCustomTemplate = customTemplate;
            }
            return $q.when(model);
        }

        function updateShowQuickAdd(model, value) {
            console.assert(model && model.board);
            model.board.boardShowQuickAdd = value;
            return $q.when(model);
        }

        /**
         * Update the modelled board's dimension to the given report column
         * which may be undefined.
         */
        function updateBoardDimension(model, dimRelName, {col} = {}) {
            if (!model || !model.board) return;

            let rel = model.board[dimRelName];
            if (!rel) {
                rel = spEntity.createEntityOfType('boardDimension', '', '');
                model.board[dimRelName] = rel;
            }

            rel.name = col && col.title || model.board.name;
            rel.boardDimensionReportColumn = col && col.eid;
        }

        /**
         * Save the board entity, returns a promise for the boardId.
         */
        function saveBoard(model) {
            console.assert(model && model.board);

            // Make an entity with only the bits we want to save.

            var b = model.board;
            var e = spEntity.fromJSON({
                id: b.idP,
                name: b.name,
                boardCardTemplateName: jsonString(b.boardCardTemplateName),
                boardCardCustomTemplate: jsonString(b.boardCardCustomTemplate),
                boardShowQuickAdd: jsonBool(b.boardShowQuickAdd),
                boardColumnDimension: makeDimensionJson(b.boardColumnDimension, model.colDimension),
                boardSwimlaneDimension: makeDimensionJson(b.boardSwimlaneDimension, model.rowDimension),
                boardStyleDimension: makeDimensionJson(b.boardStyleDimension, model.styleDimension),
                boardReport: jsonLookup(sp.result(b, 'boardReport.idP')),
                boardChildReport: jsonLookup(sp.result(b, 'boardChildReport.idP')),
                drilldownTargetBoard: jsonLookup(sp.result(b, 'drilldownTargetBoard.idP')),
                boardRankColumn: jsonLookup(sp.result(b, 'boardRankColumn.idP'))
            });

            return spEntityService.putEntity(e);
        }

        function makeDimensionJson(dimEntity, dim) {
            const reportColumn = sp.result(dimEntity, 'boardDimensionReportColumn');
            const boardDimensionShowUndefined = dim && _.some(dim.values, v => !v.value && v.show);
            const boardDimensionShowAll = dim && _.every(dim.values, v => v.show);

            // note - no need to record the relationships to the values if "showing all"
            const boardDimensionValue = !boardDimensionShowAll && dim ?
                _.compact(_.map(_.filter(dim.values, v => v.show), 'rid')) : [];

            let e = spEntity.fromJSON({
                id: dimEntity && dimEntity.idP, // may be null... if so a new one will be created
                typeId: 'boardDimension',
                name: jsonString(dimEntity && dimEntity.name || (reportColumn ? `Dimension for ${reportColumn.name}` : 'Undefined')),
                boardDimensionReportColumn: jsonLookup(sp.result(reportColumn, 'idP')),
                boardDimensionValue: boardDimensionValue,
                boardDimensionShowUndefined: jsonBool(boardDimensionShowUndefined),
                boardDimensionShowAll: jsonBool(boardDimensionShowAll)
            });

            // the jsonLookup(null) above doesn't work to clear existing relationships, so be explicit here...
            if (!reportColumn) {
                e.getRelationship('boardDimensionReportColumn').clear();
                e.boardDimensionValue = [];
            }
            // or does assigning []
            if (_.isEmpty(e.boardDimensionValue)) {
                e.boardDimensionValue.clear();
            }

            return e;
        }

        /**
         * Build and return a board model object based on the given report results.
         */
        function loadReportData(model, data) {
            // console.log('loadReportData', model, data);

            model.meta = data.meta;
            model.cols = rcolsToCols(data);
            model.data = data.gdata;

            return getDimensions(model).then(function(dims) {
                model.dimensions = dims;
                return model;
            });
        }

        function rcolsToCols(results) {
            return _(results.meta.rcols)
                .map((col, eid) => _.extend(col, {eid: sp.coerseToNumberOrLeaveAlone(eid)}))
                .sortBy('title')
                .sortBy('ord')
                .value();
        }

        function getDimensions(model) {
            let promises = _(model.cols).filter(canPartition).map(_.partial(makeDimension, model)).value();
            return $q.all(promises).then(function(dims) {
                return _.sortBy(dims, dimensionSortKey);
            });
        }

        function dimensionSortKey(d) {
            // more thought needed on this
            var dimNames = [/kbcol/, /stat/, /assigned/, /priority/];
            if (!d.col.title) return 999;
            var v = _.findIndex(dimNames, re => re.test(d.col.title.toLowerCase()));
            return v >= 0 ? v : 999;
        }

        function canPartition(col) {
            return col &&
                (col.type === 'InlineRelationship' && col.card === 'ManyToOne') ||
                (col.type === 'ChoiceRelationship') ||
                // another special for the kb .. todo remove
                (col.type === 'String' && /kb/.test(col.title));
        }

        function makeDimension(model, col) {

            console.assert(model && col);

            var dimension = {
                col: col,
                ord: sp.result(col, 'ord'),
                values: []
            };

            var noValue = {
                value: '',
                show: _.some(model.data, r => !getItemValue(r, col.ord))
            };

            // we do a request for related items and then deal with values
            // that might be in the report schema or data
            return requestLookupValues(col).then(function (values) {

                // take the related items, up to some limit and if the limit
                // is reached then prefer those that appear in our data set
                // and do this while maintaining the sort order
                // - not going to be too clever about this, just not to break if there are lots
                if (values && values.length > RELATED_LIMIT) {
                    values = _(values)
                        .map(v => {
                            return {
                                v: v,
                                inData: _.some(model.data, r => getItemValue(r, col.ord))
                            };
                        })
                        .sortBy('inData')
                        .take(RELATED_LIMIT)
                        .map('v')
                        .value();
                }

                if (_.isEmpty(values))
                    values = getChoiceValues(model, col);

                if (_.isEmpty(values))
                    values = getValuesInData(model, col);

                // add 'no value' column
                // Note - do this after the async request for lookup data otherwise
                // we get a screen draw with the single undefined column flash up, then a
                // redraw with the lookup cols. to solve this a different way, but for now...
                dimension.values = dimension.values.concat([noValue], values);

                // filter by any conds we may have
                dimension.values = _.reject(dimension.values, v => {
                    return _.some(model.conds || [], c => dimension.col.tid === c.typeId && c.id !== v.rid);
                });

                processDimensionValues(model, dimension);

                return dimension;
            });
        }

        function getChoiceValues(model, col) {

            var choiceValues = sp.result(model.meta, ['choice', col.tid]);
            return _(choiceValues || [])
                .map(function (c) {
                    return {
                        rid: c.id,
                        value: c.name,
                        description: c.name, // todo go get actual description
                        show: choiceValues.length < CHOICE_LIMIT || _.some(model.data, r => c.name === getItemValue(r, col.ord))
                    };
                })
                .value();
        }

        function getValuesInData(model, col) {
            return _(model.data)
                .uniqBy(row => getItemValue(row, col.ord))
                .map(function (row) {
                    return {
                        rid: getItemValueId(row, col.ord),
                        value: getItemValue(row, col.ord),
                        show: true
                    };
                })
                .value();
        }

        /** return a promise ... */
        function requestLookupValues(col) {
            if (!(col.type === 'InlineRelationship' && col.card === 'ManyToOne')) {
                return $q.when([]);
            }

            // If a lookup then go get possible values .. beware of large number of results
            // Should be handling this async better... but anyway...
            var tid = col.tid;
            var query = 'defaultDisplayReport.id';
            return spEntityService.getEntity(tid, query).then(function (entity) {
                // console.log('DEBUG lookup type: ' + JSON.stringify(spEntity.toJSON(entity, {skipDataState:true})));

                var reportId = sp.result(entity, 'defaultDisplayReport.idP');
                if (!reportId) {
                    console.warn('board view: failed to find default report for entity type ' + tid);
                    return [];
                }

                return spReportService.getReportData(reportId, {
                    startIndex: 0,
                    pageSize: RELATED_PAGE_SIZE,
                    metadata: 'full'
                }).then(function (results) {
                    // console.log('DEBUG report ' + reportId + ' ' + JSON.stringify(results));

                    var lookupCols = _(results.meta.rcols)
                        .map((col, eid) => _.extend(col, {eid: sp.coerseToNumberOrLeaveAlone(eid)}))
                        .sortBy('ord')
                        .value();
                    // We are assuming the first string column in the report is the text we use for the
                    // dimension values.
                    var valueCol = _.find(lookupCols, c => c.type === 'String') || _.first(lookupCols);
                    var ord = valueCol && valueCol.ord || 0;
                    var docCol = _.find(lookupCols, c => c.title === 'Description') || '';
                    var docOrd = docCol && docCol.ord || 0;
                    return _.map(results.gdata, function (r) {
                        return {
                            rid: r.eid,
                            value: getItemValue(r, ord),
                            description: getItemValue(r, docOrd),
                            show: true
                        };
                    });
                }).catch(function (err) {
                    console.error('Failed to query lookup values for ' + tid + ' and ' + reportId + ' using ' + query);
                });
            }).catch(function (err) {
                console.error('Failed to query default report id for ' + tid + ' using ' + query);
            });
        }

        function processDimensionValues(model, dimension) {

            console.assert(dimension.col);

            let cfrules = sp.result(model.meta, ['cfrules', dimension.col.eid, 'rules']);
            let assignColour = cfrules ? _.partial(assignRuleColour, cfrules) : assignDefaultColour;

            dimension.values = _.uniqBy(dimension.values, 'value');
            dimension.values = _.map(dimension.values, assignColour);

            // sync the dimension value selections
            _.forEach(['boardColumnDimension', 'boardSwimlaneDimension', 'boardStyleDimension'], dimRelName => {
                var rel = model.board[dimRelName];
                var col = rel && rel.boardDimensionReportColumn;
                if (col && col.idP === dimension.col.eid) {
                    syncValueSelections(model, dimension, rel);
                }
            });
        }

        function syncValueSelections(model, dim, dimEntity) {
            if (!dim || !dimEntity) return;

            // console.log('syncing value selections',
            //     dim, dimEntity.name,
            //     JSON.stringify(_.take(dim.values, 10)),
            //     JSON.stringify(_.take(_.map(dimEntity.boardDimensionValue, 'idP'), 10)));

            // Show all if none have been previously selected.
            // If any dimensions appear in the model conds then enable them too

            var noneSelected = _.isEmpty(dimEntity.boardDimensionValue);
            _.forEach(dim.values, v => {
                v.show = noneSelected || dimEntity.boardDimensionShowAll ||
                    _.some(model.conds || [], ({id}) => v.rid === id) ||
                    _.some(dimEntity.boardDimensionValue, e => v.rid === e.idP) ||
                    (!v.value && !!dimEntity.boardDimensionShowUndefined);
            });
        }

        function assignRuleColour(cfrules, value) {
            let newValue = _.clone(value);
            let rule = _.find(cfrules, r => r.vals && (('' + newValue.rid) in r.vals)) ||
                _.find(cfrules, r => r.oper === 'Unspecified');
            if (rule) {
                newValue.colour = sp.getCssColorFromRgb(rule.bgcolor);
            }
            return newValue;
        }

        function assignDefaultColour(value, index) {
            let newValue = _.clone(value);
            let colours = ['#E85D11', '#B71C1C', '#E91E63', '#861D9C', '#3F51B5', '#2196F3', '#00B289', '#1B5E20', '#12BB2E', '#607D8B'];
            newValue.colour = colours[index % colours.length];
            return newValue;
        }

        /**
         * Update the entity
         * @param model
         * @param item
         * @param dimValuePairs
         * @returns {promise} resolves to a {id,error} pair if update attempted and passed or failed
         * or {} if no update was required
         */
        function updateEntityDimensions(model, item, dimValuePairs) {

            // item is the item (i.e. the report row) being dragged
            // dimValuePairs is an array of [dim,value] pairs, and optionally
            // a pair of [null,item] if the card was dropped on another card

            let updates = getDimensionUpdateActions(item, dimValuePairs);

            if (_.isEmpty(updates)) {
                updates = getRankUpdateActions(model, item, dimValuePairs);
            }

            if (!_.isEmpty(updates)) {
                return updateEntity(model, item, dimValuePairs, updates);
            }

            return $q.when({});
        }

        function getDimensionUpdateActions(item, dimValuePairs) {
            return _(dimValuePairs)
                .map(([dim, targetValue]) => {
                    if (!dim || !dim.col) return null;

                    console.log('dropped entity %o with current value %o',
                        item.eid, item.values[dim.ord]);
                    console.log('.. onto column with target value %o %o for rel %o',
                        targetValue.rid, targetValue.value, dim.col.tid);

                    let update = {
                        relOrChoiceId: dim.col.rid,
                        existingValId: sp.coerseToNumberOrLeaveAlone(_.first(_.keys(item.values[dim.ord].vals))),
                        newValId: sp.coerseToNumberOrLeaveAlone(targetValue.rid)
                    };

                    // fixup undefined weirdness
                    if (update.existingValId === "undefined") update.existingValId = undefined;

                    // return the update if needed
                    return (update.existingValId !== update.newValId) && update;
                })
                .compact()
                .value();
        }

        function getRankUpdateActions(model, item, dimValuePairs) {
            let droppedOnItemPair = _.find(dimValuePairs, p => !p[0]);
            let droppedOnItem = droppedOnItemPair && droppedOnItemPair[1];

            if (droppedOnItem && droppedOnItem.eid && droppedOnItem.eid !== item.eid) {
                // card dropped on another card

                // if there is a decimal rank field in the report then adjust as needed to
                // insert the dropped item in front of (lower rank) of dropped on item.

                var rankCol = _.find(model.cols, c => /rank/ig.test(c.title) && c.type === 'Decimal');
                if (rankCol) {
                    // not assuming the items are sorted by rank so we need to search
                    var itemRank = parseFloat(getItemValue(item, rankCol.ord));
                    var droppedOnRank = parseFloat(getItemValue(droppedOnItem, rankCol.ord));
                    var prevItem = _(model.data)
                        .filter(r => getItemValue(r, rankCol.ord) < droppedOnRank)
                        .sortBy(r => 0 - getItemValue(r, rankCol.ord))
                        .first();
                    var prevItemRank = parseFloat(prevItem && getItemValue(prevItem, rankCol.ord));

                    droppedOnRank = _.isNaN(droppedOnRank) ? 0 : droppedOnRank;
                    prevItemRank = _.isNaN(prevItemRank) ? droppedOnRank : prevItemRank;

                    // not science here... just trying stuff
                    if (prevItemRank === droppedOnRank) prevItemRank = droppedOnRank - 2;
                    var newRank = prevItemRank + (droppedOnRank - prevItemRank) / 2;

                    console.log('changing rank from ', itemRank, ' to ', newRank);

                    return [{fieldId: rankCol.fid, value: newRank, typeName: rankCol.type}];
                }
            }
            return [];
        }

        function updateEntity(model, item, dimValuePairs, updates) {

            return sendEntityUpdate(item.eid, updates)
                .then(function (result) {

                    if (!result.error) {

                        //TODO - support clearing a relationship

                        // update the client side without refreshing the report
                        _.forEach(dimValuePairs, function ([dim, targetValue]) {
                            if (!dim || !dim.col) return;

                            var v = {};
                            v[targetValue.rid] = targetValue.value;
                            item = updateItemValue(item, dim.col.ord, v);

                            // replace the entire item in the data ... needed for efficient ref based watches
                            model.data = model.data.slice(0);
                        });
                    }

                    return result;
                });
        }

        /**
         * Args are entityId, plus updates where updates is an array
         * of objects with members: relOrChoiceId, existingValId, newValId.
         * Returns a promise for an object with props {id, error}
         * where error is defined when there has been an error.
         */
        function sendEntityUpdate(entityId, updates) {
            console.log('send entity update', entityId, updates);

            if (_.isEmpty(updates)) {
                return $q.when({id: entityId});
            }

            var relIds = _.reduce(updates, (a, p) =>
                    p.relOrChoiceId ? (a + (a.length && ',' || '') + '#' + p.relOrChoiceId) : a,
                '');
            var request = relIds ? 'alias,name,{' + relIds + '}.id' : 'alias,name';

            //TODO - don't need to get before put, just make a local entity and put that

            return spEntityService.getEntity(entityId, request).then(function (entity) {
                _.forEach(updates, function (u) {
                    console.log('updating related from', u.existingValId, ' to ', u.newValId);
                    if (u.newValId) {
                        entity.getRelationship(u.relOrChoiceId).add(u.newValId);
                    }
                    if (u.existingValId) {
                        entity.getRelationship(u.relOrChoiceId).remove(u.existingValId);
                    }
                    if (u.fieldId) {
                        entity.setField(u.fieldId, u.value, u.typeName);
                    }
                });
                console.log('updating', entity);
                return spEntityService.putEntity(entity).then(function (id) {
                    console.log('posted', id);
                    return {id};
                });

            }).catch(function (error) {
                console.log('error getting ', entityId, error);
                return {error};
            });
        }

        /**
         * Create a record of the root entity type of the given report results.
         * Initialise fields based on the given expression.
         * todo: support including relationships and choice
         * todo parse tags for dimensions and set those values
         *  e.g. very important task #high
         */
        function quickAddEntity(model, expression) {
            console.log('create entity', model, expression);

            if (!model || !model.cols)
                return $q.when(false);

            if (!getRootTypeId(model))
                return $q.when(false);

            // create the record with the first string field in the report
            // set to the expression
            var stringCol = _.find(model.cols, function (col) {
                return col.type === 'String';
            });
            if (!stringCol)
                return $q.when(false);

            var e = spEntity.createEntityOfType(getRootTypeId(model), '', '');
            e.setField(stringCol.fid, expression, 'String');

            // set the relationships for any that have defaults
            _.forEach(model.cols, function (col) {
                var r = col.rid && _.find(getRootTypeRels(model), function (r) {
                        return r.idP === col.rid;
                    });
                var d = r && sp.result(r, 'toTypeDefaultValue.id');

                // if the field is a dimension on the board then ensure it is
                // being set to one of the values currently selected (not style, only col and row)
                var dim = _.find([model.colDimension, model.rowDimension], p => p && p.col === col);
                if (dim && d) {
                    // ensure it is on the board
                    if (!_.find(dim.values, v => v.rid === d && v.show)) {
                        d = null;
                    }
                }

                // if the field is one of our conditions then use the given cond value
                var cond = _.find(model.conds || [], ({typeId}) => col.tid === typeId);
                if (cond) {
                    d = cond.id;
                }

                // if no default or the default isn't on the board then
                // if matching any dimension in the report then set to the first visible
                if (!d) {
                    //next line is commented out as we don't want to default for dims
                    //other than row or col and those will have been found above already
                    //dim = dim || _.find(model.dimensions, p => p.col === col);
                    d = (dim && _.find(dim.values, v => v.show) || {}).rid;
                }

                if (d) {
                    e.setRelationship(col.rid, [d]);
                }
            });

            console.log('posting entity', e);

            return spEntityService.putEntity(e).then(
                function (id) {
                    model.lastAddedId = id;
                    console.log('created new entity', id);
                    return id;
                }, console.error.bind(console));
        }

        /**
         * Return the list of report items (rows) for the given set of dimension values
         * and matching the filter if given.
         */
        function getFilteredItems(model, {dimValPairs, cardFilter}) {

            if (!model) return [];

            // Find the data rows where the field of the row matches in value, for each dimension
            // If no dimValuePairs then all will be taken
            var items = _.filter(model.data, function (item) {
                return _.every(dimValPairs, function ([dim, value]) {
                    return !dim ||
                        sp.coerseToNumberOrLeaveAlone(getItemValueId(item, dim.ord)) === value.rid ||
                        getItemValue(item, dim.ord) === value.value;
                });
            });

            // Filter where the item column values match, case insensitive
            if (cardFilter) {
                var filterText = cardFilter.toLowerCase();
                items = _.filter(items, item => {
                    if (!item.itemText) {
                        let itemValues = _.map(model.cols, col => getItemValue(item, col.ord));
                        item.itemText = JSON.stringify(itemValues).toLowerCase() || '';
                    }
                    return item.itemText.indexOf(filterText) >= 0 || item.eid === model.lastAddedId;  // don't filter last added card
                });
            }

            // Move last-added to top
            // And do it like this because lodash is behaving like an irrational child today
            var res = [];
            var lastAdded = null;
            _.forEach(items, function (item) {
                if (item.eid === model.lastAddedId) {
                    lastAdded = item;
                } else {
                    res.push(item);
                }
            });
            if (lastAdded) {
                res = _.concat([lastAdded], res);
            }
            return res;
        }

        function getItemValue(row, col) {
            var value = row.values[col];
            return value && (value.val || _.first(_.values(value.vals))) || '';
        }

        function getItemValueId(row, col) {
            var value = row.values[col];
            return value && sp.coerseToNumberOrLeaveAlone(_.first(_.keys(value.vals)));
        }

        function updateItemValue(row, col, v) {
            if (_.isObject(v))
                row.values[col].vals = v;
            else
                row.values[col].value = v;
            return row;
        }

        // this stolen from the report angular 'filters' .... to move to a common
        // routine that both can use TODO

        function structureViewToString(data) {
            // Handle strings and dictionaries of strings
            if (_.isString(data)) {
                return structureLevelsPathFromString(data);
            } else if (_.isObject(data)) {
                return structureLevelsPathFromDict(data);
            } else {
                return '';
            }
        }

        function structureLevelsPathFromString(data) {
            const STX = '\u0002'; // separates the components in a path
            const ETX = '\u0003'; // separates the paths

            if (!data || !_.isString(data)) {
                return '';
            }

            return _(data.split(ETX))
                .uniq()
                // remove paths that are a sub-path of others
                .reject(function (path, i, paths) {
                    return _.filter(paths, p => p.indexOf(path) >= 0).length > 1;
                })
                .map(function (path) {
                    // Strip the values away from the ids and concat the last 2
                    return _(path.split(STX))
                        .map(function (p) {
                            let matches = /\d+:(.*)/g.exec(p);
                            return matches ? matches[1] : '?';
                        })
                        .takeRight(2)
                        .join(' > ');
                })
                .uniq()
                .sort()
                .join(', ');
        }

        function structureLevelsPathFromDict(dict) {
            return _(dict)
                .values()
                .compact()
                .map(structureLevelsPathFromString)
                .join(', ');
        }

    }

}());
