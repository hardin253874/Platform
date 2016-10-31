/*global _, angular, console, sp, spEntity, rnBoard */

(function () {
    'use strict';

    var getValue = rnBoard.getValue,
        getValueId = rnBoard.getValueId;

    angular.module('app',
        [
            'testPagesAppHelper',
            'mod.app.board',
            'mod.common.spEntityService',
            'spApps.reportServices',
            'mod.common.ui.spDragDrop'
        ]);

    angular.module('app')
        .run(configureViewTemplates)
        .run(performDefaultLogin)
        .controller('MyController', MyController)
        .factory('debugService', function (spEntityService) {
            var service = {
                entities: {},
                getEntity: function (id, req) {
                    req = req || 'name,alias,isOfType.{name,alias}';
                    return spEntityService.getEntity(id, req).then(function (entity) {
                        console.log('debug: got entity', id, entity);
                        service.entities[id] = entity;
                    });
                }
            };
            return service;
        });

    /* @ngInject */
    function MyController(spEntityService, $scope, spReportService, debugService, $timeout) {
        var vm = this;

        // debugging... duh!
        vm.debugService = debugService;

        // properties
        vm.requestOptions = {
            startIndex: 0,
            pageSize: 200,
            metadata: 'full'
        };
        vm.meta = {};
        vm.cols = [];
        vm.data = [];
        vm.boardColumns = [];
        vm.dropOptions = {
            onAllowDrop: onAllowDrop,
            onDrop: onDrop
        };

        vm.reportFilter = "(task)|(incident)";

        // functions
        vm.loadReport = loadReport;
        vm.cardContent = cardContent;
        vm.cardContentForRow = cardContentForRow;
        vm.setPartitionColumn = setPartitionColumn;

        $scope.$on('signedin', function () {
            $timeout(loadReportList, 0);
        });

        $scope.$watch(partitionColumnOrd, setPartitionColumnForOrd);
        $scope.$watch(reportFilter, updateReportList);

        function partitionColumnOrd() {
            return vm.partitionColumnOrd;
        }

        function reportFilter() {
            return vm.reportFilter;
        }

        function setPartitionColumnForOrd(n) {
            setPartitionColumn(_.find(vm.cols, {ord: n}));
        }

        function loadReportList() {
            var query = {
                root: {
                    id: 'core:report',
                    related: []
                },
                selects: [
                    //{ field: '_id', displayAs: 'runId' },
                    {field: 'name', displayAs: 'Name'}
                ],
                conds: []
            };

            return spReportService.runQuery(query).then(function (results) {
                vm.allReports = _(results.data)
                    .map(function (row, index) {
                        return {
                            id: sp.result(row, 'item.0.value'),
                            name: sp.result(row, 'item.1.value')
                        };
                    })
                    .sortBy('name')
                    .value();
                updateReportList();
                return vm.allReports;
            });
        }

        function updateReportList() {
            vm.reports = _.filter(vm.allReports, function (r) {
                return r.name && r.name.match(new RegExp(vm.reportFilter, 'gi'));
            });

            loadReport(_.first(vm.reports));
        }

        function loadReport(report) {

            vm.reportId = report && report.id;
            if (vm.reportId) {
                spReportService.getReportData(report.id, vm.requestOptions)
                    .then(function (data) {
                        loadReportData(data);
                    }, function (error) {
                        console.error(error);
                    });
            }
        }

        function loadReportData(data) {
            vm.meta = data.meta;
            vm.cols = _(data.meta.rcols)
                .map(function (col, eid) {
                    return _.extend(col, {eid: eid});
                })
                .sortBy('ord')
                .value();
            vm.data = data.gdata;
            vm.boardColumns = [];

            _.forEach(vm.cols, noteIfCanPartitionOnCol);

            setPartitionColumn(_.find(vm.cols, '__canPartition'));

            getDebugEntities();
        }

        function getDebugEntities() {
            //debugService.getEntity(vm.meta.dfid);
            _.forEach(vm.meta.rcols, function (col, id) {
                //debugService.getEntity(id);
                //debugService.getEntity(col.fid);
                //debugService.getEntity(col.tid);
            });
        }

        function setPartitionColumn(col) {
            vm.partitionColumn = col;
            vm.partitionColumnOrd = sp.result(col, 'ord');
            if (vm.partitionColumn) {

                // choice value may be found in the meta,
                // otherwise for now just use what we see in the data
                // todo - if a lookup then maybe query db for other possible values
                var choiceValues = sp.result(vm.meta, ['choice', vm.partitionColumn.tid]);
                if (choiceValues) {
                    vm.boardColumns = _.map(choiceValues, function (c) {
                        return {
                            rid: c.id,
                            value: c.name
                        };
                    });
                } else {
                    vm.boardColumns = _(vm.data)
                        .uniq(function (row) {
                            return getValue(row, vm.partitionColumn.ord);
                        })
                        .map(function (row) {
                            return {
                                rid: getValueId(row, vm.partitionColumn.ord),
                                value: getValue(row, vm.partitionColumn.ord)
                            };
                        })
                        .value();
                }
                // add 'no value' column
                vm.boardColumns.push({value: ''});

                // ensure unique
                vm.boardColumns = _.unique(vm.boardColumns, 'value');
                vm.boardColumns = _.map(vm.boardColumns, assignColour);

                updateColumnCards();
                fixupPartitionColumnRelationshipId();

            } else {
                // a single column
                vm.boardColumns = [{
                    value: '',
                    cards: vm.data
                }];
            }
        }

        function assignColour(col, index) {
            var colours = ['#EF9A9A', '#B39DDB', '#A5D6A7', '#FFAB91', '#BCAAA4', '#B0BEC5'];
            col.colour = colours[index % colours.length];
            return col;
        }

        function fixupPartitionColumnRelationshipId() {
            var req = 'name,columnExpression.sourceNode.{' +
                '  resourceReportNodeType.{name,alias,isOfType.alias},' +
                '  followRelationship.{name,alias,isOfType.alias}' +
                '}';
            return spEntityService.getEntity(vm.partitionColumn.eid, req).then(function (entity) {
                vm.partitionColumn.rid = sp.result(entity, 'columnExpression.sourceNode.followRelationship.idP');
            });
        }

        function updateColumnCards() {
            vm.boardColumns = _.map(vm.boardColumns, function (boardColumn) {
                boardColumn.cards = _.filter(vm.data, function (row) {
                    return getValue(row, vm.partitionColumn.ord) === boardColumn.value;
                });
                return boardColumn;
            });
        }

        function noteIfCanPartitionOnCol(col) {
            if (col &&
                (col.type === 'InlineRelationship' && col.card === 'ManyToOne') ||
                (col.type === 'ChoiceRelationship')) {
                col.__canPartition = true;
            }
        }

        function cardContent(index) {
            if (vm.data && index < vm.data.length) {
                return cardContentForRow(vm.data[index]);
            }
            return index + 1;
        }

        function getBoardColumn(row) {
            if (!row) return;
            if (!vm.partitionColumn) return;
            if (!vm.boardColumns) return;

            return _.find(vm.boardColumns, {value: getValue(row, vm.partitionColumn.ord)});
        }

        function onAllowDrop(source, target, dragData, dropData) {
            return true;
        }

        function onDrop(event, source, target, dragData, dropData) {
            var row = dragData;
            var boardCol = dropData;
            console.log('onDrop', arguments, vm);
            console.log('dropped entity %o with current value %o',
                row.eid, row.values[vm.partitionColumn.ord]);
            console.log('.. onto column with target value %o %o for rel %o',
                boardCol.rid, boardCol.value, vm.partitionColumn.tid);

            var entityId = row.eid;
            var relOrChoiceId = vm.partitionColumn.rid;
            var existingValId = _.first(_.keys(row.values[vm.partitionColumn.ord].vals));
            var newValId = boardCol.rid;

            if (sendEntityUpdate(entityId, relOrChoiceId, existingValId, newValId)) {
                // eager update the client side
                //TODO - support clearing a relationship
                var v = {};
                v[newValId] = boardCol.value;
                row.values[vm.partitionColumn.ord].vals = v;
                updateColumnCards();
            }
        }

        function sendEntityUpdate(entityId, relOrChoiceId, existingValId, newValId) {
            console.log('updating', entityId, relOrChoiceId, existingValId, newValId);

            //debugService.getEntity(entityId);
            //debugService.getEntity(relOrChoiceId);
            //debugService.getEntity(existingValId);
            //debugService.getEntity(newValId);

            if (!(entityId && relOrChoiceId) || !(newValId || existingValId)) {
                return false;
            }

            //TODO - support clearing a relationship
            //TODO - don't need to get before put, just make a local entity and put that

            spEntityService.getEntity(entityId, 'alias,name,#' + relOrChoiceId + '.id').then(function (entity) {
                if (newValId) entity.getRelationship(relOrChoiceId).add(newValId);
                if (existingValId) entity.getRelationship(relOrChoiceId).remove(existingValId);
                console.log('updating', entity);
                spEntityService.putEntity(entity);
            }).catch(function (error) {
                console.log('error getting ', entityId);
            });

            return true;
        }
    }

    function cardContentForRow(row) {
        return getValue(row, 0) + ',' + getValue(row, 1) + ',' + getValue(row, 2);
    }

    /* @ngInject */
    function configureViewTemplates(testPagesAppHelper, $rootScope) {
        // add your app and component templates here...
        // this is needed to get the paths correct since we aren't using the build system
        testPagesAppHelper.appTemplate('src/app/board/rnBoard.tpl.html');
        testPagesAppHelper.requestTemplates().then(function() {
            $rootScope.templatesReady = true;
        });
    }

    /* @ngInject */
    function performDefaultLogin(spWebService, spLoginService, testPagesAppHelper) {
        spWebService.setWebApiRoot(testPagesAppHelper.getWebApiRoot());
        // hmmm can only get this working so far with test auth (the last param true)
        // something to do with not saving the cookies
        spLoginService.readiNowLogin('EDC', 'Administrator', 'Password', true, true)
            .then(function (result) {
//                    console.log('login result', result);
            }).catch(function (error) {
            console.log('login fail', error);
        });
    }

}());
