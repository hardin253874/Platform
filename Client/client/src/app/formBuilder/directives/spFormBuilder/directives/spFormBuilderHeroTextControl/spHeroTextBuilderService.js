// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Module implementing generic data services for building hero text.
    * @module spHeroTextBuilderService
    */
    angular.module('mod.common.spHeroTextBuilderService', [
            'mod.common.spVisDataService', 
            'mod.common.spVisDataBuilderService',
            'mod.common.spEntityService'
        ])
        .service('spHeroTextBuilderService', ['$q', 'spVisDataBuilderService', 'spVisDataService', 'spEntityService', spHeroTextBuilderService]);

    function spHeroTextBuilderService($q, spVisDataBuilderService, spVisDataService, spEntityService) {

        function createModel(heroTextControl) {
            var model = {
                reportId: 0,
                control: heroTextControl,
                sampleControl: heroTextControl,
                name: null,
                style: 'style1',
                columns: [], // column sources [{name, colId, ord, type, etc.. see spVisDataBuilder}]
                columnId: 0, // selected
                methods: spVisDataBuilderService.getAggregateMethods(null), // -> [{alias,name},..] (without namespace)
                method: null, // selected
                reportPickerOptions: {
                    selectedEntity: null,
                    selectedEntityId: 0,
                    selectedEntities: null,
                    entityTypeId: 'core:report',
                    multiSelect: false
                }
            };
            return model;
        }

        function blankControl() {
            return spEntity.fromJSON({
                name: jsonString(),
                heroTextReport: jsonLookup(),
                heroTextSource: jsonLookup(),
                heroTextStyle: jsonString('style1'),
                heroTextSize: jsonLookup(),
                heroTextUseCondFormattingColor: jsonBool()
            });
        }

        function initModel(model) {
            var promise = $q.when();

            if (!model.control) {
                console.error('heroTextControl is null');
            }

            // Do we have all details for control
            if (_.isUndefined(model.control.heroTextSource)) {
                if (model.control.isNew) {
                    // Create new control
                    var blank = blankControl();
                    spEntity.augment(model.control, blank, null);
                    model.control.name = 'Title';
                } else {
                    // Load existing control
                    var rq = 'let @SOURCE = { ' + spVisDataService.sourceRequest + ' }' +
                             'name, heroTextReport.name, heroTextSource.@SOURCE, heroTextSize.alias, heroTextUseCondFormattingColor, heroTextStyle';
                    promise = spEntityService.getEntity(model.control.idP, rq, { hint: 'heroText' })
                        .then(function (loadedControl) {
                            spEntity.augment(model.control, loadedControl, null);
                        });
                }
            }

            return promise
                .then(function () {
                    // Load report and column details 
                    var report = sp.result(model.control, 'heroTextReport');
                    model.name = model.control.name;
                    model.style = model.control.heroTextStyle || 'style1';
                    model.reportPickerOptions.selectedEntities = report ? [report] : [];
                    return reportChanged(model);
                });
        }

        function reportChanged(model) {
            // Get report metadata
            model.reportId = sp.result(model, 'reportPickerOptions.selectedEntities.0.idP');
            if (!model.reportId)
                return $q.when();

            return spVisDataBuilderService.loadReportMetadata(model).then(function () {
                // Update columns
                var columns = spVisDataBuilderService.getAvailableColumnSources(model, { hideRowNumber:true });
                model.columns = _(columns).orderBy('ord').value() || [];

                // Select column
                var curSource = model.control.heroTextSource;
                model.column = _(model.columns).filter(function (source) {
                    return sourceMatches(source, curSource);
                }).head();
                if (!model.column) {
                    model.column = _.last(model.columns); // default to 'count'
                }

                columnChanged(model);
            });
        }

        function sourceMatches(sourceInfo, sourceEntity) {
            // note: sp.result is returning undefined instead of null
            return sourceInfo.specialChartSource === (sp.result(sourceEntity, 'specialChartSource.nsAlias') || null) &&
                sourceInfo.colId === (sp.result(sourceEntity, 'chartReportColumn.idP') || null);
        }

        function columnChanged(model) {
            var type = sp.result(model.column, 'type');
            if (sp.result(model.column, 'specialChartSource') === 'core:countSource') {
                model.methods = [{ alias: 'aggCountWithValues', name: 'Count' }];
                model.method = model.methods[0];
            } else {                
                model.methods = spVisDataBuilderService.getAggregateMethods(type);
                // Select method
                var methodAlias = sp.result(model.control, 'heroTextSource.sourceAggMethod.aliasOnly') || null;
                model.method = _(model.methods).filter({ alias: methodAlias }).head();
            }
            updateSample(model);
        }

        function okEnabled(model) {
            if (!model || !model.reportId || !model.column) {
                return false;
            }
            return true;
        }

        function applyModelToControl(model, control) {
            control.name = model.name;
            control.heroTextReport = model.reportId;
            control.heroTextStyle = model.style || 'style1';
            if (model.column) {
                var chartSource = spVisDataBuilderService.createChartSource(model.column);
                control.heroTextSource = chartSource;
                if (sp.result(model, 'method.alias') && !chartSource.specialChartSource) {
                    chartSource.sourceAggMethod = 'core:' + model.method.alias;
                }
            } else {
                control.heroTextSource = null;
            }
        }

        function applyChanges(model) {
            applyModelToControl(model, model.control);
            model.control.refreshVisuals = {};
        }

        function updateSample(model) {
            model.sampleControl = blankControl();
            applyModelToControl(model, model.sampleControl);
            model.sampleControl.refreshTrigger = {};
        }

        function updateSampleVisuals(model) {
            model.sampleControl = blankControl();
            applyModelToControl(model, model.sampleControl);
            model.sampleControl.refreshTrigger = {};
        }

        var exports = {
            createModel: createModel,
            initModel: initModel,
            reportChanged: reportChanged,
            columnChanged: columnChanged,
            okEnabled: okEnabled,
            applyChanges: applyChanges,
            updateSample: updateSample,
            updateSampleVisuals: updateSampleVisuals
        };

        return exports;
    }

}());

