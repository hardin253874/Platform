// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
      * Module implementing a relationship filters dialog.        
      *
      * @module spRelationshipFiltersDialog    
      * @example            
         
      Using the spRelationshipFiltersDialog:
       
      */
    angular.module('mod.app.configureDialog.relationshipProperties.spRelationshipFiltersDialog',
        ['ui.bootstrap', 'mod.common.ui.spDialogService', 'mod.common.spEntityService', 'mod.common.ui.spBusyIndicator', 'mod.app.editFormServices', 'sp.app.settings'])
        .controller('spRelationshipFiltersDialogController', function($scope, $uibModalInstance, options, spEntityService, spEditForm, spAppSettings) {
            var relationshipsRequest = 'inherits*, { relationships, reverseRelationships }.{ alias, name, toName, fromName, cardinality.alias, description, { fromType, toType }.{ id } }';


            // Sets the available relationship filters for the specified control filter
            function setAvailableRelationshipFilters(relationshipControlFilter) {
                if (!relationshipControlFilter) {
                    return;
                }

                if (relationshipControlFilter.selectedRelationshipControlFilter &&
                    relationshipControlFilter.selectedRelationshipControlFilter.relationshipControl) {
                    relationshipControlFilter.availableRelationshipFilters = $scope.model.controlsToRelationshipFilters[relationshipControlFilter.selectedRelationshipControlFilter.relationshipControl.id()];
                } else {
                    relationshipControlFilter.availableRelationshipFilters = [{ name: 'No relationships found' }];
                }
            }


            // Add an error to the dialog.
            function addError(errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            }


            // Apply the filter changes to the form control.
            function applyChanges() {
                var relationshipControlFilters,
                    relationControlFiltersInstances;

                // We have filters. Ensure the filters relationship exists
                if ($scope.model.relationshipControlFilters.length) {
                    $scope.model.formControl.registerRelationship('console:relationshipControlFilters');
                }

                relationshipControlFilters = $scope.model.formControl.getRelationship('console:relationshipControlFilters');

                // Create a dictionary of filters, keyed by the control id
                relationControlFiltersInstances = _.keyBy(relationshipControlFilters.getInstances(), function(ri) {
                    var relationshipControl;

                    if (ri.entity) {
                        relationshipControl = ri.entity.getLookup('console:relationshipControlFilter');
                    }

                    return relationshipControl ? relationshipControl.id() : -1;
                });

                // Handle additions/updates
                _.forEach($scope.model.relationshipControlFilters, function (rf, index) {                    
                    var newControlId = rf.selectedRelationshipControlFilter.relationshipControl.id(),                    
                        filterRelInstance = relationControlFiltersInstances[newControlId], // Find the filter details entry for the current control  
                        filterEntity;

                    if (filterRelInstance) {
                        // Update        

                        // Resurrect any deleted instances
                        if (filterRelInstance.getDataState() === spEntity.DataStateEnum.Delete) {
                            filterRelInstance.setDataState(spEntity.DataStateEnum.Unchanged);
                        }

                        filterEntity = filterRelInstance.entity;                        
                    } else {
                        // Create
                        filterEntity = spEntity.fromJSON({
                            typeId: 'console:relationshipControlFilterDetails'
                        });

                        relationshipControlFilters.add(filterEntity);
                    }

                    filterEntity.setField('console:relationshipControlFilterOrdinal', index, spEntity.DataType.Int32);
                    filterEntity.setLookup('console:relationshipFilter', rf.selectedRelationshipFilter.relationship);
                    filterEntity.setLookup('console:relationshipControlFilter', rf.selectedRelationshipControlFilter.relationshipControl);
                    filterEntity.setLookup('console:relationshipDirectionFilter', rf.selectedRelationshipFilter.direction);
                });

                // Handle deletes
                _.forOwn(relationControlFiltersInstances, function(relInstance, controlId) {
                    var found = _.find($scope.model.relationshipControlFilters, function(rf) {
                        var newControlId = rf.selectedRelationshipControlFilter.relationshipControl.id().toString();
                        return newControlId === controlId;
                    });

                    if (!found) {
                        relationshipControlFilters.deleteEntity(relInstance.entity);
                    }
                });
            }


            // Validate the selected filters
            function validateFilters() {
                var haveInvalidControl = false,
                    haveInvalidRelationship = false,
                    filteredControls = {};

                _.forEach($scope.model.relationshipControlFilters, function(rf) {
                    if (!rf.selectedRelationshipControlFilter ||
                        !rf.selectedRelationshipControlFilter.relationshipControl) {
                        haveInvalidControl = true;
                    }

                    if (!rf.selectedRelationshipFilter ||
                        !rf.selectedRelationshipFilter.relationship) {
                        haveInvalidRelationship = true;
                    }

                    if (rf.selectedRelationshipControlFilter &&
                        rf.selectedRelationshipControlFilter.relationshipControl) {
                        if (!_.has(filteredControls, rf.selectedRelationshipControlFilter.relationshipControl.id())) {
                            filteredControls[rf.selectedRelationshipControlFilter.relationshipControl.id()] = true;
                        } else {
                            addError('The field \'' + rf.selectedRelationshipControlFilter.name + '\' has been used in multiple filters.');
                        }
                    }
                });

                if (haveInvalidControl) {
                    addError('One or more filters does not have a valid field selected.');
                }

                if (haveInvalidRelationship) {
                    addError('One or more filters does not have a valid relationship selected.');
                }

                return ($scope.model.errors.length === 0);
            }


            // Sets the busy indicator for the dialog
            function setBusy(busy) {
                $scope.model.busyIndicator.isBusy = busy;
            }
            

            // Gets the applicable relationship filters for the specified target type
            function getRelationshipFilters(targetType) {
                var relationshipFilters = [],
                    sortedRelationshipFilters = [];

                // For each filter relationship get the entity type that is being 
                // rendered. Applicable relationships are ones that can
                // point to the target type.
                _.forEach($scope.model.filteredTypeRelationships, function (r) {                    
                    var isReverse = r.isReverse(),
                        relationship = r.getEntity(),                        
                        relType = isReverse ? relationship.fromType : relationship.toType,
                        relTypes,
                        name,
                        found;                    

                    if (!relType) {
                        return true;
                    }

                    relType = $scope.model.filteredTypeRelationshipsToFromTypes[relType.id()];

                    if (!relType) {
                        return true;
                    }

                    relTypes = spResource.getDerivedTypesAndSelf(relType);

                    found = _.some(relTypes, function (t) {
                        return t.id() === targetType.id() ||
                            ($scope.model.showAllRels && t.nsAlias === 'core:type'); // dev mode
                    });

                    if (found) {
                        if (isReverse) {
                            name = relationship.fromName || relationship.name;
                        } else {
                            name = relationship.toName || relationship.name;
                        }

                        relationshipFilters.push({
                            relationship: relationship,
                            direction: isReverse ? 'reverse' : 'forward',
                            name: name
                        });
                    }

                    return true;
                });

                if (relationshipFilters.length) {
                    sortedRelationshipFilters = _.sortBy(relationshipFilters, 'name');
                    sortedRelationshipFilters.unshift({
                        name: '[Select]'
                    });
                }

                return sortedRelationshipFilters;
            }


            // Get the alias of the control type
            function getControlTypeAlias(formControl) {
                var type;

                if (formControl && formControl.getType) {
                    type = formControl.getType();

                    if (type) {
                        return type._alias;
                    }
                }

                return undefined;
            }


            // Get all the relationship controls on the form that are candidates
            // as filter controls
            function getAllCandidateFilterSourceRelationshipControls(form) {
                var relationshipControlsOnForm = [];

                if (!form) {
                    return relationshipControlsOnForm;
                }

                relationshipControlsOnForm = _.filter(spEditForm.getFormControls(form), function (formControl) {
                    var typeAlias;

                    if (!formControl) {
                        return false;
                    }

                    typeAlias = getControlTypeAlias(formControl);
                    if (typeAlias === 'inlineRelationshipRenderControl' ||
                        typeAlias === 'multiChoiceRelationshipRenderControl' ||
                        typeAlias === 'dropDownRelationshipRenderControl' ||
                        typeAlias === 'choiceRelationshipRenderControl') {
                        return true;
                    }

                    return false;
                });

                return relationshipControlsOnForm;
            }


            // Get all the types that any filtered relationships
            // refer to.
            function getFilteredTypeRelationshipsToFromTypes(targetType) {
                var typeIds = [];

                // Get all the relationships from the target type
                $scope.model.filteredTypeRelationships = new spResource.Type(targetType).getAllRelationships();

                $scope.model.filteredTypeRelationshipsToFromTypes = {};
                
                // Get all candidate source types
                _.forEach($scope.model.filteredTypeRelationships, function (r) {
                    var isReverse = r.isReverse(),
                        relationship = r.getEntity(),
                        type = isReverse ? relationship.fromType : relationship.toType;
                    if (type) {
                        typeIds.push(type.id());
                    }
                });

                typeIds = _.uniq(typeIds);

                // Get all the derived types of the source types
                return spEntityService.getEntities(typeIds, 'isOfType.alias, derivedTypes*').then(function (entities) {
                    $scope.model.filteredTypeRelationshipsToFromTypes = _.keyBy(entities, function(e) {
                        return e.id();
                    });                    
                });
            }


            // Initialise all the canidate filter controls and relationships
            function initialiseAvailableControlsAndRelationships() {
                var relationshipControlFilters = [];                    

                $scope.model.controlsToRelationshipFilters = {};
                $scope.model.availableRelationshipControlFilters = [];                                

                _.forEach(getAllCandidateFilterSourceRelationshipControls($scope.model.form), function (control) {
                    if (control.id() === $scope.model.formControl.id()) {
                        return true;
                    }

                    var relationship = control.relationshipToRender,
                        targetType = control.isReversed ? relationship.fromType : relationship.toType,
                        relationshipFilters = getRelationshipFilters(targetType);

                    if (relationshipFilters.length) {
                        $scope.model.controlsToRelationshipFilters[control.id()] = relationshipFilters;

                        relationshipControlFilters.push({
                            relationshipControl: control,
                            name: spEditForm.getControlTitle(control)
                        });
                    } else {
                        $scope.model.controlsToRelationshipFilters[control.id()] = [{ name: 'No relationships found' }];
                    }

                    return true;
                });

                if (relationshipControlFilters.length) {
                    $scope.model.availableRelationshipControlFilters = _.sortBy(relationshipControlFilters, 'name');
                    $scope.model.availableRelationshipControlFilters.unshift({ name: '[Select]' });
                } else {
                    $scope.model.availableRelationshipControlFilters.push({ name: 'No controls found' });
                }
            }


            // Set the selected filters
            function setSelectedFilterValues() {
                var relationshipControlFilters,
                    filterInstances;

                $scope.model.relationshipControlFilters = [];

                if ($scope.model.formControl.hasRelationship('console:relationshipControlFilters')) {
                    relationshipControlFilters = $scope.model.formControl.getRelationship('console:relationshipControlFilters');
                    filterInstances = _.chain(relationshipControlFilters.getInstances())
                        .filter(function(ri) {
                            return ri.getDataState() !== spEntity.DataStateEnum.Delete;
                        })
                        .sortBy(function(ri) {
                            return ri.entity.getField('console:relationshipControlFilterOrdinal') || 0;
                        })
                        .map(function(ri) {
                            return ri.entity;
                        })
                        .value();
                }                                                    

                _.forEach(filterInstances, function(rf) {
                    var relationshipControl = rf.getLookup('console:relationshipControlFilter'),
                        relationship = rf.getLookup('console:relationshipFilter'),
                        direction = rf.getLookup('console:relationshipDirectionFilter'),
                        selectedControlId = relationshipControl ? relationshipControl.id() : -1,
                        selectedRelationshipId = relationship ? relationship.id() : -1,
                        selectedDirection = direction && direction._id ? direction._id._alias : null,
                        selectedRelationshipControlFilter = _.find($scope.model.availableRelationshipControlFilters, function(acf) {
                            var availableId = acf.relationshipControl ? acf.relationshipControl.id() : 0;
                            return availableId === selectedControlId;
                        });

                    var relationshipControlFilter = {
                        selectedRelationshipControlFilter: selectedRelationshipControlFilter,
                        selectedRelationshipFilter: null,
                        availableRelationshipFilters: []
                    };

                    setAvailableRelationshipFilters(relationshipControlFilter);

                    relationshipControlFilter.selectedRelationshipFilter = _.find(relationshipControlFilter.availableRelationshipFilters, function(arf) {
                        var availableId = arf.relationship ? arf.relationship.id() : 0;
                        return availableId === selectedRelationshipId && arf.direction === selectedDirection;
                    });

                    $scope.model.relationshipControlFilters.push(relationshipControlFilter);
                });
            }


            function initialise() {
                var filteredType;                

                if ($scope.model.formControl.getField('console:isReversed')) {
                    filteredType = $scope.model.formControl.relationshipToRender.fromType;
                } else {
                    filteredType = $scope.model.formControl.relationshipToRender.toType;
                }

                setBusy(true);

                spEntityService.getEntity(filteredType.id(), relationshipsRequest).then(function (e) {                    
                    return getFilteredTypeRelationshipsToFromTypes(e).then(function() {
                        initialiseAvailableControlsAndRelationships();
                        setSelectedFilterValues();
                    });                    
                }).finally(function() {
                    setBusy(false);
                });
            }
            
            // Setup the dialog model
            $scope.model = {
                errors: [],
                formControl: options.formControl,
                form: options.form,
                filteredTypeRelationships: null,
                filteredTypeRelationshipsToFromTypes: null,
                controlsToRelationshipFilters: {},
                availableRelationshipControlFilters: [],
                relationshipControlFilters: [],
                busyIndicator: {
                    type: 'spinner',
                    text: 'Please wait...',
                    placement: 'element',
                    isBusy: false
                },
                showAllRels: false
            };

            $scope.$watch('model.showAllRels', function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    initialise();
                }
            });
            
            $scope.onRelationshipControlFilterChanges = function(relationshipControlFilter) {
                $scope.clearErrors();

                setAvailableRelationshipFilters(relationshipControlFilter);

                relationshipControlFilter.selectedRelationshipFilter = _.first(relationshipControlFilter.availableRelationshipFilters);
            };


            // Clear any errors
            $scope.clearErrors = function() {
                $scope.model.errors = [];
            };

            // Show dev options 
            $scope.showAdvancedOption = function() {
                return spAppSettings.initialSettings.devMode;
            };

            // Returns true if a new filter can be added
            $scope.canAddFilter = function() {
                var availableRelationshipControlFilters = _.filter($scope.model.availableRelationshipControlFilters, 'relationshipControl');
                return $scope.model.relationshipControlFilters.length < availableRelationshipControlFilters.length;
            };


            // Adds a new filter
            $scope.addFilter = function() {
                // Sanity check
                if (!$scope.canAddFilter()) {
                    return;
                }

                $scope.clearErrors();

                var availableRelationshipFilters = [{ name: 'No relationships found' }];

                $scope.model.relationshipControlFilters.push({
                    selectedRelationshipControlFilter: _.first($scope.model.availableRelationshipControlFilters),
                    selectedRelationshipFilter: _.first(availableRelationshipFilters),
                    availableRelationshipFilters: availableRelationshipFilters
                });
            };


            // Remove the specified filter element
            $scope.removeFilter = function(index) {
                $scope.clearErrors();

                $scope.model.relationshipControlFilters.splice(index, 1);
            };


            // Ok click handler
            $scope.ok = function() {
                $scope.clearErrors();

                if (validateFilters()) {
                    applyChanges();
                }

                if ($scope.model.errors.length === 0) {
                    $uibModalInstance.close($scope.model.formControl);
                }
            };


            // Cancel click handler
            $scope.cancel = function() {
                $uibModalInstance.close(false);
            };


            // HACK - Need to force a digest cycle as things are not updating on load
            _.delay(function() {
                $scope.$apply(function() {
                    initialise();
                });
            });
        })
        .factory('spRelationshipFiltersDialog', function(spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function(options, defaultOverrides) {
                    var dialogDefaults = {
                        keyboard: true,
                        backdropClick: false,
                        templateUrl: 'configDialogs/relationshipsProperties/spRelationshipFilters/spRelationshipFiltersDialog.tpl.html',
                        controller: 'spRelationshipFiltersDialogController',
                        windowClass: 'spRelationshipFiltersDialog',
                        resolve: {
                            options: function() {
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