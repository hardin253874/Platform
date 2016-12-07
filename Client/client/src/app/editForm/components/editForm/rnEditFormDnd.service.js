// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

// import angular from 'angular';
// import _ from 'lodash';
// import * as sp from 'js/utils/utils';
// import {
//     isContainer, isHorizontalContainer, isVerticalContainer, findContainer, applyTransformToForm,
//     createVerticalContainer, createHorizontalContainer
// } from 'js/editForm/editForm';

(function () {

    class EditFormDndService {

        constructor($rootScope, $timeout) {
            'ngInject';

            this.$rootScope = $rootScope;
            this.$timeout = $timeout;

            this.lastDragOverCoords = {};
            this.clearTransform();
        }

        clearTransform() {
            this.transform = {
                control: null,      // control being dragged
                targetState: null,  // 'justUpdated' or 'pendingMove'
                target: null,       // the target immediately after an update and rerender
                targetCell: null,   // the cell of the target
                cmds: []
            };
        }

        getTransform() {
            //TODO - review this ... i think overloading of such as target is confusing

            // examples of cmds
            // {op: 'insert', control: c, target: container}
            // {op: 'remove', control: c, target: container}
            // {op: 'insert', control: c, target: c2, mode: 'before'}
            // {op: 'insert', control: c, target: c2, mode: 'after'}

            return this.transform;
        }

        getDraggedControl() {
            return this.draggedControl;
        }

        apply() {

            //this.$rootScope.$apply();
            this.$timeout(() => (0), 100);
        }

        getDropOptions(formEntity) {
            return {
                data: {formEntity},
                onDragEnter: (...args) => {
                    return this.onDragEnter(formEntity, ...args);
                },
                onDragOver: (...args) => {
                    return this.onDragOver(formEntity, ...args);
                },
                onDragLeave: (...args) => {
                    return this.onDragLeave(formEntity, ...args);
                },
                onDrop: (...args) => {
                    return this.onDrop(formEntity, ...args);
                }
            };
        }

        getDragOptions(formEntity) {
            return {
                data: {formEntity},
                onDragStart: (...args) => {
                    return this.onDragStart(formEntity, ...args);
                },
                onDragEnd: (...args) => {
                    return this.onDragEnd(formEntity, ...args);
                }
            };
        }

        onDragStart(formEntity, e, sd) {
            console.log(`onDragStart ${sd.debugString} \n\ton form ${formEntity.debugString}`);
            this.clearTransform();
            this.transform.control = sd;
            this.apply();
        }

        onDragEnd(formEntity, e, sd) {
            console.log(`onDragEnd ${sd.debugString} \n\ton form ${formEntity.debugString}`);

            // comment out to help debug
            // this.clearTransform();
            // this.apply();
        }

        onDragEnter(formEntity, e, se, te, sd, td) {
            // const targetControl = sp.result(angular.element(e.target), 'scope.$ctrl.control');
            // const sourceControl = sp.result(angular.element(se), 'scope.$ctrl.control');
            // console.log(`onDragEnter ${sd.debugString} \n\ton ${(targetControl || {}).debugString} \n\tform ${formEntity.debugString}`);
        }

        onDragOver(formEntity, e, se, te, sd, td) {
            // console.log(`onDragOver \n\tform ${formEntity.debugString}`);

            if (this.lastDragOverCoords.x === e.clientX && this.lastDragOverCoords.y === e.clientY) {
                return;
            }
            this.lastDragOverCoords = {x: e.clientX, y: e.clientY, mx: e.offsetX, my: e.offsetY};
            // console.log('drag', this.lastDragOverCoords, sp.result(angular.element(e.target), 'scope.$ctrl.control.debugString'));

            this.updateTransform(formEntity, e, se, te, sd, td);
        }

        onDragLeave(formEntity, e, se, te, sd, td) {
            // const targetControl = sp.result(angular.element(e.target), 'scope.$ctrl.control');
            // console.log(`onDragLeave ${sd.debugString} \n\ton ${(targetControl || {}).debugString} \n\tform ${formEntity.debugString}`);
        }

        onDrop(formEntity, e, se, te, sd, td) {
            // console.log(`onDrop ${sd.debugString} \n\tform ${formEntity.debugString}`, e.originalEvent.dataTransfer.dropEffect);

            takeBookmark(formEntity);
            rnEditForm.applyTransformToForm(this.transform, formEntity);
            this.clearTransform();
            this.apply();
        }

        updateTransform(formEntity, e, se, te, sd, td) {
            // sd is the source formControl
            // e.target is the target html element

            // note - temporarily looking for either formControl or formEntity ... until we clean up the models
            const targetControl = sp.result(angular.element(e.target), 'scope.$ctrl.control') ||
                sp.result(angular.element(e.target), 'scope.$ctrl.formEntity');

            // Get the position of the pointer over the targeted element.
            // We are using a 3x3 grid, and the resulting cell # is 0..8
            // The cell layout:
            //      0 1 2
            //      3 4 5
            //      6 7 8
            const cell = getTargetCell(e.offsetX, e.offsetY, e.target);

            if (this.transform.targetState === 'justUpdated') {
                console.log('dnd state justUpdated => pendingNewTarget');
                this.transform = _.assign({}, this.transform, {targetState: 'pendingNewTarget', target: targetControl, targetCell: cell});
                this.apply();
                return;
            }

            if (this.transform.targetState === 'pendingNewTarget') {
                if (this.transform.target === targetControl && this.transform.targetCell === cell) {
                    console.log('dnd state pendingNewTarget ... same target so skip out');
                    return;
                }
                console.log('dnd state pendingNewTarget => none');
                this.transform = _.assign({}, this.transform, {targetState: null, target: null, targetCell: null});
                this.apply();
            }

            // if we are over the source control then do nothing
            if (sd === targetControl) {
                console.log('ignore dragging over self');
                return;
            }

            // prepare an empty transform
            const transform = {control: sd, cmds: []};

            if (rnEditForm.isContainer(targetControl)) {

                // For now simply remove the source control from its current container and
                // insert it in the targeted container.
                // todo - check if same container
                // todo - maybe note original container on drag start rather than on drag
                // todo - insert is sensible position relative to other items in the container

                console.log('moving to container');

                transform.cmds.push(
                    {op: 'remove', control: sd, target: rnEditForm.findContainer(formEntity, sd)},
                    {op: 'insert', control: sd, target: targetControl}
                );

            } else {

                // Find the container of the targeted control
                const targetContainer = rnEditForm.findContainer(formEntity, targetControl);

                // console.log(`target is ${cell} in ${targetControl.debugString} in container ${targetContainer.debugString}`);

                // The starter list of transform commands is to remove the source control.
                const cmds = [
                    {op: 'remove', control: sd, target: rnEditForm.findContainer(formEntity, sd)}
                ];

                // Perform some insert logic based which area (cell) of the target control we are over
                // and the type of the parent container. In some cases we simply insert, in others
                // we are creating new containers and moving the target control, in addition to moving
                // the source control.

                if (_.includes([1, 7], cell) ||
                    rnEditForm.isVerticalContainer(targetContainer) && _.includes([0, 2, 4, 6, 8], cell)) {

                    if (!rnEditForm.isVerticalContainer(targetContainer)) {
                        const newContainer = rnEditForm.createVerticalContainer();
                        cmds.push({op: 'insert', control: newContainer, target: targetControl, mode: 'before'});
                        cmds.push({op: 'remove', control: targetControl, target: targetContainer});
                        cmds.push({op: 'insert', control: targetControl, target: newContainer});
                    }
                    const mode = _.includes([0, 1, 2, 4], cell) ? 'before' : _.includes([6, 7, 8], cell) ? 'after' : null;
                    if (mode) {
                        transform.cmds.push(...cmds, {op: 'insert', control: sd, target: targetControl, mode});
                    }
                }

                if (_.includes([3, 5], cell) ||
                    rnEditForm.isHorizontalContainer(targetContainer) && _.includes([0, 2, 4, 6, 8], cell)) {

                    if (!rnEditForm.isHorizontalContainer(targetContainer)) {
                        const newContainer = rnEditForm.createHorizontalContainer();
                        cmds.push({op: 'insert', control: newContainer, target: targetControl, mode: 'before'});
                        cmds.push({op: 'remove', control: targetControl, target: targetContainer});
                        cmds.push({op: 'insert', control: targetControl, target: newContainer});
                    }
                    const mode = _.includes([0, 3, 4, 6], cell) ? 'before' : _.includes([2, 5, 8], cell) ? 'after' : null;
                    if (mode) {
                        transform.cmds.push(...cmds, {op: 'insert', control: sd, target: targetControl, mode});
                    }
                }
            }

            // note... the following only compares control and cmds
            if (transformChanged(this.transform, transform)) {
                console.log(`${this.transform.targetState} => justUpdated with ${transform.cmds.length} cmds`);
                this.transform = _.assign({}, transform, {targetState: 'justUpdated', target: null, targetCell: null});
                this.apply();
            }
        }
    }

    function getTargetCell(x, y, target) {
        x = Math.max(0, x);
        y = Math.max(0, y);
        const rect = target.getBoundingClientRect();
        const dx = rect.width / 3, dy = rect.height / 3;
        const c = Math.floor(x / dx), r = Math.floor(y / dy);
        if (r * 3 + c < 0) {
            console.log({x, y, target, dx, dy, c, r});
        }
        return r * 3 + c;
    }

    function transformChanged(t1, t2) {
        // for some reason a deep compare doesn't work.. always seems to be changed
        // besides this will be more efficient
        if (t1.control !== t2.control) return true;
        if (t1.cmds.length !== t2.cmds.length) return true;
        for (let i = 0; i < t1.cmds.length; i = i + 1) {
            if (t1.cmds[i].op !== t2.cmds[i].op) return true;
            if (t1.cmds[i].control.idP !== t2.cmds[i].control.idP) return true;
            if (t1.cmds[i].target.idP !== t2.cmds[i].target.idP) return true;
            if (t1.cmds[i].mode !== t2.cmds[i].mode) return true;
        }
        return false;
    }

    function removeFormControl(container, control) {

        if (_.indexOf(container.containedControlsOnForm, control) >= 0) {
            container.containedControlsOnForm.remove(control);
            return {container, control};
        } else {
            var result = null;
            return _.some(container.containedControlsOnForm, c => {
                    result = removeFormControl(c, control);
                    return result;
                }) && result;
        }
    }

    function addSiblingFormControl(container, targetControl, control) {

        if (rnEditForm.isContainer(targetControl)) {
            targetControl.containedControlsOnForm.add(control);
            return;
        }

        if (_.indexOf(container.containedControlsOnForm, targetControl) >= 0) {
            container.containedControlsOnForm.add(control);
            return;
        }

        _.forEach(container.containedControlsOnForm, c => {
            addSiblingFormControl(c, targetControl, control);
        });
    }

    function takeBookmark(entity) {
        console.log('taking bookmark', historyStr(entity));
        return entity.graph.history.addBookmark();
    }

    function revertToBookmark(entity, bookmark) {
        console.log('reverting bookmark', historyStr(entity));
        return entity.graph.history.undoBookmark(bookmark);
    }

    function changedSinceBookmark(entity, bookmark) {
        return entity.graph.history.changedSinceBookmark(bookmark);
    }

    function historyStr(entity) {
        return JSON.stringify(entity.graph.history, ['_changeCounter', '_undoList', 'name', 'isBookmark']);
    }

    angular.module('app.editFormComponents')
        .service('rnEditFormDndService', EditFormDndService);

})();

