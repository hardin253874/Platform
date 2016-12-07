// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    /////
    //
    // relationshipPickerController
    //
    /////
    function relationshipPickerController($scope, $element, spCachingCompile, spEntityService) {
        let ctrl = this;
        let relationshipRef;
        let relationshipList = [];

        ctrl.options = [];
        ctrl.selectedOption = {};

        ctrl.click = function() {
            ctrl.onClick({});
        };

        /////
        //
        // getAllPotentialConnectsToTypes
        //
        // Get all the derived types needed to filter the relationships by the type on the other end.
        //
        // connects         - entityRef that identifies the type we hope relationships will connect to
        //
        /////
        function getAllPotentialConnectsToTypes(connects) {
            let req = 'name, derivedTypes*.{name}';
            return spEntityService.getEntity(connects, req).then(function (type) {
                return spUtils.walkGraph(function(t) { return t.derivedTypes; }, type);
            });
        }

        /////
        //
        // getFilteredRelationships
        //
        // Retrieves all the relationships the target type may have, filtered by the desired types we want to connect to.
        //
        // targets          - entityRef that identifies the type we will inspect for relationships
        // connectsToTypes  - list of entity types used to filter the relationships by their complimentary type
        //
        /////
        function getFilteredRelationships(targets, connectsToTypes) {
            let req = 'inherits*, { relationships, reverseRelationships }.{ alias, name, toName, fromName, cardinality.alias, description, { fromType, toType }.{ id } }';
            return spEntityService.getEntity(targets, req).then(function(type) {
                let rels = new spResource.Type(type).getAllRelationships();
                return _.filter(rels, function(rel) {
                    let isReverse = rel.isReverse();
                    let relEntity = rel.getEntity();
                    let filterType = isReverse ? relEntity.fromType : relEntity.toType;
                    if (!filterType) return false;
                    return _.some(connectsToTypes, function (f) {
                        return f.idP === filterType.idP;
                    });
                });
            });
        }

        /////
        //
        // getOptions
        //
        // Constructs the list of option values for the control.
        //
        // targets          - identifies a type that the relationship should emanate from
        // connects         - identifies a type that the relationships should also connect to
        //
        /////
        function getOptions(targets, connects) {
            let targetsRef = spEntity.asEntityRef(targets);
            let connectsRef = spEntity.asEntityRef(connects);

            return getAllPotentialConnectsToTypes(connectsRef).then(function (connectsToTypes) {
                return getFilteredRelationships(targetsRef, connectsToTypes).then(function (filtered) {
                    relationshipList = filtered;
                    return _.map(filtered, function (rel) {
                        return {
                            id: rel.getEntity().idP,
                            name: rel.getName(),
                            forward: !rel.isReverse()
                        };
                    });
                });
            });
        }

        /////
        //
        // refreshOptions
        //
        // Rebuilds the options list.
        //
        /////
        function refreshOptions() {
            if (ctrl.startType && ctrl.endType) {
                getOptions(ctrl.startType, ctrl.endType).then(function (options) {
                    ctrl.options = options;

                    // pre-select the correct option if we were handed an existing relationship as input
                    if (relationshipRef) {
                        ctrl.selectedOption = _.find(ctrl.options, function(o) {
                            return o.id === relationshipRef.getId();
                        });
                    }
                });
            }
        }

        let refreshOptionsDebounce = _.debounce(refreshOptions, 100);

        /////
        //
        // Initialize
        //
        /////
        ctrl.$onInit = function () {

            // Set defaults
            if (!ctrl.selectString) {
                ctrl.selectString = '[Select]';
            }

            if (ctrl.relationship) {
                relationshipRef = spEntity.asEntityRef(ctrl.relationship);
            }
            
            refreshOptionsDebounce();
        };

        /////
        //
        // Handle input changes
        //
        /////
        ctrl.$onChanges = function (changesObj) {
            if (changesObj.startType) {
                refreshOptionsDebounce();
            }

            if (changesObj.endType) {
                refreshOptionsDebounce();
            }

            if (changesObj.relationship) {
                relationshipRef = spEntity.asEntityRef(changesObj.relationship.currentValue);
                refreshOptionsDebounce();
            }
        };

        /////
        //
        // Watch for selection
        //
        /////
        $scope.$watch('$ctrl.selectedOption', function(newValue, oldValue) {
            if (newValue !== oldValue) {
                let relId = _.get(newValue, 'id');
                let selected = relId ? _.find(relationshipList, function(rel) {
                    return rel && rel.getEntity() && rel.getEntity().idP === newValue.id;
                }) : null;
                ctrl.onSelect({ selected: selected });
            }
        });

        let cachedLinkFunc = spCachingCompile.compile('controls/rnRelationshipPicker/rnRelationshipPicker.tpl.html');
        cachedLinkFunc($scope, function (clone) {
            $element.append(clone);
        });
    }

    angular.module('mod.app.controls')
        .component('rnRelationshipPicker', {
            controller: relationshipPickerController,
            bindings: {
                relationship: '<',  // The current relationship
                startType: '<',     // Relationships that this type can participate in
                endType: '<',       // Filter the relationship further by their complimentary types
                isReadOnly: '<',    // Controls if in read only mode
                isInDesign: '<',    // Controls if operating as part of a builder
                selectString: '@?', // The string to present in the option when no selection has been made
                onClick: '&',       // Handles when the read-only link has been clicked
                onSelect: '&'       // Handles when the selection has changed in the dropdown
            }
        });
})();