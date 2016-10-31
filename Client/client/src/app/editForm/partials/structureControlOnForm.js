// Copyright 2011-2016 Global Software Innovation Pty Ltd

angular.module('app.editForm.structureControlOnFormController', ['mod.app.editForm'])
    .controller('structureControlOnFormController',
        // formControl must be set in the scope
        function ($scope, spEditForm) {
            'use strict';

            if ($scope.childFormControl) {
                $scope.parentControl = $scope.formControl;
                $scope.formControl = $scope.childFormControl;
            }

            function refreshControlsOnForm(scope) {
                if (scope.formControl) {
                    var newArray = scope.formControl.getContainedControlsOnForm().slice(0);
                    scope.controlsOnForm = newArray.sort(spEditForm.compareByRenderingOrdinal);
                }
            }

            $scope.$watch(
                'formControl.id()', // not watching the form control as it is too expensive
                function (formControl) {
                    if (formControl) {
                        spEditForm.commonFormControlInit($scope, formControl);
                        refreshControlsOnForm($scope);
                    }
                }
            ); // Warning, do not make this a deep watch. A deep watch here dramatically slows page render. We are manually controlling refreshes in design mode.

            $scope.$on("removeControl", function(event, controlToRemove) {
                event.stopPropagation();

                $scope.removeControl(controlToRemove);
            });

            $scope.removeControl = function(controlToRemove) {

                spEditForm.removeChild($scope.formControl, controlToRemove);
                refreshControlsOnForm($scope);

                $scope.$apply(); // manually update. This is needed because we are doing a shallow watch on the formControl.
            };

            $scope.$on("addControl", function(event, controlToAdd, controlDroppedOn) {
                event.stopPropagation();

                $scope.addControl(controlToAdd, controlDroppedOn);
            });

            $scope.$on('onControlsOnFormChanged', function (event, message) {
                refreshControlsOnForm($scope);
            });

            $scope.getTitleStyle = function (formControl) {
                if (!formControl || !formControl.name || $.trim(formControl.name).length === 0) {
                    return { display: 'none' };
                }
                return { display: 'block' };
            };

            $scope.addControl = function (controlToAdd, controlDroppedOn) {
                var structureControl;
                var scope;
                var isStructureControl;
                var child = null;
                
                structureControl = controlDroppedOn.formControl;
                scope = controlDroppedOn;

                isStructureControl = spEditForm.isStructureControl(structureControl);

                if (!isStructureControl) {
                    child = structureControl;
                }

                while (!isStructureControl) {
                    scope = scope.$parent;

                    if (scope) {
                        structureControl = scope.formControl;

                        if (structureControl) {
                            isStructureControl = spEditForm.isStructureControl(structureControl);
                        }
                    } else {
                        break;
                    }
                }

                if (!isStructureControl) {
                    return;
                }
                if (!child) {
                    // it has been dropped directly on a structure control, so put it at the end.
                    spEditForm.addChildToEnd(structureControl, controlToAdd);
                } else {
                    // dropped on one the child controls, so insert it after it.
                    spEditForm.addChildAfter(structureControl, controlToAdd, controlDroppedOn.$index);
                }

                controlDroppedOn.$emit('controlsOnFormChanged');

                //$scope.$apply(); // manually update. This is needed because we are doing a shallow watch on the formControl.
            };

            $scope.getFormControlFile = spEditForm.getFormControlFile;

            $scope.isVerticalStackContainerControl = function (formControl) {

                var baseFileName = formControl.firstTypeId().getAlias();

                //return false;
                return baseFileName === 'verticalStackContainerControl';
            };

            $scope.getStructureControlFile = spEditForm.getStructureControlFile;

            $scope.sortableOptions = {
                disabled: $scope.formMode !== spEditForm.formModes.design,
                revert: true,
                onSortComplete: function (event, ui, fromIndex, toIndex) {
                    var startIndex;
                    var endIndex;
                    var renderingOrdinal;
                    var originalRenderingOrdinal;

                    if (toIndex === fromIndex) {
                        return;
                    }

                    /////
                    // Reorder the controlsOnForm array.
                    /////
                    $scope.controlsOnForm.splice(toIndex, 0, $scope.controlsOnForm.splice(fromIndex, 1)[0]);

                    if (toIndex < fromIndex) {
                        startIndex = toIndex;
                        endIndex = fromIndex;
                    } else {
                        startIndex = fromIndex;
                        endIndex = toIndex;
                    }

                    originalRenderingOrdinal = $scope.controlsOnForm[startIndex].renderingOrdinal;

                    /////
                    // Update the rendering ordinals for all controls between the 'start' and 'end' indexes.
                    /////
                    for (var i = startIndex; i <= endIndex; i++) {

                        if (i + 1 <= endIndex) {
                            renderingOrdinal = $scope.controlsOnForm[i + 1].renderingOrdinal;
                        } else {
                            renderingOrdinal = originalRenderingOrdinal;
                        }
                        
                        $scope.controlsOnForm[i].renderingOrdinal = renderingOrdinal;
                    }
                }
            };

            $scope.getStructureControlClass = function (formMode) {
                if (formMode === 'design')
                    return 'structure-control-on-form-design';
                else
                    return 'structure-control-on-form';
            };
            if (!$scope.structureDepth) {
                $scope.structureDepth = 1;
            } else {
                $scope.structureDepth = $scope.structureDepth + 1;
            }

            $scope.titleClass = 'title' + $scope.structureDepth;
            

            $scope.isStructureControl = function (entity) {
                var baseFileName;
                var structureControl;

                baseFileName = entity.firstTypeId().getAlias();

                structureControl = false;

                switch (baseFileName) {
                    case 'structureControlOnForm':
                    case 'verticalStackContainerControl':
                    case 'horizontalStackContainerControl':
                    case 'headerColumnContainerControl':
                        structureControl = true;
                        break;
                }

                return structureControl;
            };
        }
    )
    .controller('structureControlOnFormRepeaterController',
        function($scope, spEditForm) {
            "use strict";

            var thingToRender;

            if ($scope.childFormControl) {
                $scope.parentControl = $scope.formControl;
                $scope.formControl = $scope.childFormControl;
            }

            if ($scope.formControl) {
                thingToRender = $scope.formControl.getLookup('console:fieldToRender') || $scope.formControl.getLookup('console:relationshipToRender');

                $scope.fieldTitle = spEditForm.getControlTitle($scope.formControl);
                $scope.fieldDescription = ( $scope.formControl.getDescription && $scope.formControl.getDescription( ) ) || (thingToRender && thingToRender.getDescription());


                if ($scope.isInTestMode) {
                    $scope.testId = spEditForm.cleanTestId(_.result(thingToRender, 'getName'));
                }
            }

            $scope.controlNeedsTitle = spEditForm.controlNeedsTitle;

            $scope.$on('controlsOnFormChanged', function (event, message) {
                
                event.stopPropagation();

                $scope.$broadcast('onControlsOnFormChanged');
            });
        }
    );

