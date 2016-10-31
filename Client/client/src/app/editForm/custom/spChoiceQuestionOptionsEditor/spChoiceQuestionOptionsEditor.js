// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spChoiceQuestionOptionsEditor', [
        'mod.common.spMobile',
        'mod.common.spEntityService',
        'mod.common.spCachingCompile'])
           .directive('spChoiceQuestionOptionsEditor', function ($q, $timeout, spMobileContext, spEntityService, spCachingCompile) {
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    formControl: '=',
                    parentControl: '=?',
                    formData: '=',
                    formMode: '=?',
                    isInTestMode: '=?',
                    isReadOnly: '=?',
                    isInDesign: '=?'
                },
                link: function (scope, element) {

                    var co = 'name, description, choiceOptionOrder, choiceOptionValue';
                    var relChoiceSet;
                    var isExistingDeregister;
                    var selectedEntitiesDeregister;
                    
                    scope.isMobile = spMobileContext.isMobile;
                    scope.isExisting = false;
                    scope.isLegacy = false; // support for old behavior of choices directly on question
                    scope.canModify = false;
                    scope.canDelete = false;
                    scope.gridLoaded = false;
                    scope.relationshipToRender = null;

                    scope.model = {
                        choiceOptions: [],
                        choiceOptionSet: null,
                        choiceOptionGridOptions: {
                            data: 'model.choiceOptions',
                            multiSelect: false,
                            enableSorting: true,
                            enableCellEdit: !scope.isReadOnly && !scope.isInDesign,
                            selectedItems: [],
                            sortInfo: { fields: ['choiceOptionOrder'], directions: ['asc']},
                            columnDefs: [
                                {
                                    field: 'name',
                                    displayName: 'Name',
                                    sortable: false,
                                    groupable: false,
                                    enableCellEdit: !scope.isReadOnly && !scope.isInDesign
                                },
                                {
                                    field: 'description',
                                    displayName: 'Description',
                                    sortable: false,
                                    groupable: false,
                                    enableCellEdit: !scope.isReadOnly && !scope.isInDesign
                                },
                                {
                                    field: 'choiceOptionValue',
                                    displayName: 'Value',
                                    sortable: false,
                                    groupable: false,
                                    enableCellEdit: !scope.isReadOnly && !scope.isInDesign,
                                    cellFilter: 'number'
                                },
                                {
                                    field: 'choiceOptionOrder',
                                    sortable: true,
                                    groupable: false,
                                    enableCellEdit: false,
                                    visible: false
                                }
                            ],
                            afterSelectionChange: function(row) {
                                scope.selectedRowIndex = row.rowIndex;
                            }
                        },
                        choiceOptionPickerOptions: {
                            entityTypeId: 'core:choiceOptionSet',
                            selectedEntity: null,
                            selectedEntities: null,
                            pickerReportId: 'core:choiceOptionSetReport',
                            multiSelect: false,
                            isDisabled: scope.isReadOnly || scope.isInDesign
                        }
                    };

                    scope.$watch("formControl", function() {
                        if (scope.formControl) {
                            scope.relationshipToRender = scope.formControl.relationshipToRender;
                        }
                    });

                    scope.$watch("formData", function () {

                        if (isExistingDeregister) {
                            isExistingDeregister();
                        }

                        if (selectedEntitiesDeregister) {
                            selectedEntitiesDeregister();
                        }

                        if (scope.formData) {
                            scope.canModify = scope.formData.canModify || false;
                            scope.canDelete = scope.formData.canDelete || false;

                            // setup the other relationships being edited
                            getRelationshipChoiceSet().then(function() {

                                if (scope.formData.dataState === spEntity.DataStateEnum.Create) {
                                    // setup for new
                                    createOptions();
                                } else {
                                    // load any existing
                                    getOptions();
                                }

                                // ng-grid doesn't go nicely with show/hide from tabs. but it's fine with ng-if.
                                $timeout(function () {
                                    scope.gridLoaded = true;

                                    isExistingDeregister = scope.$watch("isExisting", isExistingWatcher);
                                    selectedEntitiesDeregister = scope.$watchCollection("model.choiceOptionPickerOptions.selectedEntities", selectedEntitiesWatcher);
                                }, 0);
                            });
                        }
                    });

                    scope.isExistingChange = function (isExisting) {
                        scope.isExisting = isExisting;
                    };

                    //
                    // Retrieves the relationship being displayed or edited.
                    //
                    function getRelationship() {
                        var rel = null;
                        if (scope.relationshipToRender) {
                            var relId = { id: scope.relationshipToRender.eid(), isReverse: false };
                            rel = scope.formData.getRelationship(relId);
                        }
                        return rel;
                    }

                    //
                    // Gets the choice set relationship.
                    //
                    function getRelationshipChoiceSet() {
                        relChoiceSet = scope.formData.getRelationshipContainer('choiceQuestionChoiceSet');
                        if (!relChoiceSet || relChoiceSet.id < 0) {
                            return spEntityService.getEntity('core:choiceQuestionChoiceSet', 'name').then(function (rel) {
                                if (rel) {
                                    relChoiceSet = { id: rel.idP, isReverse: false };
                                    scope.formData.registerLookup(relChoiceSet);
                                }
                            });
                        }
                        return $q.when();
                    }

                    //
                    // Creates a new choice set for use in the entity model.
                    // 
                    function createNewChoiceSet() {
                        return spEntity.fromJSON({
                            typeId: 'choiceOptionSet',
                            name: jsonString(),
                            choiceOptionSetChoices: jsonRelationship()
                        });
                    }

                    //
                    // Create a new container and options on the yet to be saved form data.
                    //
                    function createOptions() {
                        var set = scope.formData.getLookup(relChoiceSet);
                        if (!set || set.dataState !== spEntity.DataStateEnum.Create) {
                            scope.formData.setLookup(relChoiceSet, createNewChoiceSet());
                        }

                        scope.model.choiceOptionSet = scope.formData.getLookup(relChoiceSet);
                        scope.model.choiceOptions = scope.model.choiceOptionSet.choiceOptionSetChoices;
                    }
                    
                    //
                    // Gets the existing options being viewed / edited.
                    //
                    function getOptions() {
                        if (!scope.isReadOnly) {

                            // there may be unsaved edits
                            scope.model.choiceOptionSet = scope.formData.getLookup(relChoiceSet);
                            if (scope.model.choiceOptionSet && scope.model.choiceOptionSet.dataState !== spEntity.DataStateEnum.Unchanged) {

                                scope.model.choiceOptionPickerOptions.selectedEntities = [scope.model.choiceOptionSet];
                                scope.model.choiceOptions = scope.model.choiceOptionSet.choiceOptionSetChoices;
                                scope.isExisting = true;

                                return $q.when();
                            }
                        }

                        // load the last saved
                        return spEntityService.getEntity(scope.formData.idP, 'name, choiceQuestionChoiceSet.{name, choiceOptionSetChoices.{' + co + '}}').then(function (question) {
                            
                            if (question.choiceQuestionChoiceSet) {

                                // an existing choice set
                                question.choiceQuestionChoiceSet._setGraph(scope.formData._graph);
                                scope.formData.setLookup(relChoiceSet, question.choiceQuestionChoiceSet);
                                scope.isExisting = true;
                            } else {

                                scope.formData.setLookup(relChoiceSet, createNewChoiceSet());
                            }

                            scope.model.choiceOptionSet = scope.formData.getLookup(relChoiceSet);
                            scope.model.choiceOptionPickerOptions.selectedEntities = [scope.model.choiceOptionSet];
                            scope.model.choiceOptions = scope.model.choiceOptionSet.choiceOptionSetChoices;
                        });
                    }

                    //
                    // Responds to a change of the isExisting flag.
                    //
                    function isExistingWatcher(value) {
                        if (scope.isExisting === false) {
                            scope.model.choiceOptionPickerOptions.selectedEntity = null;
                            scope.model.choiceOptionPickerOptions.selectedEntities = null;

                            createOptions();
                        } else {
                            scope.model.choiceOptionSet = null;
                            scope.model.choiceOptions = [];
                        }
                    }

                    //
                    // Responds to a change in the selectedEntities collection of the choice set picker.
                    //
                    function selectedEntitiesWatcher() {
                        if (scope.isExisting) {
                            var selected = scope.model.choiceOptionPickerOptions.selectedEntities;
                            if (selected && selected.length) {

                                scope.model.choiceOptionSet = _.first(selected);
                                scope.model.choiceOptionSet._setGraph(scope.formData._graph);

                                scope.formData.setLookup(relChoiceSet, scope.model.choiceOptionSet);

                                scope.model.choiceOptions = scope.model.choiceOptionSet.choiceOptionSetChoices;

                                // get the latest, if no chance of current edits
                                if (scope.model.choiceOptionSet.dataState === spEntity.DataStateEnum.Unchanged) {
                                    spEntityService.getEntity(scope.model.choiceOptionSet.idP, 'name, choiceOptionSetChoices.{' + co + '}').then(function (set) {
                                        if (set) {

                                            _.forEach(set.choiceOptionSetChoices, function (option) {
                                                option._setGraph(scope.formData._graph);
                                            });

                                            scope.model.choiceOptionSet.choiceOptionSetChoices = set.choiceOptionSetChoices;
                                            scope.model.choiceOptions = scope.model.choiceOptionSet.choiceOptionSetChoices;
                                        }
                                    });
                                }
                            }
                        }
                    }

                    //
                    // Returns the entity that has been selected in the grid.
                    //
                    function getSelectedOption() {
                        if (scope.selectedRowIndex >= 0) {
                            var grid = scope.model.choiceOptionGridOptions.ngGrid;
                            if (grid) {
                                var row = grid.rowMap.indexOf(scope.selectedRowIndex);
                                var option = scope.model.choiceOptions[row];
                                return option;
                            }
                        }
                        return null;
                    }

                    //
                    // Selects and scrolls to an item in the grid.
                    //
                    function scrollTo(item, select) {
                        $timeout(function () {
                            var rows = scope.model.choiceOptions.length;
                            if (rows > 1) {
                                var grid = scope.model.choiceOptionGridOptions.ngGrid;
                                if (grid) {

                                    var i = grid.data.indexOf(item);
                                    scope.selectedRowIndex = grid.rowMap[i];

                                    // select
                                    if (select) {
                                        scope.model.choiceOptionGridOptions.selectItem(i, true);
                                    }

                                    // scroll to selected
                                    grid.$viewport.scrollTop(scope.selectedRowIndex * grid.config.rowHeight);
                                }
                            }
                        }, 0);
                    }

                    //
                    // Creates a new option.
                    //
                    scope.newOption = function () {
                        var n = 1;
                        var startName = 'New Option';
                        var newName = startName;
                        while (_.find(scope.model.choiceOptions, { name: newName })) {
                            newName = startName + ' ' + n;
                            n++;
                        }

                        var o = (_.max(_.map(scope.model.choiceOptions, 'choiceOptionOrder')) + 1) || 0;

                        var option = spEntity.fromJSON({
                            typeId: 'choiceOption',
                            name: jsonString(newName),
                            description: jsonString(),
                            choiceOptionOrder: jsonInt(o),
                            choiceOptionValue: jsonDecimal()
                        });

                        scope.model.choiceOptions.add(option);

                        // scroll the grid
                        scrollTo(option, true);
                    };
                    
                    //
                    // Removes the currently selected option.
                    //
                    scope.removeOption = function () {
                        var selected = getSelectedOption();
                        if (selected) {
                            //scope.model.choiceOptions.remove(selected);
                            scope.model.choiceOptionSet.choiceOptionSetChoices.deleteEntity(selected);
                        }
                    };

                    //
                    // Decreases the order value of a choice option.
                    //
                    scope.moveUpOption = function () {
                        var selected = getSelectedOption();
                        if (selected) {
                            var o = selected.choiceOptionOrder;
                            if (!o && o !== 0) {
                                o = scope.model.choiceOptions.length; // set to large number if undefined
                            }
                            if (o > 0) {
                                var next = _.maxBy(_.filter(scope.model.choiceOptions, function (option) { return (option.choiceOptionOrder >= 0 && option.choiceOptionOrder < o); }), 'choiceOptionOrder');
                                if (next) {
                                    var n = next.choiceOptionOrder;
                                    next.choiceOptionOrder = o;
                                    selected.choiceOptionOrder = n;
                                } else {
                                    selected.choiceOptionOrder = 0;
                                }

                                // this is awful.
                                scope.model.choiceOptionGridOptions.sortBy('choiceOptionOrder');
                                scope.model.choiceOptionGridOptions.sortBy('choiceOptionOrder');

                                scrollTo(selected);
                            }
                        }
                    };

                    //
                    // Increases the order value of a choice option.
                    //
                    scope.moveDownOption = function () {
                        var selected = getSelectedOption();
                        if (selected) {
                            var max = _.max(_.map(scope.model.choiceOptions, 'choiceOptionOrder')) || 0;

                            _.forEach(scope.model.choiceOptions, function(option) {
                                if (!option.choiceOptionOrder && option.choiceOptionOrder !== 0) {
                                    option.choiceOptionOrder = max++;
                                }
                            });

                            var o = selected.choiceOptionOrder;

                            if (max > o) {
                                var next = _.minBy(_.filter(scope.model.choiceOptions, function (option) { return (option.choiceOptionOrder >= 0 && option.choiceOptionOrder > o); }), 'choiceOptionOrder');
                                if (next) {
                                    var n = next.choiceOptionOrder;
                                    next.choiceOptionOrder = o;
                                    selected.choiceOptionOrder = n;
                                } else {
                                    selected.choiceOptionOrder = 0;
                                }

                                // yuk.
                                scope.model.choiceOptionGridOptions.sortBy('choiceOptionOrder');
                                scope.model.choiceOptionGridOptions.sortBy('choiceOptionOrder');

                                scrollTo(selected);
                            }
                        }
                    };

                    /////
                    // Control sizing and placement.
                    /////
                    scope.$on('gather', function (event, callback) {
                        callback(scope.formControl, scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spChoiceQuestionOptionsEditor/spChoiceQuestionOptionsEditor.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());