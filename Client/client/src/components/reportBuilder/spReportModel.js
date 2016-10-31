// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spReportEntityQueryManager, spReportEntity */

angular.module('mod.common.ui.spReportModel', [
    'mod.ui.spTreeviewManager',
    'sp.app.settings'
])
   /**
    * Module for manipulating a report entity model.
    * Note: this is actually used by form builder and *not* by report builder. 
    *
    * @module spReportModel
    */
    .service('spReportModel', function (spTreeviewManager, spAppSettings) {
        'use strict';

        var exports = {};
        var svc = exports;

        // creates a default report for the type entity
        exports.createDefaultReport = function (typeEntity, formControlsFlat) {

            // Create base report entity
            var report = svc.createReport({
                type: typeEntity.idP,
                name: typeEntity.name + ' Report'
            });

            // Create report wrapper
            var reportQuery = new spReportEntity.Query(report);
            var rootNode = reportQuery.getRootNode();

            //set report's application same as typeEntity's application by bug 23716
            report.inSolution = typeEntity.inSolution;
            // Get type wrapper
            var resourceType = new spResource.Type(typeEntity);
            var resourceFields = resourceType.getFields();
            var resourceRels = resourceType.getAllRelationships();

            // Find the ID of the name field, because reports unfortunately wants it - but fortunately we have it (note: this needs to be revised if we support field overrides)
            var nameFieldId = _.find(_.invokeMap(resourceFields, 'getEntity'), function (f) { return f.nsAlias === 'core:name'; }).idP;            

            var cols = 0;
            var maxCols = 8;
            // Visit form controls
            _.forEach(formControlsFlat, function (formControl) {
                if (cols === maxCols)
                    return;

                var colInfo = null; 
                var node = null;

                // control is field
                var field = formControl.fieldToRender;
                if (field) {
                    var resField = _.find(resourceFields, function(f) { return f.getEntity().idP === field.idP; });
                    if (resField) {
                        colInfo = spTreeviewManager.getFieldJson(resField, resourceType, -1);
                        node = rootNode;
                    }
                } 

                // control is relationship
                var relationship = formControl.relationshipToRender;                
                if (relationship && nameFieldId) {
                    var isImage = formControl.type.nsAlias === 'console:imageRelationshipRenderControl';
                    var isMultiChoice = formControl.type.nsAlias === 'console:multiChoiceRelationshipRenderControl';
                    var isReversed = formControl.isReversed || false;  // undefined for choice fields
                    var card = relationship.cardinality.nsAlias;
                    var isLookupOrChoice = !isImage && (spResource.Relationship.toOne(card, isReversed) || isMultiChoice);  //the formcontrol is Loopup or choice when it is not image relationship and it's caardinality is to One or multi choice relationship
                    if (isLookupOrChoice) {
                        var resRel = _.find(resourceRels, function (r) { return r.getEntity().idP === relationship.idP && r.isReverse() === isReversed; });
                        if (resRel) {
                            colInfo = spTreeviewManager.getLookupJson(resRel, resourceType, -1, nameFieldId);
                            node = spReportEntityQueryManager.createRelatedEntity(isReversed ? relationship.fromType : relationship.toType, relationship, isReversed ? 'Reverse' : '');
                            rootNode.addRelatedReportNode(node);
                        }
                    }
                }

                if (colInfo) {
                    spReportEntityQueryManager.addColumnToReport(reportQuery, colInfo, node, -1);
                    spReportEntityQueryManager.addCondition(reportQuery, colInfo, node, -1);
                    cols++;
                }
            });

            if (cols === 0) {
                //if user didn't add any field in form. only add Name field as colInfo
                var nameField = _.find(resourceFields, function (f) { return f.getEntity().name === 'Name'; });                
                if (nameField) {
                    var colInfo = null;
                    var node = null;

                    colInfo = spTreeviewManager.getFieldJson(nameField, resourceType, -1);
                    node = rootNode;

                    spReportEntityQueryManager.addColumnToReport(reportQuery, colInfo, node, -1);
                    spReportEntityQueryManager.addCondition(reportQuery, colInfo, node, -1);
                    cols++;
                }
            }
            return report;
        };

        // creates a new report entity model structure
        // returns a new report entity
        exports.createReport = function (options) {
            // options:
            //   type: id/alias   (mandatory - type of resource being reported)
            //   name: string     (otional)

            if (!options)
                throw new Error("options not defined");
            if (!options.type)
                throw new Error("options.type not defined");

            var opts = _.defaults(options, {
                name: "New Report",
                inSolution: null
            });

            var json = {
                typeId: 'report',
                name: jsonString(opts.name),
                description: jsonString(),

                // Structure
                reportUsesDefinition: jsonLookup(opts.type),
                rootNode: {
                    typeId: 'resourceReportNode',
                    resourceReportNodeType: jsonLookup(opts.type),
                    exactType: false,
                    targetMustExist: false,
                    relatedReportNodes: [],
                },
                reportOrderBys: [],
                reportColumns: [],
                hasConditions: [],

                // Appearance
                hideActionBar: false,
                hideReportHeader: false,
                rollupOptionLabels: true,
                reportStyle: jsonLookup(),
                resourceViewerConsoleForm: jsonLookup(),

                // Presence
                inSolution: jsonLookup(opts.inSolution),
                'console:navigationElementIcon': jsonLookup(),
                hideOnDesktop: false,
                hideOnTablet: true,
                hideOnMobile: true,
                isPrivatelyOwned: !spAppSettings.publicByDefault
            };

            var report = spEntity.fromJSON(json);

            // attach hidden id column
            var expr = spEntity.fromJSON({
                typeId: 'idExpression',
                sourceNode: jsonLookup(report.rootNode)
            });
            svc.addColumn(report, { name: '_id', expr: expr, columnIsHidden: true });

            return report;
        };


        // creates and adds a new report column condition
        exports.addColumn = function (report, options) {
            var opts = _.defaults(options, {
                expr: null,    // mandatory: id or alias
                name: null,
                columnIsHidden: false
            });

            var column = spEntity.fromJSON({
                typeId: 'reportColumn',
                name: jsonString(opts.name),
                columnExpression: jsonLookup(opts.expr),
                columnIsHidden: opts.columnIsHidden,
                columnDisplayOrder: report.reportColumns.length
            });
            report.reportColumns.add(column);
            return column;
        };

        return exports;
    });