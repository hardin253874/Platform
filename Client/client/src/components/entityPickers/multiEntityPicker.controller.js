// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.common.ui.entityPickerControllers')
        .controller('multiEntityPickerController', MultiEntityPickerController);

    function MultiEntityPickerController($scope, spEntityService) {
        'ngInject';


        /*
         *  A controller that is used by the multi entity resource pickers such
         *  as the checkbox picker.
         */

        $scope.entities = [];
        $scope.entityCheckBoxItems = [];

        // This function sets the selected entity checkbox items
        function setSelectedEntityCheckBoxItems() {
            // Return if we don't have the entities
            // or the selected entity ids yet.
            if ($scope.entities.length === 0) {
                $scope.entityCheckBoxItems = [];
                return;
            }

            $scope.entityCheckBoxItems = _.map($scope.entities, function (e) {
                // Determine if this entity's id is in the selected list
                var foundEntity = null;

                if ($scope.options &&
                    $scope.options.selectedEntityIds &&
                    $scope.options.selectedEntityIds.length !== 0) {
                    foundEntity = _.find($scope.options.selectedEntityIds, function (eId) {
                        var isAlias,
                            isId;

                        isAlias = _.isString(eId);
                        isId = _.isNumber(eId);

                        if (isAlias) {
                            return eId === e.alias();
                        }

                        if (isId) {
                            return eId === e.id();
                        }

                        return false;
                    });
                }

                return { entity: e, selected: (foundEntity !== null && foundEntity !== undefined) };
            });
        }

        // This function is used to filter hidden aliases
        function filterHiddenAliases(items, hiddenAliases) {
            if (!hiddenAliases || hiddenAliases.length === 0) {
                return items;
            }

            if (hiddenAliases.length > 0) {
                items = _.reject(items, function (e) {
                    return _.includes(hiddenAliases, e.alias());
                });
            }

            return items;
        }

        if ($scope.options && !$scope.options.isInDesign) {
            // Watch for already loaded entities
            $scope.$watch('options.entities', function () {
                var updatedSelected;

                if (!$scope.options || (!$scope.options.useEntitiesToPopulatePicker && !$scope.options.entityTypeId && !$scope.options.entities)) {
                    $scope.entities = [];
                    setSelectedEntityCheckBoxItems();
                    $scope.selectionChanged();
                    return;
                }

                if (!$scope.options.entities) {
                    return;
                }

                $scope.entities = $scope.options.entities;

                if ($scope.options &&
                    $scope.entityCheckBoxItems &&
                    $scope.entityCheckBoxItems.length) {
                    updatedSelected = true;
                }

                setSelectedEntityCheckBoxItems();

                if (updatedSelected) {
                    $scope.selectionChanged();
                }
            });

            if (!$scope.options.useEntitiesToPopulatePicker) {
                // Watch the entity type id for changes
                $scope.$watch('options.entityTypeId', function () {
                    if (!$scope.options || (!$scope.options.entityTypeId && !$scope.options.entities)) {
                        $scope.entities = [];
                        setSelectedEntityCheckBoxItems();
                        $scope.selectionChanged();
                        return;
                    }

                    if (!$scope.options.entityTypeId) {
                        return;
                    }

                    if ($scope.options.entityType && $scope.options.entityType.toType.getInstancesOfType) {
                        var pickerItems = $scope.options.entityType.toType.getInstancesOfType();
                        $scope.entities = sortPickerItems(pickerItems);
                    } else {

                        // Get the list of entities from the server and re-order
                        spEntityService.getEntitiesOfType($scope.options.entityTypeId, 'alias, name, description, enumOrder', { hint: 'entityPicker', batch: true })
                            .then(function (items) {

                                $scope.entities = sortPickerItems(items);

                                setSelectedEntityCheckBoxItems();
                                $scope.selectionChanged();
                            }, function (error) {
                                $scope.entities = [];
                                setSelectedEntityCheckBoxItems();
                                $scope.selectionChanged();
                            });
                    }
                });
            }

            // Watch the selected entity id for changes and set the selected entity
            $scope.$watch('options.selectedEntityIds', function () {
                setSelectedEntityCheckBoxItems();
            });

            // Watch the disabled flag
            $scope.$watch('options.disabled', function (value) {
                setDisabled();
            });

            // Watch the isDisabled flag
            $scope.$watch('options.isDisabled', function () {
                setDisabled();
            });

            // Watch the modifyAccessDenied flag
            $scope.$watch('options.modifyAccessDenied', function () {
                setDisabled();
            });

            $scope.$on('valuesChanged', function () {
                if ($scope.options.useEntitiesToPopulatePicker) {
                    return;
                }

                $scope.entities = null;
                $scope.options.entityType.toType.instancesOfType = null;
                // Get the list of entities from the server and re-order
                spEntityService.getEntitiesOfType($scope.options.entityTypeId, 'alias, name, description, enumOrder', { hint: 'entityPicker', batch: true })
                    .then(function (items) {
                        $scope.entities = sortPickerItems(items);
                    }, function (error) {
                        $scope.entities = [];
                    });
            });

            // Update the output values
            $scope.selectionChanged = function () {
                var selectedCheckBoxItems,
                    selectedCheckBoxItem,
                    haveChanges = false,
                    i,
                    max,
                    selectedIdsCopy,
                    foundEntityId,
                    selectedEntitiesResult = [],
                    selectedIdsResult = [];

                if (!$scope.options || !$scope.entityCheckBoxItems || $scope.entityCheckBoxItems.length === 0) {
                    $scope.options.selectedEntityIds = [];
                    if (angular.isDefined($scope.options.selectedEntities)) {
                        $scope.options.selectedEntities = [];
                    }
                    return;
                }

                function findEntityId(eId) {
                    var isAlias,
                        isId;

                    isAlias = _.isString(eId);
                    isId = _.isNumber(eId);

                    if (isAlias) {
                        return eId === selectedCheckBoxItem.entity.alias();
                    }

                    if (isId) {
                        return eId === selectedCheckBoxItem.entity.id();
                    }

                    return false;
                }

                // Find all the checkbox items that are selected
                selectedCheckBoxItems = _.filter($scope.entityCheckBoxItems, function (cb) {
                    return cb.selected;
                });

                if ($scope.options.hideDescription) {
                    $scope.hideDescription = true;
                } else {
                    $scope.hideDescription = false;
                }

                // copy the selected entity ids
                selectedIdsCopy = $scope.options.selectedEntityIds.slice();

                for (i = 0, max = selectedCheckBoxItems.length; i < max; i = i + 1) {
                    selectedCheckBoxItem = selectedCheckBoxItems[i];

                    selectedEntitiesResult.push(selectedCheckBoxItem.entity);

                    // See if this id is already in the list of selected ids
                    foundEntityId = _.find(selectedIdsCopy, findEntityId);

                    if (foundEntityId) {
                        // Found existing id add it to result as is
                        selectedIdsResult.push(foundEntityId);
                    } else {
                        // Not found, add it's id
                        haveChanges = true;
                        selectedIdsResult.push(selectedCheckBoxItem.entity.id());
                    }
                }

                if (haveChanges ||
                    selectedIdsResult.length !== $scope.options.selectedEntityIds.length) {
                    $scope.options.selectedEntityIds = selectedIdsResult;

                }

                if (angular.isDefined($scope.options.selectedEntities)) {
                    $scope.options.selectedEntities = selectedEntitiesResult;
                }
            };

            if ($scope.options) {
                $scope.hideDescription = $scope.options.hideDescription;
            }
        } else {
            $scope.entityCheckBoxItems.push({
                selected: true,
                entity: {
                    getName: function () {
                        return 'Sample';
                    },
                    getDescription: function () {
                        return '';
                    }
                }
            });

            $scope.entityCheckBoxItems.push({
                selected: true,
                entity: {
                    getName: function () {
                        return 'Sample 2';
                    },
                    getDescription: function () {
                        return '';
                    }
                }
            });

            $scope.disabled = true;
        }

        function sortPickerItems(items) {
            var filteredItems = filterHiddenAliases(items, $scope.options.hiddenAliases);

            filteredItems = _.uniqBy(filteredItems, function (fi) {
                return fi.id();
            });

            filteredItems.sort(function (item1, item2) {
                var eo1 = item1.field('core:enumOrder'),
                    eo2 = item2.field('core:enumOrder');

                if (_.isNaN(eo1)) {
                    eo1 = 0;
                }

                if (_.isNaN(eo2)) {
                    eo2 = 0;
                }

                // Sort by enum then by name
                if (eo1 === eo2) {
                    return item1.field('core:name') < item2.field('core:name') ? -1 : +1;
                }

                return eo1 < eo2 ? -1 : +1;
            });

            return filteredItems;
        }

        function setDisabled() {
            $scope.disabled = !!$scope.options.isDisabled || !!$scope.options.disabled || !!$scope.options.modifyAccessDenied;
        }
    }
}());