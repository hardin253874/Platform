// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function() {
    "use strict";

    angular.module("mod.app.spFormControlVisibilityService",
    [
        "mod.app.editFormServices",
        "sp.common.spCalcEngineService",
        "mod.featureSwitch"
    ]);

    angular.module("mod.app.spFormControlVisibilityService")
        .factory("spFormControlVisibilityService", spFormControlVisibilityService);

    function spFormControlVisibilityService(spEditForm, spCalcEngineService, rnFeatureSwitch, $animate, $q, $log) {
        "ngInject";

        return {
            doesFormHaveControlsWithVisibilityCalculations,
            evaluateVisibilityCalculations,            
            getControlsWithVisibilityCalculations,
            getVisibleControls,
            isShowHideFeatureOn,
            registerControlVisibilityHandler,
            registerFormDataChangeListener,
            showHideElement,
            updateControlsVisibility
        };

        //-------------------------------- Public Methods --------------------------------

        /**
         * Returns true if the form has controls with visibility calculations false otherwise
         * @param {object} formControl The form control entity
         * @returns {boolean} True if the form has visibility calculations false otherwise
         */
        function doesFormHaveControlsWithVisibilityCalculations(formControl) {
            if (!formControl || !isShowHideFeatureOn()) {
                return false;
            }

            return _.some(spEditForm.getFormControls(formControl), "visibilityCalculation");
        }

        /**
         * Evaluates the specified calculations against the specified form data.
         * Calls a web service to perform the evaluation.
         * @param {object} formData - The form data entity
         * @param {object} calculationsToUpdate - Dictionary of control ids to calculations
         * @returns {promise} - Promise of calculation results.
         */
        function evaluateVisibilityCalculations(formData, calculationsToUpdate) {
            if (!formData || !calculationsToUpdate || !isShowHideFeatureOn()) {
                return $q.when();
            }

            const calcExpressions = {};

            // Convert calc paramater to web service params.
            _.forOwn(calculationsToUpdate, function(calculation, controlId) {
                calcExpressions[controlId] = {
                    expr: calculation,
                    expectedResultType: {
                        dataType: spEntity.DataType.Bool
                    }
                };
            });

            // Call web service to perform evaluation
            return spCalcEngineService.evaluateExpressions(formData, calcExpressions).then(function(result) {
                if (!result || !result.data || !result.data.results) {
                    return [];
                }

                const controlVisibility = {
                
                };

                _.forOwn(result.data.results, function(showControlResult, controlId) {
                    controlId = _.toInteger(controlId);

                    if (!showControlResult ||
                        showControlResult.resultType !== spEntity.DataType.Bool ||
                        spUtils.isNullOrUndefined(showControlResult.value)) {
                        return true;
                    }

                    if (showControlResult.error) {
                        $log.error(`Failed to evaluate calculation for control ${controlId} Error ${showControlResult.error}`);
                        return true;
                    }

                    const isControlVisible = spUtils.stringToBoolean(showControlResult.value);
                    controlVisibility[controlId] = isControlVisible;

                    return true;
                });

                return controlVisibility;
            });
        }

        /**
         * Gets the controls with visibility calculations
         * @param {object} formControl The form control entity
         * @returns {Array<object>} An array of form controls with visibility calculations
         */
        function getControlsWithVisibilityCalculations(formControl) {
            if (!formControl || !isShowHideFeatureOn()) {
                return [];
            }            

            return _.filter(spEditForm.getFormControls(formControl), "visibilityCalculation");
        }        

        /**
         * Gets the visible controls
         * @param {Array<object>} formControls - List of form controls
         * @param {object} controlVisibility - Map of form control id to visibility
         * @returns {Array<object> visible controls} 
         */
        function getVisibleControls(formControls, controlVisibility) {
            if (!formControls || !controlVisibility || !isShowHideFeatureOn()) {
                return formControls;
            }

            const controlsToParentControlMap = getControlsToParentControlMapping(formControls);                         

            // Filter out hidden controls
            const visibleControls = _.filter(formControls, c => {                
                return isControlAndParentsVisible(c, controlsToParentControlMap, controlVisibility);                
            });

            return visibleControls;
        }


        /**
         * Returns true if the show hide feature is on, false otherwise.
         * @returns {bool} - True if the show hide feature is on, false otherwise
         */
        function isShowHideFeatureOn() {
            return rnFeatureSwitch.isFeatureOn("fsShowHideControls");
        }

        /**
         * Registers a handler on a control which will be called when its visibility
         * needs to be updated
         * @param {object} scope The control's scope
         * @param {number} formControlId The form control id
         * @param {function} controlVisibilityHandler Callback function which will be called when the visibility is updated.
         * Callback has two parameters, formControlId (number) and isControlVisible (boolean).         
         */
        function registerControlVisibilityHandler(scope, formControlId, controlVisibilityHandler) {
            if (!scope || !formControlId || !controlVisibilityHandler || !isShowHideFeatureOn()) {
                return;
            }

            function updateControlVisibilityHandler(event, data) {
                if (!formControlId || !data || !data.controlsVisibility) {
                    return;
                }

                let isControlVisible = data.controlsVisibility[formControlId];

                if (spUtils.isNullOrUndefined(isControlVisible)) {
                    isControlVisible = true;
                }

                controlVisibilityHandler(formControlId, isControlVisible);
            }

            // ReSharper disable once AssignedValueIsNeverUsed
            const deregister = scope.$on("updateControlVisibility", updateControlVisibilityHandler);

            // Deregister the handler when the scope is detroyed
            scope.$on("$destroy", () => deregister());
        }


        /**
         * Register a callback which will be called when the form data changes so that the visibility 
         * of controls can be evaluated.
         * @param {object} formControl The form control entity.
         * @param {object} formData The form data entity.
         * @param {object} visibilityCalcDependencies Object containing per control visibility calculation dependencies
         * @param {function} onUpdateControlVisibility Callback function which will get called with the dictionary of controls and calculations to refresh.
         */
        function registerFormDataChangeListener(formControl, formData, visibilityCalcDependencies, onUpdateControlVisibility) {
            if (!formControl || !formData || !visibilityCalcDependencies || !onUpdateControlVisibility || !isShowHideFeatureOn()) {
                return;
            }

            // Reverse the object so that it is keyed by fields and relationships instead of controls            
            const membersToDependentControls = getMembersToDependentControls(visibilityCalcDependencies);

            // ReSharper disable once AssignedValueIsNeverUsed
            // Setup some state
            const state = getState(formControl, formData, membersToDependentControls);

            // Debounce the changes
            const formDataChangedDebounced = _.debounce(() => onFormDataChanged(state, formData, onUpdateControlVisibility), 200);

            // Register the listener
            formData.graph.history.addChangeListener(formDataChangedDebounced);
        }

        /**
        * Shows or hides the specified element.
        * @param {object} element - Html element to show or hide
        * @param {bool} isVisible - True to show, false to hide         
        */
        function showHideElement(element, isVisible) {
            if (!element || !isShowHideFeatureOn()) {
                return;
            }

            $animate[isVisible ? "removeClass" : "addClass"](element, "hideFormControl", {
                tempClasses: "hideFormControlAnimate"
            });
        }

        /**
         * Notifies controls to update their visibility
         * @param {object} scope The edit form scope
         * @param {object} controlsVisibility A map of form control ids to bool. True is visible, false hidden.         
         */
        function updateControlsVisibility(scope, controlsVisibility) {
            if (!scope || !isShowHideFeatureOn()) {
                return;
            }

            scope.$broadcast("updateControlVisibility", { controlsVisibility: controlsVisibility });
        }


        //-------------------------------- Private Methods --------------------------------

        /**
         * Reverses a dictionary of control id to calculation field and relationship dependencies
         * to a dictionary that is keyed by the fields and relationships.
         * @param {object} visibilityCalcDependencies - Dictionary of dependencies keyed by control id
         * @returns {object} Dictionary of control ids keyed by fields and relationships
         */
        function getMembersToDependentControls(visibilityCalcDependencies) {
            const membersToDependentControls = {
                fields: {},
                relationships: {}
            };

            if (!visibilityCalcDependencies || _.isEmpty(visibilityCalcDependencies)) {
                return membersToDependentControls;
            }

            // The input is a dictionary of control ids to dependent fields and relationships
            // Swap this around so that we get a dictionary of fields to controls that need to be evaluated

            _.forOwn(visibilityCalcDependencies, function(dependencies, controlId) {
                controlId = _.toInteger(controlId);

                if (dependencies.fields) {
                    _.forEach(dependencies.fields, function(fieldId) {
                        let controlDependencies = membersToDependentControls.fields[fieldId];

                        if (!controlDependencies) {
                            controlDependencies = [];
                            membersToDependentControls.fields[fieldId] = controlDependencies;
                        }

                        if (!_.includes(controlDependencies, controlId)) {
                            controlDependencies.push(controlId);
                        }
                    });
                }

                if (dependencies.relationships) {
                    _.forEach(dependencies.relationships, function(relationshipId) {
                        let controlDependencies = membersToDependentControls.relationships[relationshipId];

                        if (!controlDependencies) {
                            controlDependencies = [];
                            membersToDependentControls.relationships[relationshipId] = controlDependencies;
                        }

                        if (!_.includes(controlDependencies, controlId)) {
                            controlDependencies.push(controlId);
                        }
                    });
                }
            });

            return membersToDependentControls;
        }

        /**
         * The form data changed event handler.
         * @param {object} state Session state
         * @param {object} formData The form data
         * @param {function} onUpdateControlVisibility Callback function which will get called with the dictionary of controls and calculations to refresh.    
         */
        function onFormDataChanged(state, formData, onUpdateControlVisibility) {
            let controlIdsToUpdate = [];

            // Enumerate fields to be monitored and check if any have changed
            // Store the control ids that need updating.
            _.forOwn(state.fields, function(state, fieldId) {
                const newFieldValue = formData.getField(fieldId);
                if (newFieldValue !== state.value) {
                    state.value = newFieldValue;
                    controlIdsToUpdate = _.union(controlIdsToUpdate, state.dependentControlIds);
                }
            });

            // Enumerate relationships to be monitored and check if any have changed
            // Store the control ids that need updating.
            _.forOwn(state.relationships, function (state, relationshipId) {
                const container = formData.getRelationshipContainer(relationshipId);
                const newChangeId = sp.result(container, "changeId") || -1;
                if (newChangeId >= 0 &&
                    newChangeId !== state.changeId) {
                    state.changeId = newChangeId;
                    controlIdsToUpdate = _.union(controlIdsToUpdate, state.dependentControlIds);
                }
            });

            if (onUpdateControlVisibility && controlIdsToUpdate.length) {
                const calculationsToUpdate = {};

                _.forEach(controlIdsToUpdate, function(controlId) {
                    const control = state.formControls[controlId];
                    if (control && control.visibilityCalculation) {
                        calculationsToUpdate[controlId] = control.visibilityCalculation;
                    }
                });

                // Dependent fields or relationship values have changed.
                // Controls need to evaluate their visibility calculation
                onUpdateControlVisibility(calculationsToUpdate);
            }
        }

        /**
         * Returns an object containing session state
         * @param {object} formControl The form control
         * @param {object} formData The form data
         * @param {object} membersToDependentControls The members to dependent controls dictionary
         * @returns {object} Session state
         */
        function getState(formControl, formData, membersToDependentControls) {
            // Get all the form controls with visibility calculations
            const formControls = _.keyBy(_.filter(spEditForm.getFormControls(formControl), "visibilityCalculation"), c => c.id());

            const formState = {
                fields: {},
                relationships: {},
                formControls: formControls
            };

            // Save the current values for fields that should be monitored, along with the dependent controls
            _.forEach(_.keys(membersToDependentControls.fields), function(fieldId) {
                formState.fields[fieldId] = {
                    value: formData.getField(fieldId),
                    dependentControlIds: membersToDependentControls.fields[fieldId]
                };
            });

            // Save changeId for relationships that should be monitored, along with the dependent controls
            _.forEach(_.keys(membersToDependentControls.relationships), function(relationshipId) {
                const container = formData.getRelationshipContainer(relationshipId);
                const changeId = sp.result(container, "changeId") || 0;
                formState.relationships[relationshipId] = {
                    changeId: changeId,
                    dependentControlIds: membersToDependentControls.relationships[relationshipId]
                };
            });

            return formState;
        }

        /**
         * Gets a mapping of form controls to parent controls
         * @param {Array<object>} formControls 
         * @returns {object} Map of form control ids to parent controls
         */
        function getControlsToParentControlMapping(formControls) {
            const controlsToParentControlMap = {};
            const processedControls = {};
            let controlsToProcess = [];

            if (!formControls) {
                return controlsToParentControlMap;
            }

            // Get initial list of controls to process
            controlsToProcess = _.concat(controlsToProcess, formControls);

            while (controlsToProcess.length) {
                const control = _.head(_.pullAt(controlsToProcess, 0));

                if (processedControls[control.id()]) {
                    continue;
                }

                processedControls[control.id()] = true;

                const childControls = control.containedControlsOnForm;                

                if (childControls && childControls.length) {
                    for (let i = 0; i < childControls.length; i++) {
                        const child = childControls[i];
                        controlsToParentControlMap[child.id()] = control;                    
                    }
                    controlsToProcess = _.concat(controlsToProcess, childControls);       
                }                
            }

            return controlsToParentControlMap;
        }

        /**
         * Returns true if the control is visible, false otherwise
         * @param {} formControl 
         * @param {} controlVisibility 
         * @returns {} 
         */
        function isControlVisible(formControl, controlVisibility) {
            if (!formControl) {
                return false;
            }            

            let isControlVisible = controlVisibility[formControl.id()];
            if (spUtils.isNullOrUndefined(isControlVisible)) {
                isControlVisible = true;
            }

            return isControlVisible;
        }

        /**
         * Returns true if the control and all it's parents are visible, false otherwise
         * @param {} formControl 
         * @param {} controlsToParentControlMap 
         * @param {} controlVisibility 
         * @returns {} 
         */
        function isControlAndParentsVisible(formControl, controlsToParentControlMap, controlVisibility) {
            if (!formControl || !controlsToParentControlMap || !controlVisibility) {
                return true;
            }

            if (!isControlVisible(formControl, controlVisibility)) {
                // The control itself is not visible
                return false;
            }

            // The control is visible, check that all the parents are visible
            let parentControl = controlsToParentControlMap[formControl.id()];

            while (parentControl) {
                if (!isControlVisible(parentControl, controlVisibility)) {
                    // The parent is not visible
                    return false;
                }

                // Get the next parent up the chain
                parentControl = controlsToParentControlMap[parentControl.id()];
            }

            return true;
        }
    }
}());