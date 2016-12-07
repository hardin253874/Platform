// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, jQuery, sp, spEntity, spResource, jsonString, jsonLookup, jsonInt, jsonBool */

angular.module('mod.app.formBuilder.services.spFormBuilderService', [
    'mod.common.spEntityService',
    'mod.app.editFormServices',
    'mod.app.editFormCache',
    'sp.navService',
    'mod.common.ui.spReportModel',
    'sp.app.settings'
])
/**
 * Module implementing the form builder service.
 * spFormBuilderService provides methods to interact with the form builder between directives.
 *
 * @module spFormBuilderToolbox
 */
    .service('spFormBuilderService', function ($state, $q, $compile, spEntityService, spEditForm, editFormCache, spNavService, spReportModel, spAppSettings) {
        'use strict';

        var exports = {};

        /**
         * Stateful data.
         */
        exports._form = undefined;
        exports.isDefaultForm = false;
        exports.moveItemCallback = null; // callback that will place the generated report into the correct nav location
        exports.returnToReport = null; // ID of generated report so we can nav back to it
        exports.definitionRevision = 0;
        exports.lookupCache = {};
        exports.selectedLookup = undefined;
        exports.initializationPromise = $q.defer();
        exports.initialFormBookmark = undefined;
        exports.cachedTypes = [];

        /**
         * Stateless data.
         */
        exports.serviceRevision = 0;
        exports.isDragging = false;
        exports.currentDropTarget = undefined;
        exports.currentDropTargetIsField = undefined;
        exports.currentDropTargetQuadrant = undefined;
        exports.isResizing = undefined;
        exports.definitionFieldsLoading = false;
        exports.screenObjectsLoading = false;
        exports.insertIndicator = undefined;
        exports.insertOverlayIndicator = undefined;
        exports.canDrop = true;
        exports.selectedApp = undefined;
        exports.lastDragValues = {
            event: undefined,
            source: undefined,
            target: undefined,
            dragData: undefined,
            dropData: undefined,
            fieldContainer: undefined,
            control: undefined
        };


        /**
         * Resets the form builder service.
         */
        exports.reset = function () {
            /////
            // Bypass the property setter
            /////
            exports._form = undefined;

            exports.isDefaultForm = false;
            exports.moveItemCallback = null;
            //exports.returnToReport = 0;
            exports.definitionRevision = 0;
            exports.lookupCache = {};
            exports.selectedLookup = undefined;
            exports.selectedApp = undefined;
            exports.isDragging = false;
            exports.currentDropTarget = undefined;
            exports.currentDropTargetIsField = undefined;
            exports.currentDropTargetQuadrant = undefined;
            exports.isResizing = undefined;
            exports.definitionFieldsLoading = false;
            exports.screenObjectsLoading = false;
            exports.insertIndicator = undefined;
            exports.insertOverlayIndicator = undefined;
            exports.canDrop = true;
            exports.cachedTypes = [];
            exports.initializationPromise.reject();
            exports.initializationPromise = $q.defer();
            exports.initialFormBookmark = undefined;
            exports.lastDragValues = {
                event: undefined,
                source: undefined,
                target: undefined,
                dragData: undefined,
                dropData: undefined,
                fieldContainer: undefined,
                control: undefined
            };
            exports.serviceRevision = 0;
        };

        /**
         * Gets a state object to store on the nav item
         */
        exports.getState = function () {
            return {
                definition: exports.definition,
                form: exports.form,
                folder: exports.moveItemCallback,
                isDefaultForm: exports.isDefaultForm,
                definitionRevision: exports.definitionRevision,
                lookupCache: exports.lookupCache,
                selectedLookup: exports.selectedLookup,
                cachedTypes: exports.cachedTypes,
                initialFormBookmark: exports.initialFormBookmark,
                selectedApp: exports.selectedApp
            };
        };

        /**
         * Restores a state object from the nav item
         * @param {object} state - The state being restored.
         */
        exports.setState = function (state) {

            /////
            // Bypass the property setter
            /////
            if (state.hasOwnProperty('form')) {
                exports._form = state.form;
            } else {
                exports.form = undefined;
            }

            if (state.hasOwnProperty('isDefaultForm')) {
                exports.isDefaultForm = state.isDefaultForm;
            } else {
                exports.isDefaultForm = false;
            }

            if (state.hasOwnProperty('definitionRevision')) {
                exports.definitionRevision = state.definitionRevision;
            } else {
                exports.definitionRevision = 0;
            }

            if (state.hasOwnProperty('lookupCache')) {
                exports.lookupCache = state.lookupCache;
            } else {
                exports.lookupCache = {};
            }

            if (state.hasOwnProperty('selectedLookup')) {
                exports.selectedLookup = state.selectedLookup;
            } else {
                exports.selectedLookup = undefined;
            }

            if (state.hasOwnProperty('selectedApp')) {
                exports.selectedApp = state.selectedApp;
            } else {
                exports.selectedApp = undefined;
            }

            exports.isDragging = false;
            exports.currentDropTarget = undefined;
            exports.currentDropTargetIsField = undefined;
            exports.currentDropTargetQuadrant = undefined;
            exports.isResizing = undefined;
            exports.definitionFieldsLoading = false;
            exports.screenObjectsLoading = false;
            exports.insertIndicator = undefined;
            exports.insertOverlayIndicator = undefined;
            exports.canDrop = true;
            exports.moveItemCallback = state.moveItemCallback || null;

            if (state.hasOwnProperty('cachedTypes')) {
                exports.cachedTypes = state.cachedTypes;
            } else {
                exports.cachedTypes = [];
            }

            if (state.hasOwnProperty('initialFormBookmark')) {
                exports.initialFormBookmark = state.initialFormBookmark;
            } else {
                exports.initialFormBookmark = undefined;
            }

            exports.lastDragValues = {
                event: undefined,
                source: undefined,
                target: undefined,
                dragData: undefined,
                dropData: undefined,
                fieldContainer: undefined,
                control: undefined
            };
            exports.serviceRevision++;

            refreshMemoizedFunctions();
        };

        /**
         * 'form' property
         */
        Object.defineProperty(exports, 'form', {
            get: function () {
                return exports._form;
            },
            set: function (newForm) {
                exports._form = newForm;
                refreshMemoizedFunctions();
            },
            enumerable: true,
            configurable: true
        });


        var changeListenerUnhooker;

        function addFormChangeListener(form) {

            if (changeListenerUnhooker) {
                changeListenerUnhooker();
            }

            if (form) {
                changeListenerUnhooker = form.graph.history.addChangeListener(refreshMemoizedFunctions); // TODO: think about unhooking the listener at some point
            }
        }

        exports.refreshMemoizedFunctions = refreshMemoizedFunctions;

        function refreshMemoizedFunctions() {

            function getId(thing) {
                return thing.id;
            }

            function getRel(thing) {
                return thing.id + '|' + !!thing.value._isReverse;
            }    // key based on relationship id and direction

            exports.isRelationshipOnForm = _.memoize(isRelationshipOnForm, getRel);
            exports.isFieldOnForm = _.memoize(isFieldOnForm, getId);
            exports.isRelationshipOnFormNoCache = isRelationshipOnForm;
            exports.isFieldOnFormNoCache = isFieldOnForm;
            exports.isDirectField = _.memoize(isDirectField);
            exports.isDirectRelationship = _.memoize(isDirectRelationship);

            addFormChangeListener(exports._form);
        }


        /**
         * 'definition' property
         */
        Object.defineProperty(exports, 'definition', {
            get: function () {
                if (exports._form && exports._form.typeToEditWithForm) {
                    return exports._form.typeToEditWithForm;
                }

                return undefined;
            },
            set: function (newDefinition) {
                if (exports._form) {
                    if (!exports._form.typeToEditWithForm) {
                        exports._form.registerLookup('k:typeToEditWithForm');
                    }

                    exports._form.typeToEditWithForm = null;
                    exports._form.typeToEditWithForm = newDefinition;
                }
            },
            enumerable: true,
            configurable: true
        });

        /**
         * Ensures that we have all the required components ready to edit, or creates them if necessary.
         * @param {bool} initData - Whether the definition and form are to be initialized. These may be
         *                          set externally through the setState method.
         */
        exports.initialize = function (initData) {

            exports.form = undefined;
            exports.definition = undefined;

            return $q.when()
                .then(function () {
                    if (initData) {
                        return exports.initializeForm();
                    } else {
                        return $q.when();
                    }
                })
                .then(function () {

                    if (initData && exports.isDefinitionBuilder()) {
                        return exports.initializeDefinition();
                    } else {
                        return $q.when();
                    }
                })
                .finally(function () {
                    exports.initializationPromise.resolve();
                });
        };

        /**
         * Returns the initialization promise.
         */
        exports.initializationComplete = function () {
            return exports.initializationPromise.promise;
        };

        /**
         * Quadrants.
         */
        exports.quadrants = {
            left: 'left',
            top: 'top',
            right: 'right',
            bottom: 'bottom',
            center: 'center'
        };

        /**
         * Stack Containers.
         */
        exports.containers = {
            horizontal: 'horizontalStackContainerControl',
            vertical: 'verticalStackContainerControl',
            form: 'customEditForm',
            header: 'headerColumnContainerControl',
            tab: 'tabContainerControl',
            screen: 'screen'
        };

        /**
         * Does the alias refer to a container.
         */
        exports.isContainer = function (alias) {
            return alias === exports.containers.horizontal || alias === exports.containers.vertical || alias === exports.containers.form || alias === exports.containers.header || alias === exports.containers.tab || alias === exports.containers.screen;
        };

        /**
         * Does the alias refer to a vertical container.
         */
        exports.isVerticalContainer = function (alias) {
            return alias === exports.containers.vertical || alias === exports.containers.form || alias === exports.containers.header || alias === exports.containers.tab || alias === exports.containers.screen;
        };

        /**
         * Does the alias refer to a horizontal container.
         */
        exports.isHorizontalContainer = function (alias) {
            return alias === exports.containers.horizontal;
        };

        /**
         * Does the alias refer to a tab container.
         */
        exports.isTabContainer = function (alias) {
            return alias === exports.containers.tab;
        };

        /**
         * Does the entity represent an implicit container.
         */
        exports.isImplicitContainer = function (entity) {
            var alias = entity.type.getAlias();

            return exports.isContainer(alias) && !exports.isExplicitContainer(entity);
        };

        /**
         * Does the entity represent an explicit container.
         */
        exports.isExplicitContainer = function (entity) {
            var alias = entity.type.getAlias();

            return exports.isContainer(alias) && (entity.name || alias === 'headerColumnContainerControl' || alias === 'tabContainerControl');
        };

        /**
         * Builders.
         */
        exports.builders = {
            form: 'formBuilder',
            screen: 'screenBuilder'
        };

        /**
         * CreateSource.
         */
        exports.createSource = {
            definition: 'definition',
            form: 'form'
        };

        /**
         * Hides the insert indicator.
         */
        exports.hideInsertIndicator = function () {

            // if (exports.insertIndicator && exports.insertIndicator.is(':visible')) {
            //     console.log('hiding insert indicator', exports.insertIndicator);
            // }
            if (exports.insertIndicator) {
                exports.insertIndicator.hide();
            }
        };

        /**
         * Shows the insert indicator.
         */
        exports.showInsertIndicator = function () {

            // if (exports.insertIndicator && !exports.insertIndicator.is(':visible')) {
            //     console.log('showing insert indicator', exports.insertIndicator);
            // }
            if (exports.insertIndicator) {
                exports.insertIndicator.show();
            }
        };

        /**
         * Positions the insert indicator.
         * @param {number} top - The top coordinate of the insert indicator.
         * @param {number} left - The left coordinate of the insert indicator.
         * @param {number} height - The height of the insert indicator.
         * @param {number} width - The width of the insert indicator.
         */
        exports.positionInsertIndicator = function (top, left, height, width, scope) {

            if (!_.isNumber(top) || !_.isNumber(left) || !_.isNumber(height) || !_.isNumber(width)) {
                return;
            }

            if (!exports.insertIndicator) {
                exports.createInsertIndicator(scope);
            }

            exports.insertIndicator.css({
                width: width,
                height: height,
                top: top,
                left: left
            }).show();

            // console.log('showing insert indicator at', left, top, width, height, exports.insertIndicator);
        };

        /**
         * Create the insert indicator.
         */
        exports.createInsertIndicator = function (scope) {

            if (exports.insertIndicator) {
                return;
            }

            var element = jQuery('<div />', {
                id: 'insertIndicator',
                'sp-droppable': 'insertIndicatorDropOptions'
            }).css({
                'background-color': '#52838c',
                'border-radius': '4px',
                'pointer-events': 'none',
                opacity: '0.75',
                position: 'absolute'
            });

            var compiledElement = $compile(element)(scope);

            compiledElement.appendTo('body').hide();

            exports.insertIndicator = compiledElement;

            /*
             exports.insertIndicator = jQuery('<div sp-droppable="insertIndicatorDropOptions" />', {
             id: 'insertIndicator'
             }).css({
             'background-color': '#52838c',
             'border-radius': '4px',
             opacity: '0.75',
             position: 'absolute'
             }).appendTo('body').hide();
             */

            // console.log('created insert indicator', exports.insertIndicator);
        };

        /**
         * Destroy the insert indicator.
         */
        exports.destroyInsertIndicator = function () {

            // console.log('destroy insert indicator', exports.insertIndicator);

            if (!exports.insertIndicator) {
                return;
            }

            exports.insertIndicator.remove();
            exports.insertIndicator = undefined;
        };

        /**
         * Positions the insert overlay indicator.
         */
        exports.positionInsertOverlayIndicator = function (parentElement, scope) {
            var clientRect = parentElement.getBoundingClientRect();

            var top = parentElement.clientTop;
            var left = parentElement.clientLeft;
            var width = clientRect.width;
            var height = clientRect.height;

            if (!_.isNumber(top) || !_.isNumber(left) || !_.isNumber(height) || !_.isNumber(width)) {
                return;
            }

            if (!exports.insertOverlayIndicator) {
                exports.createInsertOverlayIndicator(parentElement, scope);
            }

            exports.insertOverlayIndicator.css({
                width: width,
                height: height,
                top: top,
                left: left
            }).show();
        };

        /**
         * Create the insert overlay indicator.
         */
        exports.createInsertOverlayIndicator = function (parentElement, scope) {

            if (exports.insertOverlayIndicator) {
                return;
            }
            var element = jQuery('<div />', {
                id: 'insertOverlayIndicator'
            }).css({
                'background-color': '#52838c',
                'border-radius': '4px',
                opacity: '0.75',
                position: 'absolute'
            });

            var compiledElement = $compile(element)(scope);

            parentElement.appendChild(compiledElement[0]); // append to parentElement

            exports.insertOverlayIndicator = compiledElement;
        };

        /**
         * Destroy the insert overlay indicator.
         */
        exports.destroyInsertOverlayIndicator = function () {

            if (!exports.insertOverlayIndicator) {
                return;
            }

            exports.insertOverlayIndicator.remove();
            exports.insertOverlayIndicator = undefined;
        };

        /**
         * Create a temporary instance.
         * @param {string} baseName - The name of the instance.
         * @param {string} baseDescription - The description of the instance.
         * @param {function} createCallback - Callback used to create the instance.
         * @param {function} paramSetCallback - Callback used to set the parameters.
         */
        function createTemporaryInstance(baseName, baseDescription, createCallback, paramSetCallback) {
            var instance = null;
            var params;

            params = $state.params;

            if (createCallback) {
                instance = createCallback(baseName, baseDescription);
            }

            if (paramSetCallback && instance) {
                paramSetCallback(params, instance.id());
            }

            return instance;
        }

        /**
         * Ensures that we have a definition ready to edit, or creates one if necessary.
         * (To handle the scenario that one wasn't passed in on the URL)
         */
        exports.initializeDefinition = function () {

            return exports.getDefinitionId()
                .then(function (definitionId) {
                    if (definitionId) {
                        return exports.getDefinition(definitionId, true).then(function (existingDefinition) {
                            return existingDefinition;
                        }, function () {
                            /////
                            // Create definition
                            /////
                            return createTemporaryDefinition();
                        });
                    } else {
                        /////
                        // Create definition
                        /////
                        return $q.when(createTemporaryDefinition());
                    }
                }, function () {
                    /////
                    // Create definition
                    /////
                    return $q.when(createTemporaryDefinition());
                });
        };

        /**
         * Creates a temporary definition.
         */
        function createTemporaryDefinition() {

            return createTemporaryInstance('New Definition', 'A new user-created definition.', exports.createDefinition, function (params, id) {
                params.definitionId = id;
            });
        }

        /**
         * Creates a temporary screen.
         */
        function createTemporaryScreen() {

            return createTemporaryInstance('New Screen', null, function (name) {
                return exports.createScreen(name);
            }, function (params, id) {
                params.formId = id;
            });
        }

        /**
         * Creates a temporary form.
         */
        function createTemporaryForm() {

            return createTemporaryInstance('New Definition Form', 'A new user-created form.', function (name, description) {
                return exports.createForm(null, name, description);
            }, function (params, id) {
                params.formId = id;
            });
        }

        /**
         * Creates a temporary form.
         */
        exports.createTemporaryForm = function () {
            return createTemporaryForm();
        };

        /**
         * Ensures that we have a form ready to edit, or creates one if necessary.
         * (To handle the scenario that one wasn't passed in on the URL)
         */
        exports.initializeForm = function () {
            var formId;

            // Create a new form or screen
            var createNew = function () {
                var result;
                if (exports.getBuilder() === exports.builders.screen) {
                    result = createTemporaryScreen();
                } else {
                    result = createTemporaryForm();
                }
                return $q.when(result);
            };

            // Check to see if an ID has been specified
            formId = exports.getFormId();

            if (formId) {
                // Load existing form or screen
                return exports.getForm(formId).then(function (existingForm) {

                    // If no defn specified, use the one that is associated with this form
                    $state.params.definitionId = $state.params.definitionId || sp.result(existingForm, 'typeToEditWithForm.idP');

                    return existingForm;
                }, function () {
                    // On failure, create a new one
                    return createNew();
                });
            } else {
                return createNew();
            }
        };

        /**
         * Return 'formBuilder' or 'screenBuilder'
         */
        exports.getBuilder = function () {
            var res = $state.current.name === exports.builders.screen ? exports.builders.screen : exports.builders.form;
            return res;
        };

        /**
         * Return true for formBuilder, false for screen builder
         */
        exports.isDefinitionBuilder = function () {
            var res = exports.getBuilder() === exports.builders.form;
            return res;
        };

        /**
         * Retrieves the definition with the specified id.
         * @param {number | string} id - the id/alias of the definition to be retrieved.
         * @param {boolean} force - true to force a refresh.
         */
        exports.getDefinition = function (id, force) {

            if (exports.definition && exports.definition.id() === id && !force) {
                return $q.when(exports.definition);
            }

            return exports.loadDefinitions([id]).then(function (definitions) {
                var definition = definitions[0];
                exports.definition = definition;

                exports.definitionRevision++;
                return definition;
            }, function () {
                return createTemporaryDefinition();
            });
        };

        /**
         * Retrieves the definition with the specified id.
         * @param {array} ids - the ids to load.
         * @returns A promise for array of definition entities.
         */
        exports.loadDefinitions = function (ids) {
            var options;

            options = {
                fields: true,
                relationships: true,
                fieldGroups: true,
                ignoreInheritance: false,
                ignoreOverrides: false,
                derivedTypes: false,
                resourceKeys: true,
                additional: 'fields.{allowMultiLines, fieldScriptName},relationships.toScriptName,inSolution.name,isAbstract,supportMultiTypes,inherits.name,k:defaultEditForm.id,defaultDisplayReport.id,k:typeIcon.name,typeScriptName,isOfType.{name, alias}, k:typeConsoleBehavior.{name, isOfType.{name, alias}, k:treeIcon.{ name, imageBackgroundColor} }'
            };

            var rq = spResource.makeTypeRequest(options);
            return spEntityService.getEntities(ids, rq);
        };

        /**
         * Retrieves the current definition as an spResource.Type.
         */
        exports.getDefinitionType = function () {

            if (!exports.definition) {
                console.error('No current definition specified.');
                return undefined;
            }

            return new spResource.Type(exports.definition);
        };


        /**
         * Retrieves the definitions forward relationships.
         * @param {bool} localRelationshipsOnly - Whether the local forward relationships are the only ones returned.
         */
        exports.getDefinitionForwardRelationships = function (localRelationshipsOnly) {

            if (!exports.definition) {
                console.error('No current definition specified.');
                return undefined;
            }

            if (localRelationshipsOnly) {
                var type = new spResource.Type(exports.definition);

                return _.map(exports.definition.getRelationship('core:relationships'), function (relationship) {
                    return new spResource.Relationship(relationship, type, false);
                });
            } else {
                return _.filter(exports.getDefinitionType().getAllRelationships(), function (relationship) {
                    return !relationship.isReverse();
                });
            }
        };

        /**
         * Retrieves the definitions reverse relationships.
         * @param {bool} localRelationshipsOnly - Whether the local reverse relationships are the only ones returned.
         */
        exports.getDefinitionReverseRelationships = function (localRelationshipsOnly) {
            if (!exports.definition) {
                console.error('No current definition specified.');
                return undefined;
            }

            if (localRelationshipsOnly) {
                var type = new spResource.Type(exports.definition);

                return _.map(exports.definition.getRelationship('core:reverseRelationships'), function (relationship) {
                    return new spResource.Relationship(relationship, type, true);
                });
            } else {
                return _.filter(exports.getDefinitionType().getAllRelationships(), function (relationship) {
                    return relationship.isReverse();
                });
            }
        };

        /**
         * Retrieves the definition relationships.
         * @param {bool} localRelationshipsOnly - Whether the local relationships are the only ones returned.
         */
        exports.getDefinitionRelationships = function (localRelationshipsOnly) {

            if (!exports.definition) {
                console.error('No current definition specified.');
                return undefined;
            }

            return _.union(exports.getDefinitionForwardRelationships(localRelationshipsOnly), exports.getDefinitionReverseRelationships(localRelationshipsOnly));
        };

        /**
         * Retrieves the definition fields.
         * @param {bool} localFieldsOnly - Whether the local fields are the only ones returned.
         */
        exports.getDefinitionFields = function (localFieldsOnly) {

            if (!exports.definition) {
                return undefined;
            }

            if (localFieldsOnly) {
                return exports.definition.getRelationship('core:fields');
            } else {
                return exports.getDefinitionType().getFields();
            }
        };

        /**
         * Retrieves the definition field groups.
         * @param {bool} localFieldGroupsOnly - Whether the local field groups are the only ones returned.
         */
        exports.getDefinitionFieldGroups = function (localFieldGroupsOnly) {

            if (!exports.definition) {
                return undefined;
            }

            if (localFieldGroupsOnly) {
                return exports.definition.getRelationship('core:fieldGroups');
            } else {
                return exports.getDefinitionType().getFieldGroups();
            }
        };

        /**
         * Reorder the contained controls on form.
         * @param {object} container - The container whose child controls are to be reordered.
         * @param {array} containedControlsOnForm - The child controls to be sorted.
         */
        function reorderContainedControlsOnForm(container, containedControlsOnForm) {
            var sortedArray;

            if (container && containedControlsOnForm) {
                sortedArray = _.sortBy(containedControlsOnForm, function (containedControlOnForm) {

                    if (containedControlOnForm.containedControlsOnForm) {
                        reorderContainedControlsOnForm(containedControlOnForm, containedControlOnForm.containedControlsOnForm);
                    }

                    return containedControlOnForm.renderingOrdinal;
                });

                container.containedControlsOnForm = sortedArray;
            }
        }

        /**
         * Parse the form and make any structural adjustments.
         * @param {object} form - The form to be parsed.
         */
        function parseForm(form) {
            if (form && form.containedControlsOnForm) {
                reorderContainedControlsOnForm(form, form.containedControlsOnForm);
            }
        }

        /**
         * returns a title for control.
         */
        exports.getControlTitle = function (control) {
            return spEditForm.getControlTitle(control);
        };

        /**
         * Retrieves the form with the specified id.
         * @param {number | string} id - the id/alias of the form to be retrieved.
         * @param {boolean} force - true to force a refresh.
         */
        exports.getForm = function (id, force) {

            if (exports.form && exports.form.id && exports.form.id() === id && !force) {
                return $q.when(exports.form);
            }

            /////
            // Hack: Support test page ids.
            /////
            if (id > 10000000 && id < 10001000) {
                return $q.reject();
            }

            spNavService.middleLayoutBusy = true;

            return spEditForm.getFormDefinition(id, true).then(function (form) { // bypass the cache to get it in design mode

                parseForm(form);

                exports._form = form;
                addFormChangeListener(form);

                return form;
            }).finally(function () {
                spNavService.middleLayoutBusy = false;
            });
        };

        /**
         * Retrieves the definition with the specified id.
         * @param {number | string} id - the id/alias of the definition to be retrieved.
         */
        exports.reloadDefinition = function (id) {

            return exports.getDefinition(id, true).then(function (definition) {
                if (definition && definition.graph && definition.graph.history) {
                    definition.graph.history.clear();
                }

                return definition;
            });
        };

        /**
         * Retrieves the form with the specified id, as well as its definition.
         * @param {number | string} id - the id/alias of the form to be retrieved.
         */
        exports.reloadForm = function (id) {

            return exports.getForm(id, true).then(function (form) {

                if (form && form.graph && form.graph.history) {
                    form.graph.history.clear();
                }

                exports.setInitialFormBookmark();

                return form;
            });
        };

        /**
         * Gets the current formId from state.
         */
        exports.getFormId = function () {
            var id;

            if (exports.form) {
                id = exports.form.id();
            } else {
                /////
                // Get form ID from the URL
                /////
                id = $state.params.eid || undefined;
            }

            if (id) {
                id = parseFloat(id);
            }

            if (id < 0) {
                id = undefined;
            }

            return id;
        };

        /**
         * Gets the current definitionId from state.
         */
        exports.getDefinitionId = function () {
            var id;

            if (exports.definition) {
                id = exports.definition.id();

                return $q.when(parseFloat(id));
            } else if (!_.isUndefined($state.params.definitionId) && $state.params.definitionId !== null) {
                /////
                // Get definition ID from the URL
                /////
                id = $state.params.definitionId;

                return $q.when(parseFloat(id));
            } else {
                id = exports.getFormId();

                if (id) {
                    return exports.getForm(id)
                        .then(function (form) {
                            if (form && form.typeToEditWithForm) {
                                return form.typeToEditWithForm.id();
                            } else {
                                return $q.reject('No definitionId found');
                            }
                        });
                }

            }

            return $q.reject('No definitionId found');
        };

        var beforeSave = function (form, definition) {
            var isNew = definition.dataState === spEntity.DataStateEnum.Create;

            // Create default report
            if (isNew) {
                definition.registerLookup('defaultDisplayReport');
                definition.registerLookup('defaultPickerReport');
                var controls = spEditForm.flattenControls(form);
                var report = spReportModel.createDefaultReport(definition, controls);
                definition.defaultDisplayReport = report;
                definition.defaultPickerReport = report;
            }
        };

        /**
         * Saves the current form and definition.
         * Returns true if the ID of the form was saved. (i.e. if this was a create)
         */
        exports.saveFormAndDefinition = function (options) {
            var result = {
                definitionId: null, // new (or existing)
                formId: null,       // new (or existing)
                definitionIsNew: false,
                formIsNew: false
            };

            sp.resetTime();

            return $q.when()
            // save form and definition
                .then(function () {
                    if (!exports.form) {
                        return $q.when();
                    }

                    result.formIsNew = exports.form.dataState === spEntity.DataStateEnum.Create;

                    if (exports.definition) {
                        result.definitionIsNew = exports.definition.dataState === spEntity.DataStateEnum.Create;
                        var definitionType = exports.definition.getType && exports.definition.getType() ? exports.definition.getType().nsAlias : 'core:definition';
                        var isManagedType = !result.definitionIsNew && definitionType === "core:managedType";
                        if (result.definitionIsNew) {
                            beforeSave(exports.form, exports.definition, exports.folder);
                        }

                        //if current definition type is manged type, do not update current object.
                        if (!isManagedType) {
                            if (exports.isDefaultForm || !exports.definition.defaultEditForm) {
                                exports.form.setLookup('console:isDefaultForEntityType', exports.definition);
                            }
                        } else {
                            //do not update current typeToEditWithForm (object) if it is managedType
                            if (exports.form.typeToEditWithForm._dataState !== spEntity.DataStateEnum.Unchanged) {
                                exports.form.typeToEditWithForm = spEntity.fromId(exports.form.typeToEditWithForm.idP);
                            }
                        }
                    } else {
                        result.definitionIsNew = false;
                    }

                    sp.logTime('Save form - before put');

                    return spEntityService.putEntity(exports.form, true);
                })
                .then(function (formId) {

                    sp.logTime('Save form - after put');

                    return $q.when()
                        .then(function () {
                            return formId;
                        });
                })
                // reload form
                .then(function (formId) {

                    sp.logTime('Save form - reload form');

                    editFormCache.remove(formId);   // ensure it is loaded fresh

                    if (options && options.onSaveComplete) {
                        options.onSaveComplete();
                    }

                    if (formId) {
                        result.formId = formId;
                        return exports.reloadForm(formId);
                    } else {
                        return $q.when();
                    }
                })
                // reload definition
                .then(function (form) {
                    var definitionId;

                    sp.logTime('Save form - reload def');

                    if (form && form.typeToEditWithForm) {
                        definitionId = form.typeToEditWithForm.id();

                        if (definitionId) {
                            result.definitionId = definitionId;
                            return exports.reloadDefinition(definitionId);
                        }
                    }

                    return $q.when();
                })
                .then(function () {
                    sp.logTime('Save form - after reload def');

                    if (result.definitionIsNew && exports.moveItemCallback && sp.result(exports, 'definition.defaultDisplayReport')) {
                        var moveItemCallback = exports.moveItemCallback;
                        exports.returnToReport = exports.definition.defaultDisplayReport.idP;
                        exports.moveItemCallback = null;
                        return moveItemCallback(exports.definition.defaultDisplayReport);
                    }

                    return $q.when();
                })
                .then(function () {
                    sp.logTime('Save form - after all');
                    return result;
                });
        };

        /**
         * Creates a new definition entity.
         */
        exports.createDefinitionEntity = function () {

            /////
            // JSON structure for the new definition.
            /////
            var json = {
                name: jsonString(),
                description: jsonString(),
                typeScriptName: jsonString(),
                typeId: 'core:definition',
                inherits: [
                    {
                        id: 'core:userResource',
                        inherits: [
                            {
                                id: 'core:resource'
                            }
                        ]
                    }
                ],
                'core:fieldGroups': [],
                'core:inSolution': jsonLookup(),
                'console:typeConsoleBehavior': exports.createEmptyBehavior()
            };

            var definition = spEntity.fromJSON(json);
            definition.inherits[0]._dataState = spEntity.DataStateEnum.Unchanged;
            definition.inherits[0].inherits[0]._dataState = spEntity.DataStateEnum.Unchanged;
            return definition;
        };

        /**
         * Creates a new definition object.
         * @param {string} name - the name to be assigned to the definition.
         * @param {string} description - the description to be assigned to the definition.
         * @param {number} id - the id to be assigned to the definition.
         */
        exports.createDefinition = function (name, description, id) {
            var definition;
            var inherits;

            /////
            // Definition with this id already loaded.
            /////
            if (exports.definition && exports.definition.idP === id) {
                return exports.definition;
            }

            /////
            // Create a blank definition entity
            /////
            definition = exports.createDefinitionEntity();

            /////
            // Set the name (if available).
            /////
            if (!_.isUndefined(name)) {
                definition.name = name;
            }

            /////
            // Set the description (if available).
            /////
            if (!_.isUndefined(description)) {
                definition.description = description;
            }

            /////
            // Set the current application id.
            /////
            var appId = spNavService.getCurrentApplicationId();

            if (appId) {
                definition.setInSolution(appId);
            }

            /////
            // HACK: The inherited types seem to load with data state 'Updated'?
            /////
            inherits = spResource.getAncestorsAndSelf(definition);

            _.forEach(inherits, function (inherit) {

                /////
                // For each inherited type (excluding the current)...
                /////
                if (inherit.id() !== definition.id()) {
                    /////
                    // Set the data state back to Unchanged.
                    /////
                    inherit.setDataState(spEntity.DataStateEnum.Unchanged);
                }

            });

            /////
            // Manually set the id.
            /////
            if (id) {
                definition.setId(id);
            }

            /////
            // Store the definition.
            /////
            exports.definition = definition;
            exports.isDefaultForm = true;

            return definition;
        };

        /**
         * Creates a new form entity.
         */
        exports.createFormEntity = function () {
            var json = {
                typeId: 'console:customEditForm',
                'console:typeToEditWithForm': jsonLookup(),
                'console:isDefaultForEntityType': jsonLookup(),
                'console:renderingOrdinal': jsonInt(0),
                'console:renderingWidth': jsonInt(100),
                'console:renderingHeight': jsonInt(100),
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeSpring'),
                'console:hideLabel': jsonBool(true),
                'console:showFormHelpText': jsonBool(false),
                'console:containedControlsOnForm': [],
                'core:inSolution': jsonLookup(),
                'console:navElementTreeIconBackgroundColor': jsonString()
            };
            var form = spEntity.fromJSON(json);
            return form;
        };

        /**
         * Creates a new form object.
         * @param {object} definition - The definition that the form belongs to.
         * @param {string} name - the name to be assigned to the form.
         * @param {string} description - the description to be assigned to the form.
         * @param {number} id - the id to be assigned to the form.
         */
        exports.createForm = function (definition, name, description, id) {
            var form;

            /////
            // Form with this id already loaded.
            /////
            if (exports.form && exports.form.id && exports.form.id() === id) {
                return exports.form;
            }

            /////
            // Create a blank form entity
            /////
            form = exports.createFormEntity();

            if (definition) {
                /////
                // Set the definition
                /////
                form.typeToEditWithForm = definition;
            }

            /////
            // Set the name (if available).
            /////
            if (!_.isUndefined(name)) {
                form.name = name;
            }

            /////
            // Set the description (if available).
            /////
            if (!_.isUndefined(description)) {
                form.description = description;
            }

            /////
            // Manually set the id.
            /////
            if (id) {
                form.setId(id);
            }

            /////
            // Store the form.
            /////
            exports.form = form;

            return form;
        };

        /**
         * Creates a new screen object.
         * @param {string} name - the name to be assigned to the form.
         * @param {string} description - the description to be assigned to the form.
         * @param {number} id - the id to be assigned to the form.
         */
        exports.createScreen = function (name, description, id) {

            /////
            // Screen with this id already loaded.
            /////
            if (exports.form && exports.form.id() === id) {
                return exports.form;
            }

            /////
            // Create the entity structure.
            /////
            var form = exports.createFormEntity();
            form.type = 'console:screen';
            form.name = name;
            form.description = description;

            form.registerField('core:isPrivatelyOwned', spEntity.DataType.Bool);
            form.isPrivatelyOwned = !spAppSettings.publicByDefault;

            /////
            // Manually set the id.
            /////
            if (id) {
                form.setId(id);
            }

            /////
            // Store the screen.
            /////
            exports.form = form;

            return form;
        };

        /**
         * Get the type.
         * @param {object} definition - the definition whose type information is to be loaded.
         */
        exports.getType = function (definition) {

            var type;

            /////
            // Invalid definition.
            /////
            if (!definition) {
                return $q.reject('Invalid definition.');
            }

            /////
            // If the current definition is temporary, load the base type information.
            /////
            type = (definition.getDataState() === spEntity.DataStateEnum.Create) ? 'core:definition' : definition.id();

            return exports.getTypeRequest(type);
        };

        /**
         * Requests the type from the server.
         * @param {object} type - the definition whose type information is to be loaded.
         */
        exports.getTypeRequest = function (type) {

            var options;

            options = {
                fields: true,
                relationships: true,
                fieldGroups: true,
                ignoreInheritance: false,
                ignoreOverrides: false,
                derivedTypes: false,
                resourceKeys: true,
                additional: 'fields.allowMultiLines, relationships.relationshipIsMandatory,relationships.revRelationshipIsMandatory,inSolution.name,isAbstract,supportMultiTypes,inherits.name,k:defaultEditForm.id,defaultDisplayReport.id,k:typeIcon.name'
            };

            return spEntityService.getEntity(type, spResource.makeTypeRequest(options), {hint: 'fbType'});
        };

        /**
         * Cache the lookup fields.
         * @param {object} entity - the entity whose lookup relationships are to be cached.
         */
        exports.cacheLookupFields = function (entity) {

            var id;
            var deferred;

            if (!entity) {
                throw 'Invalid entity specified.';
            }

            id = entity.id();

            /////
            // Check whether the entity has already been cached, or the request from the server is still pending.
            /////
            if (exports.cachedTypes.indexOf(entity) >= 0) {
                return $q.when(entity);
            }

            deferred = $q.defer();

            if (!exports.lookupCache.hasOwnProperty(id)) {

                exports.lookupCache[id] = {
                    cachedEntity: undefined,
                    requestActive: true,
                    queuedRequests: [
                        {
                            entity: entity,
                            deferred: deferred
                        }
                    ]
                };
            } else {
                if (exports.lookupCache[id].cachedEntity) {
                    /////
                    // Lookup fields have already been cached, so use them.
                    /////
                    spEntity.augment(entity, exports.lookupCache[id].cachedEntity);

                    return $q.when(entity);
                } else {
                    if (exports.lookupCache[id].requestActive) {

                        exports.lookupCache[id].queuedRequests.push({
                            entity: entity,
                            deferred: deferred
                        });

                        return deferred.promise;
                    }
                }
            }

            exports.getType(entity).then(function (type) {
                var queuedRequests = exports.lookupCache[id].queuedRequests;

                exports.lookupCache[id].cachedEntity = type;

                _.forEach(queuedRequests, function (queuedRequest) {
                    spEntity.augment(queuedRequest.entity, type);

                    exports.cachedTypes.push(queuedRequest.entity);

                    queuedRequest.deferred.resolve(queuedRequest.entity);
                });

                exports.lookupCache[id].queuedRequests = [];

                exports.definitionRevision++;

            }, function (error) {
                console.error(error);

                var queuedRequests = exports.lookupCache[id].queuedRequests;

                _.forEach(queuedRequests, function (queuedRequest) {
                    queuedRequest.deferred.reject(error);
                });

            }).finally(function () {
                exports.lookupCache[id].requestActive = false;
            });

            return deferred.promise;
        };

        /**
         * Pre-cache the field render controls in an asynchronous call.
         */
        exports.cacheFieldRenderControls = function () {

            var deferred;

            if (exports.fieldRenderControls && !exports.cacheFieldRenderControlsCallInProgress) {
                /////
                // Already cached.
                /////
                return $q.when(exports.fieldRenderControls);
            }

            deferred = $q.defer();

            if (!exports.cacheFieldRenderControlsCallInProgress) {

                exports.cacheFieldRenderControlsCallInProgress = true;

                spEntityService.getEntity('core:fieldType', 'instancesOfType.{alias,console:defaultRenderingControls.{alias,console:context.{alias}},console:renderingControl.{alias,console:context.{alias}}}', {hint: 'fbRC'}).then(function (requestResult) {
                    exports.fieldRenderControls = {};

                    _.forEach(requestResult.instancesOfType, function (instance) {

                        var found;

                        found = _.find(instance.defaultRenderingControls, function (renderingControl) {
                            return renderingControl.context.getAlias() === 'console:uiContextHtml';
                        });

                        if (!found) {
                            found = _.find(instance.renderingControl, function (renderingControl) {
                                return renderingControl.context.getAlias() === 'console:uiContextHtml';
                            });
                        }

                        if (found) {
                            exports.fieldRenderControls[instance.getAlias()] = found._id;
                        }
                    });

                    deferred.resolve(exports.fieldRenderControls);

                }, function (error) {
                    console.log(error);
                    deferred.reject(error);
                }).finally(function () {
                    delete exports.cacheFieldRenderControlsCallInProgress;
                });
            }

            return deferred.promise;
        };

        /**
         * Gets the relationship render control alias.
         * @param {string} relationship - The relationship.
         */
        exports.getRelationshipRenderControlAlias = function (relationship) {

            if (!relationship) {
                console.error('No relationship specified.');
                return undefined;
            }

            if (relationship.isChoiceField()) {
                if (relationship.getEntity().cardinality.alias() === 'core:manyToMany') {
                    return 'console:multiChoiceRelationshipRenderControl';
                } else {
                    return 'console:choiceRelationshipRenderControl';
                }
            } else if (relationship.isLookup()) {
                if (relationship.getEntity().toType.alias() === 'core:photoFileType') {
                    return 'console:imageRelationshipRenderControl';
                } else {
                    return 'console:inlineRelationshipRenderControl';
                }
            } else {
                return 'console:tabRelationshipRenderControl';
            }
        };

        /**
         * Gets the relationship render control instance.
         * @param {string} relationshipTypeAlias - The relationship type alias.
         * @param {object} relationship - The relationship instance.
         * @param {object} omitControlName - Whether the control name should be omitted.
         */
        exports.getRelationshipRenderControlInstance = function (relationshipTypeAlias, relationship, omitControlName) {
            var renderControl;
            var json;

            if (!relationshipTypeAlias) {
                console.error('No relationship type alias specified.');
                return undefined;
            }

            if (!relationship) {
                console.error('No relationship specified.');
                return undefined;
            }

            renderControl = exports.getRelationshipRenderControlAlias(new spResource.Relationship(relationship, exports.getDefinitionType(), false));

            if (renderControl) {
                json = {
                    typeId: renderControl,
                    'console:renderingOrdinal': jsonInt(),
                    'console:renderingWidth': jsonInt(),
                    'console:renderingHeight': jsonInt(),
                    'console:renderingBackgroundColor': 'white',
                    'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:hideLabel': jsonBool(false),
                    'console:relationshipToRender': relationship,
                    'console:containedControlsOnForm': []
                };

                if (renderControl === 'console:tabRelationshipRenderControl') {
                    json['console:renderingHorizontalResizeMode'] = jsonLookup('console:resizeSpring');
                    json['console:renderingVerticalResizeMode'] = jsonLookup('console:resizeSpring');
                }

                if (relationship.name && !omitControlName) {
                    json.name = relationship.name;
                }

                if (relationship.description) {
                    json.description = relationship.description;
                }

                if (relationship.isReverse) {
                    json['console:isReversed'] = !!relationship.isReverse();
                }

                return spEntity.fromJSON(json);
            }

            return null;
        };

        /**
         * Gets the field render control alias.
         * @param {string} fieldTypeAlias - The field type alias (i.e. core:stringField, core:intField etc).
         */
        exports.getFieldRenderControlAlias = function (fieldTypeAlias) {

            if (!fieldTypeAlias) {
                console.error('No field type alias specified.');
                return undefined;
            }

            if (!exports.fieldRenderControls) {
                console.error('spFormBuilderService.fieldRenderControls is not defined.');
                return undefined;
            }

            return exports.fieldRenderControls[fieldTypeAlias];
        };

        /**
         * Gets the field render control instance.
         * @param {string} fieldTypeAlias - The field type alias (i.e. core:stringField, core:intField etc).
         * @param {object} field - The field instance.
         */
        exports.getFieldRenderControlInstance = function (fieldTypeAlias, field, controlLabel) {
            var renderControl;
            var json;

            if (!fieldTypeAlias) {
                console.error('No field type alias specified.');
                return undefined;
            }

            if (!field) {
                console.error('No field specified.');
                return undefined;
            }

            renderControl = exports.getFieldRenderControlAlias(fieldTypeAlias);

            if (renderControl) {
                json = {
                    typeId: renderControl,
                    'console:renderingOrdinal': jsonInt(),
                    'console:renderingWidth': jsonInt(),
                    'console:renderingHeight': jsonInt(),
                    'console:renderingBackgroundColor': 'white',
                    'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:hideLabel': jsonBool(true),
                    'console:fieldToRender': field,
                    'console:containedControlsOnForm': []
                };

                if (field.description) {
                    json.description = field.description;
                }
                if (controlLabel) {
                    json.name = controlLabel;
                }

                return spEntity.fromJSON(json);
            }

            return null;
        };

        /**
         * Determines whether a field name already exists in a collection of fields.
         * @param {string} fieldDisplayName - The field display name (i.e. 'Yes/No', 'String' etc).
         */
        function fieldNameExists(existingFields, newFieldName) {

            if (!existingFields || !newFieldName) {
                return undefined;
            }

            return _.find(existingFields, function (existingField) {
                var existingFieldEntity = existingField.getEntity();

                return existingFieldEntity.name.toLowerCase() === newFieldName.toLowerCase() && existingFieldEntity.getDataState() !== spEntity.DataStateEnum.Delete;
            });
        }

        /**
         * Gets the name for a new field instance.
         * @param {string} fieldDisplayName - The field display name (i.e. 'Yes/No', 'String' etc).
         */
        exports.getNewFieldInstanceName = function (fieldDisplayName) {
            var existingFields;
            var existingRelationships;
            var newFieldName;
            var counter = 2;

            if (!fieldDisplayName) {
                console.error('Invalid field display name specified.');
                return undefined;
            }

            existingFields = exports.getDefinitionFields(false);
            existingRelationships = exports.getDefinitionForwardRelationships(false);

            existingFields = _.union(existingFields, existingRelationships);

            newFieldName = fieldDisplayName;

            while (fieldNameExists(existingFields, newFieldName)) {
                newFieldName = fieldDisplayName + ' ' + counter++;
            }

            /////
            // Need to be localized.
            /////
            return newFieldName;
        };

        /**
         * Gets the name for a new field instance.
         * @param {string} fieldDisplayName - The field display name (i.e. 'Yes/No', 'String' etc).
         */
        exports.getNewFieldInstanceDescription = function (fieldDisplayName) {

            if (!fieldDisplayName) {
                console.error('Invalid field display name specified.');
                return undefined;
            }

            /////
            // Need to be localized.
            /////
            return 'User created \'' + fieldDisplayName + '\' field.';
        };

        /**
         * Creates a new relationship instance.
         * @param {string} entityTypeAlias - The entity type alias (i.e. core:photoFileType etc).
         * @param {string} entityDisplayName - The entity display name (i.e. 'Image' etc).
         * @param {string} cardinality - Cardinality alias.
         * @param {object} additionalData - Additional json data.
         * @param {number} targetFieldGroup - The target field group id.
         */
        exports.createRelationship = function (entityTypeAlias, entityDisplayName, cardinality, additionalData, targetFieldGroupId) {
            var json;

            if (!entityTypeAlias) {
                console.error('No entity type alias specified.');
                return undefined;
            }

            if (!cardinality) {
                console.error('Invalid cardinality specified.');
                return undefined;
            }

            json = {
                typeId: 'core:relationship',
                hideOnToType: false,
                hideOnFromType: false,
                fromType: jsonLookup(exports.definition.id()),
                toType: jsonLookup(entityTypeAlias),
                toScriptName: jsonString(''),
                cardinality: jsonLookup(cardinality),
                inSolution: jsonLookup()
            };

            if (entityDisplayName) {
                json.name = exports.getNewFieldInstanceName(entityDisplayName);
            }

            /////
            // If there is any additional data, extend the json structure with it.
            /////
            if (additionalData) {
                _.extend(json, additionalData);
            }

            if (targetFieldGroupId || targetFieldGroupId === 0) {
                json['relationshipInFromTypeGroup'] = jsonLookup(targetFieldGroupId);
            }

            var instance = spEntity.fromJSON(json);

            var appId = spUtils.result(exports.definition, 'inSolution.idP');

            if (!appId) {
                appId = spNavService.getCurrentApplicationId();
            }

            if (appId) {
                instance.setInSolution(appId);
            }

            return instance;
        };

        /**
         * Creates a new field instance.
         * @param {string} fieldTypeAlias - The field type alias (i.e. core:stringField, core:intField etc).
         * @param {string} fieldDisplayName - The field display name (i.e. 'Yes/No', 'String' etc).
         * @param {object} additionalData - Additional json data.
         * @param {number} targetFieldGroup - The target field group id.
         */
        exports.createField = function (fieldTypeAlias, fieldDisplayName, additionalData, targetFieldGroupId) {
            var json;

            if (!fieldTypeAlias) {
                console.error('No field type alias specified.');
                return undefined;
            }

            if (!fieldDisplayName) {
                console.error('Invalid field display name specified.');
                return undefined;
            }

            json = {
                typeId: fieldTypeAlias,
                isOfType: [{
                    id: fieldTypeAlias,
                    alias: fieldTypeAlias
                }],
                name: exports.getNewFieldInstanceName(fieldDisplayName),
                fieldScriptName: exports.getNewFieldInstanceName(fieldDisplayName)
            };

            /////
            // If there is any additional data, extend the json structure with it.
            /////
            if (additionalData) {
                _.extend(json, additionalData);
            }

            if ((targetFieldGroupId || targetFieldGroupId === 0) && targetFieldGroupId >= 0) {
                json['fieldInGroup'] = {
                    id: targetFieldGroupId,
                    typeId: 'core:fieldGroup'
                };
            }

            var instance = spEntity.fromJSON(json);

            var appId = spUtils.result(exports.definition, 'inSolution.idP');

            if (!appId) {
                appId = spNavService.getCurrentApplicationId();
            }

            if (appId) {
                instance.setLookup('core:inSolution', appId);
            }

            return instance;
        };

        /**
         * Gets the fields that belong to a particular field group.
         * @param {number} fieldGroupId - The id of the field group whose fields are to be retrieved.
         */
        exports.getFieldsBelongingToFieldGroup = function (fieldGroupId) {
            var fields;

            if (!fieldGroupId) {
                console.error('Invalid field group id.');
                return undefined;
            }

            /////
            // Get all the fields including base class fields.
            /////
            fields = _.sortBy(_.uniqBy(exports.getDefinitionFields(false), function (field) {
                return field.getEntity().id();
            }), function (e) {
                return e.getEntity().name.toLowerCase();
            });

            if (fields) {
                return _.filter(fields, function (field) {
                    var fieldGroup;

                    /////
                    // Get the field group that this field belongs to.
                    /////
                    fieldGroup = field.getFieldGroupEntity();

                    /////
                    // Determine whether this
                    /////
                    if (fieldGroup) {
                        return fieldGroup.id() === fieldGroupId;
                    } else {
                        return false;
                    }
                });
            }

            return undefined;
        };

        /**
         * Gets the relationships that belong to a particular field group.
         * @param {number} fieldGroupId - The id of the field group whose relationships are to be retrieved.
         */
        exports.getRelationshipsBelongingToFieldGroup = function (fieldGroupId) {
            var relationships;

            if (!fieldGroupId) {
                console.error('Invalid field group id.');
                return undefined;
            }

            /////
            // Get all the relationships including base class relationships.
            /////
            relationships = _.sortBy(exports.getDefinitionRelationships(false), function (e) {
                return e.getEntity().name.toLowerCase();
            });

            if (relationships) {
                return _.filter(relationships, function (relationship) {
                    var fieldGroup;

                    if (relationship.isReverse()) {
                        /////
                        // Get the field group that this relationship belongs to.
                        /////
                        fieldGroup = relationship.getEntity().getRelationshipInToTypeGroup();
                    } else {
                        /////
                        // Get the field group that this relationship belongs to.
                        /////
                        fieldGroup = relationship.getEntity().getRelationshipInFromTypeGroup();
                    }

                    /////
                    // Determine whether this
                    /////
                    if (fieldGroup) {
                        return fieldGroup.id() === fieldGroupId;
                    } else {
                        return false;
                    }
                });
            }

            return undefined;
        };

        /**
         * Determines whether the specified field id belongs directly to the definition.
         * @param {number} fieldId - The id of the field.
         */

        exports.isDirectField = _.memoize(isDirectField);

        function isDirectField(fieldId) {
            var fields;

            if (!exports.definition) {
                return false;
            }

            if (!fieldId) {
                console.error('Invalid field id specified.');
                return false;
            }

            fields = exports.getDefinitionFields(true);

            return _.some(fields, function (field) {
                return field.id() === fieldId;
            });
        }

        /**
         * Determines whether the specified relationship id belongs directly to the definition.
         * @param {number} relationshipId - The id of the relationship.
         */
        exports.isDirectRelationship = _.memoize(isDirectRelationship);

        function isDirectRelationship(relationshipId) {
            var relationships;

            if (!exports.definition) {
                return false;
            }

            if (!relationshipId) {
                console.error('Invalid relationship id specified.');
                return false;
            }

            relationships = exports.getDefinitionRelationships(true);

            return _.some(relationships, function (relationship) {
                return relationship.getEntity().id() === relationshipId;
            });
        }

        /**
         * Determines whether the specified field group id belongs directly to the definition.
         * @param {number} fieldGroupId - The id of the field group.
         */
        exports.isDirectFieldGroup = function (fieldGroupId) {
            var fieldGroups;

            if (!exports.definition) {
                return false;
            }

            if (!fieldGroupId) {
                console.error('Invalid field group id specified.');
                return false;
            }

            fieldGroups = exports.getDefinitionFieldGroups(true);

            return _.some(fieldGroups, function (fieldGroup) {
                return fieldGroup.id() === fieldGroupId;
            });
        };

        /**
         * Returns true if the specified field group contains any fields or relationships, false otherwise.
         * @param {object} fieldGroup - The field group to check for fields.
         */
        exports.fieldGroupHasFields = function (fieldGroup) {
            var fields;
            var relationships;
            var fieldGroupId;
            var fieldGroupEntity;
            var hasContents = false;

            if (!fieldGroup) {
                return false;
            }

            if (fieldGroup instanceof spEntity._Entity) {
                fieldGroupEntity = fieldGroup;
            } else {
                fieldGroupEntity = fieldGroup.getEntity();
            }

            /////
            // Get the fields.
            /////
            fields = exports.getDefinitionFields(false);

            fieldGroupId = fieldGroupEntity.id();

            /////
            // Determine if the field group has any fields.
            /////
            hasContents = _.some(fields, function (field) {
                return field && field.getEntity().fieldInGroup && field.getEntity().fieldInGroup.id() === fieldGroupId;
            });

            if (!hasContents) {
                relationships = exports.getDefinitionRelationships(false);

                /////
                // Remove the fields from the field group.
                /////
                hasContents = _.some(relationships, function (relationship) {
                    if (relationship) {
                        if (relationship.isReverse()) {
                            return relationship.getEntity().relationshipInToTypeGroup && relationship.getEntity().relationshipInToTypeGroup.id() === fieldGroupId;
                        } else {
                            return relationship.getEntity().relationshipInFromTypeGroup && relationship.getEntity().relationshipInFromTypeGroup.id() === fieldGroupId;
                        }
                    }

                    return false;
                });
            }

            return hasContents;
        };

        /**
         * Removes the specified field group from the definition, removing the fields from both the definition and the form in the process.
         * @param {object} fieldGroup - The field group to be removed from the definition.
         */
        exports.removeFieldGroup = function (fieldGroup) {

            if (!fieldGroup) {
                return;
            }

            exports.removeFieldGroupFromDefinition(fieldGroup);
        };

        /**
         * Removes the specified field group from the definition.
         * @param {object} fieldGroup - The field group to be removed from the definition.
         */
        exports.removeFieldGroupFromDefinition = function (fieldGroup) {
            var fields;
            var relationships;
            var fieldGroups;
            var fieldGroupId;
            var fieldArray;
            var relationshipArray;
            var fieldGroupEntity;

            if (!fieldGroup) {
                return;
            }

            if (fieldGroup instanceof spEntity._Entity) {
                fieldGroupEntity = fieldGroup;
            } else {
                fieldGroupEntity = fieldGroup.getEntity();
            }

            /////
            // Get the field groups.
            /////
            fieldGroups = exports.getDefinitionFieldGroups(true);

            if (fieldGroups) {
                /////
                // Remove the field group.
                /////
                fieldGroups.remove(fieldGroupEntity);

                fieldGroupEntity.dataState = spEntity.DataStateEnum.Delete;
            }

            /////
            // Get the fields.
            /////
            fields = exports.getDefinitionFields(true);

            fieldArray = fields.slice();

            fieldGroupId = fieldGroupEntity.id();

            /////
            // Remove the fields from the field group.
            /////
            _.forEach(fieldArray, function (field) {
                if (field && field.fieldInGroup && field.fieldInGroup.id() === fieldGroupId) {
                    exports.removeField(field);
                }
            });

            /////
            // Get the relationships.
            /////
            relationships = exports.getDefinitionRelationships(true);

            relationshipArray = relationships.slice();

            /////
            // Remove the relationships from the field group.
            /////
            _.forEach(relationshipArray, function (relationship) {
                if (relationship) {
                    if (relationship.isReverse() && relationship.getEntity().relationshipInToTypeGroup && relationship.getEntity().relationshipInToTypeGroup.id() === fieldGroupId) {
                        exports.removeRelationship(relationship);
                    } else if (!relationship.isReverse() && relationship.getEntity().relationshipInFromTypeGroup && relationship.getEntity().relationshipInFromTypeGroup.id() === fieldGroupId) {
                        exports.removeRelationship(relationship);
                    }
                }
            });
        };

        /**
         * Removes the specified field from both the definition and form.
         * @param {object} field - The field to be removed from the definition and form.
         */
        exports.removeField = function (field) {

            if (!field) {
                return;
            }

            exports.removeFieldFromDefinition(field);
            exports.removeFieldFromForm(field);
        };

        /**
         * Removes the specified relationship from both the definition and form.
         * @param {object} relationship - The relationship to be removed from the definition and form.
         */
        exports.removeRelationship = function (relationship) {

            if (!relationship) {
                return;
            }

            exports.removeRelationshipFromDefinition(relationship);
            exports.removeRelationshipFromForm(relationship);
        };

        /**
         * Removes the specified field from the definition.
         * @param {object} field - The field to be removed from the definition.
         */
        exports.removeFieldFromDefinition = function (field) {
            var fields;
            var fieldEntity;

            if (!field) {
                return;
            }

            if (field instanceof spEntity._Entity) {
                fieldEntity = field;
            } else {
                fieldEntity = field.getEntity();
            }

            /////
            // Get the definition fields.
            /////
            fields = exports.getDefinitionFields(true);

            if (fields) {

                if (fieldEntity.getDataState() === spEntity.DataStateEnum.Create) {
                    /////
                    // Remove the field.
                    /////
                    fields.remove(fieldEntity);
                }

                fieldEntity.dataState = spEntity.DataStateEnum.Delete;
            }
        };

        /**
         * Adds a relationship to the definition.
         * @param {object} relationship - The relationship to add to the definition.
         */
        exports.addRelationshipToDefinition = function (relationship) {
            if (!exports.definition) {
                console.error('No current definition specified.');
            }

            if (relationship) {
                exports.definition.getRelationship('core:relationships').add(relationship);
            }
        };

        /**
         * Removes the specified relationship from the definition.
         * @param {object} relationship - The relationship to be removed from the definition.
         */
        exports.removeRelationshipFromDefinition = function (relationship) {
            var relationshipEntity;

            if (!relationship) {
                return;
            }

            if (relationship instanceof spEntity._Entity) {
                relationshipEntity = relationship;
            } else {
                relationshipEntity = relationship.getEntity();
            }

            if (relationshipEntity.getDataState() === spEntity.DataStateEnum.Create) {
                exports.definition.getRelationship('core:relationships').remove(relationshipEntity);
                exports.definition.getRelationship('core:reverseRelationships').remove(relationshipEntity);
            }

            /////
            // Delete the actual relationship also.
            /////
            relationshipEntity.dataState = spEntity.DataStateEnum.Delete;
        };

        /**
         * Removes the specified field from the form.
         * @param {object} field - The field to be removed from the form.
         */
        exports.removeFieldFromForm = function (field) {
            var fieldEntity;

            if (!field) {
                return;
            }

            if (field instanceof spEntity._Entity) {
                fieldEntity = field;
            } else {
                fieldEntity = field.getEntity();
            }

            exports.walkGraph(exports.form, function (node) {
                if (node) {
                    return node.containedControlsOnForm;
                }

                return null;
            }, function (node, parent) {
                if (node && node.fieldToRender) {
                    if (node.fieldToRender.id() === fieldEntity.id()) {
                        if (parent && parent.containedControlsOnForm) {

                            if (node.getDataState() === spEntity.DataStateEnum.Create) {
                                parent.containedControlsOnForm.remove(node);
                            }

                            node.dataState = spEntity.DataStateEnum.Delete;

                            return true;
                        }
                    }
                }

                return false;
            });
        };

        /**
         * Removes the specified relationship from the form.
         * @param {object} relationship - The relationship to be removed from the form.
         */
        exports.removeRelationshipFromForm = function (relationship) {
            var relationshipEntity;

            if (!relationship) {
                return;
            }

            if (relationship instanceof spEntity._Entity) {
                relationshipEntity = relationship;
            } else {
                relationshipEntity = relationship.getEntity();
            }

            exports.walkGraph(exports.form, function (node) {
                if (node) {
                    return node.containedControlsOnForm;
                }

                return null;
            }, function (node, parent) {
                if (node && node.relationshipToRender) {
                    if (node.relationshipToRender.id() === relationshipEntity.id()) {
                        if (parent && parent.containedControlsOnForm) {

                            if (node.getDataState() === spEntity.DataStateEnum.Create) {
                                parent.containedControlsOnForm.remove(node);
                            }

                            node.dataState = spEntity.DataStateEnum.Delete;

                            return true;
                        }
                    }
                } else if (node && node.containedControlsOnForm) {
                    //remove tab, bug 24861 delete a relationship from the LH pane, thautomatically removed the tab from the RH pane
                    removeTabFromFormByRelationship(node, relationshipEntity.id());
                }

                return false;
            });
        };

        /**
        * Get the lookups target type.
        */
        function getTargetType(lookup) {
            var targetType;
            var entity;

            if (lookup && lookup.getEntity && lookup.isReverse) {
                entity = lookup.getEntity();

                if (entity) {
                    if (!lookup.isReverse()) {
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
         * Gets the selected type.
         */

        function getSelectedType() {
            var type;
            var lookup;
            var targetType;

            lookup = exports.selectedLookup;

            if (lookup) {
                //comment out the following broken line until Anurag fixes it
                //targetType = getTargetType(lookup);
            } else {
                targetType = exports.definition;
            }

            type = new spResource.Type(targetType);

            return type;
        }

        /**
         * Gets the Type script name
         */
        function getTypeScriptName() {
            var targetType = exports.definition;

            if (targetType && targetType.typeScriptName) {
                return targetType.typeScriptName;
            }

            return null;
        }

        /**
         * Validate field name.
         * @param string newName -  The new value of name field.
         * @param long fieldId - The id of the field.
         */
        exports.validateFieldName = function (newName, fieldId) {

            var result = {
                hasError: false,
                message: ''
            };

            if (!newName || !fieldId) {
                result.hasError = true;
                result.message = 'invalid field info provided.';
                return result;
            }

            var newNameTrimmedLowercase = newName.toLowerCase().trim();

            var fields = getSelectedType().getFields();

            // check for duplicate name
            if (_.some(fields, function (field) {
                    var fieldEntity = field.getEntity();
                    if (fieldEntity && fieldEntity.fieldScriptName) {
                        return (fieldEntity.idP !== fieldId && field.getEntity().name.toLowerCase().trim() === newNameTrimmedLowercase);
                    }
                })) {
                result.hasError = true;
                result.message = 'The field name \'' + newName + '\' already exists on this object';
                return result;
            }

            // check new name is not same as the script name of the object
            var typeScriptName = getTypeScriptName();

            if (typeScriptName && typeScriptName.toLowerCase().trim() === newNameTrimmedLowercase) {
                result.hasError = true;
                result.message = 'The field name \'' + newName + '\' is currently being used as the object\'s script name';
                return result;
            }

            // check new name is not same as the script name of any of the existing fields
            if (_.some(fields, function (field) {
                    var fieldEntity = field.getEntity();
                    if (fieldEntity && fieldEntity.fieldScriptName) {
                        return (fieldEntity.idP !== fieldId && fieldEntity.fieldScriptName.toLowerCase().trim() === newNameTrimmedLowercase);
                    }
                })) {
                result.hasError = true;
                result.message = 'The field name \'' + newName + '\' is currently being used as the script name of a different field';
                return result;
            }

            // check new name is not same as the script name of any of the existing relationships
            var relationships = getSelectedType().getAllRelationships();

            if (relationships) {
                if (_.some(relationships, function (relationship) {
                        var relationshipEntity = relationship.getEntity();
                        return relationshipEntity && relationshipEntity.toScriptName && relationshipEntity.toScriptName.toLowerCase().trim() === newNameTrimmedLowercase && relationshipEntity.id() !== fieldId;
                    })) {
                    result.hasError = true;
                    result.message = 'The field name \'' + newName + '\' is currently being used on a different relationship';
                    return result;
                }
            }

            // todo: check for inherited fields and relationships too
            return result;
        };

        /**
         * Validate field script name.
         * @param string newScriptName -  The new value of name field.
         * @param long fieldId - The id of the field.
         */
        exports.validateFieldScriptName = function (newScriptName, fieldId) {

            var result = {
                hasError: false,
                message: ''
            };

            if (!newScriptName || !fieldId) {
                result.hasError = true;
                result.message = 'invalid field info provided.';
                return result;
            }

            var newScriptNameTrimmedLowercase = newScriptName.toLowerCase().trim();

            // check new name is not same as the script name of the object
            var typeScriptName = getTypeScriptName();

            if (typeScriptName && typeScriptName.toLowerCase().trim() === newScriptNameTrimmedLowercase) {
                result.hasError = true;
                result.message = 'The field script name \'' + newScriptName + '\' is currently being used as the object\'s script name';
                return result;
            }

            // check the script name doesn't collide with script name of any other field
            var fields = getSelectedType().getFields();

            if (_.some(fields, function (field) {
                    var fieldEntity = field.getEntity();
                    if (fieldEntity && fieldEntity.fieldScriptName) {
                        return (fieldEntity.idP !== fieldId && fieldEntity.fieldScriptName.toLowerCase().trim() === newScriptNameTrimmedLowercase);
                    }
                })) {
                result.hasError = true;
                result.message = 'The field script name \'' + newScriptName + '\' is currently being used as script name on a different field';
                return result;
            }

            // check the script name doesn't collide with script name of any other relationship
            var relationships = getSelectedType().getAllRelationships();

            if (relationships) {
                if (_.some(relationships, function (relationship) {
                        var relationshipEntity = relationship.getEntity();
                        return relationshipEntity && relationshipEntity.toScriptName && relationshipEntity.toScriptName.toLowerCase().trim() === newScriptNameTrimmedLowercase && relationshipEntity.id() !== fieldId;
                    })) {
                    result.hasError = true;
                    result.message = 'The field script name \'' + newScriptName + '\' already exists on a different relationship';
                    return result;
                }
            }
            return result;
        };

        /**
         * Update choice field render control type.
         * @param relationship.
         */
        exports.updateChoiceFieldControlType = function (relationship) {
            var relationshipEntity;

            if (!relationship) {
                return;
            }

            if (relationship instanceof spEntity._Entity) {
                relationshipEntity = relationship;
            } else {
                relationshipEntity = relationship.getEntity();
            }

            exports.walkGraph(exports.form, function (node) {
                if (node) {
                    return node.containedControlsOnForm;
                }

                return null;
            }, function (node, parent) {
                if (node && node.relationshipToRender) {
                    if (node.relationshipToRender.id() === relationshipEntity.id()) {
                        // change the control type
                        var relAlias = relationshipEntity.cardinality.nsAlias;

                        if (relAlias === 'core:manyToMany' && node.type.nsAlias !== 'console:multiChoiceRelationshipRenderControl') {
                            node.type = new spEntity.EntityRef('console:multiChoiceRelationshipRenderControl');
                        }

                        if (relAlias === 'core:manyToOne' && node.type.nsAlias !== 'console:choiceRelationshipRenderControl') {
                            node.type = new spEntity.EntityRef('console:choiceRelationshipRenderControl');
                        }
                    }
                }
                return false;
            });
        };

        /**
         * Removes the tab from the form by specified relationship id.
         * @param {object} node -  The node of the graph to walk.
         * @param long relationshipId - The relationship id to be removed from the form.
         */
        function removeTabFromFormByRelationship(node, relationshipId) {

            var tabControls = _.filter(node.containedControlsOnForm, function (cnotrol) {
                return cnotrol.type.alias() === 'console:tabContainerControl';
            });
            if (tabControls) {
                //loop through all tabContainerControl
                _.forEach(tabControls, function (tab) {
                    if (tab && tab.containedControlsOnForm) {
                        //remove through any child tab containner controls
                        removeTabFromFormByRelationship(tab, relationshipId);

                        //loop through all tab under each tabContainerControl
                        _.forEach(tab.containedControlsOnForm, function (tabContain) {
                            if (tabContain.containedControlsOnForm && tabContain.containedControlsOnForm[0] && tabContain.containedControlsOnForm[0].relationshipToRender && tabContain.containedControlsOnForm[0].relationshipToRender.id() === relationshipId) {
                                tab.containedControlsOnForm.remove(tabContain);
                            }
                        });
                    }
                });
            }

        }

        /**
         * Walks the graph represented by the specified root node.
         * @param {object} root - The root node of the graph to walk.
         * @param {function} branchQuery - Function that returns the branches.
         * @param {function} nodeCallback - Function that gets called for every node in the graph. Return true to stop traversal.
         * @param {object} parentNode - The parent node.
         */

        function walkGraph(root, branchQuery, nodeCallback, parentNode) {
            var branches;


            if (!root || !branchQuery || !nodeCallback) {
                return false;
            }

            if (nodeCallback(root, parentNode)) {
                return true;
            }

            branches = branchQuery(root);

            if (branches && _.isArray(branches)) {
                for (var index = 0; index < branches.length; index++) {
                    if (walkGraph(branches[index], branchQuery, nodeCallback, root)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Walks the graph represented by the specified root node.
         * @param {object} root - The root node of the graph to walk.
         * @param {function} branchQuery - Function that returns the branches.
         * @param {function} nodeCallback - Function that gets called for every node in the graph. Return true to stop traversal.
         */
        exports.walkGraph = function (root, branchQuery, nodeCallback) {
            walkGraph(root, branchQuery, nodeCallback, null);
        };

        /**
         * Sets the initial form bookmark for change detection (and potentially undo)
         */
        exports.setInitialFormBookmark = function () {
            exports.initialFormBookmark = exports.form ? exports.form.graph.history.addBookmark('Set form') : null;
        };

        /**
         * Determines if there are unsaved changes to the form or definition.
         */
        exports.unsavedChanges = function () {

            return exports.unsavedFormChanges();
        };

        /**
         * Determines if there are unsaved changes to the form.
         */
        exports.unsavedFormChanges = function () {
            if (exports.form) {
                if (exports.form.getDataState() === spEntity.DataStateEnum.Create) {
                    return true;
                }
                if (exports.initialFormBookmark && exports.form.graph.history.changedSinceBookmark(exports.initialFormBookmark)) {
                    return true;
                }
            }

            return false;
        };

        /**
         * Determines whether the relationship currently resides on the form.
         * @param {object} relationship - Relationship to look for.
         */
        function isRelationshipOnForm(relationship, callback) {
            if (!relationship || !relationship.isRelationship || !_.result(relationship, 'isRelationship') || !relationship.value) {
                return false;
            }

            return findRelationship(exports.form, relationship, callback);
        }


        /**
         * Determines whether the field currently resides on the form.
         * @param {object} field - Field to look for.
         */
        function isFieldOnForm(field, callback) {
            if (!field || !field.isField || !field.isField()) {
                return false;
            }

            return findField(exports.form, field, callback);
        }


        /**
         * Finds the field within the container.
         * @param {object} container - The container to search.
         * @param {object} relationship - Relationship to look for.
         */
        function findRelationship(container, relationship, callback) {

            return findControlOnForm(container, relationship, function (containedControl) {
                return containedControl.relationshipToRender;
            }, function (rel, control) {
                var reverseEqual;
                var controlReversed;

                if (rel && control) {
                    controlReversed = (control.isReversed === undefined || control.isReversed === null) ? false : control.isReversed;
                    reverseEqual = rel.value.isReverse() === controlReversed;

                    if (!reverseEqual) {
                        return false;
                    }

                    return checkDataPath(control);
                }

                return false;
            }, callback);
        }

        /**
         * Finds the field within the container.
         * @param {object} container - The container to search.
         * @param {object} field - Field to look for.
         */
        function findField(container, field, callback) {

            return findControlOnForm(container, field, function (containedControl) {
                return containedControl.fieldToRender;
            }, function (fld, control) {
                if (fld && control) {
                    return checkDataPath(control);
                }

                return false;
            }, callback);
        }

        /**
         * Checks the relationship data path.
         * @param {object} control - The control to check.
         */
        function checkDataPath(control) {
            if (control.controlRelatedEntityDataPathNodes && control.controlRelatedEntityDataPathNodes.length) {
                var firstElement = _.maxBy(control.controlRelatedEntityDataPathNodes, function (node) {
                    return node.dataPathNodeOrdinal;
                });

                if (firstElement && firstElement.dataPathNodeRelationship) {
                    if (!exports.selectedLookup) {
                        return false;
                    }

                    return firstElement.dataPathNodeRelationship.id() === exports.selectedLookup.getEntity().id();
                }
            }

            return !exports.selectedLookup;
        }

        /**
         * Finds the control within the container.
         * @param {object} container - The container to search.
         * @param {object} control - Control to look for.
         * @param {function} getComparison - The compare callback.
         * @param {function} postCompare - Post compare callback.
         */
        function findControlOnForm(container, control, getComparison, postCompare, callback) {
            var comparison;
            var entity;

            if (container && container.containedControlsOnForm) {
                for (var index = 0; index < container.containedControlsOnForm.length; index++) {

                    if (container.containedControlsOnForm[index].getDataState() === spEntity.DataStateEnum.Delete) {
                        continue;
                    }

                    if (getComparison) {
                        comparison = getComparison(container.containedControlsOnForm[index]);
                        entity = control.getEntity();

                        if (comparison && entity && (comparison === entity || comparison.id() === entity.id())) {
                            if (postCompare) {
                                if (postCompare(control, container.containedControlsOnForm[index])) {
                                    if (callback) {
                                        callback(container.containedControlsOnForm[index], container);
                                    }

                                    if (container.containedControlsOnForm[index].getDataState() === spEntity.DataStateEnum.Delete) {
                                        continue;
                                    }

                                    return true;
                                }
                            } else {
                                if (callback) {
                                    callback(container.containedControlsOnForm[index], container);
                                }

                                if (container.containedControlsOnForm[index].getDataState() === spEntity.DataStateEnum.Delete) {
                                    continue;
                                }

                                return true;
                            }
                        }
                    }

                    if (findControlOnForm(container.containedControlsOnForm[index], control, getComparison, postCompare, callback)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Store the last drag values.
         * @param {object} event - The last event.
         * @param {object} source - The last source.
         * @param {object} target - The last target.
         * @param {object} dragData - The last drag data.
         * @param {object} dropData - The last drop data.
         * @param {bool} fieldContainer - Whether the container holds a field.
         * @param {object} control - The last control.
         */
        exports.setLastDragValues = function (event, source, target, dragData, dropData, fieldContainer, control) {
            exports.lastDragValues.event = event;
            exports.lastDragValues.source = source;
            exports.lastDragValues.target = target;
            exports.lastDragValues.dragData = dragData;
            exports.lastDragValues.dropData = dropData;
            exports.lastDragValues.fieldContainer = fieldContainer;
            exports.lastDragValues.control = control;
        };

        /**
         * Clear the last drag values.
         */
        exports.clearLastDragValues = function () {
            exports.lastDragValues.event = undefined;
            exports.lastDragValues.source = undefined;
            exports.lastDragValues.target = undefined;
            exports.lastDragValues.dragData = undefined;
            exports.lastDragValues.dropData = undefined;
            exports.lastDragValues.fieldContainer = undefined;
            exports.lastDragValues.control = undefined;
        };

        /**
         * Gets the last drag values.
         */
        exports.getLastDragValues = function () {
            return exports.lastDragValues;
        };

        /////
        // Creates a new hidden stack container.
        /////
        exports.createHiddenStackContainer = function (alias) {
            var json;

            /////
            // Construct a JSON structure.
            /////
            json = {
                typeId: 'console:' + alias,
                'console:renderingOrdinal': jsonInt(),
                'console:renderingWidth': jsonInt(),
                'console:renderingHeight': jsonInt(),
                'console:renderingBackgroundColor': 'white',
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:hideLabel': jsonBool(true),
                'console:containedControlsOnForm': []
            };

            return spEntity.fromJSON(json);
        };

        /////
        // Creates a new console behavior.
        /////
        exports.createEmptyBehavior = function () {
            return spEntity.fromJSON({
                typeId: 'console:consoleBehavior',
                'k:treeIconBackgroundColor': jsonString(),
                'k:treeIcon': jsonLookup()
            });
        };

        return exports;
    });