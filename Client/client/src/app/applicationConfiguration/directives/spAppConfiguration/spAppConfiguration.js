// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    angular.module('mod.app.applicationConfiguration.directives.spAppConfiguration', [
        'mod.common.alerts',
        'mod.common.spWebService',
        'mod.common.spEntityService',
        'mod.ui.spReportModelManager',
        'mod.common.spXsrf',
        'sp.navService',
        'sp.common.spDialog'
    ])
    .directive('spAppConfiguration', function ($rootScope, $q, spAlertsService, spWebService, spEntityService, spReportModelManager, spXsrf, spNavService, spDialog) {
        return {
            restrict: 'E',
            templateUrl: 'applicationConfiguration/directives/spAppConfiguration/spAppConfiguration.tpl.html',
            replace: true,
            transclude: false,
            link: function (scope) {
                scope.model = {
                    selectedApplication: null,
                    dependsOnView: 'dependsOnViewTable',
                    requiredByView: 'requiredByViewTable',
                    dependsOn: [],
                    dependsOnTree: {},
                    requiredBy: [],
                    requiredByTree: {}
                };

                scope.canSaveDependency = canSaveDependency;

                scope.selectApplication = function (application) {

                    if (!application) {
                        return;
                    }

                    if (checkForModifiations()) {
                        return;
                    }

                    scope.model.selectedApplication = application;

                    extractProperties(application);

                    fetchApplicationTypes();

                    scope.model.dependsOnTree = {};
                    scope.model.requiredByTree = {};

                    // Get the Depends On collection
                    buildDependencies(application,
                        scope.model.dependsOn,
                        scope.model.dependsOnTree,
                        function(dep) {
                            return dep.dependencyApplication;
                        },
                        function (dep) {
                            return dep.dependentApplication;
                        },
                        function (indirect) {
                            return indirect.dependencies;
                        });

                    // Get the Required By collection
                    buildDependencies(application,
                        scope.model.requiredBy,
                        scope.model.requiredByTree,
                        function (dep) {
                            return dep.dependentApplication;
                        },
                        function (dep) {
                            return dep.dependencyApplication;
                        },
                        function (indirect) {
                            return indirect.dependents;
                        });

                    if (scope.model.dependsOnView === 'dependsOnViewDiagram') {
                        generateGraph(scope.model.dependsOnTree, '.dependsOnDiagram', 'marker-end', 'url(#arrowheadDownDependsOn)', 25, -38, 'DependsOn');
                    }

                    if (scope.model.requiredByView === 'requiredByViewDiagram') {
                        generateGraph(scope.model.requiredByTree, '.requiredByDiagram', 'marker-start', 'url(#arrowheadUpRequiredBy)', 38, -25, 'RequiredBy');
                    }

                };

                scope.addDependency = function (target) {

                    if (!target) {
                        return;
                    }

                    if (checkForModifiations()) {
                        return;
                    }

                    var applications = getAllApplications();

                    var dependency = {
                        isRequired: true,
                        isDirect: true,
                        isEditing: true,
                        isNew: true,
                        availableApplications: getAvailableApplications(applications)
                    };

                    target.push(dependency);
                };

                scope.removeDependency = function (dependency, target) {

                    if (!dependency) {
                        return;
                    }

                    if (!target) {
                        return;
                    }

                    if (dependency.isNew) {
                        var index = target.indexOf(dependency);

                        if (index >= 0) {
                            target.splice(index, 1);
                        }
                    } else {

                        if (dependency.isEditing) {
                            dependency.name = dependency._name;
                            dependency.minimumVersion = dependency._minimumVersion;
                            dependency.maximumVersion = dependency._maximumVersion;
                            dependency.isRequired = dependency._isRequired;
                            dependency.isEditing = false;
                        } else {
                            if (checkForModifiations(dependency)) {
                                return;
                            }

                            spDialog.confirmDialog('Confirm delete', 'Are you sure you want to remove the selected dependency?').then(function (result) {
                                if (result) {
                                    spEntityService.deleteEntity(dependency.entity.id())
                                .then(function () {
                                    var index = _.findIndex(scope.model.dependencies,
                                        function (existingDependency) {
                                            return dependency.entity === existingDependency;
                                        });

                                    if (index >= 0) {
                                        scope.model.dependencies.splice(index, 1);
                                    }

                                    scope.model.map = {};

                                    _.forEach(scope.model.dependencies, updateDependencyMap);

                                    scope.selectApplication(scope.model.selectedApplication);

                                });
                                }
                            });

                            
                        }
                    }
                };

                scope.editDependency = function (dependency) {

                    if (!dependency) {
                        return;
                    }

                    if (checkForModifiations()) {
                        return;
                    }

                    var applications = getAllApplications();

                    dependency.availableApplications = getAvailableApplications(applications);

                    var selectedApp = _.find(applications,
                        function(app) {
                            return app.id === dependency.id;
                        });

                    if (selectedApp) {
                        dependency.availableApplications.unshift(selectedApp);
                        dependency.selectedApp = selectedApp;
                    }

                    dependency.isEditing = true;
                };

                scope.saveDependency = function(dependency, isDependsOn) {

                    dependency.name = dependency.selectedApp.name;
                    dependency.isRequired = dependency.isRequired;
                    delete dependency.availableApplications;

                    var name;
                    var desc;
                    if (isDependsOn) {
                        name = scope.model.selectedApplication.name + ' Depends on ' + dependency.name;
                        desc = scope.model.selectedApplication.name + ' depends on ' + dependency.name + ' being installed.';
                    } else {
                        name = dependency.name + ' Depends on ' + scope.model.selectedApplication.name;
                        desc = dependency.name + ' depends on ' + scope.model.selectedApplication.name + ' being installed.';
                    }

                        var applicationDependency;

                    if (dependency.isNew) {
                        var json = {
                            name: name,
                            description: desc,
                            typeId: 'core:applicationDependency',
                            'core:applicationMinimumVersion': dependency.minimumVersion,
                            'core:applicationMaximumVersion': dependency.maximumVersion,
                            'core:applicationIsRequired': dependency.isRequired
                        };

                        if (isDependsOn) {
                            json['core:dependentApplication'] = jsonLookup(scope.model.selectedApplication.id());
                            json['core:dependencyApplication'] = jsonLookup(dependency.selectedApp.entity.id());
                            json['core:inSolution'] = jsonLookup(scope.model.selectedApplication.id());
                        } else {
                            json['core:dependentApplication'] = jsonLookup(dependency.selectedApp.entity.id());
                            json['core:dependencyApplication'] = jsonLookup(scope.model.selectedApplication.id());
                            json['core:inSolution'] = jsonLookup(dependency.selectedApp.entity.id());
                        }

                        applicationDependency = spEntity.fromJSON(json);
                    } else {
                        dependency.entity.name = name;
                        dependency.entity.description = desc;
                        dependency.entity.applicationMinimumVersion = dependency.minimumVersion;
                        dependency.entity.applicationMaximumVersion = dependency.maximumVersion;
                        dependency.entity.applicationIsRequired = dependency.isRequired;

                        if (isDependsOn) {
                            if (dependency.entity.dependencyApplication.id() !== dependency.selectedApp.entity.id()) {
                                dependency.entity.dependencyApplication = dependency.selectedApp.entity;
                            }
                        } else {
                            if (dependency.entity.dependentApplication.id() !== dependency.selectedApp.entity.id()) {
                                dependency.entity.dependentApplication = dependency.selectedApp.entity;
                            }
                        }

                        applicationDependency = dependency.entity;
                    }

                    spEntityService.putEntity(applicationDependency)
                        .then(function (id) {

                            spEntityService.getEntity(id,
                                    'applicationIsRequired,applicationMinimumVersion,applicationMaximumVersion,dependentApplication.name,dependencyApplication.name')
                                .then(function(entity) {
                                    dependency.isEditing = false;
                                    dependency.entity = entity;
                                    delete dependency.isNew;

                                    scope.model.dependencies.push(entity);

                                    scope.model.map = {};

                                    _.forEach(scope.model.dependencies, updateDependencyMap);

                                    scope.selectApplication(scope.model.selectedApplication);
                                });
                            
                        });
                };

                scope.getDeleteTitle = function (dependency) {
                    if (dependency.isEditing) {
                        if (dependency.isNew) {
                            return "Delete dependency";
                        } else {
                            return "Undo changes";
                        }
                    } else {
                        return "Delete dependency";
                    }
                };

                scope.validateVersion = function (evt) {

                    var keyCode = evt.keyCode || evt.charCode;

                    if (_.includes([46, 8, 9, 27, 13, 127], keyCode) ||
                        // Allow: Ctrl+A
                        ((keyCode === 65 || keyCode === 97) && evt.ctrlKey === true) ||
                        // Allow: Ctrl+C
                        ((keyCode === 67 || keyCode === 99) && evt.ctrlKey === true) ||
                        // Allow: Ctrl+X
                        ((keyCode === 88 || keyCode === 120) && evt.ctrlKey === true) ||
                        // Allow: home, end, left, right
                        (keyCode >= 35 && keyCode <= 39)) {
                        // let it happen, don't do anything
                        return;
                    }
                    // Ensure that it is a number and stop the keypress
                    if ((evt.shiftKey || (keyCode < 48 || keyCode > 57))) {
                        evt.preventDefault();
                    }
                };

                scope.typeExpand = function(type) {

                    if (type.expanded) {
                        var query = 'name,description,createdDate,modifiedDate,inSolution.id,indirectInSolution.id';

                        var filter = '((id([Resource in application]) = ' +
                            scope.model.selectedApplication.id() +
                            ') or (id([Resource indirectly in application]) = ' +
                            scope.model.selectedApplication.id() +
                            '))';

                        spEntityService.getEntitiesOfType(type.alias, query, { filter: filter })
                            .then(function (results) {
                                type.instances = _.chain(results).uniq().filter(function (e) { return e.name; }).orderBy(['name']).value();
                            });
                    } else {
                        delete type.instances;
                    }
                };

                scope.linkClicked = function (link, type) {

                    if (checkForModifiations()) {
                        return;
                    }

                    switch (type) {
                        case 'core:accessRule':
                            spNavService.navigateToChildState('securityQueries', 'console:securityQueriesPage');
                            break;
                        case 'core:board':
                            spNavService.navigateToSibling('board', link.id());
                            break;
                        case 'core:chart':
                            spNavService.navigateToSibling('chart', link.id());
                            break;
                        case 'core:report':
                            spNavService.navigateToSibling('report', link.id());
                            break;
                        case 'core:workflow':
                            spNavService.navigateToSibling('workflowEdit', link.id());
                            break;
                        case 'console:screen':
                            spNavService.navigateToSibling('screen', link.id());
                            break;
                        default:
                            spNavService.navigateToSibling('viewForm', link.id());
                            break;
                    }
                };

                scope.processDate = function (date) {
                    if (date) {
                        var d = spUtils.parseDateString(date);

                        if (d) {
                            return d.toLocaleString();
                        }
                    }

                    return null;
                };

                scope.isCoreApp = function() {
                    if (scope.model.selectedApplication && scope.model.selectedApplication.id) {
                        var id = scope.model.selectedApplication.id();

                        return id === scope.model.coreSolutionId ||
                            id === scope.model.consoleSolutionId ||
                            id === scope.model.coreDataSolutionId;
                    }

                    return false;
                };

                scope.$watch('model.dependsOnView',
                    function (newVal, oldVal) {

                        if (newVal === oldVal) {
                            return;
                        }
                        generateGraph(scope.model.dependsOnTree, '.dependsOnDiagram', 'marker-end', 'url(#arrowheadDownDependsOn)', 25, -38, 'DependsOn');
                    });

                scope.$watch('model.requiredByView',
                    function (newVal, oldVal) {

                        if (newVal === oldVal) {
                            return;
                        }
                        generateGraph(scope.model.requiredByTree, '.requiredByDiagram', 'marker-start', 'url(#arrowheadUpRequiredBy)', 38, -25, 'RequiredBy');
                    });

                function generateGraph(source, parentClass, marker, markerClass, sourceOffset, targetOffset, idSuffix) {

                    var root = _.cloneDeep(source);

                    var transitioning = false;

                    var i = 0,
                        duration = 750,
                        rectW = 100,
                        rectH = 50;

                    var tree = d3.layout.tree().nodeSize([120, 50]);
                    var diagonal = d3.svg.diagonal()
                        .source(function (d) { return { "x": d.source.x, "y": d.source.y + sourceOffset }; })
                        .target(function (d) { return { "x": d.target.x, "y": d.target.y + targetOffset }; })
                        .projection(function (d) {
                            return [d.x + rectW / 2, d.y + rectH / 2];
                        });

                    var height = 400;
                    var width = 1000;
                    var horizontalOffset = 350;

                    var div = $(parentClass);

                    if (div && div[0]) {
                        width = div[0].clientWidth;
                        horizontalOffset = width / 2 - 60;
                        div.empty();
                    }

                    var zm;
                    var svg = d3.select(parentClass)
                        .append("svg")
                        .attr("width", '100%')
                        .attr("height", height)
                        .call(zm = d3.behavior.zoom().scaleExtent([0.25, 2]).on("zoom", redraw)).append("g")
                        .attr("transform", "translate(" + horizontalOffset + "," + 20 + ")");

                    svg.append("defs").append("marker")
                        .attr("id", "arrowheadDown" + idSuffix)
                        .attr("viewBox", "0 -5 10 10")
                        .attr("refX", 0)
                        .attr("refY", 0)
                        .attr("markerUnits", "strokeWidth")
                        .attr("markerWidth", 6)
                        .attr("markerHeight", 6)
                        .attr("orient", "auto")
                        .append("path")
                        .attr("d", "M0,-5L10,0L0,5");

                    svg.append("defs").append("marker")
                        .attr("id", "arrowheadUp" + idSuffix)
                        .attr("viewBox", "0 -5 10 10")
                        .attr("refX", 10)
                        .attr("refY", 0)
                        .attr("markerUnits", "strokeWidth")
                        .attr("markerWidth", 6)
                        .attr("markerHeight", 6)
                        .attr("orient", "auto")
                        .append("path")
                        .attr("d", "M10,-5L0,0L10,5");

                    //necessary so that zoom knows where to zoom and unzoom from
                    zm.translate([horizontalOffset, 20]);

                    root.x0 = 0;
                    root.y0 = height / 2;

                    function collapse(d) {
                        if (d.children) {
                            d._children = d.children;
                            d._children.forEach(collapse);
                            d.children = null;
                        }
                    }

                    function wrap(text, width) {
                        text.each(function () {
                            var text = d3.select(this),
                                words = text.text().split(/\s+/).reverse(),
                                word,
                                line = [],
                                lineNumber = 0,
                                lineHeight = 1.1, // ems
                                x = text.attr("x"),
                                y = text.attr("y"),
                                dy = 0, //parseFloat(text.attr("dy")),
                                tspan = text.text(null)
                                            .append("tspan")
                                            .attr("x", x)
                                            .attr("y", y)
                                            .attr("dy", dy + "em");

                            word = words.pop();

                            while (word) {
                                line.push(word);
                                tspan.text(line.join(" "));

                                if (tspan.node().getComputedTextLength() > width) {
                                    line.pop();
                                    tspan.text(line.join(" "));
                                    line = [word];
                                    tspan = text.append("tspan")
                                                .attr("x", x)
                                                .attr("y", y)
                                                .attr("dy", ++lineNumber * lineHeight + dy + "em")
                                                .text(word);
                                }

                                word = words.pop();
                            }
                        });
                    }

                    if (root.children) {
                        root.children.forEach(collapse);
                    }

                    update(root);

                    function update(source) {

                        // Compute the new tree layout.
                        var nodes = tree.nodes(root).reverse(),
                            links = tree.links(nodes);

                        // Normalize for fixed-depth.
                        nodes.forEach(function (d) {
                            d.y = d.depth * 100;
                        });

                        // Update the nodes…
                        var node = svg.selectAll("g.node")
                            .data(nodes, function (d) {
                                return d.id || (d.id = ++i);
                            });

                        // Enter any new nodes at the parent's previous position.
                        var nodeEnter = node.enter().append("g")
                            .attr("class", "node")
                            .attr("transform", function () {
                                return "translate(" + source.x0 + "," + source.y0 + ")";
                            })
                            .on("click", click);

                        nodeEnter.append("svg:title")
                            .text(function (d) {
                                var title = 'Name: ' + d.app.name;

                                if (d.app.description) {
                                    title += '\nDescription: ' + d.app.description;
                                }

                                if (d.app.solutionPublisher) {
                                    title += '\nPublisher: ' + d.app.solutionPublisher;
                                }

                                if (d.app.solutionVersionString) {
                                    title += '\nVersion: ' + d.app.solutionVersionString;
                                }
                                return title;
                            });

                        nodeEnter.append("rect")
                            .attr("width", rectW)
                            .attr("height", rectH)
                            .attr("stroke", "black")
                            .attr("stroke-width", 1)
                            .style("fill", function (d) {
                                return d._children ? "lightsteelblue" : "#fff";
                            });

                        nodeEnter.append("text")
                            .attr("x", rectW / 2)
                            .attr("y", 20 )
                            .attr("dy", ".35em")
                            .attr("text-anchor", "middle")
                            .text(function (d) {
                                return d.name;
                            })
                            .call(wrap, 100);

                        // Transition nodes to their new position.
                        var nodeUpdate = node.transition()
                            .duration(duration)
                            .attr("transform", function (d) {
                                return "translate(" + d.x + "," + d.y + ")";
                            })
                            .each("start", function () {
                                transitioning = true;
                            })
                            .each("end", function () {
                                transitioning = false;
                            });

                        nodeUpdate.select("rect")
                            .attr("width", rectW)
                            .attr("height", rectH)
                            .attr("stroke", "black")
                            .attr("stroke-width", 1)
                            .style("fill", function (d) {
                                return d._children ? "lightsteelblue" : "#fff";
                            });

                        nodeUpdate.select("text")
                            .style("fill-opacity", 1);

                        // Transition exiting nodes to the parent's new position.
                        var nodeExit = node.exit().transition()
                            .duration(duration)
                            .attr("transform", function () {
                                return "translate(" + source.x + "," + source.y + ")";
                            })
                            .each("start", function () {
                                transitioning = true;
                            })
                            .each("end", function () {
                                transitioning = false;
                            })
                            .remove();

                        nodeExit.select("rect")
                            .attr("width", rectW)
                            .attr("height", rectH)
                        .attr("stroke", "black")
                            .attr("stroke-width", 1);

                        nodeExit.select("text");

                        // Update the links…
                        var link = svg.selectAll("path.link")
                            .data(links, function (d) {
                                return d.target.id;
                            });

                        // Enter any new links at the parent's previous position.
                        link.enter().insert("path", "g")
                            .style("opacity", 0)
                            .attr("class", "link")
                            //.attr("marker-end", "url(#arrowheadDown)")
                            .attr(marker, markerClass)
                            .attr("x", rectW / 2)
                            .attr("y", rectH / 2)
                            .attr("d", function () {
                                var o = {
                                    x: source.x0,
                                    y: source.y0
                            };
                                return diagonal({
                                    source: o,
                                    target: o
                                });
                            })
                            .append("svg:title")
                            .text(function (d)
                            {
                                return 'Minimum Version: ' + d.target.item.minimumVersion + '\nMaximum Version: ' + d.target.item.maximumVersion + '\nRequired: ' + ( d.target.item.isRequired ? 'True' : 'False' );
                            });

                        // Transition links to their new position.
                        link.transition()
                            .duration(duration)
                            .style("opacity", 1)
                            .attr("d", diagonal);

                        // Transition exiting nodes to the parent's new position.
                        link.exit().transition()
                            .duration(duration)
                            .style("opacity", 0)
                            .attr("d", function () {
                                var o = {
                                    x: source.x,
                                    y: source.y
                                };
                                return diagonal({
                                    source: o,
                                    target: o
                                });
                            })
                            .remove();

                        // Stash the old positions for transition.
                        nodes.forEach(function (d) {
                            d.x0 = d.x;
                            d.y0 = d.y;
                        });
                    }

                    // Toggle children on click.
                    function click(d) {
                        if (transitioning) {
                            return;
                        }

                        if (d.children) {
                            d._children = d.children;
                            d.children = null;
                        } else {
                            d.children = d._children;
                            d._children = null;
                        }
                        update(d);
                    }

                    //Redraw for zoom
                    function redraw() {
                        //console.log("here", d3.event.translate, d3.event.scale);
                        svg.attr("transform",
                            "translate(" + d3.event.translate + ")" + " scale(" + d3.event.scale + ")");
                    }
                }

                function checkForModifiations(dependency) {
                    if (_.some(scope.model.dependsOn,
                        function (dependsOn) {
                            if (dependency && dependency === dependsOn) {
                                return false;
                            } else {
                                return dependsOn.isEditing;
                            }
                            
                    })) {
                        spAlertsService.addAlert('There are unsaved \'Depends on\' settings.', { severity: spAlertsService.sev.Warning, expires: true });
                        return true;
                    }

                    if (_.some(scope.model.requiredBy,
                        function (requiredBy) {
                            if (dependency && dependency === requiredBy) {
                                return false;
                            } else {
                                return requiredBy.isEditing;
                            }
                            
                    })) {
                        spAlertsService.addAlert('There are unsaved \'Required by\' settings.', { severity: spAlertsService.sev.Warning, expires: true });
                        return true;
                    }

                    return false;
                }

                function parseVersionString(str) {
                    if (typeof (str) != 'string') { return false; }
                    var x = str.split('.');
                    // parse from string or default to 0 if can't parse
                    var maj = parseInt(x[0]) || 0;
                    var min = parseInt(x[1]) || 0;
                    var bld = parseInt(x[2]) || 0;
                    var rev = parseInt(x[3]) || 0;
                    return {
                        major: maj,
                        minor: min,
                        build: bld,
                        revision: rev,
                        compareTo: function(version) {
                            if (typeof version === 'string') {
                                version = parseVersionString(version);
                            }

                            if (this.major < version.major) {
                                return -1;
                            } else if (this.major > version.major) {
                                return 1;
                            } else {
                                if (this.minor < version.minor) {
                                    return -1;
                                } else if (this.minor > version.minor) {
                                    return 1;
                                } else {
                                    if (this.build < version.build) {
                                        return -1;
                                    } else if (this.build > version.build) {
                                        return 1;
                                    } else {
                                        if (this.revision < version.revision) {
                                            return -1;
                                        } else if (this.revision > version.revision) {
                                            return 1;
                                        } else {
                                            return 0;
                                        }
                                    }
                                }
                            }
                        }
                    };
                }

                function canSaveDependency(dependency) {
                    if (!dependency || !dependency.selectedApp || !dependency.minimumVersion || !dependency.maximumVersion)
                        return false;

                    // Ensure the max version > min version
                    if (dependency.minimumVersion && dependency.minimumVersion !== 'Any' && dependency.maximumVersion && dependency.maximumVersion !== 'Any') {
                        var minVersion = parseVersionString(dependency.minimumVersion);
                        var maxVersion = parseVersionString(dependency.maximumVersion);

                        if (minVersion.compareTo(maxVersion) > 0) {
                            return false;
                        }
                    }

                    return true;
                }

                function getAllApplications() {
                    var applications = [];

                    _.forEach(scope.model.applications,
                        function (application) {
                            applications.push({
                                id: application.id(),
                                name: application.name,
                                entity: application,
                                availableVersions: [
                                    {
                                        name: 'Current (' + application.solutionVersionString + ')',
                                        value: application.solutionVersionString
                                    },
                                    {
                                        name: 'Any',
                                        value: 'Any'
                                    }
                                ]
                            });
                        });

                    return applications;
                }

                function getAvailableApplications(applications) {
                    var candidates = applications.slice();

                    // Find and remove the current application.
                    var selectedAppIndex = _.findIndex(candidates,
                        function(candidate) {
                            return candidate.entity === scope.model.selectedApplication;
                        });

                    if (selectedAppIndex >= 0) {
                        candidates.splice(selectedAppIndex, 1);
                    }

                    var dependencies = getAllDependencyApplications(scope.model.selectedApplication);

                    _.forEach(dependencies,
                        function (dependency) {
                            var selectedAppIndex = _.findIndex(candidates,
                                function(candidate) {
                                    return candidate.entity.id() === dependency.dependencyApplication.id();
                                });

                            if (selectedAppIndex >= 0) {
                                candidates.splice(selectedAppIndex, 1);
                            }
                        });

                    var dependents = getAllDependentApplications(scope.model.selectedApplication);

                    _.forEach(dependents,
                        function(dependent) {
                            var selectedAppIndex = _.findIndex(candidates,
                                function (candidate) {
                                    return candidate.entity === dependent.dependentApplication;
                                });

                            if (selectedAppIndex >= 0) {
                                candidates.splice(selectedAppIndex, 1);
                            }
                        });

                    return candidates;
                }

                function getAllDependentApplications(dependency) {

                    var dependents = [];

                    if (!dependency) {
                        return dependents;
                    }

                    dependents = getDependents(dependency);

                    return dependents;
                }

                function getDependents(dependency) {
                    var dependents = [];

                    if (!dependency) {
                        return dependents;
                    }

                    var directDependentsCollection = scope.model.map[dependency.id()];

                    if (directDependentsCollection) {

                        var directDependents = directDependentsCollection.dependents;

                        _.forEach(directDependents,
                            function (directDependent) {
                                dependents.push(directDependent);

                                var indirectDependents = getDependents(directDependent.dependentApplication);

                                dependents = _.union(dependents, indirectDependents);
                            });
                    }

                    return dependents;
                }

                function getAllDependencyApplications(dependent) {
                    var dependencies = [];

                    if (!dependent) {
                        return dependencies;
                    }

                    dependencies = getDependencies(dependent);

                    return dependencies;
                }

                function getDependencies(dependent) {
                    var dependencies = [];

                    if (!dependent) {
                        return dependencies;
                    }

                    var directDependenciesCollection = scope.model.map[dependent.id()];

                    if (directDependenciesCollection) {

                        var directDependencies = directDependenciesCollection.dependencies;

                        _.forEach(directDependencies,
                            function (directDependency) {
                                dependencies.push(directDependency);

                                var indirectDependencies = getDependencies(directDependency.dependencyApplication);

                                dependencies = _.union(dependencies, indirectDependencies);
                            });
                    }

                    return dependencies;
                }

                function fetchApplicationTypes() {
                    scope.model.types = [];

                    var accessRules= {
                        name: 'Access Rules',
                        alias: 'core:accessRule'
                    };
                    var board = {
                        name: 'Board',
                        alias: 'core:board'
                    };
                    var chart = {
                        name: 'Chart',
                        alias: 'core:chart'
                    };
                    var definitions = {
                        name: 'Object',
                        alias: 'core:definition'
                    };
                    var editForms = {
                        name: 'Forms',
                        alias: 'console:customEditForm'
                    };
                    var fields = {
                        name: 'Field',
                        alias: 'core:field'
                    };
                    var report = {
                        name: 'Report',
                        alias: 'core:report'
                    };
                    var roles = {
                        name: 'Roles',
                        alias: 'core:role'
                    };
                    var screen = {
                        name: 'Screen',
                        alias: 'console:screen'
                    };
                    var workflow = {
                        name: 'Workflow',
                        alias: 'core:workflow'
                    };

                    scope.model.types.push(accessRules);
                    scope.model.types.push(board);
                    scope.model.types.push(chart);
                    scope.model.types.push(definitions);
                    scope.model.types.push(editForms);
                    scope.model.types.push(fields);
                    scope.model.types.push(report);
                    scope.model.types.push(roles);
                    scope.model.types.push(screen);
                    scope.model.types.push(workflow);
                }

                function buildDependencies(application, destination, destinationTree, getSource, getTarget, getIndirect) {

                    if (!application) {
                        return;
                    }

                    if (!destination) {
                        return;
                    }

                    destinationTree.name = application.name;
                    destinationTree.app = application;

                    destination.length = 0;

                    var encounteredIds = {};

                    var currentPass = [];

                    encounteredIds[application.id()] = true;

                    buildDirectDependencies(application, destination, destinationTree, getSource, getTarget, currentPass);

                    buildIndirectDependencies(application, destination, destinationTree, getSource, getIndirect, encounteredIds, currentPass);
                }

                function buildDirectDependencies(application, destination, destinationTree, getSource, getTarget, currentPass) {
                    _.forEach(scope.model.dependencies, function filterDependencies(dependency) {
                        if (dependency) {
                            if (getTarget) {

                                var target = getTarget(dependency);

                                if (target && target.id() === application.id()) {

                                    if (getSource) {

                                        var source = getSource(dependency);

                                        if (source) {
                                            var dependencyModel = {
                                                id: source.id(),
                                                name: source.name,
                                                _name: source.name,
                                                minimumVersion: dependency.applicationMinimumVersion,
                                                _minimumVersion: dependency.applicationMinimumVersion,
                                                maximumVersion: dependency.applicationMaximumVersion,
                                                _maximumVersion: dependency.applicationMaximumVersion,
                                                isRequired: dependency.applicationIsRequired,
                                                _isRequired: dependency.applicationIsRequired,
                                                isDirect: true,
                                                entity: dependency
                                            };

                                            if (!destinationTree.children) {
                                                destinationTree.children = [];
                                            }

                                            var node = {
                                                name: dependencyModel.name,
                                                app: source,
                                                item: dependencyModel
                                            };

                                            if (_.findIndex(destinationTree.children, function (existingNode) {
                                                return existingNode.item.id === node.item.id;
                                            }) < 0) {
                                                destinationTree.children.push(node);
                                            }

                                            if (_.findIndex(destination, function(dest) {
                                                return dest.id === dependencyModel.id;
                                            }) < 0) {
                                                destination.push(dependencyModel);
                                            }

                                            currentPass.push({
                                                id: source.id(),
                                                node: node
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    });
                }

                function buildIndirectDependencies(application, destination, destinationTree, getSource, getIndirect, encounteredIds, currentPass) {
                    _.forEach(currentPass,
                        function determineIndirectDependencies(val) {
                            var encounteredIdsPass = _.clone(encounteredIds);

                            var id = val.id;

                            if (_.has(encounteredIdsPass, id)) {
                                console.warn('Found circular reference when processing application dependencies.');
                                return;
                            }

                            encounteredIdsPass[id] = true;

                            var indirectContainer = scope.model.map[id];

                            if (indirectContainer) {
                                if (getIndirect) {
                                    var indirectCollection = getIndirect(indirectContainer);

                                    if (indirectCollection && indirectCollection.length) {
                                        _.forEach(indirectCollection,
                                            function processIndirect(indirect) {

                                                if (getSource) {
                                                    var source = getSource(indirect);

                                                    if (source) {
                                                        if (_.has(encounteredIdsPass, source.id())) {
                                                            console.warn('Found circular reference when processing application dependencies.');
                                                            return;
                                                        }

                                                        var indirectModel = {
                                                            id: source.id(),
                                                            name: source.name,
                                                            minimumVersion: indirect.applicationMinimumVersion,
                                                            maximumVersion: indirect.applicationMaximumVersion,
                                                            isRequired: indirect.applicationIsRequired,
                                                            isDirect: false,
                                                            entity: indirect
                                                        };

                                                        if (_.findIndex(destination, function (dest) {
                                                            return dest.id === indirectModel.id;
                                                        }) < 0) {
                                                            destination.push(indirectModel);
                                                        }

                                                        if (!val.node.children) {
                                                            val.node.children = [];
                                                        }

                                                        var node = {
                                                            name: indirectModel.name,
                                                            app: source,
                                                            item: indirectModel
                                                        };

                                                        if (_.findIndex(val.node.children, function (existingNode) {
                                                            return existingNode.item.id === node.item.id;
                                                        }) < 0) {
                                                            val.node.children.push(node);
                                                        }

                                                        buildIndirectDependencies(application,
                                                            destination,
                                                            destinationTree,
                                                            getSource,
                                                            getIndirect,
                                                            encounteredIdsPass,
                                                            [{ id: source.id(), node: node }]);
                                                    }
                                                }

                                            });
                                    }
                                }
                            }



                        });
                }

                function extractProperties(application) {
                    scope.model.primaryProperties = [];
                    scope.model.secondaryProperties = [];

                    var releaseDate = new Date(application.solutionReleaseDate);

                    if (_.isDate(releaseDate)) {
                        releaseDate = $.datepicker.formatDate("DD, d MM, yy", releaseDate);
                    } else {
                        releaseDate = "No release date";
                    }

                    scope.model.primaryProperties.push({ name: "Name", value: application.name });
                    scope.model.primaryProperties.push({ name: "Description", value: application.description });
                    scope.model.primaryProperties.push({ isBreak: true });
                    scope.model.primaryProperties.push({ name: "Publisher", value: application.solutionPublisher });
                    scope.model.primaryProperties.push({ name: "Publisher Url", value: application.solutionPublisherUrl, isLink: true });
                    scope.model.primaryProperties.push({ name: "Release Date", value: releaseDate });
                    scope.model.primaryProperties.push({ name: "Version", value: application.solutionVersionString });

                    scope.model.secondaryProperties.push({ name: "Hidden On Desktop", value: application.hideOnDesktop ? "Yes" : "No" });
                    scope.model.secondaryProperties.push({ name: "Hidden On Mobile", value: application.hideOnMobile ? "Yes" : "No" });
                    scope.model.secondaryProperties.push({ name: "Hidden On Tablet", value: application.hideOnTablet ? "Yes" : "No" });
                }

                function updateDependencyMap(dependency) {

                    var map = scope.model.map[dependency.dependentApplication.id()];

                    if (!map) {
                        map = {
                            dependents: [],
                            dependencies: []
                        };

                        scope.model.map[dependency.dependentApplication.id()] = map;
                    }

                    map.dependencies = _.union(map.dependencies, [dependency]);
                                
                    map = scope.model.map[dependency.dependencyApplication.id()];

                    if (!map) {
                        map = {
                            dependents: [],
                            dependencies: []
                        };

                        scope.model.map[dependency.dependencyApplication.id()] = map;
                    }

                    map.dependents = _.union(map.dependents, [dependency]);
                }

                function initialize() {

                    var batch = new spEntityService.BatchRequest();

                    var r1 = spEntityService.getEntitiesOfType('core:solution', 'name,description,solutionPublisher,solutionPublisherUrl,solutionReleaseDate,solutionVersionString,hideOnDesktop,hideOnMobile,hideOnTablet', { batch: batch });

                    var r2 = spEntityService.getEntitiesOfType('core:applicationDependency', 'applicationIsRequired,applicationMinimumVersion,applicationMaximumVersion,dependentApplication.name,dependencyApplication.name', { batch: batch });

                    var promise = $q.all({
                        solutions: r1,
                        dependencies: r2
                    });

                    batch.runBatch();

                    var navItem = spNavService.getCurrentItem();

                    navItem.isDirty = function () {
                        if (_.some(scope.model.dependsOn,
                        function (dependsOn) {
                            return dependsOn.isEditing;
                        })) {
                            return true;
                        }

                        if (_.some(scope.model.requiredBy,
                            function (requiredBy) {
                                return requiredBy.isEditing;
                        })) {
                            return true;
                        }

                        return false;
                    };

                    return promise.then(function (results) {
                        // Sort the solutions into a specific order
                        // Core, Console, CoreData, <remaining in sorted order>
                        var solutions = results.solutions.slice();
                        var sortedSolutions = [];

                        var coreSolution = _.find(solutions,
                            function(solution) {
                                return solution.name && solution.name === 'ReadiNow Core';
                            });

                        if (coreSolution) {
                            sortedSolutions.push(coreSolution);
                            _.remove(solutions, coreSolution);

                            scope.model.coreSolutionId = coreSolution.id();
                        }

                        var consoleSolution = _.find(solutions,
                            function (solution) {
                                return solution.name && solution.name === 'ReadiNow Console';
                            });

                        if (consoleSolution) {
                            sortedSolutions.push(consoleSolution);
                            _.remove(solutions, consoleSolution);

                            scope.model.consoleSolutionId = consoleSolution.id();
                        }

                        var coreDataSolution = _.find(solutions,
                            function (solution) {
                                return solution.name && solution.name === 'ReadiNow Core Data';
                            });

                        if (coreDataSolution) {
                            sortedSolutions.push(coreDataSolution);
                            _.remove(solutions, coreDataSolution);

                            scope.model.coreDataSolutionId = coreDataSolution.id();
                        }

                        solutions = _.sortBy(solutions,
                            function(solution) {
                                return solution.name;
                            });

                        _.forEach(solutions,
                            function(solution) {
                                sortedSolutions.push(solution);
                            });

                        scope.model.applications = sortedSolutions;
                        scope.model.dependencies = results.dependencies;

                        scope.model.map = {};

                        _.forEach(results.dependencies, updateDependencyMap);

                        if (scope.model.applications && scope.model.applications.length) {
                            scope.selectApplication(scope.model.applications[0]);
                        }
                    });
                }

                initialize();
                
            }
        };
    });
}());