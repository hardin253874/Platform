// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the aggregate options dialog.    
    * 
    * @module spAggregateOptionsDialog   
    * @example
        
    Using the spAggregateOptionsDialog:
    
    spAggregateOptionsDialog.showModalDialog(options).then(function(result) {
    });
    
    where options is an object with the following properties:
        - name - {string}. The name of the column.
        - type - {string}. The type of the column.         
        - haveGroups - {bool}. True if groups exist.                
        - showGrandTotals - {bool}. True to show grand totals, false otherwise.
        - showSubTotals - {bool}. True to show sub totals, false otherwise.
        - aggregateMethodIds - {array of string}. The selected aggregate method entity aliases.

    where result is:
        - object with the following properties if ok is clicked
            - showGrandTotals - {bool}. As above.
            - showSubTotals - {bool}. As above.
            - aggregateMethodIds - {array of string}. As above.
        - false, if cancel is clicked       
    */
    angular.module('mod.common.ui.spAggregateOptionsDialog', [
        'ui.bootstrap',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spEntityCheckBoxPicker',
        'mod.common.spEntityService'
    ])
        .controller('spAggregateOptionsDialogController', function ($scope, $uibModalInstance, options, spEntityService) {

            // Setup the dialog model
            $scope.model = {
                errors: [],
                type: options.type,
                name: options.name,
                haveGroups: options.haveGroups,
                showGrandTotals: options.showGrandTotals,
                showSubTotals: options.showSubTotals,
                showOptionLabels: !spUtils.isNullOrUndefined(options.showOptionLabels) ? options.showOptionLabels : true,
                disableOptionLabel : false,
                aggregateMethodPickerOptions: {
                    selectedEntities: [],
                    selectedEntityIds: [],
                    entities: [],
                    disabled: false
                }
            };                                  

            // Get the aggregate methods
            spEntityService.getInstancesOfType('core:aggregateMethodEnum', 'alias, name, enumOrder', { hint: 'aggOptions', batch: true }).then(function (items) {
                var selectedEntities = [];
                var entities = _.map(items, function (i) {
                    return i.entity;
                });                

                // Filter these based on the type
                filterAggregateMethodEntities(entities);

                // Sort them based on their enum order
                sortAggregateMethodEntities(entities);

                $scope.model.aggregateMethodPickerOptions.entities = entities;

                $scope.model.aggregateMethodPickerOptions.selectedEntityIds = getAggregateMethodIds(options.aggregateMethodIds);

                selectedEntities = _.map(options.aggregateMethodIds, function (id) {
                    return _.find(entities, function (e) {
                        return e.eid().getAlias() === id;
                    });
                });

                $scope.model.aggregateMethodPickerOptions.selectedEntities = _.filter(selectedEntities, function (se) {
                    return se;
                });
            });


            // Methods

            function getAggregateMethodIds(aggregateMethodIds) {
                if (aggregateMethodIds) {
                    return _.map(aggregateMethodIds, function (id) {
                        return 'core:' + id;
                    });
                } else {
                    return [];
                }
            }


            // Update the state of the aggregate method picker
            function updateAggMethodPickerState() {
                $scope.model.aggregateMethodPickerOptions.disabled =
                    !($scope.model.showGrandTotals ||
                    ($scope.model.haveGroups && $scope.model.showSubTotals));

                $scope.model.disableOptionLabel =
                    !($scope.model.showGrandTotals ||
                    ($scope.model.haveGroups && $scope.model.showSubTotals));
            }


            // Sort the aggregate method entities based on enum order.
            function sortAggregateMethodEntities(entities) {
                entities.sort(function (entity1, entity2) {
                    var name1,
                        name2,
                        eo1 = entity1.field('core:enumOrder'),
                        eo2 = entity2.field('core:enumOrder');

                    if (_.isNaN(eo1)) {
                        eo1 = 0;
                    }

                    if (_.isNaN(eo2)) {
                        eo2 = 0;
                    }

                    // Sort by enum then by name
                    if (eo1 === eo2) {

                        // js sorting shows upper case letters first and then lower case. which means record starting with 'a' will come after 'Z'.
                        name1 = entity1.field('core:name');
                        name1 = _.isString(name1) ? name1.toLowerCase() : '';

                        name2 = entity2.field('core:name');
                        name2 = _.isString(name2) ? name2.toLowerCase() : '';

                        return name1 < name2 ? -1 : +1;
                    }

                    return eo1 < eo2 ? -1 : +1;
                });
            }


            // Filter the aggregate method entities based on the filter type.
            function filterAggregateMethodEntities(entities) {
                var isNumeric = false,
                    isDateTime = false,
                    isChoiceType = false,
                    isBoolean = false;

                switch ($scope.model.type) {
                    case spEntity.DataType.Bool:
                        isBoolean = true;
                        break;
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                    case spEntity.DataType.Int32:
                        isNumeric = true;
                        break;
                    case spEntity.DataType.Date:
                    case spEntity.DataType.Time:
                    case spEntity.DataType.DateTime:
                        isDateTime = true;
                        break;
                    case 'ChoiceRelationship':
                        isChoiceType = true;
                        break;
                }

                _.remove(entities, function (entity) {
                    var alias = entity.eid().getAlias(),
                        remove = false;

                    switch (alias) {
                    case 'aggSum':
                    case 'aggAverage':
                    case 'aggStandardDeviation':
                    case 'aggPopulationStandardDeviation':
                    case 'aggVariance':
                    case 'aggPopulationVariance':
                        remove = !isNumeric;
                        break;
                    case 'aggMin':
                    case 'aggMax':
                        remove = !isNumeric && !isDateTime && !isChoiceType;
                        break;
                    case 'aggList':
                        remove = true;
                        break;
                    case 'aggCountUniqueItems':
                        remove = isBoolean;
                        break;
                    case 'aggCountUniqueNotBlanks':
                        //remove aggCountUniqueNotBlanks option
                        remove = true;
                        break;
                    }

                    return remove;
                });
            }


            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
            };


            // Add an error
            $scope.model.addError = function (errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            };


            // Ok click handler
            $scope.ok = function () {
                var aggregateOptionsResult = {                                        
                };

                $scope.model.clearErrors();

                aggregateOptionsResult.showGrandTotals = $scope.model.showGrandTotals;
                aggregateOptionsResult.showOptionLabels = $scope.model.showOptionLabels;
                if ($scope.model.haveGroups) {
                    aggregateOptionsResult.showSubTotals = $scope.model.showSubTotals;
                }

                aggregateOptionsResult.aggregateMethodIds = _.map($scope.model.aggregateMethodPickerOptions.selectedEntities, function (entity) {
                    return entity.eid().getAlias();
                });

                if ($scope.model.errors.length === 0) {
                    $uibModalInstance.close(aggregateOptionsResult);
                }
            };


            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(false);
            };


            // Watch for changes to the show grand totals checkbox
            $scope.$watch('model.showGrandTotals', function () {
                updateAggMethodPickerState();
            });


            // Watch for changes to the show sub totals checkbox
            $scope.$watch('model.showSubTotals', function () {
                updateAggMethodPickerState();
            });
        })
        .factory('spAggregateOptionsDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        keyboard: true,
                        backdropClick: false,
                        templateUrl: 'reportBuilder/dialogs/aggregateOptionsDialog/spAggregateOptionsDialog.tpl.html',
                        controller: 'spAggregateOptionsDialogController',
                        windowClass: 'spAggregateOptionsDialog',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    if (defaultOverrides) {
                        angular.extend(dialogDefaults, defaultOverrides);
                    }

                    return spDialogService.showModalDialog(dialogDefaults);
                }
            };

            return exports;
        });
}());