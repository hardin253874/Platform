// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.common.ui.entityPickerControllers')
        .controller('singleEntityPickerController', SingleEntityPickerController);

    function SingleEntityPickerController($scope, spEntityService) {
        'ngInject';

        /*
         *  A controller that is used by the single entity resource pickers such
         *  as the radio and combo pickers.
         */

        $scope.entities = [];
        $scope.selectedEntity = null;
        $scope.selectOptionStyle = {};
        // This function sets the selected entity property
        function setSelectedEntity() {
            var isAlias,
                isId,
                entity;

            // Return if we don't have the entities
            // or the selected entity id yet.
            if ($scope.entities.length === 0 ||
                !$scope.options) {
                return;
            }

            if (!$scope.options.selectedEntityId ||
                $scope.options.selectedEntityId === 0) {
                $scope.selectedEntity = null;
                return;
            }

            isAlias = _.isString($scope.options.selectedEntityId);
            isId = _.isNumber($scope.options.selectedEntityId);

            // Find the entity using id or alias
            entity = _.find($scope.entities, function (e) {

                if (isAlias) {
                    return $scope.options.selectedEntityId === e.alias();
                }

                if (isId) {
                    return $scope.options.selectedEntityId === e.id();
                }

                return false;
            });

            // Set the local selected entity.
            if (entity !== $scope.selectedEntity) {
                $scope.selectedEntity = entity;
            }
        }

        // This function is used to filter hidden aliases
        function filterHiddenAliases(items, hiddenAliases) {
            if (!hiddenAliases || hiddenAliases.length === 0) {
                return items;
            }

            if (hiddenAliases.length > 0) {
                items = _.reject(items, function (e) {
                    return _.includes(hiddenAliases, e.alias);
                });
            }

            return items;
        }

        // Watch for already loaded entities
        $scope.$watch('options.entities', function () {
            if (!$scope.options || (!$scope.options.useEntitiesToPopulatePicker && !$scope.options.entityTypeId && !$scope.options.entities)) {
                $scope.entities = [];
                $scope.selectedEntity = null;
                return;
            }

            if (!$scope.options.entities) {
                return;
            }

            $scope.entities = $scope.options.entities;

            setSelectedEntity();
        });

        if ($scope.options && !$scope.options.useEntitiesToPopulatePicker) {
            // Watch the entity type id for changes
            $scope.$watch('options.entityTypeId', function() {
                if (!$scope.options || (!$scope.options.entityTypeId && !$scope.options.entities)) {
                    $scope.entities = [];
                    $scope.selectedEntity = null;
                    return;
                }

                if (!$scope.options.entityTypeId) {
                    return;
                }

                // Get the list of entities from the server and re-order
                spEntityService.getInstancesOfType($scope.options.entityTypeId, 'alias, name, description, enumOrder', { hint: 'entityPicker', batch: true })
                    .then(function(items) {
                        var filteredItems = filterHiddenAliases(items, $scope.options.hiddenAliases);

                        filteredItems = _.uniqBy(filteredItems, function(fi) {
                            return fi.entity.id();
                        });

                        filteredItems.sort(function(item1, item2) {
                            var eo1 = item1.entity.field('core:enumOrder'),
                                eo2 = item2.entity.field('core:enumOrder');

                            if (_.isNaN(eo1)) {
                                eo1 = 0;
                            }

                            if (_.isNaN(eo2)) {
                                eo2 = 0;
                            }

                            // Sort by enum then by name
                            if (eo1 === eo2) {

                                // js sorting shows upper case letters first and then lower case. which means record starting with 'a' will come after 'Z'.
                                var name1 = item1.entity.field('core:name');
                                name1 = _.isString(name1) ? name1.toLowerCase() : '';

                                var name2 = item2.entity.field('core:name');
                                name2 = _.isString(name2) ? name2.toLowerCase() : '';

                                return name1 < name2 ? -1 : +1;
                            }

                            return eo1 < eo2 ? -1 : +1;
                        });

                        // restrict the number of items to the given number
                        if ($scope.restrictNumRecords && _.isNumber($scope.restrictNumRecords) && filteredItems.length > $scope.restrictNumRecords) {
                            filteredItems = filteredItems.slice(0, $scope.restrictNumRecords);

                            // add a dummy item to indicate that there are more items than what is available in combobox
                            var dummyItem = { entity: getDummyEntity() };
                            filteredItems.push(dummyItem);
                        }

                        $scope.entities = _.map(filteredItems, function(fi) {
                            return fi.entity;
                        });

                        setSelectedEntity();

                    }, function(error) {
                        $scope.entities = [];

                        setSelectedEntity();
                    });
            });
        }


        // Watch the selected entity id for changes and set the selected entity
        $scope.$watch('options.selectedEntityId', function () {
            setSelectedEntity();
        });

        // Watch the showSelectOption flag
        $scope.$watch('options.showSelectOption', function () {
            if ($scope.options && $scope.options.showSelectOption !== undefined && $scope.options.showSelectOption !== null && $scope.options.showSelectOption === false) {
                $scope.selectOptionStyle = {display:'none'};
            }
        });

        // Watch the disabled flag
        $scope.$watch('options.disabled', function () {
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

        // clears the local selected entity
        $scope.clear = function () {
            if ($scope.selectedEntity) {
                $scope.selectedEntity = null;
            }
        };

        // Update the output value if there has been a change in selected entity (and we actually have entities)
        $scope.$watch('selectedEntity', function () {
            if ($scope.options) {
                if ($scope.entities.length > 0) {
                    var isAlias,
                        isId;

                    if (!$scope.options) {
                        return;
                    }

                    if (!$scope.selectedEntity) {
                        if ($scope.options.selectedEntityId !== 0) {
                            $scope.options.selectedEntityId = 0;
                        }
                        if (angular.isDefined($scope.options.selectedEntity) &&
                            $scope.options.selectedEntity !== null) {
                            $scope.options.selectedEntity = null;
                        }
                        return;
                    }

                    isAlias = _.isString($scope.options.selectedEntityId);
                    isId = _.isNumber($scope.options.selectedEntityId);

                    if ((isId && $scope.options.selectedEntityId !== $scope.selectedEntity.id()) ||
                        (isAlias && $scope.options.selectedEntityId !== $scope.selectedEntity.alias())) {
                        $scope.options.selectedEntityId = $scope.selectedEntity.id();
                    }

                    if (angular.isDefined($scope.options.selectedEntity) &&
                        ($scope.options.selectedEntity === null ||
                        ($scope.options.selectedEntity.id() !== $scope.selectedEntity.id()))) {
                        $scope.options.selectedEntity = $scope.selectedEntity;
                    }
                } else {
                    if (!$scope.options.useEntitiesToPopulatePicker && !$scope.options.entityTypeId) {
                        if (angular.isDefined($scope.options.selectedEntity) &&
                            $scope.options.selectedEntity !== null) {
                            $scope.options.selectedEntity = null;
                        }
                        if ($scope.options.selectedEntityId !== 0) {
                            $scope.options.selectedEntityId = 0;
                        }
                    }
                }
            }
        });

        function setDisabled() {
            $scope.disabled = !!$scope.options.isDisabled || !!$scope.options.disabled || !!$scope.options.modifyAccessDenied;
        }
        // Creates a dummy entity that represents a dummy combobox item '[more..]'
        function getDummyEntity() {
            var dummyEntity = spEntity.createEntityOfType('dummyEntityType', '[more..]', '');
            return dummyEntity;
        }
    }

}());