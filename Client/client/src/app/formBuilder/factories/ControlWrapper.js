// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, spEntity */

(function () {
    'use strict';

    /**
     * Module implementing the form builder control wrapper.
     *
     * @module ControlWrapper
     */

    angular.module('mod.app.formBuilder.factories.ControlWrapper', [])
        .factory('ControlWrapper', controlWrapper);

    function controlWrapper() {
        'ngInject';

        /////
        // The ControlWrapper handles modifications to the controls
        // containedControlsOnForm collection while maintaining the correct rendering
        // ordinals.
        /////
        function ControlWrapper(control) {

            if (!control) {
                throw 'invalid control specified';
            }

            this.value = control;

            ensureContainerControlInterface(control);
            ensureUniqueOrdinals(getOrderedContainedControls(control));
        }

        /////
        // ControlWrapper prototype.
        /////
        ControlWrapper.prototype = {
            add: function (control) {
                /////
                // Add a control to the containedControlsOfForm collection.
                /////
                if (!control) {
                    return;
                }

                this.addRange([control]);
            },
            addRange: function (controls) {
                /////
                // Add a range of controls to the containedControlsOfForm collection.
                /////
                if (!controls) {
                    return;
                }

                if (!_.isArray(controls)) {
                    throw 'invalid controls';
                }

                if (!this.value) {
                    throw 'invalid control';
                }

                var ordinals = _.map(this.value.containedControlsOnForm, getRenderingOrdinal);
                var nextOrdinal = _.isEmpty(ordinals) ? 0 : Math.max.apply(null, ordinals) + 1;

                _.forEach(controls, function (control) {
                    ensureControlInterface(control);
                    control.renderingOrdinal = nextOrdinal;
                });
                ensureUniqueOrdinals(controls);

                this.value.containedControlsOnForm.add(controls);
            },
            remove: function (control) {
                /////
                // Remove a control from the containedControlsOfForm collection.
                /////
                if (!control) {
                    return;
                }

                if (!this.value) {
                    throw 'invalid control';
                }

                this.value.containedControlsOnForm.remove(control);

                return [control];
            },
            removeAll: function () {
                /////
                // Remove all controls from the containedControlsOfForm collection.
                /////
                if (!this.value) {
                    throw 'invalid control';
                }

                var existingControls = this.value.containedControlsOnForm.slice(0);

                this.value.containedControlsOnForm.clear();

                return existingControls;
            },
            find: function (ordinal) {
                /////
                // Locate a control in the containedControlsOfForm collection with the specified ordinal.
                /////
                if (!_.isNumber(ordinal)) {
                    return;
                }

                if (!this.value) {
                    throw 'invalid control';
                }

                return _.find(this.value.containedControlsOnForm, function (control) {
                    return control.renderingOrdinal === ordinal;
                });
            },
            insert: function (control, ordinal) {
                /////
                // Insert a control into the containedControlsOnForm collection.
                // If an existing control has the same ordinal then we insert in front of it.
                /////
                if (!control) {
                    return;
                }

                if (!_.isNumber(ordinal)) {
                    return;
                }

                if (!this.value) {
                    throw 'invalid control';
                }

                ensureControlInterface(control);
                control.renderingOrdinal = ordinal;

                // put the new control in front, then sort by ordinal (will maintain order
                // of equals) and then ensure unique which will rewrite ordinals as needed
                var controls = [control].concat(this.value.containedControlsOnForm);
                ensureUniqueOrdinals(_.sortBy(controls, getRenderingOrdinal));

                // add it to the entity relationship collection
                this.value.containedControlsOnForm.add(control);
            },
            insertAtIndex: function (control, index) {
                /////
                // Insert a control into the containedControlsOnForm collection.
                // Only change rendering ordinals on the collection where needed.
                /////
                if (!control) return;
                if (!this.value) throw 'invalid control';

                ensureControlInterface(control);
                control.renderingOrdinal = 0;

                // 1. Get the existing controls ordered
                var controls = getOrderedContainedControls(this.value);

                // 2. Insert the new control according to index
                controls.splice(index, 0, control);

                // 3. Fix up rendering ordinals as needed
                ensureUniqueOrdinals(controls);

                // add it to the entity relationship collection
                this.value.containedControlsOnForm.add(control);
            }
        };

        return (ControlWrapper);
    }

    function getRenderingOrdinal(control) {
        ensureControlInterface(control);
        return control.renderingOrdinal || 0;
    }

    function getOrderedContainedControls(container) {
        return _.sortBy(container.containedControlsOnForm, getRenderingOrdinal);
    }

    function ensureContainerControlInterface(control) {
        if (!control.containedControlsOnForm) {
            // note - don't want to have to do this check all over the place.. let's see if happening
            console.warn('form container control being inserted has not been initialised correctly: ' + control.debugString);
            control.registerRelationship('console:containedControlsOnForm');
        }

        _.forEach(control.containedControlsOnForm, ensureControlInterface);
    }

    function ensureControlInterface(control) {
        if (!control.hasOwnProperty('renderingOrdinal')) {
            // note - don't want to have to do this check all over the place.. let's see if happening
            console.warn('form control being inserted has not been initialised correctly: ' + control.debugString);
            control.registerField('console:renderingOrdinal', spEntity.DataType.Int32);
        }
    }

    function ensureUniqueOrdinals(controls) {
        // Must give controls in the order you want to be maintained as we update ordinals.

        // Don't care about return of reduce as we are mutating the controls as we go.
        // The reduce function is called for each pair of controls.
        _.reduce(controls, function (prev, control) {
            prev.renderingOrdinal = prev.renderingOrdinal || 0;
            control.renderingOrdinal = Math.max(prev.renderingOrdinal + 1, (control.renderingOrdinal || 0));
            return control; // will be the prev arg in the next iteration
        });
    }

})();

