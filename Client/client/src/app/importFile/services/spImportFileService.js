// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular*/

// This service is responsible for client-side business logic of the import file configuration wizard.
// It is explicitly not responsible for calling various web services.

(function () {
    'use strict';

    angular
        .module('mod.app.importFile.services.spImportFileService', [
            'ng',
            'sp.common.fileUpload',
            'mod.common.spEntityService',
            'mod.app.importFile.services.spImportFileWebService',
            'mod.app.connector.spResourceEndpointService',
            'mod.common.alerts',
            'mod.common.ui.spDialogService',
            'sp.common.spDialog'
        ])
        .service('spImportFileService', ['$q', '$timeout', '$window', 'spImportFileWebService', 'spUploadManager', 'spEntityService', 'spResourceEndpointService', 'spAlertsService', 'spDialogService', 'spDialog', ImportFileService]);

    function ImportFileService($q, $timeout, $window, spImportFileWebService, spUploadManager, spEntityService, spResourceEndpointService, spAlertsService, spDialogService, spDialog) {

        // Create a new empty model that will contain everything being tracked
        function createEmptyModel() {

            // #27447 Import from Excel: uploading a file does not work in MS Edge.  Because input "accept" attribute on file input doesn't work in MS Edge now
            // To make the upload function working in MS Edge, disable the spreetSheet filter now. Remove if MS fixes the Edge bug.
            var isEdgeBrowser = $window.navigator.userAgent.indexOf('Edge') > -1;
            var fileFilter = isEdgeBrowser ? '' : spUtils.spreadsheetFileTypeFilter;

            var uploadSession = spUploadManager.createUploadSession();
            var model = {
                importConfig: null, // the root importConfig entity
                importConfigId: 0,
                configName: null,
                initialBookmark: null,
                upload: {
                    uploadSession: uploadSession,
                    fileFilter: fileFilter,
                    message: '',
                    busyIndicator: {
                        type: 'progressBar',
                        text: 'Uploading...',
                        placement: 'window',
                        isBusy: false,
                        percent: 0
                    },
                    onFileUploadComplete: function(fileName, fileUploadId) {
                        return onFileUploadComplete(model, fileName, fileUploadId); // promise
                    },
                    documentMessage: ''
                },
                file: {
                    fileName: null,
                    fileId: null,
                    fileFormat: null
                },
                nav: {
                    curPage: 1,
                    uploadPage: 1,
                    objectPage: 2,
                    mappingPage: 3,
                    optionsPage: 4,
                    importPage: 5
                },
                typePickerOptions: {
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: 'console:importableTypesReport',
                    multiSelect: false
                },
                resource: {
                    mergeOptionText: '',
                    mergeOptionDisabled: false,
                    mergeExistingData: true
                },
                mapping: {
                    busyIndicator: {
                        type: 'spinner',
                        placement: 'element',
                        isBusy: false
                    }
                },
                options: {
                    suppressWorkflows: false,
                    testRun: false
                },
                mappingReady: false,
                mappingTable: [],
                noHeadingRow: false,
                headingRow: 1,
                dataRow: 2,
                lastRow: null,
                sheets: [],
                sheetName: null,
                sampleDataTable: null,
                showSheetSettings: false,
                isDocumentReady: false, // until spreadsheetInfo is available
                spreadsheetInfo: null, // holds data returned about spreadsheet
                typeLoaded: null, // ID of the last type loaded
                importRunning: false,
                handle: null  // handle to ensure responses come to valid requests
            };
            return model;
        }

        //
        // MODEL MANAGEMENT
        //

        // Create a model based on a (possibly zero) ID
        function createModel(importConfigId) {
            var model = createEmptyModel();
            var handle = getHandle(model);

            model.importConfigId = importConfigId;

            var promise;
            if (importConfigId === 0) {
                createImportConfig(model);
                promise = $q.when();
            } else {
                promise = loadImportConfig(model, importConfigId);
            }
            return promise.then(function () {
                if (!checkHandle(model, handle)) return null;

                // set type for UI
                var type = sp.result(model, 'importConfig.importConfigMapping.mappedType');
                model.typePickerOptions.selectedEntities = type ? [type] : [];
                // set sheet for UI
                var sheet = sp.result(model, 'importConfig.importConfigMapping.mappingSourceReference');
                if (sheet) {
                    model.sheetName = sheet;
                    model.sheets = [ { sheetId: sheet, sheetName: sheet } ];
                }
                return model;
            });
        }

        // Check the entity structure of an import config
        function verifyStructure(importConfig) {
            if (!importConfig.importConfigMapping) {
                importConfig.importConfigMapping = spEntity.fromJSON({
                    typeId: 'apiResourceMapping',
                    mappedType: jsonLookup(),
                    importMergeExisting: true,
                    resourceMemberMappings: [],
                    importHeadingRow: jsonInt(),
                    importDataRow: jsonInt(),
                    importLastDataRow: jsonInt(),
                    mappingSourceReference: jsonString() // sheet name
                });
            }
        }

        // Create a new empty import config entity
        function createImportConfig(model) {
            var json = {
                name: jsonString(),
                typeId: 'importConfig',
                importConfigMapping: jsonLookup(),
                importFileType: jsonLookup()
            };

            var importConfig = spEntity.fromJSON(json);
            verifyStructure(importConfig);
            model.importConfig = importConfig;
        }

        // Load an import-config from the server into the model
        function loadImportConfig(model, importConfigId) {
            var handle = getHandle(model);
            return spImportFileWebService.loadImportConfig(importConfigId).then(function (importConfig) {
                if (!checkHandle(model, handle)) return;

                verifyStructure(importConfig);
                var mapping = importConfig.importConfigMapping;
                model.importConfig = importConfig;
                model.selectedResourceName = sp.result(mapping, 'mappedType.name');
                model.headingRow = mapping.importHeadingRow;
                model.noHeadingRow = !model.headingRow;
                model.dataRow = mapping.importDataRow || 1;
                model.lastRow = mapping.importLastDataRow || null;
                model.sheetName = mapping.mappingSourceReference;
                model.initialBookmark = importConfig.graph.history.addBookmark('Import File');
                model.showSheetSettings = true;
                model.resource.mergeExistingData = importConfig.importConfigMapping.importMergeExisting;
                model.configName = importConfig.name;
                model.options.suppressWorkflows = sp.result(importConfig, 'importConfigMapping.mappingSuppressWorkflows');
                var isChoice = sp.result(mapping, 'mappedType.type.nsAlias') === 'core:enumType';
                model.typePickerOptions.pickerReportId = isChoice ? 'console:enumReport' : 'console:importableTypesReport';
                var fileTypeAlias = sp.result(importConfig, 'importFileType.nsAlias');
                if (fileTypeAlias) {
                    model.file.fileFormat = fileTypeAlias.substring('core:importFileType'.length);
                }
            }, handleErrorResponse);
        }

        function setValuesIntoImportConfig(model) {
            var importConfig = model.importConfig;
            if (!importConfig)
                return;

            var mapping = importConfig.importConfigMapping;
            mapping.importHeadingRow = model.noHeadingChanged ? 0 : model.headingRow;
            mapping.importDataRow = model.dataRow;
            mapping.importLastDataRow = model.lastRow;
            mapping.mappingSourceReference = model.sheetName;
            mapping.importMergeExisting = model.resource.mergeExistingData;
            mapping.mappingSuppressWorkflows = model.options.suppressWorkflows;
            importConfig.name = model.configName;

            var fileTypeAlias = 'core:importFileType' + model.file.fileFormat;
            if (sp.result(model.importConfig.importFileType, 'nsAlias') !== fileTypeAlias) {
                model.fileTypeAlias = fileTypeAlias;
                model.importConfig.importFileType = spEntity.fromId(fileTypeAlias);
            }
        }

        // Save an import-config to the server
        function saveImportConfig(model) {
            var importConfig = model.importConfig;
            if (!importConfig)
                return $q.when();

            var handle = getHandle(model);

            setValuesIntoImportConfig(model);

            return spEntityService.putEntity(importConfig).then(function (importConfigId) {
                if (!checkHandle(model, handle)) return;

                // Do we have any reason to reload the importConfig, as it is probably unused from this point ??
                // Unsure, so set it to null for now, so at least we'll notice
                model.importConfig = null;
                model.importConfigId = importConfigId;
                model.initialBookmark = null;

                spAlertsService.addAlert("Configuration saved.",
                    { expires: 3, severity: spAlertsService.sev.Success });
            });
        }

        function hasUnsavedChanges(model, skipUnsavedChangeCheck) {
            if (!model.importConfig || skipUnsavedChangeCheck)
                return false;
            setValuesIntoImportConfig(model);           
            return model.importConfig.graph.history.changedSinceBookmark(model.initialBookmark);
        }

        function isPageValid(model) {
            var page = model.nav.curPage;

            if (page === model.nav.uploadPage) {
                return model.isDocumentReady;
            }
            if (page === model.nav.objectPage) {
                return !!getTypeId(model);
            }
            if (page === model.nav.mappingPage) {
                return model.importConfig.importConfigMapping.resourceMemberMappings.length > 0 && allRequiredMembersAreMapped(model);
            }
            if (page === model.nav.optionsPage) {
                var name = model.configName;
                return name && name.indexOf('<') === -1 && name.indexOf('>') === -1; // meh
            }
            if (page === model.nav.importPage) {
                return model.importRunning; // valid to cancel?
            }
            return true;
        }

        function allRequiredMembersAreMapped(model) {
            var allRequiredAreMapped = true;
            _.forEach(model.targetMembers, function(target) {
                if (!target.memberInfo.isRequired())
                    return;
                var mapping = findMappingForTarget(model, target);
                if (!mapping)
                    allRequiredAreMapped = false;
            });
            return allRequiredAreMapped;
        }

        function getTypeId(model) {
            return sp.result(model, 'importConfig.importConfigMapping.mappedType.idP');
        }


        function handleErrorResponse(errorMessage) {
            spAlertsService.addAlert('A problem occurred: ' + errorMessage, { expires: false, severity: spAlertsService.sev.Error });
        }

        //
        // Page 1 - UPLOAD AND SAMPLE
        //

        // Called when the upload is complete
        function onFileUploadComplete(model, fileName, fileUploadId) {
            var handle = getHandle(model);
            model.upload.documentMessage = 'Spreadsheet successfully uploaded.';
            model.file.fileName = fileName;
            model.file.fileId = fileUploadId;

            var newFileFormat = getFileFormat(fileName);
            if (model.file.fileFormat === 'Tab' && newFileFormat === 'Csv') {
                newFileFormat = 'Tab';
            }
            model.file.fileFormat = newFileFormat;
            model.sampleDataTable = null;

            // Get spreadsheet info
            return spImportFileWebService.getSpreadsheetInfo(model.file.fileId, model.file.fileFormat, model.file.fileName, model.sheetName).then(function (spreadsheetInfo) {
                if (!checkHandle(model, handle)) return;

                model.spreadsheetInfo = spreadsheetInfo;
                model.sheets = spreadsheetInfo.sheetCollection;
                if (model.sheets && model.sheets.length > 0) {
                    model.sheetName = model.sheetName || spreadsheetInfo.initialSheetId;
                }
                model.sampleDataTable = spreadsheetInfo.initialSampleTable;
                checkSampleData(model.sampleDataTable);
                model.isDocumentReady = true;
                model.showSheetSettings = true;

            }, handleErrorResponse);
        }

        // Load a sample sheet
        function loadSheetSample(model) {
            var handle = getHandle(model);
            model.sampleDataTable = null;
            var headingRow = model.noHeadingRow ? 0 : model.headingRow;
            spImportFileWebService.getSampleTable(model.file.fileId, headingRow, model.dataRow, model.lastRow, model.file.fileFormat, model.sheetName).then(function (sample) {
                if (!checkHandle(model, handle)) return;

                if (sample) {
                    model.sampleDataTable = sample;
                    checkSampleData(sample);
                }
                model.sheetInfoChanged = false;
            }, handleErrorResponse);
        }

        function checkSampleData(sampleTable) {
            if (sampleTable.rows.length === 0 || sampleTable.columns.length === 0) {
                spAlertsService.addAlert("No records were found in the uploaded file.",
                    { expires: 5, severity: spAlertsService.sev.Error });
            }
        }

        // Get the file format from the file name
        function getFileFormat(fileName) {
            if (fileName) {
                var lastDot = fileName.lastIndexOf('.');
                var ext = fileName.substring(lastDot + 1).toLowerCase();
                if (ext === 'csv' || ext === 'txt')
                    return 'Csv';
                else if (ext === 'xlsx') {
                    return 'Excel';
                } else {
                    return null;
                }
            }
            return null;
        }

        // Ensure that row numbers all make sense
        function noHeadingChanged(model) {
            if (model.noHeadingRow) {
                if (model.headingRow === 1 && model.dataRow === 2) {
                    model.dataRow = 1;
                }
                model.headingRow = 0;
            } else {
                if (model.headingRow === 0) {
                    model.headingRow = 1;
                    if (model.dataRow === 1) {
                        model.dataRow = 2;
                    }
                }
            }
            return loadSheetSample(model);
        }

        function headingRowChanged(model) {
            if (model.dataRow <= model.headingRow)
                model.dataRow = model.headingRow + 1;
            return loadSheetSample(model);
        }

        function dataRowChanged(model) {
            if (model.dataRow <= model.headingRow)
                model.headingRow = model.dataRow - 1;
            if (model.headingRow <= 0)
                model.noHeadingRow = true;
            return loadSheetSample(model);
        }

        function sheetChanged(model) {
            return loadSheetSample(model);
        }

        function separatorChanged(model) {
            return loadSheetSample(model);
        }

        function lastRowChanged(model) {
            return loadSheetSample(model);
        }

        //
        // Page 2 - OBJECT
        //

        function setSelectedType(model, typeEntity) {
            if (!typeEntity)
                return $q.when();

            var updateName = model.configName === suggestName(model) || !model.configName;

            if (getTypeId(model) !== typeEntity.idP) {
                model.importConfig.importConfigMapping.mappedType = spEntity.fromId(typeEntity.idP);
                model.importConfig.importConfigMapping.resourceMemberMappings.clear();
            }
            model.selectedResourceName = typeEntity.name;

            if (updateName) {
                model.configName = suggestName(model);
            }

            return prepareMappings(model, typeEntity.idP);
        }

        function suggestName(model) {
            return 'Import ' + model.selectedResourceName + ' from ' + model.file.fileFormat;
        }

        //
        // Page 3 - MAPPINGS
        //

        // Called prior to showing mappings, so ensure everything is loaded
        function prepareMappings(model) {
            var handle = resetHandle(model);
            var typeId = getTypeId(model);
            return loadMembersForType(model, typeId).then(function () {
                if (!checkHandle(model, handle)) return;

                // Create defaults mappings
                if (model.importConfig.getDataState() === spEntity.DataStateEnum.Create && model.importConfig.importConfigMapping.resourceMemberMappings.length === 0) {
                    createDefaultMappings(model);
                }
                // Build mapping object
                model.mappingTable = getMappingRows(model);
            });
        }

        function loadMembersForType(model, typeId) {
            if (!typeId || typeId === model.typeLoaded)
                return $q.when();

            // Create a request to load schema info
            var opts = {
                fields: true,
                relationships: true,
                fieldGroups: true,
                ignoreInheritance: false,
                ignoreOverrides: false,
                derivedTypes: false,
                resourceKeys: true,
                scriptInfo: true
            };
            var rq = spResource.makeTypeRequest(opts);
            var handle = getHandle(model);

            return spEntityService.getEntity(typeId, rq, { hint: 'schema for import' }).then(function (typeEntity) {
                if (!checkHandle(model, handle)) return;

                var type = new spResource.Type(typeEntity);
                model.mappingReady = true;
                model.typeLoaded = typeId;

                var resource = model.resource;
                resource.name = type.getName();
                resource.description = type.getDescription();
                resource.fieldGroups = type.getFieldGroups();

                var resourceKeys = type.getAllResourceKeys();
                if (resourceKeys.length) {
                    resource.mergeOptionText = 'Merge with existing data';
                    if (_.some(resourceKeys, function (k) { return k.mergeDuplicates; })) {
                        resource.mergeOptionDisabled = true;
                    } else {
                        resource.mergeOptionDisabled = false;
                    }
                } else {
                    resource.mergeOptionText = 'Merge with existing data (requires a resource key)';
                    resource.mergeOptionDisabled = true;
                }

                model.targetMembers = getAvailableTargetMembers(type);
                //$scope.busyIndicator.isBusy = false;
            },
                function () {
                    model.resource.name = '';
                    model.resource.description = '';
                    model.resource.fieldGroups = [];
                    //$scope.getCurrentPage().errorMessage = 'Can\'t load definition.';
                    //$scope.getCurrentPage().isValid = false;
                    //$scope.busyIndicator.isBusy = false;
                }
            );
        }

        // Format a field name if mandatory
        function formatFieldName(f) {
            var name = f.getName();
            if (f.isRequired()) {
                name += '*';
            }
            return name;
        }

        // Get list of fields that are available as import targets
        function getAvailableTargetMembers(type) {
            if (!type)
                return [];
            var fieldGroups = type.getFieldGroups();
            var targetMembers = [];
            _.forEach(fieldGroups, function (fg) {
                var members = fg.getAllMembers({ hideNonWritable: true });
                _.forEach(members, function (mi) {
                    targetMembers.push({
                        fieldGroup: fg.getName(),
                        memberInfo: mi,
                        memberName: formatFieldName(mi),                        
                        memberTypeDesc: mi.memberTypeDesc()
                    });
                });
            });
            return targetMembers;
        }

        // Build up the data for the mapping table
        function getMappingRows(model) {
            // Get mapping entities, keyed by colId
            var mappingsByColId = getMappingsByColId(model);
            var availableCols = getAvailableSourceColumns(model);

            var res = _.map(availableCols, function (col) {
                var mapping = mappingsByColId[col.colId] || null;
                var curTarget = findTargetForMapping(model, mapping);
                var memberTypeDesc = sp.result(curTarget, 'memberTypeDesc');
                var lookupField = sp.result(mapping, 'mappedRelationshipLookupField.name') || 'Name';
                var isRel = memberTypeDesc === 'Lookup' || memberTypeDesc === 'Relationship';
                var row = {
                    colId: col.colId,
                    spreadsheetColumnName: col.colName,
                    targetMember: curTarget, // as returned from getAvailableTargets
                    targetDesc: memberTypeDesc ? memberTypeDesc + (isRel ? ' using ' + lookupField : '') : '',
                    hasOptions: isRel,
                    sample1: col.sample1,
                    sample2: col.sample2,
                    mapping: mappingsByColId[col.colId] || null
                };
                row.handleChange = function handleChange() {
                    handleMappingChange(model, row);
                };
                row.showOptions = function showOptions() {
                    showMappingOptions(model, row);
                };
                return row;
            });
            return res;
        }

        // Create new mapping instances when column names match 
        function createDefaultMappings(model) {

            var mappings = sp.result(model, 'importConfig.importConfigMapping.resourceMemberMappings');
            var availableCols = getAvailableSourceColumns(model);

            var availableMembersByName = _.reduce(model.targetMembers, function (acc, targetMember) {
                var name = targetMember.memberInfo.getName().toLowerCase();
                var lname = name.toLowerCase();
                acc[lname] = targetMember;  //colId is stored in the mapping name
                return acc;
            }, {});           

            var visitedNames = {};
            _.forEach(availableCols, function (col) {
                var colHeading = col.colHeading;
                if (colHeading === model.selectedResourceName)
                    colHeading = 'Name'; // treat object name equivalent to name

                if (visitedNames[colHeading])
                    return;
                visitedNames[colHeading] = true;

                var lheading = colHeading.toLowerCase();
                var matchedMember = availableMembersByName[lheading];
                if (!matchedMember)
                    return;

                var mappingEntity = spResourceEndpointService.createMemberMappingEntity(matchedMember.memberInfo, col.colId);
                mappings.add(mappingEntity);
            });
        }

        // Return a 'dictionary' of mapping entities, keyed by the column ID they map to.
        function getMappingsByColId(model) {
            var mappingsByColId = {};
            var mappings = sp.result(model, 'importConfig.importConfigMapping.resourceMemberMappings');
            if (mappings) {
                mappingsByColId = _.reduce(mappings, function (acc, mapping) {
                    acc[mapping.name] = mapping;  //colId is stored in the mapping name
                    return acc;
                }, {});            
            }
            return mappingsByColId;
        }

        // Find the member target instance that corresponds to a particular mapping.
        function findTargetForMapping(model, mapping) {
            if (!mapping) return null;
            var targetMembers = model.targetMembers;
            var target = _.find(targetMembers, function (member) {
                return spResourceEndpointService.mappingRefersToMember(mapping, member.memberInfo);
            });
            return target;
        }

        // Find the mapping that corresponds to a particular target instance.
        function findMappingForTarget(model, target) {
            if (!target) return null;
            var mappings = model.importConfig.importConfigMapping.resourceMemberMappings;
            var mapping = _.find(mappings, function (mapping) {
                return spResourceEndpointService.mappingRefersToMember(mapping, target.memberInfo);
            });
            return mapping;
        }

        // Callback received after a mapping selection is changed
        function handleMappingChange(model, mappingRow) {
            var oldMapping = mappingRow.mapping;
            var newTarget = mappingRow.targetMember; // will contain the newly selected target, or null
            var mappings = sp.result(model, 'importConfig.importConfigMapping.resourceMemberMappings');
            var colId = mappingRow.colId;
            if (oldMapping) {
                if (newTarget && spResourceEndpointService.mappingRefersToMember(oldMapping, newTarget.memberInfo) && oldMapping.name === colId)
                    return; // nothing to do
                mappings.deleteEntity(oldMapping);
            }
            if (newTarget) {                
                // check that its not already used
                var alreadyMapped = _.some(mappings, function(mapping) {
                    return spResourceEndpointService.mappingRefersToMember(mapping, newTarget.memberInfo);
                });
                if (alreadyMapped) {
                    spDialog.messageBox('Mapping Error', 'The field is already mapped');
                } else {
                    var mappingEntity = spResourceEndpointService.createMemberMappingEntity(newTarget.memberInfo, colId);
                    mappings.add(mappingEntity);
                }
            }

            // refresh mapping table
            model.mappingTable = getMappingRows(model);
        }

        // Show the options dialog for a mapping row 
        function showMappingOptions(model, mappingRow) {
            var defaults = {
                templateUrl: 'importFile/controllers/spMemberMappingOptions.tpl.html',
                controller: 'spMemberMappingOptionsController',
                resolve: {
                    mappingRow: function () { return mappingRow; }
                }
            };
            return spDialogService.showDialog(defaults).then(function () {
                // refresh mapping table
                model.mappingTable = getMappingRows(model);
            });
        }

        // Get list of columns that are available as data sources
        function getAvailableSourceColumns(model) {
            var sample = model.sampleDataTable;
            if (!sample)
                return [];
            var res = _.map(sample.columns, function(col, index) {
                return {
                    colId: col.colId,
                    colHeading: col.colName,
                    colName: '(' + col.colId + ') ' + col.colName,
                    sample1: getSampleValue(sample, 0, index),
                    sample2: getSampleValue(sample, 1, index)
                };
            });
            return res;
        }

        // Return a sample value from the sample table
        function getSampleValue(sampleTable, rowNum, columnNum) {
            if (sampleTable.rows[rowNum]) {
                var row = sampleTable.rows[rowNum];
                return row.vals[columnNum];
            }
            return '';
        }



        //
        // Page 4 - OPTIONS
        //

        function reloadModel(model) {

            return createModel(model.importConfigId)
                .then(function (newModel) {
                    // load the file that the old model was using
                    return onFileUploadComplete(newModel, model.file.fileName, model.file.fileId).then(function () { return newModel; });
                })
                .then(function (newModel) {
                    // clear the old model, then copy the new model into it (so we preserve object reference)
                    var oldOptions = model.options;
                    var oldUpload = model.upload;
                    _.forEach(_.keys(model), function (member) { delete model[member]; });
                    _.forEach(_.keys(newModel), function (member) { model[member] = newModel[member]; });
                    model.upload.uploadSession = oldUpload.uploadSession;                    
                    model.upload.displayName = model.file.fileName;
                    model.upload.message = oldUpload.message;
                    model.options.testRun = oldOptions.testRun;
                });
        }

        function saveAndReload(model) {
            var handle = resetHandle(model);
            var curPage = model.nav.curPage;

            if (!isPageValid(model)) {
                spAlertsService.addAlert("Please specify a valid name.",
                    { expires: 3, severity: spAlertsService.sev.Error });
                return $q.when();
            }

            // Save model
            return saveImportConfig(model)
                .then(function() {
                    if (!checkHandle(model, handle)) return null;
                    return reloadModel(model);
                }).then(function() {
                    model.nav.curPage = curPage;
                });
        }


        //
        // Page 5 - RUN IMPORT
        //

        function startImport(model, poll) {
            model.importMessage = 'Starting...';
            model.importRunning = true;

            var handle = resetHandle(model);

            // Save model
            var promise;
            if (hasUnsavedChanges(model))
                promise = saveImportConfig(model);
            else
                promise = $q.when();

            promise = promise.then(function () {
                if (!checkHandle(model, handle)) return null;
                return spImportFileWebService.importData(model.importConfigId, model.file.fileId, model.file.fileName, model.options.testRun);
            }).then(function (importRunId) {
                if (!checkHandle(model, handle)) return;
                model.importRunId = importRunId;
            }, handleErrorResponse);

            if (poll) {
                promise = promise.then(function () {
                    if (!checkHandle(model, handle)) return null;
                    return pollStatus(model, 100);
                });
            }
            return promise;
        }

        function pollStatus(model, prevInterval) {
            var intervalMs = prevInterval >= 1000 ? 1000 : prevInterval + 100;
            if (!model.importRunId) return $q.when();

            var messages = {
                'success': 'Spreadsheet import completed',
                'inProgress': 'Import running...',
                'cancelled': 'Spreadsheet import cancelled',
                'failed': 'Spreadsheet import failed'
            };

            var handle = getHandle(model);

            return spImportFileWebService.getImportStatus(model.importRunId).then(function (status) {
                if (!checkHandle(model, handle)) return;

                // { importStatus, importMessages, recordsSucceeded, recordsFailed }
                model.importStatus = status; // ImportSpreadsheet.ImportResultInfo
                model.importSummary = messages[status.importStatus];
                model.importMessage = createStatusMessage(model, status);
                if (model.importStatus.importStatus === 'inProgress') {
                    model.importPercent = 100 * (status.recordsSucceeded + status.recordsFailed) / status.recordsTotal;
                    $timeout(function() { pollStatus(model, intervalMs); }, intervalMs);
                } else {
                    model.importPercent = 100;
                    model.importRunning = false;
                }
            });
        }

        function createStatusMessage(model, status) {
            var verb = model.options.testRun ? 'verified' : 'imported';
            var res = status.recordsSucceeded + ' of ' + status.recordsTotal + ' records successfully ' + verb + '.';
            if (status.recordsFailed) {
                var verb2 = model.options.testRun ? 'verify' : 'import';
                res += '\n' + status.recordsFailed + ' of ' + status.recordsTotal + ' records failed to ' + verb2 + '.';
            }
            if (status.importMessages) {
                res += '\n\n' + status.importMessages;
            }
            return res;
        }

        function cancelImport(model) {
            resetHandle(model);
            if (!model.importRunning) return $q.when();
            if (!model.importRunId) return $q.when();
            return spImportFileWebService.cancelImport(model.importRunId);
        }

        function restartWizard(model) {

            return cancelImport(model)
                .then(function () {
                    // create a new model
                    return reloadModel(model);
                });
        }

        function getHandle(model) {
            if (!model.handle)
                model.handle = {};
            return model.handle;
        }

        function checkHandle(model, handle) {
            return handle === model.handle;
        }

        function resetHandle(model) {
            model.handle = {};
            return model.handle;
        }

        var exports = {
            createModel: createModel,
            createEmptyModel: createEmptyModel,
            loadSheetSample: loadSheetSample,
            noHeadingChanged: noHeadingChanged,
            headingRowChanged: headingRowChanged,
            dataRowChanged: dataRowChanged,
            lastRowChanged: lastRowChanged,
            sheetChanged: sheetChanged,
            separatorChanged: separatorChanged,
            isPageValid: isPageValid,
            setSelectedType: setSelectedType,
            startImport: startImport,
            hasUnsavedChanges: hasUnsavedChanges,
            prepareMappings: prepareMappings,
            saveAndReload: saveAndReload,
            pollStatus: pollStatus,
            cancelImport: cancelImport,
            restartWizard: restartWizard,
            getAvailableTargetMembers: getAvailableTargetMembers,
            test: {
                getFileFormat: getFileFormat,
                onFileUploadComplete: onFileUploadComplete,
                getAvailableTargetMembers: getAvailableTargetMembers,
                getAvailableSourceColumns: getAvailableSourceColumns,                
                saveImportConfig: saveImportConfig,
                allRequiredMembersAreMapped: allRequiredMembersAreMapped
            }
        };
        return exports;
    }

}());
