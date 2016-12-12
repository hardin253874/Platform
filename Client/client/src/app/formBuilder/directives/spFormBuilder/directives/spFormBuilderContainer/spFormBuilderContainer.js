// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, $, sp, spEntity, spResource, jsonInt, jsonBool, jsonLookup */

(function () {
    'use strict';

    /**
     * Module implementing a form builder container.
     * spFormBuilderContainer provides the container element for use when building forms.
     *
     * @module spFormBuilderContainer
     * @example

     Using the spFormBuilderContainer:

     &lt;sp-form-builder-container&gt;&lt;/sp-form-builder-container&gt

     */
    angular.module('mod.app.formBuilder.directives.spFormBuilderContainer', [
        'mod.common.spUuidService',
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.configureDialog.Controller',
        'mod.common.alerts',
        'sp.navService',
        'mod.app.formBuilder.factories.ControlWrapper',
        'mod.app.formBuilder.factories.FieldContainer',
        'mod.common.ui.spMeasureArrange'
    ]);

    angular.module('mod.app.formBuilder.directives.spFormBuilderContainer')
        .directive('spFormBuilderContainer', spFormBuilderContainer);

    function spFormBuilderContainer($q, spUuidService, spFormBuilderService, controlConfigureDialogFactory,
                                    spAlertsService, spNavService, spState, $timeout, ControlWrapper, FieldContainer,
                                    spMeasureArrangeService) {

        'ngInject';

        // convenience aliases
        var quadrants = spFormBuilderService.quadrants;
        var containers = spFormBuilderService.containers;

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            replace: true,
            transclude: true,
            scope: {
                control: '=?',
                parentControl: '=?',
                parentTab: '=?',
                fieldContainer: '=?',
                tabContainer: '=?',
                disableResize: '=?',
                resizeHandles: '=?',
                canResizeVertically: '=?',
                canResizeHorizontally: '=?',
                transcludeConfigureButton: '=?',
                transcludeCloseButton: '=?'
            },
            templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderContainer/spFormBuilderContainer.tpl.html',
            link: link
        };

        function link(scope, element) {

            scope.hoverOptions = {
                className: 'sp-form-builder-container',
                hoverClassName: 'sp-form-builder-hover',
                childSelector: '> .ui-resizable-handle, > .sp-form-builder-padding-target > .sp-form-builder-toolbar'
            };

            scope.tabDesignData = {
                tabs: [
                    {
                        name: 'Add Tabs',
                        isActive: true,
                        tooltip: 'Add relationships by dragging them into this control',
                        ordinal: 0,
                        url: 'formBuilder/directives/spFormBuilder/templates/tabControl.tpl.html',
                        model: {
                            control: scope.control,
                            parentControl: scope.parentControl,
                            isInDesign: true,
                            isPlaceholder: true,
                        }
                    }
                ],
                configureCallback: configureClick
            };

            /////
            // Ensure the insert indicator is hidden when dragging over the navigation view.
            /////
            scope.$on('navViewDragOver', function () {
                spFormBuilderService.hideInsertIndicator();
                spFormBuilderService.destroyInsertOverlayIndicator();
            });

            scope.tab = scope.control && !scope.parentControl;
            scope.explicitContainer = scope.control && scope.parentControl && spFormBuilderService.isExplicitContainer(scope.control);
            scope.showLabel = scope.control && !scope.control.hideLabel;
            scope.splittable = getCanSplit();

            if (_.isUndefined(scope.canResizeHorizontally)) {
                scope.canResizeHorizontally = true;
            }

            if (_.isUndefined(scope.canResizeVertically)) {
                scope.canResizeVertically = true;
            }

            if (_.isUndefined(scope.fieldContainer)) {
                scope.fieldContainer = false;
            }

            if (element) {

                /////
                // Assign an id.
                /////
                element[0].id = element[0].id || spUuidService.create();
            }

            /////
            // Add an existing field to the form.
            /////
            function addExistingFieldToForm(fieldEntity) {
                var renderControl;
                var json;

                if (fieldEntity.isOfType && fieldEntity.isOfType.length > 0) {

                    if (spFormBuilderService.fieldRenderControls) {
                        renderControl = spFormBuilderService.fieldRenderControls[fieldEntity.isOfType[0].getAlias()];

                        if (renderControl) {
                            /////
                            // Construct a JSON structure.
                            /////
                            json = {
                                typeId: renderControl.getNsAlias(),
                                'console:fieldToRender': fieldEntity,
                                'console:renderingOrdinal': jsonInt(),
                                'console:renderingWidth': jsonInt(),
                                'console:renderingHeight': jsonInt(),
                                'console:renderingBackgroundColor': 'white',
                                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                            };

                            handleDataPath(json);

                            return spEntity.fromJSON(json);
                        }
                    }
                }

                return undefined;
            }

            /////
            // Add an existing relationship to the form.
            /////
            function addExistingRelationshipToForm(relationship) {
                var renderControlAlias;
                var json;

                if (relationship) {

                    renderControlAlias = spFormBuilderService.getRelationshipRenderControlAlias(relationship);

                    if (renderControlAlias) {
                        /////
                        // Construct a JSON structure.
                        /////
                        json = {
                            typeId: renderControlAlias,
                            'console:relationshipToRender': relationship.getEntity(),
                            'console:renderingOrdinal': jsonInt(),
                            'console:renderingWidth': jsonInt(),
                            'console:renderingHeight': jsonInt(),
                            'console:renderingBackgroundColor': 'white',
                            'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                            'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                        };

                        if (relationship.isReverse) {
                            json['console:isReversed'] = !!relationship.isReverse();
                        }

                        if (renderControlAlias === 'console:tabRelationshipRenderControl') {
                            json['console:renderingHorizontalResizeMode'] = jsonLookup('console:resizeSpring');
                            json['console:renderingVerticalResizeMode'] = jsonLookup('console:resizeSpring');
                        }

                        handleDataPath(json);

                        return spEntity.fromJSON(json);
                    }
                }

                return undefined;
            }

            /////
            // Handle the data path nodes.
            /////
            function handleDataPath(json) {

                if (!json) {
                    return;
                }

                if (spFormBuilderService.selectedLookup) {
                    json['console:controlRelatedEntityDataPathNodes'] = [
                        {
                            typeId: 'console:relatedEntityDataPathNode',
                            'console:dataPathNodeOrdinal': jsonInt(0),
                            'console:dataPathNodeRelationshipDirection': jsonLookup('core:forward'),
                            'console:dataPathNodeRelationship': spFormBuilderService.selectedLookup.getEntity()
                        }
                    ];
                }
            }

            /////
            // Generic entity search algorithm.
            /////
            function searchEntity(entity, entityCollectionCallback, target, foundAction) {
                var collection;
                var found;

                if (!entity || !entityCollectionCallback || !target) {
                    return undefined;
                }

                collection = entityCollectionCallback(entity);

                if (!collection) {
                    return undefined;
                }

                for (var index = 0; index < collection.length; index++) {

                    if (collection[index] === target) {

                        if (foundAction) {
                            foundAction(entity);
                        }

                        return entity;
                    }

                    found = searchEntity(collection[index], entityCollectionCallback, target, foundAction);

                    if (found) {

                        if (foundAction) {
                            foundAction(entity);
                        }

                        return found;
                    }
                }

                return undefined;
            }

            /////
            // Locate the entities parent.
            /////
            function locateEntityParent(entity) {

                if (!entity) {
                    return undefined;
                }

                return searchEntity(spFormBuilderService.form, function (container) {
                    if (!container) {
                        return undefined;
                    }
                    //set containedControlsOnForm relationship autoCardinality
                    container.containedControlsOnForm.autoCardinality();
                    return container.containedControlsOnForm;
                }, entity);
            }

            /////
            // Moves the entity.
            /////
            function moveEntity(entity) {
                var parent;
                var parentControls;

                if (!entity) {
                    return undefined;
                }

                parent = locateEntityParent(entity);

                if (parent) {
                    parentControls = new ControlWrapper(parent);
                    parentControls.remove(entity);
                }

                return {
                    entity: entity,
                    parent: parent
                };
            }

            /////
            // Add an existing field group to the form.
            /////
            function addExistingFieldGroupToForm(fieldGroupEntity) {
                var json;
                var fields;
                var relationships;

                /////
                // Construct a JSON structure.
                /////
                json = {
                    typeId: 'console:verticalStackContainerControl',
                    name: fieldGroupEntity.name,
                    'console:renderingOrdinal': jsonInt(),
                    'console:renderingWidth': jsonInt(),
                    'console:renderingHeight': jsonInt(),
                    'console:renderingBackgroundColor': 'white',
                    'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                    'console:hideLabel': jsonBool(false),
                    'console:containedControlsOnForm': []
                };

                fields = spFormBuilderService.getFieldsBelongingToFieldGroup(fieldGroupEntity.id());
                relationships = spFormBuilderService.getRelationshipsBelongingToFieldGroup(fieldGroupEntity.id());

                var entries = _.sortBy(_.union(fields, relationships), function (entry) {
                    return entry.getName();
                });

                spFormBuilderService.refreshMemoizedFunctions();

                _.forEach(entries, function (entry) {
                    var entity;
                    var renderCtrl;

                    if (entry instanceof spResource.Field) {
                        if (!spFormBuilderService.isFieldOnForm(entry)) {
                            entity = entry.getEntity();

                            renderCtrl = spFormBuilderService.fieldRenderControls[entity.isOfType[0].getAlias()];

                            json['console:containedControlsOnForm'].push({
                                typeId: renderCtrl.getNsAlias(),
                                'console:renderingOrdinal': jsonInt(),
                                'console:renderingWidth': jsonInt(),
                                'console:renderingHeight': jsonInt(),
                                'console:renderingBackgroundColor': 'white',
                                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                                'console:fieldToRender': entity
                            });
                        }
                    } else if (entry instanceof spResource.Relationship) {
                        if (!spFormBuilderService.isRelationshipOnForm(entry)) {
                            entity = entry.getEntity();

                            renderCtrl = spFormBuilderService.getRelationshipRenderControlAlias(entry);

                            json['console:containedControlsOnForm'].push({
                                typeId: renderCtrl,
                                'console:renderingOrdinal': jsonInt(),
                                'console:renderingWidth': jsonInt(),
                                'console:renderingHeight': jsonInt(),
                                'console:renderingBackgroundColor': 'white',
                                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                                'console:relationshipToRender': entity,
                                'console:isReversed': entry.isReverse()
                            });
                        }
                    }

                });

                return spEntity.fromJSON(json);
            }

            /////
            // Inserts a new control in the collection of contained controls of the form.
            /////
            function insertAtContainedControlsOnForm(container, ordinal, control) {
                var containedControlsWrapper = new ControlWrapper(container);
                containedControlsWrapper.insert(control, ordinal);
            }

            /**
             * Inserts
             * @param {Object} container entity
             * @param {Number} index
             * @param {Object} control entity
             */
            function insertAtIndexContainedControlsOnForm(container, index, control) {
                var containedControlsWrapper = new ControlWrapper(container);
                containedControlsWrapper.insertAtIndex(control, index);
            }

            /////
            // Removes the control with the specified ordinal.
            /////
            function removeAtContainedControlsOnForm(container, ordinal) {
                var controls;
                var existingControls;

                controls = new ControlWrapper(container);

                existingControls = controls.find(ordinal);

                if (existingControls) {
                    controls.remove(existingControls);
                }

                /////
                // Returns at most a single element.
                /////
                return existingControls;
            }

            /////
            // Removes all the controls.
            /////
            function removeAllContainedControlsOnForm(container) {
                var controls;
                var existingControls;

                controls = new ControlWrapper(container);
                existingControls = controls.removeAll();

                /////
                // Returns an array.
                /////
                return existingControls;
            }

            /////
            // Inserts a control before/after a specific index;
            /////
            function insertControlAt(targetControl, index, newControl, insertBefore) {

                if (!targetControl || index < 0) {
                    return;
                }

                if (insertBefore) {
                    insertAtContainedControlsOnForm(targetControl, index, newControl);
                } else {
                    insertAtContainedControlsOnForm(targetControl, index + 1, newControl);
                }
            }

            /////
            // Repackage the controls in the target control into a new container of the specified type.
            /////
            function repackageControls(targetControl, containerType, index, newControl, insertBefore) {
                var newContainer;
                var existingControls;

                if (!targetControl || !containerType) {
                    return;
                }

                newContainer = spFormBuilderService.createHiddenStackContainer(containerType);

                existingControls = removeAllContainedControlsOnForm(targetControl, true);

                var newContainerWrapper = new ControlWrapper(newContainer);

                newContainerWrapper.addRange(existingControls);

                insertControlAt(targetControl, 0, newContainer, true, true);

                if (newControl) {
                    insertControlAt(newContainer, index, newControl, insertBefore);
                }
            }

            /////
            // Inserts the new control relative to the existing control.
            /////
            function insertControl(newControl, containerType, insertBefore, dropControl, dropControlIsField) {
                var ordinal;
                var typeName;
                var targetControl;
                var newStackContainer;
                var existingControl;

                /////
                // Get the target control.
                /////
                targetControl = scope.parentControl || scope.control;

                ordinal = dropControl.renderingOrdinal;

                if (!ordinal && ordinal !== 0) {
                    ordinal = _.indexOf(targetControl.containedControlsOnForm, dropControl);
                }

                if (ordinal >= 0) {

                    typeName = targetControl.firstTypeId().getAlias();

                    if (typeName === containers.form || typeName === containers.header || typeName === containers.screen || typeName === containers.tab) {
                        typeName = containers.vertical;
                    }

                    if (typeName !== containerType) {
                        /////
                        // Create a new  stack container control.
                        /////
                        newStackContainer = spFormBuilderService.createHiddenStackContainer(containerType);

                        /////
                        // Remove the existing element from its parent.
                        /////
                        existingControl = removeAtContainedControlsOnForm(targetControl, ordinal, true);

                        if (existingControl && existingControl.renderingHorizontalResizeMode.alias() === 'console:resizeAutomatic') {
                            if (existingControl.renderingWidth) {
                                newStackContainer.renderingWidth = existingControl.renderingWidth;
                            }
                        }

                        if (existingControl && existingControl.renderingVerticalResizeMode.alias() === 'console:resizeAutomatic') {
                            if (existingControl.renderingHeight) {
                                newStackContainer.renderingHeight = existingControl.renderingHeight;
                            }
                        }

                        var newStackContainerControls = new ControlWrapper(newStackContainer);

                        if (insertBefore) {
                            newStackContainerControls.add(newControl);
                            newStackContainerControls.add(existingControl);
                        } else {
                            newStackContainerControls.add(existingControl);
                            newStackContainerControls.add(newControl);
                        }

                        insertAtContainedControlsOnForm(targetControl, ordinal, newStackContainer, true);

                    } else {
                        insertControlAt(targetControl, ordinal, newControl, insertBefore);
                    }
                } else {
                    if (targetControl.getType().getAlias() === containers.form || targetControl.getType().getAlias() === containers.screen) {
                        if (containerType === containers.vertical || targetControl.containedControlsOnForm.length === 0) {
                            /////
                            // If inserting vertically on the base custom form, or there are no controls on the form, just insert.
                            /////
                            insertControlAt(targetControl, insertBefore ? 0 : targetControl.containedControlsOnForm.length, newControl, insertBefore);
                        } else {
                            if (targetControl.containedControlsOnForm.length === 1) {
                                if (targetControl.containedControlsOnForm[0].getType().getAlias() === containers.horizontal) {
                                    /////
                                    // Only one child and its orientation matches the requested orientation, just add.
                                    /////
                                    insertControlAt(targetControl.containedControlsOnForm[0], insertBefore ? 0 : targetControl.containedControlsOnForm[0].containedControlsOnForm.length, newControl, insertBefore);
                                } else {
                                    /////
                                    // Only one child but its orientation doesn't match the requested orientation, repackage the child and the new control into a new container that matches the requested orientation.
                                    /////
                                    repackageControls(targetControl, containerType, insertBefore ? 0 : targetControl.containedControlsOnForm.length, newControl, insertBefore);
                                }
                            } else {
                                /////
                                // More than one child so repackage all the children into a new vertical container and then repackage that container into another new container that matches the requested orientation.
                                /////
                                repackageControls(targetControl, containers.vertical);
                                repackageControls(targetControl, containerType, insertBefore ? 0 : targetControl.containedControlsOnForm.length, newControl, insertBefore);
                            }
                        }
                    } else {
                        insertAtContainedControlsOnForm(targetControl, 0, newControl);
                    }
                }

                return true;
            }

            /////
            // Inserts the new control to the left of the existing control.
            /////
            function insertAtLeft(newControl, dropControl, dropControlIsField) {
                insertControl(newControl, containers.horizontal, true, dropControl, dropControlIsField);

                return true;
            }

            /////
            // Inserts the new control above the existing control.
            /////
            function insertAtTop(newControl, dropControl, dropControlIsField) {
                insertControl(newControl, containers.vertical, true, dropControl, dropControlIsField);

                return true;
            }

            /////
            // Inserts the new control to the right of the existing control.
            /////
            function insertAtRight(newControl, dropControl, dropControlIsField) {
                insertControl(newControl, containers.horizontal, false, dropControl, dropControlIsField);

                return true;
            }

            /////
            // Inserts the new control below the existing control.
            /////
            function insertAtBottom(newControl, dropControl, dropControlIsField) {
                insertControl(newControl, containers.vertical, false, dropControl, dropControlIsField);

                return true;
            }

            /////
            // Finds the closest descendants with the specified selector.
            /////
            function closestDescendants(source, selector) {

                if (!source || source.length === 0 || !selector) {
                    return;
                }

                var children = source.children(selector);
                if (children.length > 0) {
                    return children;
                }

                return closestDescendants(source.children(), selector);
            }

            /////
            // Finds the closest selector descendant of each source element.
            /////
            function closestDescendantsExhaustive(source, selector) {
                var children;

                if (!source || source.length === 0 || !selector) {
                    return;
                }

                var results = [];

                _.forEach(source, function (sourceItem) {
                    var item = $(sourceItem);

                    var found = item.find(selector).first();

                    if (found && found.length) {
                        results.push(found[0]);
                    }
                });

                return $(results);
            }

            /////
            // Performs a directional insert based on the mouse coordinates.
            /////
            function performDirectionalInsert(event, target, newControl, positionCheck, exceedCheck, sameDestination, control) {
                var position = null;
                var descendants;
                var clientRect;
                var exceed = true;

                if (!event || !target || !newControl || !positionCheck || !exceedCheck) {
                    return;
                }

                if (control.containedControlsOnForm.length === 0) {
                    position = 0;

                } else {

                    descendants = closestDescendants($(target), '.child-container');
                    if (descendants) {
                        for (var index = 0; index < descendants.length; index++) {

                            clientRect = descendants[index].getBoundingClientRect();

                            if (positionCheck(clientRect) && position === null) {
                                position = index;
                            }

                            exceed = exceed && exceedCheck(clientRect);
                        }

                        if (position === null || exceed) {
                            position = descendants.length;
                        }
                    } else {
                        position = 0;
                    }
                }

                if (sameDestination && newControl.renderingOrdinal < position) {
                    position--;
                }

                insertAtIndexContainedControlsOnForm(control, position, newControl);
            }

            /////
            // Inserts the new control at a calculated position within a horizontal container.
            /////
            function performHorizontalInsert(event, target, newControl, sameDestination, control) {

                performDirectionalInsert(event, target, newControl, positionCheck, exceedCheck, sameDestination, control);

                /////
                // Addition of 10 is to account for margin.
                /////

                function positionCheck(clientRect) {
                    return event.originalEvent.clientX <= clientRect.left + 10;
                }

                function exceedCheck(clientRect) {
                    return event.originalEvent.clientY > clientRect.bottom - 10;
                }
            }

            /////
            // Inserts the new control at a calculated position within a vertical container.
            /////
            function performVerticalInsert(event, target, newControl, sameDestination, control) {

                performDirectionalInsert(event, target, newControl, positionCheck, exceedCheck, sameDestination, control);

                /////
                // Addition of 10 is to account for margin.
                /////

                function positionCheck(clientRect) {
                    return event.originalEvent.clientY <= clientRect.top + 10;
                }

                function exceedCheck(clientRect) {
                    return event.originalEvent.clientX > clientRect.right - 10;
                }
            }

            function removeDeletedControl(control, parentControl) {
                parentControl.containedControlsOnForm.remove(control);
            }

            /////
            // Drop function.
            /////
            function drop(event, source, target, dragData, dropData, fieldContainer, control) {
                var newControl;
                var alias;
                var moveResult;
                var promise;
                var existingControl;
                var existingParentControl;
                var canDrop = getCanDrop(dragData, fieldContainer, control, undefined, function (c, pc) {
                    existingControl = c;
                    existingParentControl = pc;
                });

                if (!canDrop) {

                    // try again
                    if (control !== scope.parentControl) {

                        control = scope.parentControl;
                        var canDeferDrop = getCanDrop(dragData, false, control, undefined, function (c, pc) {
                            existingControl = c;
                            existingParentControl = pc;
                        });

                        if (canDeferDrop) {

                            console.log('form builder: deferring drop to ' + control.getType().getAlias());

                            scope.$emit('deferDrop', {
                                event: event,
                                source: source,
                                target: target,
                                dragData: dragData,
                                dropData: dropData,
                                control: control,
                                fieldContainer: false,
                                quadrant: spFormBuilderService.currentParentDropTargetQuadrant
                            });
                        }
                    }

                    return false;
                }

                if (existingControl && existingParentControl) {
                    removeDeletedControl(existingControl, existingParentControl);
                }

                if (control) {

                    if (!control.containedControlsOnForm) {
                        control.registerRelationship('console:containedControlsOnForm');
                    }

                    if (dragData && dragData.newControl) {
                        /////
                        // Passed through from previous handler
                        /////
                        newControl = dragData.newControl;

                    } else if (dragData && dragData.newControlFactory) {
                        /////
                        // Screen Builder controls.
                        /////
                        newControl = dragData.newControlFactory();

                    } else if (dragData && dragData.getRenderControlInstance) {
                        /////
                        // Dragging new fields and display controls directly onto the form.
                        /////
                        promise = dragData.getRenderControlInstance().then(function (result) {
                            spFormBuilderService.definitionRevision++;

                            return result;
                        });

                    } else if (dragData && dragData.getValue && dragData.getValue() instanceof spResource.Field) {
                        /////
                        // Dragging an existing field onto the form.
                        /////
                        newControl = addExistingFieldToForm(dragData.getEntity());

                    } else if (dragData && dragData.getValue && dragData.getValue() instanceof spResource.Relationship) {
                        /////
                        // Dragging an existing relationship onto the form.
                        /////
                        newControl = addExistingRelationshipToForm(dragData.getValue());

                    } else if (dragData && dragData instanceof spResource.FieldGroup) {
                        /////
                        // Dragging an existing field group onto the form.
                        /////
                        newControl = addExistingFieldGroupToForm(dragData.getEntity());

                    } else if (dragData && dragData instanceof spEntity._Entity) {

                        if (spFormBuilderService.currentDropTarget && spFormBuilderService.currentDropTarget !== dragData && control !== dragData) {
                            /////
                            // On form rearrangement.
                            /////
                            moveResult = moveEntity(dragData);

                            newControl = moveResult.entity;
                        }
                    }
                }

                if (!promise) {
                    promise = $q.when(newControl);
                }

                promise.then(function (newControl) {

                    if (!newControl) {
                        console.warn(['formBuilder - ignoring drop on ', target.tagName, target.className,
                            'id=' + scope.$id, 'formControl=' + scope.formControl].join());
                        return;
                    }

                    var dropControl = spFormBuilderService.currentDropTarget;
                    var dropControlIsField = spFormBuilderService.currentDropTargetIsField;
                    var quadrant = spFormBuilderService.currentDropTargetQuadrant;
                    var success = false;

                    adjustControlHeight(newControl, dropControl, dropControlIsField, quadrant);

                    if (dropControl && quadrant) {
                        switch (quadrant) {
                            case quadrants.left:
                                success = insertAtLeft(newControl, dropControl, dropControlIsField);
                                break;
                            case quadrants.top:
                                success = insertAtTop(newControl, dropControl, dropControlIsField);
                                break;
                            case quadrants.right:
                                success = insertAtRight(newControl, dropControl, dropControlIsField);
                                break;
                            case quadrants.bottom:
                                success = insertAtBottom(newControl, dropControl, dropControlIsField);
                                break;
                        }
                    }

                    if (!success) {

                        alias = control.getType().getAlias();

                        var sameDestination = false;

                        if (moveResult && moveResult.parent === control) {
                            sameDestination = true;
                        }

                        var controlControlsWrapper = new ControlWrapper(control);

                        /////
                        // If the source and target containers are the same and the item is being moved down/right then decrement the position.
                        /////
                        if (alias === containers.horizontal) {
                            performHorizontalInsert(event, target, newControl, sameDestination, control);
                        } else if (alias === containers.vertical || alias === containers.header || alias === containers.form) {
                            performVerticalInsert(event, target, newControl, sameDestination, control);
                        } else if (alias === containers.tab) {
                            // create a hidden container and add newControl to it. And add the container to the array.
                            var newStackContainer = spFormBuilderService.createHiddenStackContainer(containers.vertical);
                            newStackContainer.setRenderingVerticalResizeMode('console:resizeSpring');
                            newStackContainer.setRenderingBackgroundColor('transparent');
                            // set new container autoCardinality for move existing control in this tab
                            newStackContainer.containedControlsOnForm.autoCardinality();
                            newStackContainer.name = spFormBuilderService.getControlTitle(newControl);
                            var newStackContainerControls = new ControlWrapper(newStackContainer);

                            newStackContainerControls.add(newControl);
                            controlControlsWrapper.add(newStackContainer);
                        } else {

                            /////
                            // Append to the end of the array.
                            /////
                            controlControlsWrapper.add(newControl);
                        }
                    }

                    performLayout();

                })
                    .finally(function () {
                        if (!fieldContainer && control) {
                            spFormBuilderService.currentDropTarget = undefined;
                            spFormBuilderService.currentDropTargetIsField = undefined;
                            spFormBuilderService.currentDropTargetQuadrant = undefined;
                            spFormBuilderService.currentParentDropTargetQuadrant = undefined;
                        }

                        dropCleanup();
                    });

                return fieldContainer;
            }

            /////
            // Modifies some properties for layout based on new control and parent types
            /////
            function adjustControlHeight(newControl, dropControl, dropControlIsField, quadrant) {

                /////
                // For rules see wiki article:
                // Revised Screen & Form Drag & Drop Layout
                // http://spwiki.sp.local/pages/viewpage.action?pageId=86310917
                /////

                if (!dropControl) {
                    return;
                }

                var newControlType = newControl ? newControl.getType().getAlias() : '';
                var dropControlType = dropControl ? dropControl.getType().getAlias() : '';
                var parentControlType = scope.parentControl ? scope.parentControl.getType().getAlias() : '';
                var isDroppingOnBuilder = getIsDroppingOnBuilder(dropControlType, parentControlType, quadrant);

                // allow certain controls to inherit the height of controls to the left or right
                if (quadrant === quadrants.left || quadrant === quadrants.right) {

                    switch (newControlType) {
                        case containers.tab:
                        case containers.vertical:
                        case 'chartRenderControl':
                        case 'reportRenderControl':
                        case 'tabRelationshipRenderControl':
                        case 'formRenderControl':

                            newControl.setRenderingVerticalResizeMode(dropControl.renderingVerticalResizeMode);
                            newControl.setRenderingHeight(dropControl.renderingHeight);
                            break;
                    }
                } else {

                    // If the parent is the root form or screen...
                    if (isDroppingOnBuilder) {

                        switch (newControlType) {
                            case containers.tab:
                            case containers.vertical:
                            case 'chartRenderControl':
                            case 'reportRenderControl':
                            case 'tabRelationshipRenderControl':
                                newControl.setRenderingVerticalResizeMode('console:resizeFifty');
                                break;

                            case 'formRenderControl':
                                newControl.setRenderingVerticalResizeMode('console:resizeAutomatic'); // compact
                                break;
                        }
                    }
                }
            }

            /////
            // Returns true if the current control being dragged to is within a horizontal stack.
            /////
            function isParentHorizontal() {
                return scope.parentControl && spFormBuilderService.isHorizontalContainer(scope.parentControl.getType().getAlias());
            }

            /////
            // Returns true if the current control being dragged is within the hidden vertical stack of a tab control.
            /////
            function isParentTab() {
                return scope.parentTab === true;
            }

            /////
            // Determines based on the state of this container, if it can be split vertically.
            /////
            function getCanSplit() {

                var split = true;

                // Need to have a control and parent set
                if (!scope.parentControl || !scope.control) {
                    split = false;

                } else {

                    // Check if the type is splittable
                    var type = scope.control.getType().getAlias();

                    if (type !== containers.vertical) {
                        split = false;
                    }

                    // Must be an explicit container to split
                    if (!scope.explicitContainer) {
                        split = false;
                    }
                }

                return split;
            }

            /////
            // Determines, based on the scope and parameters, if a drop should occur.
            /////
            function getCanDrop(dragData, isField, control, quadrant, callback) {

                var drop = true;
                var because = 'unknown';

                if (dragData && dragData instanceof FieldContainer) {
                    // do not allow dropping a field which is already on the form
                    if (dragData.isField && dragData.isField()) {
                        drop = !spFormBuilderService.isFieldOnFormNoCache(dragData, callback);
                    }

                    // do not allow dropping a lookup or choice field which is already on the form
                    if ((dragData.isLookup && dragData.isLookup()) || (dragData.isChoiceField && dragData.isChoiceField())) {
                        drop = !spFormBuilderService.isRelationshipOnFormNoCache(dragData, callback);
                    }

                    if (!drop) {
                        because = 'cannot drop field, choice or lookup that is already on the form';
                    }
                }

                control = control || spFormBuilderService.currentDropTarget;
                isField = isField || spFormBuilderService.currentDropTargetIsField;
                quadrant = quadrant || spFormBuilderService.currentDropTargetQuadrant;

                var dragType = dragData && dragData.getType ? dragData.getType().getAlias() : '';
                var controlType = control ? control.getType().getAlias() : '';
                var parentControlType = scope.parentControl ? scope.parentControl.getType().getAlias() : '';
                var isExplicit = scope.explicitContainer && scope.explicitContainer !== false;
                var isParentExplicit = getIsParentExplicit();
                var isDraggingTabOrContainer =
                    (dragData && (dragData.name === 'Container' || dragData.name === 'Tabbed Container') || dragData._fieldGroupEntity) ||
                    (dragType === containers.vertical || dragType === containers.horizontal || dragType === containers.tab || dragType === containers.header);

                //if ((controlType === containers.screen || controlType === containers.form) && control.containedControlsOnForm.length > 0) {
                //    /////
                //    // Do not show the root builder drop zones if a control has already been placed
                //    /////
                //    drop = false;
                //}

                if (quadrant === quadrants.center) {
                    /////
                    // If this is a direct drop
                    /////

                    if (isField) {
                        /////
                        // Do not allow direct drop on to a field container
                        /////
                        drop = false;

                        because = 'cannot drop on a field container';
                    }

                    if (isDraggingTabOrContainer && isExplicit && controlType === containers.vertical) {
                        /////
                        // Do not allow a tabbed container or container to be placed within another explicit container
                        /////
                        drop = false;

                        because = 'no tabs or containers can go in an explicit container';
                    }

                } else {
                    /////
                    // If this is an indirect drop on to the current control's parent
                    /////

                    if (scope.tab) {
                        /////
                        // Do not allow any interaction with the hidden stack created for each tab
                        /////
                        drop = false;

                        because = 'this is a hidden stack container';
                    }

                    if (isDraggingTabOrContainer && isParentExplicit &&
                        parentControlType === containers.vertical && !isParentTab()) {
                        /////
                        // Do not allow a tabbed container or container to be placed within a parent that is another explicit container
                        /////
                        drop = false;

                        because = 'cannot indirectly add a container to this explicit parent container';
                    }

                    switch (quadrant) {

                        case quadrants.left:
                        case quadrants.right:
                            if (isParentExplicit && parentControlType === containers.vertical && !isParentTab()) {
                                ////
                                // Do not allow placement at the left or right of an element in an explicit vertical stack (unless tab/screen/form)
                                ////
                                drop = false;

                                because = 'we can only stack vertically here';
                            }
                            break;

                        case quadrants.top:
                        case quadrants.bottom:
                            if (control !== scope.parentControl && isParentHorizontal() && !isParentTab()) {
                                ////
                                // Do not allow placement at the top or bottom of an element in a horizontal stack
                                ////
                                drop = false;

                                because = 'we can only stack horizontally here';
                            }
                            break;
                    }
                }

                var msg = 'form builder can ' + (drop ? '' : 'NOT ') + 'drop a ' +
                    (dragType.length > 0 ? dragType : dragData.name) + ' on to the ' +
                    quadrant + ' quadrant of ' + controlType;

                if (!drop) {
                    msg += (' because ' + because);
                }

                console.log(msg);

                return drop;
            }

            /////
            // Respond to the drop being deferred.
            /////
            scope.$on('deferDrop', function (event, args) {
                if (args.control === scope.control) {

                    spFormBuilderService.currentDropTarget = scope.control;
                    spFormBuilderService.currentDropTargetIsField = args.fieldContainer;
                    spFormBuilderService.currentDropTargetQuadrant = args.quadrant;

                    drop(args.event, args.source, args.target, args.dragData, args.dropData, args.fieldContainer, scope.control);
                }
            });

            /////
            // Determines if the target location of the drop will be the builder.
            /////
            function getIsDroppingOnBuilder(controlType, parentControlType, quadrant) {

                var isBuilder = controlType === containers.screen || controlType === containers.form;

                var isParentBuilder = (parentControlType === containers.screen || parentControlType === containers.form) && (quadrant !== quadrants.center);

                return isBuilder || isParentBuilder;
            }

            /////
            // Checks if the control, expected to be a container, is considered as explicit.
            /////
            function getIsParentExplicit() {

                var isExplicit = false;

                if (scope.parentControl) {

                    var container = spFormBuilderService.isExplicitContainer(scope.parentControl);
                    if (container && container !== false) {

                        isExplicit = true;
                    }
                }

                return isExplicit;
            }

            /////
            // Perform any drop cleanup.
            /////
            function dropCleanup() {

                /////
                // Remove the class from the current element.
                /////
                element.removeClass('sp-form-builder-container-selected');

                spFormBuilderService.destroyInsertIndicator();
                spFormBuilderService.destroyInsertOverlayIndicator();
                spFormBuilderService.clearLastDragValues();
            }

            /////
            // Drag Enter.
            /////
            function dragEnter() {

                if (spFormBuilderService.canDrop) {

                    spFormBuilderService.showInsertIndicator();
                }

                return scope.fieldContainer;
            }

            /////
            // Drag Leave.
            /////
            function dragLeave() {

                //spFormBuilderService.hideInsertIndicator();

                return scope.fieldContainer;
            }

            /////
            // Get the entities path.
            /////
            function getEntityPath(entity) {

                var path = [];

                searchEntity(spFormBuilderService.form, function (container) {
                    if (!container) {
                        return undefined;
                    }

                    return container.containedControlsOnForm;
                }, entity, function (ancestor) {
                    path.unshift(ancestor);
                });

                return path;
            }

            /////
            // Drag Over.
            /////
            function dragOver(event, source, target, dragData, dropData) {

                spFormBuilderService.setLastDragValues(event, source, target, dragData, dropData, scope.fieldContainer, scope.control);

                var quadrant = getMouseQuadrant(event, target);

                if (spFormBuilderService.currentDropTarget === scope.control && spFormBuilderService.currentDropTargetQuadrant === quadrant) {
                    /////
                    // Element and quadrant have not changed...
                    /////
                    return;
                }

                spFormBuilderService.currentDropTarget = scope.control;
                spFormBuilderService.currentDropTargetIsField = scope.fieldContainer;
                spFormBuilderService.currentDropTargetQuadrant = quadrant;
                spFormBuilderService.canDrop = getCanDrop(dragData);

                if (dragData && scope.control) {

                    if (spFormBuilderService.canDrop) {

                        if (dragData !== scope.control && !_.includes(getEntityPath(scope.control), dragData) && !spFormBuilderService.isFieldOnForm(dragData)) {

                            showDropIndicator(event, target);
                        }
                    } else {

                        if (isParentHorizontal()) {

                            showDropIndicatorForHorizontal(event, target);
                        }
                    }
                }

                autoScroll(event.originalEvent.clientY);

                scope.$emit('autoScroll', event.originalEvent.clientY);

                return false;
            }

            /////
            // Automatically scroll when dragging near the container border.
            /////
            function autoScroll(clientY) {
                var y = clientY - element.offset().top;
                var scrollElement;

                if (scope.parentControl) {
                    scrollElement = closestDescendants(element, '.sp-form-builder-container-content').first();
                } else {
                    scrollElement = element;
                }

                if (y <= 15) {
                    scrollElement.scrollTop(scrollElement.scrollTop() - 10);
                } else if (y > element.height() - 15) {
                    scrollElement.scrollTop(scrollElement.scrollTop() + 10);
                }
            }

            /////
            // Auto scroll this container.
            /////
            scope.$on('autoScroll', function (event, clientY) {
                autoScroll(clientY);
            });

            /////
            // Whether this control can be dragged onto the container
            /////
            function allowDrop(source, target, dragData) {

                if (!spFormBuilderService.canDrop) {
                    spFormBuilderService.hideInsertIndicator();
                    spFormBuilderService.destroyInsertOverlayIndicator();
                }

                return spFormBuilderService.canDrop;
            }

            /////
            // Gets the current mouse quadrant.
            /////
            function getMouseQuadrantByBorder(clientX, clientY, target) {
                var clientRect;
                var x;
                var y;
                var height;
                var width;

                if (!target) {
                    console.error('No target defined.');
                    return undefined;
                }

                clientRect = target.getBoundingClientRect();

                x = clientX - clientRect.left;
                y = clientY - clientRect.top;
                height = clientRect.height;
                width = clientRect.width;

                if (x <= 10)
                    return quadrants.left;

                if (x >= width - 10)
                    return quadrants.right;

                if (y <= 10)
                    return quadrants.top;

                if (y >= height - 10)
                    return quadrants.bottom;

                return quadrants.center;
            }

            /////
            // Gets the current mouse quadrant.
            /////
            function getMouseQuadrantByPosition(clientX, clientY, clientRect) {
                var x, y, tl, bl;

                if (inCenter(clientX, clientY, clientRect)) {
                    return quadrants.center;
                }

                /////
                // 50 pixel borders with preference to the left and right
                /////
                if (clientRect.width > 100) {

                    if (clientX < (clientRect.left + 50)) {
                        return quadrants.left;
                    }

                    if (clientX > (clientRect.right - 50)) {
                        return quadrants.right;
                    }
                }

                if (clientRect.height > 100) {

                    if (clientY < (clientRect.top + 50)) {
                        return quadrants.top;
                    }

                    if (clientY > (clientRect.bottom - 50)) {
                        return quadrants.bottom;
                    }
                }

                /////
                // Divide the remainder up on the diagonals
                /////
                x = clientX - clientRect.left;
                y = clientY - clientRect.top;

                tl = 1 - (y / clientRect.height) > x / clientRect.width;
                bl = y / clientRect.height > x / clientRect.width;

                if (tl && bl) {
                    return quadrants.left;
                }

                if (!tl && !bl) {
                    return quadrants.right;
                }

                if (tl && !bl) {
                    return quadrants.top;
                }

                if (!tl && bl) {
                    return quadrants.bottom;
                }

                return undefined;
            }

            function inCenter(clientX, clientY, clientRect) {

                // idea here is to use a border of 10, or 25%, such that
                // the center is at least 50%

                var marginHeight = Math.min(10, clientRect.height / 4);
                var marginWidth = Math.min(10, clientRect.width / 4);
                var centerRect = {
                    left: clientRect.left + marginWidth,
                    top: clientRect.top + marginHeight,
                    right: clientRect.right - marginWidth,
                    bottom: clientRect.bottom - marginHeight
                };

                return clientX > centerRect.left && clientX < centerRect.right &&
                    clientY > centerRect.top && clientY < centerRect.bottom;
            }

            /////
            // Gets the mouse quadrant.
            /////
            function getMouseQuadrant(event, target) {
                var alias, quad;

                if (!event || !target || !scope.control) {
                    return undefined;
                }

                alias = scope.control.getType().getAlias();

                if (alias === containers.form || alias === containers.screen) {

                    quad = quadrants.center;
                } else if (alias === containers.tab) {

                    quad = getMouseQuadrantByBorder(event.originalEvent.clientX, event.originalEvent.clientY, target);
                } else {

                    quad = getMouseQuadrantByPosition(event.originalEvent.clientX, event.originalEvent.clientY, target.getBoundingClientRect());
                }

                return quad;
            }

            /////
            // Show the drop indicator.
            /////
            function showDropIndicator(event, target) {
                var quadrant = spFormBuilderService.currentDropTargetQuadrant;
                var clientRect;
                var parentRect;
                var childClientRect;

                clientRect = target.getBoundingClientRect();
                parentRect = $(".sp-form-builder-edit-form-control")[0].getBoundingClientRect();

                var top = clientRect.top < parentRect.top ? parentRect.top : clientRect.top;
                var bottom = clientRect.bottom > parentRect.bottom ? parentRect.bottom : clientRect.bottom;
                var width = clientRect.right - clientRect.left - 5; // 5px padding
                var height = bottom - top - 5; // 5px padding

                //if (scope.parentControl && scope.parentControl.getType().getAlias() === containers.tab && scope.control && scope.control.getType().getAlias() === containers.vertical) {
                //    // special case: do not allow dropping around the vertical container if its parent is tabContainerControl. Only allow dropping inside the vertical container. override the quadrant
                //    quadrant = quadrants.center;
                //}

                var jQTarget = $(target);

                // if dropping something on tab host control then show 'insertOverlayIndicator' and hide 'insertIndicator'
                if (scope.control.getType().getAlias() === containers.tab && quadrant && quadrant === quadrants.center) {

                    spFormBuilderService.hideInsertIndicator();

                    var tabset = closestDescendants(jQTarget, '.nav-tabs').first();
                    if (tabset && tabset.length > 0) {
                        spFormBuilderService.positionInsertOverlayIndicator(tabset[0], scope);
                    }

                    spFormBuilderService.currentDropTarget = scope.control;
                    spFormBuilderService.currentDropTargetIsField = scope.fieldContainer;
                    spFormBuilderService.currentDropTargetQuadrant = quadrant;
                    return;
                } else {
                    spFormBuilderService.destroyInsertOverlayIndicator();
                }

                switch (quadrant) {
                    case quadrants.top:
                        spFormBuilderService.positionInsertIndicator(clientRect.top - 5, clientRect.left, 4, width, scope);
                        break;
                    case quadrants.bottom:
                        spFormBuilderService.positionInsertIndicator(clientRect.bottom - 4, clientRect.left, 4, width, scope); // 5px padding
                        break;
                    case quadrants.left:
                        spFormBuilderService.positionInsertIndicator(top, clientRect.left - 6, height, 4, scope);
                        break;
                    case quadrants.right:
                        spFormBuilderService.positionInsertIndicator(top, clientRect.right - 3, height, 4, scope); // 5px padding
                        break;
                    case quadrants.center:
                        var alias = scope.control.getType().getAlias();

                        var jTarget = $(target);

                        var descendants = closestDescendants(jTarget, '.child-container') || [];

                        // debug
                        // _.forEach(descendants, function (e) {
                        //     var r = e.getBoundingClientRect();
                        //     if (!r.width && !r.height) {
                        //         console.warn('target control has a zero dimension', e);
                        //     }
                        // });

                        var index;

                        if (alias === containers.vertical || alias === containers.header || alias === containers.form || alias === containers.tab || alias === containers.screen) {

                            if (descendants.length === 0) {

                                if (scope.control.name && !scope.control.hideLabel && alias !== containers.form && alias !== containers.screen) {
                                    spFormBuilderService.positionInsertIndicator(clientRect.top + 45 < top ? top : clientRect.top + 45, clientRect.left, 4, width, scope);
                                } else {
                                    spFormBuilderService.positionInsertIndicator(clientRect.top - 5 < top ? top : clientRect.top - 5, clientRect.left, 4, width, scope);
                                }
                            } else {
                                for (index = 0; index < descendants.length; index++) {
                                    childClientRect = descendants[index].getBoundingClientRect();

                                    if (event.originalEvent.clientY <= childClientRect.top + 10) {
                                        break;
                                    }
                                }

                                if (index === 0) {
                                    childClientRect = descendants[0].getBoundingClientRect();

                                    spFormBuilderService.positionInsertIndicator(childClientRect.top - 5 < top ? top : childClientRect.top - 5, childClientRect.left, 4, descendants[0].clientWidth, scope);
                                } else {
                                    childClientRect = descendants[index - 1].getBoundingClientRect();

                                    spFormBuilderService.positionInsertIndicator(childClientRect.bottom + 1 > bottom ? bottom : childClientRect.bottom + 1, childClientRect.left, 4, descendants[index - 1].clientWidth, scope);
                                }
                            }
                        } else if (alias === containers.horizontal) {

                            if (descendants.length === 0) {
                                spFormBuilderService.positionInsertIndicator(clientRect.top < top ? top : clientRect.top, clientRect.left - 5, target.clientHeight, 4, scope);
                            } else {
                                for (index = 0; index < descendants.length; index++) {
                                    childClientRect = descendants[index].getBoundingClientRect();

                                    if (event.originalEvent.clientX <= childClientRect.left + 10) {
                                        break;
                                    }
                                }

                                if (index === 0) {
                                    childClientRect = descendants[0].getBoundingClientRect();

                                    spFormBuilderService.positionInsertIndicator(clientRect.top < top ? top : clientRect.top, childClientRect.left - 5, descendants[0].clientHeight, 4, scope);
                                } else {
                                    childClientRect = descendants[index - 1].getBoundingClientRect();

                                    spFormBuilderService.positionInsertIndicator(clientRect.top < top ? top : clientRect.top, childClientRect.right + 1, descendants[index - 1].clientHeight, 4, scope);
                                }
                            }
                        }

                        break;
                }
            }

            /////
            // Show the drop indicator for the special case, inline, horizontal stack parent.
            /////
            function showDropIndicatorForHorizontal(event, target) {

                var hr = sp.result(target, 'offsetParent.parentElement.children.1');
                if (hr) {

                    var hrDim = hr.getBoundingClientRect();
                    if (hrDim) {

                        var p = target.parentElement.getBoundingClientRect();
                        var container = {
                            left: hrDim.left,
                            right: hrDim.right,
                            width: hrDim.width,
                            top: p.top,
                            bottom: hrDim.top - 5,
                            height: hrDim.top - p.top - 5
                        };

                        var horizontalQuadrant = getMouseQuadrantByPosition(event.originalEvent.clientX, event.originalEvent.clientY, container);

                        if (horizontalQuadrant === quadrants.top) {

                            spFormBuilderService.positionInsertIndicator(container.top - 5, hrDim.left, 4, hrDim.width, scope);
                            spFormBuilderService.currentParentDropTargetQuadrant = quadrants.top;
                            spFormBuilderService.canDrop = true;
                        } else if (horizontalQuadrant === quadrants.bottom) {

                            spFormBuilderService.positionInsertIndicator(hrDim.top - 4, hrDim.left, 4, hrDim.width, scope);
                            spFormBuilderService.currentParentDropTargetQuadrant = quadrants.bottom;
                            spFormBuilderService.canDrop = true;
                        }
                    }
                }
            }

            /////
            // Drag End handler.
            /////
            function dragEnd() {
                /////
                // Perform cleanup
                /////
                spFormBuilderService.isDragging = false;
                spFormBuilderService.destroyInsertIndicator();
                spFormBuilderService.destroyInsertOverlayIndicator();
            }

            /////
            // Drag Start handler.
            /////
            function dragStart() {

                spFormBuilderService.isDragging = true;

                /////
                // Remove these in drag start rather then drag end so that the drop handler has access to them.
                /////
                spFormBuilderService.currentDropTarget = undefined;
                spFormBuilderService.currentDropTargetIsField = undefined;
                spFormBuilderService.currentDropTargetQuadrant = undefined;
                spFormBuilderService.currentParentDropTargetQuadrant = undefined;
            }

            /////
            // Collapses the container.
            /////
            function collapseContainer(ordinal) {
                var removedControl;

                removedControl = removeAtContainedControlsOnForm(scope.control, ordinal, true);

                promoteChildControls(removedControl, ordinal);
            }

            /////
            // Moves the container controls out into the parent container.
            /////
            function promoteChildControls(container, existingOrdinal) {
                var parentOrdinal;
                var alias;

                parentOrdinal = existingOrdinal;
                alias = scope.control.getType().getAlias();

                if (alias === containers.form || alias === containers.header) {
                    alias = containers.vertical;
                }

                console.log('Collapsing', container.getType().getAlias(), 'container');

                var controlWrappper = new ControlWrapper(scope.control);
                var childControlWrapper = new ControlWrapper(container);

                var sortedChildren = _.sortBy(container.containedControlsOnForm, function (element) {
                    return element.renderingOrdinal;
                });

                _.forEach(sortedChildren, function (removedSubControl) {
                    if (!removedSubControl.name && removedSubControl.getType().getAlias() === alias) {
                        promoteChildControls(removedSubControl, existingOrdinal);
                    } else {
                        if (removedSubControl.renderingHorizontalResizeMode && removedSubControl.renderingHorizontalResizeMode.alias() === 'console:resizeAutomatic') {
                            removedSubControl.renderingWidth = container.renderingWidth;
                        }

                        controlWrappper.insert(removedSubControl, parentOrdinal++);
                        childControlWrapper.remove(removedSubControl);
                    }
                });
            }

            /////
            // Gets the string field resize handles.
            /////
            scope.getStringFieldResizeHandles = function (control) {

                if (control && control.fieldToRender && control.fieldToRender.allowMultiLines) {

                    /////
                    // All handles.
                    /////
                    return 'se';
                } else {

                    /////
                    // Horizontal only.
                    /////
                    return 'e';
                }
            };

            /////
            // Drag options.
            /////
            scope.dragOptions = {
                onDragEnd: function (event, data) {
                    dragEnd(event, data);
                },
                onDragStart: function (event, data) {
                    dragStart(event, data);
                }
            };

            /////
            // Drop options.
            /////
            scope.dropOptions = {
                simpleEventsOnly: true,
                propagateDragEnter: scope.fieldContainer && spFormBuilderService.currentDropTargetQuadrant === quadrants.center,
                propagateDragLeave: scope.fieldContainer && spFormBuilderService.currentDropTargetQuadrant === quadrants.center,
                propagateDrop: scope.fieldContainer && spFormBuilderService.currentDropTargetQuadrant === quadrants.center,
                propagateDragOver: false,
                onAllowDrop: function (source, target, dragData, dropData) {
                    return allowDrop(source, target, dragData, dropData);
                },
                onDrop: function (event, source, target, dragData, dropData) {
                    return drop(event, source, target, dragData, dropData, scope.fieldContainer, scope.control);
                },
                onDragEnter: function (event, source, target, dragData, dropData) {
                    return dragEnter(event, source, target, dragData, dropData);
                },
                onDragLeave: function (event, source, target, dragData, dropData) {
                    return dragLeave(event, source, target, dragData, dropData);
                },
                onDragOver: function (event, source, target, dragData, dropData) {
                    return dragOver(event, source, target, dragData, dropData);
                }
            };

            /////
            // Insert indicator drop options.
            /////
            scope.insertIndicatorDropOptions = {
                onDrop: function () {
                    var lastValues = spFormBuilderService.getLastDragValues();

                    if (lastValues && lastValues.event) {
                        drop(lastValues.event, lastValues.source, lastValues.target, lastValues.dragData, lastValues.dropData, lastValues.fieldContainer, lastValues.control);
                        return true;
                    }

                    return false;
                }
            };

            /////
            // Resize start event.
            /////
            function onResizeStart() {
                spFormBuilderService.isResizing = true;
            }

            /////
            // Resize stop event.
            /////
            function onResizeStop(event, el) {
                var parentContainers;
                var parentContainer;
                var implicitParentContainer;
                var parentWidth;
                var parentHeight;
                var width;
                var height;

                parentContainers = el.element.parents('.sp-form-builder-container');

                if (!parentContainers || !parentContainers.length) {
                    return;
                }

                for (var index = 0; index < parentContainers.length; index++) {

                    parentContainer = $(parentContainers[index]);

                    if (!parentContainer.hasClass('sp-form-builder-implicit') || parentContainer.hasClass('sp-form-builder-edit-form-control')) {
                        break;
                    }

                    parentContainer = undefined;
                }

                if (!parentContainer) {
                    return;
                }

                var parentBody = parentContainer.parent();
                if (parentBody && parentBody.hasClass('sp-Edit-Form-Body')) {
                    parentContainer = parentBody;
                }

                /////
                // Get the parent dimensions
                /////
                parentWidth = parentContainer.width();
                parentHeight = parentContainer.height();

                if (!scope.control.hasRelationship('console:renderingHorizontalResizeMode')) {
                    scope.control.registerLookup('console:renderingHorizontalResizeMode');
                }

                if (!scope.control.hasRelationship('console:renderingVerticalResizeMode')) {
                    scope.control.registerLookup('console:renderingVerticalResizeMode');
                }

                if (!scope.control.hasField('console:renderingHeight')) {
                    scope.control.registerField('console:renderingHeight', spEntity.DataType.Int32);
                }

                if (!scope.control.hasField('console:renderingWidth')) {
                    scope.control.registerField('console:renderingWidth', spEntity.DataType.Int32);
                }

                var apply = false;

                if (el.size.width !== el.originalSize.width) {
                    scope.control.setRenderingHorizontalResizeMode('console:resizeManual');

                    if (el.size.width !== el.originalSize.width) {
                        width = Math.ceil(el.element.outerWidth(true) / parentWidth * 100);

                        if (width > 100) {
                            width = 100;
                        }

                        scope.control.setRenderingWidth(width);
                    }

                    apply = true;
                }

                if (el.size.height !== el.originalSize.height) {
                    scope.control.setRenderingVerticalResizeMode('console:resizeManual');


                    if (el.size.height !== el.originalSize.height) {
                        height = Math.ceil(el.element.outerHeight(true) / parentHeight * 100);

                        if (height > 100) {
                            height = 100;
                        }

                        scope.control.setRenderingHeight(height);
                    }

                    apply = true;
                }

                if (apply) {

                    scope.$apply(performLayout);
                }

                spFormBuilderService.isResizing = false;
            }

            /////
            // Resize options.
            /////
            scope.resizeOptions = {
                disabled: !!scope.disableResize || (!scope.fieldContainer && !scope.explicitContainer),
                handles: scope.resizeHandles,
                onResizeStart: onResizeStart,
                onResizeStop: onResizeStop
            };

            /////
            // Handles the split button click for this container.
            /////
            scope.onSplitClick = function () {

                split();
            };

            /////
            // Splits a container in to two equal parts stacked vertically.
            /////
            function split() {

                if (!scope.splittable) {
                    return;
                }

                var height = scope.control.renderingHeight;
                var halfHeight = height;
                if (height) {

                    halfHeight = Math.ceil(height / 2);

                    scope.control.setRenderingHeight(halfHeight);
                    scope.control.setRenderingVerticalResizeMode('console:resizeManual');
                }

                var json = {
                    typeId: 'console:' + containers.vertical,
                    'console:renderingOrdinal': jsonInt(),
                    'console:renderingWidth': jsonInt(scope.control.renderingWidth),
                    'console:renderingHeight': jsonInt(halfHeight),
                    'console:renderingBackgroundColor': scope.control.renderingBackgroundColor,
                    'console:renderingHorizontalResizeMode': scope.control.renderingHorizontalResizeMode,
                    'console:renderingVerticalResizeMode': scope.control.renderingVerticalResizeMode,
                    'console:hideLabel': jsonBool(false),
                    'console:containedControlsOnForm': []
                };

                var newContainer = spEntity.fromJSON(json);

                // same name?
                newContainer.name = scope.control.name;

                var success = insertAtBottom(newContainer, scope.control, scope.fieldContainer);
                if (!success) {
                    console.log('form builder: failed to split control');
                }

                performLayout();
            }

            scope.getContainedControls = function () {
                if (scope.control && scope.control.containedControlsOnForm) {
                    var filtered = _.filter(scope.control.containedControlsOnForm, function (ctrl) {
                        return ctrl.getDataState() !== spEntity.DataStateEnum.Delete;
                    });

                    var sorted = _.sortBy(filtered, function (ctrl) {
                        return ctrl.renderingOrdinal;
                    });

                    return sorted;
                }

                return [];
            };

            /////
            // Removes the selected control from the collection of parent controls.
            /////
            scope.onCloseClick = function () {

                if (!scope.parentControl) {
                    return;
                }

                if (scope.control.dataState === spEntity.DataStateEnum.Create) {
                    scope.parentControl.containedControlsOnForm.remove(scope.control);
                }

                scope.control.dataState = spEntity.DataStateEnum.Delete;

                if (scope.control.graph && scope.control.graph.history && scope.control.graph.history.addBookmark) {
                    scope.control.graph.history.addBookmark();
                }

                spFormBuilderService.refreshMemoizedFunctions();

                performLayout();
            };

            /////
            // Signals to all the parent containers that a control has been removed.
            /////
            function controlRemoved() {
                var containedControl;
                var alias;
                var parentAlias = null;

                alias = scope.control.getType().getAlias();

                if (alias === containers.horizontal || alias === containers.vertical) {
                    /////
                    // If this is a stack container control.
                    /////
                    if (scope.control.containedControlsOnForm && scope.control.containedControlsOnForm.length === 1) {
                        containedControl = scope.control.containedControlsOnForm[0];

                        if (containedControl) {

                            if (scope.parentControl) {
                                parentAlias = scope.parentControl.getType().getAlias();

                                if (parentAlias === containers.form || parentAlias === containers.header) {
                                    parentAlias = containers.vertical;
                                }
                            }

                            /////
                            // If there is no parent container or the control has a name, it cannot collapse.
                            /////
                            if (!parentAlias || scope.control.name) {
                                return;
                            }

                            scope.$emit('collapse', scope.control);
                        }
                    }
                }
            }

            /////
            // Signals to the parent containers that a collapse is to take place.
            /////
            scope.$on('collapse', function (event, control) {

                if (scope.control.containedControlsOnForm) {

                    var indexFallback;
                    var ordinal;

                    indexFallback = _.findIndex(scope.control.containedControlsOnForm, function (item) {
                        return item === control;
                    });

                    if (indexFallback >= 0) {
                        ordinal = control.renderingOrdinal;

                        if (!ordinal && ordinal !== 0) {
                            ordinal = indexFallback;
                        }

                        collapseContainer(ordinal);

                        event.stopPropagation();
                    }
                }
            });

            /////
            // Watch the number of controls in the collection.
            /////
            scope.$watch('control.containedControlsOnForm.length', function (newVal, oldVal) {

                if (newVal === oldVal) {
                    return;
                }

                if (newVal < oldVal) {
                    controlRemoved();
                }
            });

            /////
            // Allows configuration of fields.
            /////
            scope.onConfigureClick = function () {

                configureClick(scope.control);
            };

            /////
            // Configure selected control.
            /////
            function configureClick(control, callerOptions) {

                if (control.type.getAlias() === 'reportRenderControl' && control.reportToRender) {

                    var navItem = spNavService.getCurrentItem();
                    navItem.data = navItem.data || {};

                    spState.scope.state = spFormBuilderService.getState();

                    var report = control.reportToRender;
                    spNavService.navigateToChildState(
                        'reportBuilder',
                        report.idP);
                } else {
                    var options = {
                        formControl: control,
                        isFieldControl: true,
                        isFormControl: true,
                        relationshipType: controlConfigureDialogFactory.getRelationshipType(control),
                        isReverseRelationship: control.isReversed,
                        tabContainer: scope.tabContainer,
                        definition: spFormBuilderService.definition
                    };

                    _.extend(options, callerOptions);

                    controlConfigureDialogFactory.createDialog(options).then(function (result) {
                        if (result !== false) {
                            control = result;
                            scope.$broadcast('formControlUpdated');
                            spFormBuilderService.definitionRevision++;
                        }
                    });
                }
            }

            /////
            // Get the controls style.
            /////
            scope.getOnceStyle = function () {
                var style = {};

                if (scope.control) {

                    if (scope.control.type && scope.control.type.getAlias && scope.control.type.getAlias() === 'tabRelationshipRenderControl') {

                        style['padding'] = '0';
                        style['box-shadow'] = 'none';
                        style['margin-bottom'] = '0px';
                    }

                    if (scope.parentControl && spFormBuilderService.isHorizontalContainer(scope.parentControl.type.getAlias())) {
                        style['display'] = 'inline-block';
                    }

                    return style;
                }
            };

            /////
            // Get the padding class.
            /////
            scope.getPaddingClass = function () {
                var classes = '';

                if (scope.explicitContainer || scope.fieldContainer) {
                    classes += ' sp-form-builder-padding';
                }

                if (!scope.fieldContainer && !scope.explicitContainer) {
                    classes += ' sp-form-builder-implicit';
                }

                return classes;
            };

            /////
            // Get the field render control.
            /////
            scope.getFieldRenderControl = function (field) {
                var type;
                var template;

                if (field && field.getType) {
                    type = field.getType();

                    if (type && type._alias) {

                        if (type._alias === 'singleLineTextControl') {
                            if (field.fieldToRender && field.fieldToRender.allowMultiLines) {
                                template = 'formBuilder/directives/spFormBuilder/templates/multiLineTextControl.tpl.html';
                            } else {
                                template = 'formBuilder/directives/spFormBuilder/templates/singleLineTextControl.tpl.html';
                            }
                        } else {
                            template = 'formBuilder/directives/spFormBuilder/templates/' + type._alias + '.tpl.html';
                        }
                    }
                }

                // console.log('getFieldRenderControl: ' + (field && field.idP) + ' ' + template);

                return template;
            };

            scope.showHelp = function (formControl) {
                var showHelp = false;

                if (formControl && formControl.showControlHelpText && formControl.description && !isContainerControl(formControl)) {
                    showHelp = true;
                }

                return showHelp;
            };

            function isContainerControl(formControl) {
                var type;

                if (formControl && formControl.getType) {
                    type = formControl.getType();
                    return type && type._alias && type._alias.toLowerCase().includes('container');
                }

                return false;
            }

            /////
            // Gets the css class.
            /////
            scope.getClass = function () {

                var classes = 'sp-form-builder-container';

                if (scope.control) {

                    if (!scope.control.name) {
                        classes += ' sp-form-builder-container-hidden sp-form-builder-non-margined-control';
                    } else {
                        if (element.hasClass('sp-form-builder-edit-form-control')) {
                            classes += ' sp-form-builder-non-margined-control';
                        } else {
                            classes += ' sp-form-builder-margined-control';
                        }
                    }
                }

                if (scope.control && scope.control.type && scope.control.type.getAlias && scope.control.type.getAlias() === containers.tab) {
                    classes += ' sp-form-builder-container-tab';
                }

                return classes;
            };

            /////
            // Container is being renamed.
            /////
            scope.isValidContainerName = function (newName, oldName) {
                if (newName && oldName && newName.toLowerCase() === oldName.toLowerCase()) {
                    return true;
                }

                if (!newName) {
                    spAlertsService.addAlert('Invalid name specified.', {
                        severity: spAlertsService.sev.Warning,
                        expires: true
                    });
                    return false;
                }

                return true;
            };

            /////
            // onLoad function.
            /////
            function onLoad() {

                if (scope.control) {

                    /////
                    // Processes the contained controls on form ensuring they have ordinals.
                    /////
                    var containedControlWrapper = new ControlWrapper(scope.control);

                    ///////
                    //// Watch for changes in the rendering background color.
                    ///////
                    scope.$watch('control.renderingBackgroundColor', function (newVal) {

                        var color = newVal || 'white';
                        if (scope && scope.control) {

                            // Never show the horizontal control
                            if (scope.control.getType().getAlias() === containers.horizontal) {

                                color = 'transparent';
                            }

                            // Nor an implicit vertical stack container
                            if (scope.control.getType().getAlias() === containers.vertical &&
                                spFormBuilderService.isImplicitContainer(scope.control)) {

                                color = 'transparent';
                            }
                        }

                        if (element) {

                            var contentElement = element.find('> .sp-form-builder-padding-target > .sp-form-builder-container-content');
                            if (contentElement && contentElement.length > 0) {

                                contentElement.css('background-color', color);
                            }
                        }
                    });

                    /////
                    // Watch for changes in the rendering resize mode.
                    /////
                    scope.$watch('control.renderingHorizontalResizeMode', function (newVal, oldVal) {
                        if (newVal && oldVal && oldVal.alias() !== newVal.alias()) {
                            performLayout();
                        }
                    });

                    scope.$watch('control.renderingVerticalResizeMode', function (newVal, oldVal) {
                        if (newVal && oldVal && oldVal.alias() !== newVal.alias()) {
                            performLayout();
                        }
                    });

                    /////
                    // Watch for changes in the label visibility.
                    /////
                    scope.$watch('control.hideLabel', function (newVal, oldVal) {

                        if (newVal === oldVal) {
                            return;
                        }

                        scope.showLabel = !newVal;

                        performLayout();
                    });

                    /////
                    // Process the gather request.
                    /////
                    scope.$on('gather', function (event, callback) {
                        var options = {
                            titleClass: '.sp-form-builder-container-label',
                            contentClass: '.sp-form-builder-container-content',
                            inlineElements: {
                                selector: '.sp-form-builder-padding-target',
                                inlineElements: {
                                    selector: '.sp-form-builder-container-content',
                                    inlineElements: {
                                        selector: '.sp-form-builder-container-content-child'
                                    }
                                }
                            }

                        };

                        callback(scope.control, scope.parentControl, element, options);
                    });

                } else {
                    var unregister = scope.$watch('control', function (newVal, oldVal) {
                        if (newVal) {
                            unregister();
                            onLoad();
                        }
                    });
                }
            }

            onLoad();

            /////
            // Perform layout.
            /////
            function performLayout() {
                $timeout(function () {
                    spMeasureArrangeService.performLayout('formBuilder');
                });
            }
        }
    }
}());