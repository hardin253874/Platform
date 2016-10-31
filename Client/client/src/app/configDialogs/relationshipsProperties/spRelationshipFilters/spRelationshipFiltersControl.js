// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
      * Module implementing a relationship filters control.        
      *
      * @module spRelationshipFiltersControl    
      * @example            
         
      Using the spRelationshipFiltersControl:
       
      */
    angular.module('mod.app.configureDialog.relationshipProperties.spRelationshipFiltersControl',
        ['ui.bootstrap', 'mod.app.configureDialog.relationshipProperties.spRelationshipFiltersDialog', 'mod.app.formBuilder.services.spFormBuilderService', 'mod.app.editFormServices'])
        .directive('spRelationshipFiltersControl', function(spRelationshipFiltersDialog, spFormBuilderService, spEditForm) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'configDialogs/relationshipsProperties/spRelationshipFilters/spRelationshipFiltersControl.tpl.html',
                scope: {
                    formControl: '=?'
                },
                link: function (scope) {

                    // Update the display string, which will be a comma separated list of control filter titles
                    function updateDisplayString() {
                        var relationshipControlFilters;

                        if (!scope.formControl ||
                            !scope.formControl.hasRelationship('console:relationshipControlFilters')) {
                            scope.model.displayString = '';
                            return;
                        }

                        relationshipControlFilters = scope.formControl.getRelationship('console:relationshipControlFilters');
                        scope.model.displayString = _.chain(relationshipControlFilters.getInstances())
                            .filter(function (ri) {
                                return ri.getDataState() !== spEntity.DataStateEnum.Delete;
                            })
                            .sortBy(function (ri) {
                                return ri.entity.getField('console:relationshipControlFilterOrdinal') || 0;
                            })
                            .map(function (ri) {
                                var relationshipControl = ri.entity.getLookup('console:relationshipControlFilter');
                                return spEditForm.getControlTitle(relationshipControl);
                            })
                            .join(', ')
                            .value();
                    }


                    scope.model = {
                        displayString: ''
                    };


                    scope.$watch('formControl', function() {
                        updateDisplayString();
                    });


                    scope.$watch('formControl.isOfType', function () {
                        var withType = scope.formControl.isOfType && scope.formControl.isOfType.length > 0;
                        if (withType) {
                            _.forEach(scope.formControl.isOfType, function (type) {
                                //the type object is system type which cannot be updated, reset back to unchanged
                                if (type && type.getDataState() === spEntity.DataStateEnum.Update) {
                                    type.setDataState(spEntity.DataStateEnum.Unchanged);
                                }
                            });
                        }


                    });

                    // Clear the relationship filters for this control
                    scope.clear = function() {
                        var relationshipControlFilters,
                            filterEntities;

                        if (!scope.formControl ||
                            !scope.formControl.hasRelationship('console:relationshipControlFilters')) {
                            return;
                        }

                        relationshipControlFilters = scope.formControl.getRelationship('console:relationshipControlFilters');
                        filterEntities = _.map(relationshipControlFilters.getInstances(), function(ri) {
                            return ri.entity;
                        });

                        _.forEach(filterEntities, function(fe) {
                            relationshipControlFilters.deleteEntity(fe);
                        });

                        updateDisplayString();
                    };


                    // Show the relationship filters dialog
                    scope.showFiltersDialog = function() {
                        var options = {
                            formControl: scope.formControl,
                            form: spFormBuilderService.form
                        };

                        spRelationshipFiltersDialog.showModalDialog(options).then(function() {
                            updateDisplayString();
                        });
                    };                    
                }
            };
        });
}());