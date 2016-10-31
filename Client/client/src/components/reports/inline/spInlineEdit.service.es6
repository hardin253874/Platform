// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

// @flow

// todo - fixup this lastSaveResult handling... poorly named, no error messages

(function () {
    'use strict';

    angular.module('mod.common.ui.spReport')
        .factory('spInlineEditService', spInlineEditService);

    /**
     * Manages an editing session.
     * Multiple resources may be edited in a session.
     * Each resource's editing is performed based on a given form.
     */
    function spInlineEditService(spEditForm, $q, rnFeatureSwitch) {
        "ngInject";

        const inlineEditingAllTypes = rnFeatureSwitch.isFeatureOn('inlineEditingAllTypes');

        const editableControlTypes = {
            'singleLineTextControl': true,
            'choiceRelationshipRenderControl': true,
            'multiChoiceRelationshipRenderControl': true,
            'checkboxKFieldRenderControl': true,
            'currencyKFieldRenderControl': true,
            'decimalKFieldRenderControl': true,
            'numericKFieldRenderControl': true,
            'inlineRelationshipRenderControl': true,
            'dropDownRelationshipRenderControl': true,
            'dateKFieldRenderControl': true,
            'timeKFieldRenderControl': true,
            'dateAndTimeKFieldRenderControl': true,
            'autoNumberFieldRenderControl': false,
            'tabRelationshipRenderControl': false

            // any others are governed by inlineEditingAllTypes
        };

        const nonEditableColTypes = {
            'StructureLevels' : true,
            'Image' : true
        };

        let activeSessions = [];

        return {
            startSession,
            endSession,
            getActiveSessions,
            getEditingState,
            anyChanged,
            isChanged,
            isInError,
            isSaved,
            getFormControl,
            saveSession,
            saveResource,
            startEditing
        };

        /**
         * Start a new editing session.
         * Call endSession when done with it as this service tracks sessions
         * and some functionality in the client is blocked while there are active sessions.
         * @returns {Object} an empty session
         */
        function startSession() {
            const session = {};
            activeSessions.push(session);
            return session;
        }

        /**
         * End a session.
         * @param {Object} session to remove from set of registered sessions
         */
        function endSession(session) {
            if (!session) return;
            activeSessions = _.reject(activeSessions, s => s === session);
        }

        /**
         * Return the list of known sessions. Always return an array
         * even if empty, never null or undefined.
         * @returns {Array}
         */
        function getActiveSessions() {
            return _.reject(activeSessions, _.isEmpty);
        }

        /**
         * Saves the edited resources in the given session.
         * Each resource is saved independently and the result is noted for each.
         */
        function saveSession(session) {

            _.forEach(session, state => _resetSaveStatus(state));

            const changedResources = _.filter(session, state => {
                if (!state.resourceEntity) return false;
                return _changedSinceBookmark(state.resourceEntity, state.resourceBookmark);
            });

            return $q.all(_.map(changedResources, saveResource))
                .then(() => {
                    return session; // will have been mutated to include save results
                }, e => {
                    console.error('Error saving edited records', e);
                });
        }

        /**
         * Given editing state and a resource id to edit, return a promise
         * for the new state and editing entry for the given resource.
         * The entry includes the form and the resource entity itself.
         */
        function startEditing(session, eid, formId, attrIds) {
            console.assert(session && eid && attrIds);

            const key = eid.toString();
            let state = session[key];

            if (!state) {

                session[key] = state = {};

                // Kick of some requests, but don't wait.
                // Stick promises in the state in case someone wants to wait on the
                // form or the resource.
                state.formPromise = _requestForm(state, formId, eid);
                // We wait on the form before requesting the resource.
                state.resourcePromise = state.formPromise
                    .then(state => _requestResource(state, eid, attrIds));
            }

            return state;
        }

        /**
         * Get the editing state for the given resource id.
         * Return falsy if none.
         */
        function getEditingState(session, eid) {
            return session && session[eid.toString()];
        }

        /**
         * Has the resource with the given eid any unsaved changes.
         * Returns true if the resource exists in the session and has changes
         * otherwise it returns false.
         * (Probably should throw if the resource doesn't exist in the session...)
         */
        function isChanged(session, eid) {
            return _isResourceChanged(getEditingState(session, eid));
        }

        /**
         * Return whether there is an error status on this resource
         * but only if there are no changes since the last validation or save.
         */
        function isInError(session, eid) {
            const state = getEditingState(session, eid);
            //todo - better handling for save errors
            return state && state.lastSaveResult &&
                state.lastSaveResult !== 'ok' &&
                !_isResourceChangedSinceError(state);
        }

        /**
         * Return whether there is an ok status on this resource
         * but only if there are no changes since the last validation or save.
         */
        function isSaved(session, eid) {
            const state = getEditingState(session, eid);
            //todo - better handling for save errors
            return state && state.lastSaveResult &&
                state.lastSaveResult === 'ok' &&
                !_isResourceChangedSinceError(state);
        }

        /**
         * Return true if any resource in the session has changed.
         */
        function anyChanged(session) {
            return _.some(session || [], _isResourceChanged);
        }

        /**
         * Save the resource handled in the given state object.
         * Note the result in the state.
         * Return a promise for the updated state.
         * Assumes the state has valid form and resource entities.
         */
        function saveResource(state) {
            _resetSaveStatus(state);

            if (!_validateResource(state)) {
                return $q.when(_setSaveStatus(state, 'validation failed'));
            }

            return spEditForm.saveFormData(state.resourceEntity)
                .then(() => {
                    // The edit form service saves the resource and then 'resets' it
                    // to set the id if needed and clear change markers. So reset our bm.
                    return _.assign(state, {lastSaveResult: 'ok', resourceBookmark: _takeBookmark(state.resourceEntity)});
                })
                .catch(e => {
                    //todo - better error message
                    return _setSaveStatus(state, 'save failed');
                });
        }

        /**
         * Perform form validation on the edited resource and note if there are any issues
         * in the state object.
         * @param {Object} state
         * @returns {boolean}
         */
        function validateResource(state) {
            _resetSaveStatus(state);

            if (!_validateResource(state)) {
                _setSaveStatus(state, 'validation failed');
                return false;
            }

            return true;
        }

        /**
         * Get the form control for the given field or relationship
         * based on the form defined in the given editing state.
         */
        function getFormControl(state, colMeta) {
            const controlEntity = state.formEntity && _findControl(state.formEntity, colMeta);
            return _isEditableControlType(controlEntity) ? controlEntity : null;
        }

        ///////////////////////////////////////////////////////////////////////
        // Internal functions

        function _isResourceChanged(state) {
            if (!state || !state.resourceEntity) {
                return false;
            }
            return _changedSinceBookmark(state.resourceEntity, state.resourceBookmark);
        }

        function _isResourceChangedSinceError(state) {
            if (!state || !state.resourceEntity || !state.errorBookmark) {
                return false;
            }
            return _changedSinceBookmark(state.resourceEntity, state.errorBookmark);
        }

        function _requestForm(state, formId, eid) {
            return (formId && spEditForm.getFormDefinition(formId, false, true) || spEditForm.getFormForInstance(eid, false, true))
                .then(formEntity => {
                    return _.assign(state, {eid, formEntity});
                });
        }

        function _requestResource(state, eid, attrIds) {
            if (!state.formEntity) return state;

            // todo - don't include fields and rels that we don't care about here
            const requestStrings = spEditForm.buildRequestStrings([], state.formEntity);

            return spEditForm.getFormData(eid, requestStrings)
                .then(resourceEntity => {
                    _resetSaveStatus(state);
                    spEditForm.markAutoCardinalityOfAllRelationships(state.formEntity, resourceEntity);
                    return _.assign(state, {eid, resourceEntity, resourceBookmark: _takeBookmark(resourceEntity)});
                });
        }

        function _findControl(formControl, {fid, rid, rev, type}) {
            // we don't do certain report column types
            if (nonEditableColTypes[type]) return null;

            const controls = spEditForm.getFormControls(formControl);

            // note - we only look at the fid if there isn't a rid as when we have a rid the
            // fid is for the related entity, not the record's entity
            return _.first(_.filter(controls, c => {
                return !rid && _controlIsForField(fid, c) || _controlIsForRelationship(rid, rev, c);
            }));
        }

        function _controlIsForField(fid, controlEntity) {
            return fid && fid === sp.result(controlEntity, 'fieldToRender.idP');
        }

        function _controlIsForRelationship(rid, isReversed, controlEntity) {
            return rid && rid === sp.result(controlEntity, 'relationshipToRender.idP') &&
                (isReversed && controlEntity.isReversed || !isReversed && !controlEntity.isReversed);
        }

        function _isEditableControlType(controlEntity) {
            let canEdit = false;
            const typeAlias = sp.result(controlEntity, 'getType.getAlias');
            if (typeAlias) {
                canEdit = editableControlTypes[typeAlias] ||
                    (inlineEditingAllTypes && _.isUndefined(editableControlTypes[typeAlias]));
                if (!canEdit) {
                    console.log(`cannot edit control type: ${typeAlias}`);
                } else if (!editableControlTypes[typeAlias]) {
                    console.log(`trial editing control type: ${typeAlias}, id: ${controlEntity.idP}`);
                }
            }
            return canEdit;
        }

        function _resetSaveStatus(state) {
            state.lastSaveResult = null;
            state.errorBookmark = null;
            state.validationMessages = [];
            return state;
        }

        function _setSaveStatus(state, value) {
            state.lastSaveResult = value;
            if (value !== 'ok') {
                state.errorBookmark = _takeBookmark(state.resourceEntity);
            }
            return state;
        }

        function _validateResource({formEntity, resourceEntity}) {
            const controls = spEditForm.getFormControls(formEntity);
            const ok = _validateFormControls(controls, resourceEntity);
            // can't see a way to get the messages out of the controls... todo: look a little harder
            // .. look at our inlineEditControl component ... have wired up something there to
            // collect and added validation messages to the resource editing state
            return ok;
        }

        function _validateFormControls(controls, resourceEntity) {
            var errors = 0;

            controls = _.filter(controls, c => _.isFunction(c.spValidateControl));

            _.map(controls, function (control) {
                if (!control.spValidateControl(resourceEntity)) {
                    errors += 1;
                    console.warn('Validation failed for control ' + control.idP + ' \'' + control.name + '\'');
                }
            });

            return errors === 0;
        }

        function _takeBookmark(resourceEntity) {
            return resourceEntity.graph.history.addBookmark();
        }

        function _changedSinceBookmark(resourceEntity, bookmark) {
            return resourceEntity.graph.history.changedSinceBookmark(bookmark);
        }
    }

}());

