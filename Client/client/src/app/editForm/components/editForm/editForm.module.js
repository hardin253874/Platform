// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals Globalize */

var rnEditForm = {};

(function (rnEditForm) {

    _.assign(rnEditForm, {
        isContainer,
        isHorizontalContainer,
        isVerticalContainer,
        flattenControls,
        findContainer,
        getTransformedControls,
        applyTransformToForm,
        createContainer,
        createHorizontalContainer,
        createVerticalContainer,
        getFieldDisplayType,
        getFieldInputType,
        getFieldDisplayString
    });

    /**
     * Return true if the given control is a container control.
     * @param c
     * @returns {boolean}
     */
    function isContainer(c) {
        return _.includes([
            'horizontalStackContainerControl',
            'verticalStackContainerControl',
            'tabContainerControl',
            'screen',
        ], c.firstTypeId().getAlias());
    }

    function isHorizontalContainer(c) {
        return _.includes([
            'horizontalStackContainerControl',
        ], c.firstTypeId().getAlias());
    }

    function isVerticalContainer(c) {
        return _.includes([
            'verticalStackContainerControl',
            'tabContainerControl'
        ], c.firstTypeId().getAlias());
    }

    /**
     * Return an array of all controls in the hierarchical structure,
     * depth first, each level ordered by the renderingOrdinal prop.
     * @param container
     * @returns {Array}
     */
    function flattenControls(container) {
        var orderedChildren = _.sortBy(container.containedControlsOnForm, 'renderingOrdinal');
        return _.reduce(orderedChildren, function (acc, value) {
            return acc.concat(flattenControls(value));
        }, [container]);
    }

    /**
     * Return the container control of the given control, searching into the
     * recursive control structure as needed.
     * @param container
     * @param control
     * @returns {Object}
     */
    function findContainer(container, control) {
        if (!container || !control) {
            return null;
        }
        if (_.indexOf(container.containedControlsOnForm, control) >= 0) {
            return container;
        }
        var result = null;
        _.some(container.containedControlsOnForm, c => {
            result = findContainer(c, control);
            return result;
        });
        return result;
    }

    /**
     * Get an array of controls based on the given container's child controls
     * and with the given transform applied. Each returned control is
     * a map of the actual form control and a state that may be
     * '', 'removed', or 'inserted'.
     * @param container
     * @param transform
     * @returns {Array}
     */
    function getTransformedControls(container, transform) {

        let controls = _(getOrderedContainedControls(container))
            .map(c => ({control: c, state: ''}))
            .value();

        // console.group(container.idP);

        // console.log('before', {
        //      container: container.debugString,
        //      controls: _.map(controls, 'control.debugString'), states: _.map(controls, 'state')
        // });

        _.forEach(transform.cmds, cmd => {
            if (cmd.op === 'remove' && cmd.target === container) {
                const c = _.find(controls, c => c.control === cmd.control);
                c.state = 'removed';
                // console.log({control: c.control.debugString, state: c.state});
                return;
            }
            if (cmd.op === 'insert') {
                if (cmd.target === container && !cmd.mode) {
                    controls.push({control: cmd.control, state: 'inserted'});
                    // console.log({control: cmd.control.debugString, state: 'inserted'});
                } else if (cmd.mode) {
                    let index = _.findIndex(controls, c => c.control === cmd.target && c.state !== 'removed');
                    if (index >= 0) {
                        if (cmd.mode === 'after' && index < controls.length) index += 1;
                        controls.splice(index, 0, {control: cmd.control, state: 'inserted'});
                        // console.log({control: cmd.control.debugString, state: 'inserted'});
                    }
                }
            }
        });

        _.forEach(controls, c => {
            c.dragging = c.control === transform.control;
        });

        // console.log('after', {
        //     container: container.debugString,
        //     controls: _.map(controls, 'control.debugString'),
        //     states: _.map(controls, 'state'),
        //     dragging: _.map(controls, 'dragging')
        // });

        // console.groupEnd(container.idP);

        return controls;
    }

    /**
     * Apply the given transforms to the given form container.
     * Applies all transforms to each container in turn, depth first.
     *
     * Note - maybe should consider applying the transforms in turn
     * each to all containers otherwise we can have a short term case
     * of the same control existing in the 'tree' multiple times -
     * but not sure if that is an issue.
     *
     * @param container
     * @param transform
     * @param visited
     */
    function applyTransformToForm(transform, container, visited = []) {

        _.forEach(transform.cmds, cmd => {
            if (cmd.op === 'remove' && cmd.target === container) {
                container.containedControlsOnForm.remove(cmd.control);
                return;
            }
            if (cmd.op === 'insert') {
                if (cmd.target === container && !cmd.mode) {
                    container.containedControlsOnForm.add(cmd.control);
                    ensureUniqueOrdinals(getOrderedContainedControls(container));
                } else if (cmd.mode) {
                    const controls = getOrderedContainedControls(container);
                    let index = _.findIndex(controls, c => c === cmd.target);
                    if (index >= 0) {
                        if (cmd.mode === 'after' && index < controls.length) index += 1;
                        insertAtIndex(container, cmd.control, index);
                    }
                }
            }
        });

        // note that we've transformed this container and recurse into our children
        visited.push(container);
        _.forEach(container.containedControlsOnForm, c => {
            if (isContainer(c)) {
                applyTransformToForm(transform, c, visited);
            }
        });
    }

    /**
     * Ensure all controls in the given ordered sequence have unique rendering ordinals.
     * Only adjust the ordinal if necessary and so it will leave gaps if already there.
     * @param controls
     */
    function ensureUniqueOrdinals(controls) {
        // Don't care about return of reduce as we are mutating the controls as we go.
        // The reduce function is called for each pair of controls.
        _.reduce(controls, function (prev, control) {
            prev.renderingOrdinal = prev.renderingOrdinal || 0;
            control.renderingOrdinal = Math.max(prev.renderingOrdinal + 1, control.renderingOrdinal || 0);
            return control; // will be the prev arg in the next iteration
        });
    }

    /**
     * Insert a control into the containedControlsOnForm collection.
     * Only change rendering ordinals on the collection where needed.
     * @param container
     * @param control
     * @param index
     */
    function insertAtIndex(container, control, index) {

        ensureControlInterface(control);
        control.renderingOrdinal = 0;

        // 1. Get the existing controls ordered
        const controls = getOrderedContainedControls(container);

        // 2. Insert the new control according to index
        controls.splice(index, 0, control);

        // 3. Fix up rendering ordinals as needed
        ensureUniqueOrdinals(controls);

        // add it to the entity relationship collection
        container.containedControlsOnForm.add(control);
    }

    function getOrderedContainedControls(container) {
        return _.sortBy(container.containedControlsOnForm, getRenderingOrdinal);
    }

    function getRenderingOrdinal(control) {
        ensureControlInterface(control);
        return control.renderingOrdinal || 0;
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

    function createContainer(alias) {
        return spEntity.fromJSON({
            typeId: 'console:' + alias,
            'console:renderingOrdinal': jsonInt(),
            'console:renderingWidth': jsonInt(),
            'console:renderingHeight': jsonInt(),
            'console:renderingBackgroundColor': 'white',
            'console:renderingHorizontalResizeMode': jsonLookup('console:resizeSpring'),
            'console:renderingVerticalResizeMode': jsonLookup('console:resizeSpring'),
            'console:hideLabel': jsonBool(true),
            'console:containedControlsOnForm': []
        });
    }

    function createVerticalContainer() {
        return createContainer('verticalStackContainerControl');
    }

    function createHorizontalContainer() {
        return createContainer('horizontalStackContainerControl');
    }

    /**
     * Return how to render the given field.
     * @param {object} field
     * @returns {string}
     */
    function getFieldDisplayType(field) {
        const represents = field.getLookup('core:fieldRepresents');
        switch (represents && represents.alias()) {
            case 'core:fieldRepresentsEmail':
                return 'email';
            case 'core:fieldRepresentsUrl':
                return 'url';
            case 'core:fieldRepresentsPhoneNumber':
                return 'phoneNumber';
            case 'core:fieldRepresentsColor':
                return 'color';
            case 'core:fieldRepresentsPassword':
                return 'password';
            default:
                return 'string';
        }
    }

    /**
     * Return the type of the INPUT element for the given field.
     * @param {object} field
     * @returns {string}
     */
    function getFieldInputType(field) {
        const represents = field.getLookup('core:fieldRepresents');
        switch (represents && represents.alias()) {
            case 'core:fieldRepresentsPassword':
                return 'password';
            default:
                return 'text';
        }
    }

    function getFieldDisplayString(fieldDisplayType, value) {
        var displayString = '';
        switch (fieldDisplayType) {
            //TODO: Remove
            case 'dateTime':
                if (value && _.isDate(value)) {
                    displayString = Globalize.format(value, 'd') + ' ' + Globalize.format(value, 't');
                }
                break;
            //TODO: Remove
            case 'time':
                if (value && _.isDate(value)) {
                    var tempDate = new Date(1973, 0, 1, value.getUTCHours(), value.getUTCMinutes(), 0, 0);
                    displayString = Globalize.format(tempDate, 't');
                }
                break;
            case 'url':
            case 'email':
            case 'color':
            case 'string':
                displayString = value || '';
                break;
        }
        return displayString;
    }

})(rnEditForm);

