// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // A field title plus the mandatory and error markers.
    /////
    angular.module('mod.app.editForm.designerDirectives.spHeroTextControl', [
            'mod.common.spCachingCompile',
            'mod.common.spEntityService',
            'mod.common.spVisDataService',
            'mod.common.spVisDataActionService',
            'mod.app.resourceScopeService'
        ])
        .directive('spHeroTextControl', ['$q', 'spCachingCompile', 'spEntityService', 'spVisDataService', 'spVisDataActionService', 'spResourceScope', spHeroTextControlDirective]);

    function spHeroTextControlDirective($q, spCachingCompile, spEntityService, spVisDataService, spVisDataActionService, spResourceScope) {

        var nothingToShow = '-';

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                formControl: '=?',
                parentControl: '=?',
                formData: '=?',
                formMode: '=?',
                isInTestMode: '=?',
                isReadOnly: '=?',
                isInDesign: '=?'
            },
            link: function (scope, element) {

                var model = {
                    controlId: 0,
                    reportId: 0,
                    loadedControl: null, // control loaded independently of edit form model
                    heroTitle: '',
                    heroData: nothingToShow,
                    style: 'style1',
                    onScopeUpdateUnregisterListener: null,
                    externalConds: null
                };
                scope.model = model;

                scope.$on('gather', function (event, callback) {
                    callback(scope.formControl, scope.parentControl, element);
                });

                scope.$watch('formControl', formControlChanged);

                function formControlChanged(control) {
                    if (control && control.getReceiveContextFrom) {

                        // check if we are getting our context from elsewhere
                        var contextSender = control.getReceiveContextFrom();

                        if (contextSender) {
                            var channelId = spResourceScope.getChannelIdFromReceiver(control);

                            if (model.onScopeUpdateUnregisterListener) {
                                model.onScopeUpdateUnregisterListener();
                            }

                            // receive context update.
                            model.onScopeUpdateUnregisterListener = spResourceScope.onScopeUpdate(channelId, function (actionData) {
                                if (actionData.drilldownConds) {
                                    model.externalConds = actionData.drilldownConds;
                                    loadData();
                                }
                            });
                        }
                    }
                }

                scope.$on('$destroy', function () {
                    if (model.onScopeUpdateUnregisterListener) {
                        model.onScopeUpdateUnregisterListener();
                        model.onScopeUpdateUnregisterListener = null;
                    }
                });

                scope.$watch('formControl.refreshTrigger', function () {
                    init();
                });

                scope.$watch('formControl.refreshVisuals', function () {
                    var control = model.loadedControl;
                    if (control) {
                        model.heroTitle = control.name;
                        model.style = control.heroTextStyle || 'style1';
                    }
                });

                function drilldown() {
                    console.log('heroText drilldown');
                    if (scope.isInDesign) {
                        return;
                    }
                    var actionData = { drilldownConds: model.externalConds || {} };
                    spVisDataActionService.executeClickAction(actionData, true, sp.result(model.loadedControl, 'heroTextReport'));
                }
                scope.dataClicked = drilldown;

                function isLoaded() {
                    return !_.isUndefined(scope.formControl.heroTextSource);
                }

                function init() {
                    if (!scope.formControl)
                        return $q.when();
                    model.controlId = scope.formControl.idP;
                    if (isLoaded()) {
                        model.loadedControl = scope.formControl;
                        getDetailsFromControl();
                        return loadData();
                        // in screen-designer, after editing properties
                    } else if (scope.formControl.isNew) {
                        // in screen-designer, before editing properties
                        model.heroTitle = 'Title';
                        return $q.when();
                    } else {
                        // on a screen at view time, where the details haven't been loaded yet
                        return loadControlDetails().then(loadData);                        
                    }
                }

                function loadControlDetails() {
                    if (!model.controlId)
                        return $q.when();
                    var rq = 'let @SOURCE = { ' + spVisDataService.sourceRequest + ' }' +
                             'name, heroTextReport.name, heroTextSource.@SOURCE, heroTextSize.alias, heroTextUseCondFormattingColor, heroTextStyle';
                    return spEntityService.getEntity(model.controlId, rq, { hint: 'heroText' })
                        .then(function (htControl) {
                            model.loadedControl = htControl;
                            getDetailsFromControl();
                        });
                }

                function getDetailsFromControl() {
                    var control = model.loadedControl;
                    model.heroTitle = control.name;
                    model.reportId = sp.result(control, 'heroTextReport.idP');
                    model.source = control.heroTextSource;
                    model.style = control.heroTextStyle || 'style1';
                }

                function loadData() {
                    if (!model.reportId)
                        return $q.when();
                    var visModel = {
                        reportId: model.reportId,
                        sources: [model.source],
                        sourcesRequestingAllData: []
                    };
                    var options = {
                        isRefresh: true, // for now
                        pageSize: 1, // only get first row
                        externalConds: model.externalConds
                    };
                    return spVisDataService.requestVisData(visModel, options).then(function (visData) {
                        var accessor = spVisDataService.createDataAccessor(model.source, { visData: visData });
                        var rowData = visData.rowData;
                        if (!accessor || !rowData || rowData.length === 0) {
                            console.warn('No data to chart');
                            model.heroData = '-';
                        } else {
                            var row = rowData[0];
                            model.heroData = accessor.getText(row) || '-';
                        }
                    });
                }

                var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spHeroTextControl/spHeroTextControl.tpl.html');
                cachedLinkFunc(scope, function (clone) {
                    element.append(clone);
                });

                init();
            }
        };
    }

}());