// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing a form builder toolbox relationship viewer control.
    * spFormBuilderToolboxRelationships provides the toolbox pane for interacting with relationships.
    *
    * @module spFormBuilderToolboxRelationships
    * @example

    Using the spFormBuilderToolboxRelationships:

    &lt;sp-form-builder-toolbox-relationships&gt;&lt;/sp-form-builder-toolbox-relationships&gt

    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderToolboxRelationships', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.common.spCachingCompile'
    ])
        .directive('spFormBuilderToolboxRelationships', function(spFormBuilderService, $q, spCachingCompile) {

            /**
             * Directive structure.
             */
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {},                
                link: function(scope, element) {

                    /////
                    // Ensure that the model exists.
                    /////
                    scope.model = scope.model || {};
                    scope.model.lookups = scope.model.lookups || [];

                    scope.popover = {
                        isOpen: false
                    };

                    /////
                    // Attach the form builder service to the scope so we can bind to it.
                    /////
                    scope.spFormBuilderService = spFormBuilderService;

                    /////
                    // The LookupContainer type.
                    /////
                    var LookupContainer = function(lookup) {
                        this.lookup = lookup;
                        this.readOnly = true;
                        this.selected = false;
                    };

                    scope.getPopoverMinHeight = function() {
                        var style = {};

                        /////
                        // Header height;
                        /////
                        var height = 34;

                        /////
                        // Items
                        /////
                        height += scope.model.lookups.length * 23;

                        /////
                        // Items outer margin.
                        /////
                        height += 18;

                        style['min-height'] = height + 'px';

                        return style;
                    };

                    /**
                     * getEntity method on the prototype.
                     */
                    LookupContainer.prototype.getEntity = function() {
                        return this.lookup.getEntity();
                    };

                    /**
                     *  getLookup method on the prototype.
                     */
                    LookupContainer.prototype.getLookup = function() {
                        return this.lookup;
                    };

                    /**
                     * getName method on the prototype.
                     */
                    LookupContainer.prototype.getName = function() {
                        return this.lookup.getName();
                    };

                    /**
                     * getDescription method on the prototype.
                     */
                    LookupContainer.prototype.getDescription = function() {
                        return this.lookup.getDescription();
                    };

                    /**
                     * Get the definition object.
                     */

                    function getDefinition() {

                        /////
                        // Get the definition id from $state.
                        /////
                        return spFormBuilderService.getDefinitionId()
                            .then(function(definitionId) {
                                if (!definitionId) {
                                    return $q.reject('No definition id specified.');
                                }

                                /////
                                // See whether the definition exists within the spFormBuilderService.
                                /////
                                return spFormBuilderService.getDefinition(definitionId);
                            });
                    }

                    /**
                     * Loads the existing form.
                     */

                    function load() {

                        /////
                        // Get the definition;
                        /////
                        spFormBuilderService.initializationComplete()
                            .then(getDefinition)
                            .then(function() {
                                scope.setSelectedLookup(null);

                                loadAvailableLookups();
                            });
                    }

                    /**
                     * Load the available lookups.
                     */

                    function loadAvailableLookups() {

                        if (!spFormBuilderService.definition)
                            return;

                        spFormBuilderService.definitionFieldsLoading = true;

                        spFormBuilderService.getType(spFormBuilderService.definition).then(
                            function(typeEntity) {
                                if (typeEntity) {

                                    /////
                                    // If the type being loaded is 'core:definition' then either the definition being edited is the 'core:definition' itself
                                    // or a newly created definition. In the case of a newly created definition, the alias field must be removed as the
                                    // augment will set the new definitions alias to be 'core:definition' and that = bad.
                                    /////
                                    if (typeEntity.alias() === 'core:definition' && typeEntity.id() !== spFormBuilderService.definition.id()) {
                                        var aliasField = typeEntity.getFieldContainer('core:alias');

                                        if (aliasField) {
                                            typeEntity._fields.splice(typeEntity._fields.indexOf(aliasField), 1);
                                        }
                                    }

                                    /////
                                    // Augment the definition with the type information.
                                    /////
                                    spEntity.augment(spFormBuilderService.definition, typeEntity);

                                    spFormBuilderService.definitionRevision++;

                                    var type = new spResource.Type(spFormBuilderService.definition);

                                    scope.model.lookups = _.map(_.sortBy(type.getLookups(), function(lookup) {
                                        return lookup.getName();
                                    }), function(lookup) {
                                        return new LookupContainer(lookup);
                                    });

                                    return typeEntity;
                                }

                                return undefined;
                            },
                            function(error) {
                                console.error(error);
                                throw error;
                            }
                        ).then(
                            function(typeEntity) {
                                if (typeEntity) {
                                    /////
                                    // If the type being loaded is 'core:definition' then either the definition being edited is the 'core:definition' itself
                                    // or a newly created definition. In the case of a newly created definition, the inherited type may be either 'core:userResource'
                                    // or a custom type in which case the custom type must be loaded.
                                    /////
                                    if (typeEntity.alias() === 'core:definition' && typeEntity.id() !== spFormBuilderService.definition.id()) {
                                        if (spFormBuilderService.definition.inherits && (spFormBuilderService.definition.inherits.length > 1 || spFormBuilderService.definition.inherits[0].alias() != 'core:userResource')) {

                                            var promises = [];

                                            _.forEach(spFormBuilderService.definition.inherits, function(inherits) {
                                                var promise = spFormBuilderService.getType(inherits).then(function(inheritsType) {
                                                    spEntity.augment(inherits, inheritsType);
                                                });

                                                promises.push(promise);
                                            });

                                            return $q.all(promises);
                                        }
                                    }
                                }

                                return $q.when();
                            }).then(function () {
                                spFormBuilderService.setInitialFormBookmark();
                                spFormBuilderService.definitionRevision++;
                            })
                            .finally(function() {
                                spFormBuilderService.definitionFieldsLoading = false;
                            });
                    }

                    /**
                     * Get the lookups toType.
                     */

                    function getTargetType(lookup) {
                        var targetType;
                        var relationship;
                        var entity;

                        if (lookup && lookup.getLookup && lookup.getEntity) {
                            relationship = lookup.getLookup();
                            entity = lookup.getEntity();

                            if (relationship && entity && relationship.isReverse) {
                                if (!relationship.isReverse()) {
                                    targetType = entity.getToType();
                                } else {
                                    targetType = entity.getFromType();
                                }

                                return targetType;
                            }
                        }

                        return null;
                    }

                    /**
                     * Watch for changes in the service revision.
                     */
                    scope.$watch('spFormBuilderService.serviceRevision', function(newVal, oldVal) {

                        if (newVal === oldVal || newVal === 0 ) {
                            return;
                        }

                        $q.when()
                            .then(getDefinition)
                            .then(function() {
                                scope.setSelectedLookup(null);

                                loadAvailableLookups();
                            });
                    });

                    /**
                     * Removes the current lookup.
                     */
                    scope.removeLookup = function() {

                        if (this.lookup) {

                            if (spFormBuilderService.selectedLookup && spFormBuilderService.selectedLookup.getEntity().id() === this.lookup.getEntity().id()) {
                                scope.setSelectedLookup(null);
                            }

                            this.lookup.selected = false;
                        }
                    };

                    /**
                     * Get the icon that represents this lookup.
                     */
                    scope.getLookupIcon = function() {

                        if (!this.lookup) {
                            return null;
                        }

                        var val = this.lookup.getLookup();

                        if (val.isFromOne) {
                            if (val.isToOne) {
                                return 'lookupstype.png';
                            } else {
                                return 'relationshipstype.png';
                            }
                        } else {
                            if (val.isToOne) {
                                return 'manyToOne.png';
                            } else {
                                return 'manyToMany.png';
                            }
                        }
                    };

                    /**
                     * A lookup has been clicked.
                     */
                    scope.lookupCheckboxClicked = function(toggle, event) {
                        var targetType;

                        if (event && event.target && $(event.target).hasClass('click-bypass')) {
                            return;
                        }

                        if (toggle) {
                            this.lookup.selected = !this.lookup.selected;
                        }

                        if (this.lookup.selected) {

                            targetType = getTargetType(this.lookup);

                            if (targetType) {
                                spFormBuilderService.cacheLookupFields(targetType);
                            }
                        } else {
                            if (spFormBuilderService.selectedLookup && spFormBuilderService.selectedLookup.getEntity().id() === this.lookup.getEntity().id()) {
                                scope.setSelectedLookup(null);
                            }
                        }

                        if (event) {
                            event.stopPropagation();
                        }
                    };

                    /**
                     * Set the parent lookup (current definition).
                     */
                    scope.setSelectedLookup = function(type) {

                        if (type) {
                            spFormBuilderService.definitionFieldsLoading = true;
                        }

                        spFormBuilderService.selectedLookup = type;
                    };

                    /**
                     * Set the child selected lookup (child lookup of the current definition).
                     */
                    scope.setSelectedChildLookup = function(lookup) {
                        var targetType;

                        targetType = getTargetType(lookup);

                        if (targetType) {

                            scope.setSelectedLookup(lookup.getLookup());
                        }
                    };

                    /**
                     * Gets the selected item style.
                     */
                    scope.getItemStyle = function(lookup, isRelationship) {
                        var style = {};

                        style['width'] = '100%';

                        var v = lookup;

                        if (spFormBuilderService.selectedLookup) {
                            if (isRelationship && v.id() === spFormBuilderService.selectedLookup.getEntity().id()) {
                                style['background-color'] = '#202020';
                            }
                        } else {
                            if (!isRelationship) {
                                style['background-color'] = '#202020';
                            }
                        }

                        return style;
                    };

                    /**
                     * Stops certain characters from being entered into the editable labels.
                     */
                    scope.validateinput = function(evt) {
                        var e = evt || event;

                        if (e.shiftKey) {
                            switch (e.which) {
                            case 188:
                            case 190:
                                // < (188), > (190)
                                e.stopPropagation();
                                e.preventDefault();
                                return false;
                            }
                        }

                        return true;
                    };

                    /**
                     * Change validate.
                     */
                    scope.changeValidate = function(value) {

                        if (value) {
                            return value.replace(/[<>]+/g, '');
                        }
                        return value;
                    };

                    /**
                     * Load the definition and/or form.
                     */
                    load();

                    var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilderToolbox/directives/spFormBuilderToolboxRelationships/spFormBuilderToolboxRelationships.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        })
        .filter('filterLookups', function() {

            /**
            * Filter the fields that belong to the specified field group.
            */
            return function(lookups) {

                var passed;

                passed = _.filter(lookups, function(lookup) {
                    return lookup.selected;
                });

                return passed || [];
            };
        });
}());