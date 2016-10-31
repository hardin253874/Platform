// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, console, sp, Showdown, spResource, spEntity */

(function () {
    'use strict';

    /**
     * AngularJS controller function for the entity explorer.
     *
     * note - if you change the parameters here then ensure you change the controller registration to match.
     */
    function entityExplorerController($scope, $stateParams, $timeout, $q, spEntityService, spNavService, spResource) {

        function renderDoc(value) {
            return value;
            //We don't include Showdown as not using anywhere else.
            //return new Showdown.converter().makeHtml(value);
        }

        function receivedEntityAsType(entity) {
            var inherited, derived;

            if (!entity || !$scope.entity || entity.id() !== $scope.entity.id()) {
                return;
            }

            inherited = spResource.getAncestorsAndSelf(entity);
            derived = spResource.getDerivedTypesAndSelf(entity);

            $scope.entityMeta = {
                inherited: _.without(inherited, entity),
                derived: _.without(derived, entity),
                fields: _.flatten(_.invokeMap(inherited, 'getRelationship', 'fields')),
                relationships: _.flatten(_.invokeMap(inherited, 'getRelationship', 'relationships')),
                revRelationships: _.flatten(_.invokeMap(inherited, 'getRelationship', 'reverseRelationships'))
            };
        }

        function receivedEntity(entity) {
            var desc;

            if (!entity) {
                return;
            }
            if (entity.id() === $scope.rootEid) {
                $scope.rootEntityName = entity.getName();
            }

            $scope.entity = entity;
            $scope.entityJson = spEntity.toJSON(entity);

            desc = entity.field('description') || '_no description_';
            $scope.entity.docHtml = renderDoc(desc);

            // request type info in case the entity itself is a "type"
            spEntityService.getEntity(entity.id(), 'alias,name,description,' +
                '{fields,relationships}.{alias,name,*,{fromType,toType}.{alias,name,isOfType.{alias,name}},cardinality.{alias,name}},' +
                'inherits*.{alias,name,description,{fields,relationships}.{alias,name,*,' +
                '  {fromType,toType}.{alias,name,isOfType.{alias,name}},cardinality.{alias,name}}},' +
                'derivedTypes.{alias,name,description}')
                .then(receivedEntityAsType);

            // request instances in case the entity itself is a "type"
            spEntityService.getEntitiesOfType(entity.id(), 'alias,name').then(function (entities) {
                $scope.instances = _.take(entities, 500);
            });
        }

        function receivedTypeEntity(entity) {
            var desc, typeEntity, inherited, derived, rels;

            if (!entity) {
                return;
            }

            //only grabbing the first at the moment
            $scope.typeEntity = _.first(entity.getIsOfType());

            if (!$scope.typeEntity) {
                return;
            }

            typeEntity = $scope.typeEntity;

            desc = typeEntity.field('description') || '_no description_';
            typeEntity.docHtml = renderDoc(desc);

            inherited = spResource.getAncestorsAndSelf(typeEntity);
            derived = spResource.getDerivedTypesAndSelf(typeEntity);

            $scope.typeEntityMeta = {
                inherited: _.without(inherited, typeEntity),
                derived: _.without(derived, typeEntity),
                fields: _.flatten(_.invokeMap(inherited, 'getFields')),
                relationships: [].concat(
                    _.map(_.flatten(_.invokeMap(inherited, 'getRelationships')), function (r) {
                        return { rel: r, id: r.id(), reverse: false,
                            name: r.getToName() || r.getName() || r.alias() || r.id() };
                    }),
                    _.map(_.flatten(_.invokeMap(inherited, 'getRelationship', 'reverseRelationships')), function (r) {
                        return { rel: r, id: r.id(), reverse: true,
                            name: r.getFromName() || r.getName() || r.getField('reverseAlias') || r.id() };
                    }))
            };

            entity = $scope.entity;

            if ($scope.typeEntityMeta.fields && entity) {
                $scope.fields = _.chain($scope.typeEntityMeta.fields)
                    .map(function (f) {
                        return { id: f.id(), alias: f.alias(), name: f.name, value: entity.getField(f.id())};
                    })
                    .filter(function (f) {
                        return !_.isUndefined(f.value) && !_.isNull(f.value);
                    })
                    .value();
            }

            _.each($scope.typeEntityMeta.relationships, function (relWrapper) {

                function ensureRelationshipAliases(rel) {
                    var id = rel.id(),
                        alias = rel.getField('alias'),
                        reverseAlias = rel.getField('reverseAlias'),
                        relEntity = spEntity.fromId(id);

                    if (!alias || !reverseAlias) {
                        if (!alias) {
                            relEntity.setField('alias', alias = 'relAlias' + id, 'String');
                            rel.setField('alias', alias, 'String');
                        }
                        if (!reverseAlias) {
                            relEntity.setField('reverseAlias', reverseAlias = 'relReverseAlias' + id, 'String');
                            rel.setField('reverseAlias', reverseAlias, 'String');
                        }
                        console.log('Updating missing alias for rel', rel.id(), alias, reverseAlias, relEntity);
                        spEntityService.putEntity(relEntity);
                    }
                }

                var rel = relWrapper.rel;

                //console.log('rel', _.map(_.map(rel._fields, 'id'), '_alias'));
                if (!rel.getField('alias') || !rel.getField('reverseAlias')) {
                    ensureRelationshipAliases(rel);
                }
            });
        }

        $scope.go = function (eid) {
            if (eid && $scope.eid !== eid) {
                if ($scope.eid) {
                    $scope.eidStack.push($scope.eid);
                }
                $scope.eid = eid;
            }
        };

        $scope.home = function () {
            $scope.eid = $scope.rootEid;
            $scope.eidStack = [];
        };

        $scope.back = function () {
            $scope.eid = $scope.eidStack.pop() || $scope.rootEid;
        };

        $scope.getRelated = function (rel) {
            if (rel && $scope.eid) {
                //... ugly ... its past midnight....
                rel.alias = rel.alias || (rel.reverse ? rel.rel.getField('reverseAlias') : rel.rel.getField('alias'));
                $scope.currentRel = rel;
                $scope.related = [];
                return spEntityService.getEntity(sp.coerseToNumberOrLeaveAlone($scope.eid), rel.alias + '.name').then(function (entity) {
                    $scope.related = entity.getRelationship(rel.id);
                    $scope.currentRel.instanceCount = $scope.related.length;
                    return $scope.related.length;
                });
            }
        };

        function getNextRelated() {
            $scope.autoGetRelatedRunning = false;
            if ($scope.autoGetRelated) {
                if ($scope.typeEntityMeta && $scope.typeEntityMeta.relationships) {
                    $scope.currentRelIndex = (($scope.currentRelIndex || 0) + 1) % $scope.typeEntityMeta.relationships.length;
                    $q.when($scope.getRelated($scope.typeEntityMeta.relationships[$scope.currentRelIndex])).then(function (n) {
                        // if we got related instances then pause a bit longer
                        $timeout(getNextRelated, n > 0 ? 3000 : 500);
                        $scope.autoGetRelatedRunning = true;
                    }).catch(function () {
                        console.error('getRelated failed', arguments);
                    });
                } else {
                    // waiting for type entity...
                    $timeout(getNextRelated, 500);
                    $scope.autoGetRelatedRunning = true;
                }
            }
        }

        $scope.$watch('autoGetRelated', function () {
            if ($scope.autoGetRelated && !$scope.autoGetRelatedRunning) {
                $timeout(getNextRelated, 0);
                $scope.autoGetRelatedRunning = true;
            }
        });

        $scope.filteredRelationships = function () {
            if (!$scope.typeEntityMeta || !$scope.typeEntityMeta.relationships) return [];
            return !$scope.withInstancesFilter ? $scope.typeEntityMeta.relationships :
                _.filter($scope.typeEntityMeta.relationships, 'instanceCount');
        };

        $scope.toOneRelationships = function () {
            if (!$scope.entityMeta || !$scope.entityMeta.relationships) return [];
            return _.filter($scope.entityMeta.relationships, function (r) {
                return r.cardinality.eid().getNsAlias() === 'core:oneToOne' || r.cardinality.eid().getNsAlias() === 'core:manyToOne';
            });
        };

        $scope.toManyRelationships = function () {
            if (!$scope.entityMeta || !$scope.entityMeta.relationships) return [];
            return _.filter($scope.entityMeta.relationships, function (r) {
                return !(r.cardinality.eid().getNsAlias() === 'core:oneToOne' || r.cardinality.eid().getNsAlias() === 'core:manyToOne');
            });
        };

        $scope.choiceRelationships = function () {
            if (!$scope.entityMeta || !$scope.entityMeta.relationships) return [];
            return _.filter($scope.entityMeta.relationships, function (r) {
                return sp.result(r, 'toType.isOfType.0.eid.getNsAlias') === 'core:enumType';
            });
        };

        $scope.onEntityIdEntered = function () {
            $scope.go($scope.aliasOrId);
        };

        $scope.$on('explore-entity', function (e, eid) {
            console.log('on explore-entity', eid);
            $scope.rootEid = eid;
            $scope.go(eid);
        });

        $scope.$watch('eid', function (eid) {
            if (eid) {
                console.log('getting eid ', eid);
                eid = sp.coerseToNumberOrLeaveAlone(eid);
                $scope.entity = {};
                $scope.entityMeta = {};
                $scope.typeEntity = {};
                $scope.typeEntityMeta = {};
                $scope.fields = [];
                $scope.instances = [];
                $scope.related = [];
                $scope.currentRel = {};
                $scope.currentRelIndex = 0;

                // request basic stuff for the entity
                spEntityService.getEntity(eid, '*,isOfType.{alias,name}').then(receivedEntity);

                // request information from its type so we know what fields and relationships are possible
                spEntityService.getEntity(eid,
                        'isOfType.{alias,name,description,' +
                        '{fields,relationships,reverseRelationships}.{alias,name,*,{fromType,toType}.{alias,name}},' +
                        'inherits*.{alias,name,description,{fields,relationships,reverseRelationships}.{alias,name,*,{fromType,toType}.{alias,name}}},' +
                        'derivedTypes.{alias,name,description}}')
                    .then(receivedTypeEntity);
            }
        });

        $scope.$watch('selectedRelId', function (selectedRelId) {
            //todo do the forward and reverse rel thing
            $scope.getRelated(selectedRelId);
        });

        $scope.rootEid = sp.coerseToNumberOrLeaveAlone($stateParams.eid || 0);
        $scope.eidStack = [];

        $scope.eid = $scope.rootEid;
        $scope.entity = {};
        $scope.typeEntity = {};

        //////////////////////////////////////////////////////
        // Hook up to the nav system

        $scope.item = spNavService.getCurrentItem();

        if (!$scope.item) {

            console.error('explorerController: no current nav item');

        } else {

            if (!$scope.item.data) {
                $scope.item.data = {};
                $scope.item.isDirty = false;
            }

            $scope.parentItem = spNavService.getParentItem();

            if ($scope.parentItem && $scope.parentItem.data) {
                $scope.parentResources = $scope.parentItem.data.resources;
                if ($scope.parentItem.data.setSelected) {
                    $scope.parentItem.data.setSelected($scope.item);
                }
            }

            // for our test UI
            $scope.hasDirty = true; // to light up the 'toggle dirty' button
            $scope.toggleDirty = function () {
                $scope.item.isDirty = !$scope.item.isDirty;
            };
        }

        // End of nav system hookup
        //////////////////////////////////////////////////////
    }

    entityExplorerController.$inject = ['$scope', '$stateParams', '$timeout', '$q', 'spEntityService', 'spNavService', 'spResource'];

    angular.module('app.entityExplorer', ['ui.router', 'mod.common.spEntityService', 'mod.common.spResource', 'sp.navService'])
        .config(function ($stateProvider) {
            $stateProvider.state('explorer', {
                url: '/{tenant}/{eid}/explorer?path',
                templateUrl: 'devViews/explorer/entityExplorer.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.explorer = { name: 'Entity Explorer' };

        })
        .controller('EntityExplorerController', entityExplorerController);

}());

