// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('app.editForm.fileNameUploadController', ['mod.app.editForm', 'sp.common.fileUpload', 'mod.common.spWebService', 'mod.common.ui.spDialogService', 'mod.common.spXsrf', 'mod.common.spBrowserService'])
    .controller('fileNameUploadController',
        function ($scope, $http, $q, spFieldValidator, spEditForm, spUploadManager, spWebService, spEntityService, spNavService, spDialogService, spXsrf, spBrowserService) {
            'use strict';

            var loadedDocumentTypes = [];
            let isFirstValidation = true;

            $scope.fieldToRender = $scope.formControl.hasRelationship('console:fieldToRender') && $scope.formControl.getFieldToRender() !== null ? $scope.formControl.getFieldToRender() : null;
            $scope.isReadOnlyControl = false;
            $scope.isSafari = spBrowserService.isSafari;
            
            spEditForm.commonFieldControlInit($scope.fieldToRender);
            
            $scope.uploadSession = spUploadManager.createUploadSession();
            $scope.uploadMessage = "";
            $scope.uploadBusyIndicatorOptions = {
                type: 'progressBar',
                text: 'Uploading...',
                placement: 'element',
                isBusy: false,
                percent: 0
            };
            $scope.selectedEntity = null;
            $scope.uploadImagePreviewSrc = null;
            $scope.uploadFileTypeFilter = '';
            $scope.uploadButtonLabel = 'Browse';
            $scope.uploadDisplayName = '';
            $scope.downloadUri = '';

            // Watches            

            $scope.$watch('formControl', function() {
                if ($scope.formControl) {
                    var fieldToRender = $scope.formControl.getFieldToRender();

                    $scope.isMandatoryOnForm = $scope.formControl.mandatoryControl;
                    $scope.isRequired = $scope.isMandatoryOnForm || (fieldToRender && fieldToRender.isRequired);
                    $scope.titleModel = spEditForm.createTitleModel($scope.formControl);

                    $scope.formControl.spValidateControl = function (entity) {
                        validate();
                        return $scope.customValidationMessages.length === 0;
                    };

                    spEditForm.commonFieldControlInit(fieldToRender);

                    //$scope.isReadOnly = $scope.isFormReadOnly || $scope.formControl.readOnlyControl;
                    $scope.isReadOnly = $scope.isFormReadOnly;

                    if ($scope.formData) {
                        //model.fieldValue = $scope.formData.getField(fieldToRender.eid());
                    }
                }
            });

            $scope.$watch("formMode", function () {
                if ($scope.formMode && $scope.formMode === 'edit') {
                    $scope.isReadOnlyControl = $scope.isReadOnly;
                } else if ($scope.formMode && $scope.formMode === 'view') {
                    $scope.isReadOnlyControl = true;
                    $scope.customValidationMessages = [];                    
                }                
            });

            $scope.$watch("formData", function () {                
                if ($scope.formData) {                    
                    $scope.downloadUri = spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v2/file/' + ($scope.formData.id()));
                    $scope.selectedEntity = $scope.formData;
                    $scope.uploadDisplayName = $scope.formData.name;
                }
            });

            $scope.$watch("uploadSession.fileNames[0]", function () {
                if ( $scope.uploadSession.fileNames[0]) {
                    $scope.uploadDisplayName = $scope.uploadSession.fileNames[0].substr(0, $scope.uploadSession.fileNames[0].lastIndexOf('.'));
                } else if ($scope.formData) {
                    $scope.uploadDisplayName = $scope.formData.name;
                } else {
                    $scope.uploadDisplayName = '';
                }

                validate();
            });

            // Event Handlers
            
            $scope.uploadOnComplete = function (fileName, fileUploadId) {
                $scope.fileUploadFileName = fileName;
                $scope.fileUploadFileId = fileUploadId;
            };           
            
            
            $scope.formControl.handlePreSave = function (entity) {
                var promise;
                
                if ($scope.uploadSession.files && $scope.uploadSession.files.length > 0) {
                    promise = $scope.uploadSession.uploadFiles().then(function (result) {
                        var fileExtension;

                        console.log('uploaded: ', result[0].fileName);

                        $scope.fileUploadFileName = result[0].fileName;
                        $scope.fileUploadFileId = result[0].hash;

                        entity.name = result[0].fileName;

                        if (!entity.fileDataHash) {
                            entity.registerField('core:fileDataHash', spEntity.DataType.String);
                        }
                        entity.fileDataHash = result[0].hash;

                        fileExtension = spUploadManager.getFileExtension(result[0].fileName);
                        if (fileExtension) {
                            entity.registerField('core:fileExtension', spEntity.DataType.String);
                            entity.setFileExtension(fileExtension);
                        }

                        return entity;
                    }).then($scope.onPreSave);
                } else {
                    promise = $q.when($scope.onPreSave);
                }

                return promise.catch(function (error) {
                    $scope.uploadDisplayName = '';
                    console.error('fileNameUploadControl.handlePreSave error:', error);
                    throw error;
                }).finally(function () {
                    // recreate the session so we can send again
                    $scope.uploadSession = spUploadManager.createUploadSession();
                });
            };

            $scope.onPreSave = function (entity) {
                // save a relationship to the folder
                var itemContainer = spNavService.getCurrentItemContainer();
                if (itemContainer) {
                    entity.setLookup('core:inFolder', itemContainer.id);
                }
                
                // save a relationship to the relevant document type based on extension
                var ext = entity.name.substr((entity.name.lastIndexOf('.') + 1));
                var type = _.find(loadedDocumentTypes, function (dt) {
                    return _.includes(dt.extensions, ext);
                });
                if (type) {
                    if (!entity.documentFileType) {
                        entity.registerRelationship('core:documentFileType');
                    }
                    entity.documentFileType.add(type.id);
                }
                // Trim the entity name to exclude the file extension
                var originalName = entity.name;
                if (_.includes(originalName, '.')) {
                    entity.name = originalName.substr(0, originalName.lastIndexOf('.'));
                }
            };

            // Init
            
            if (spEntityService) {
                spEntityService.getEntitiesOfType('documentType', 'id,mimeType,extension', { hint:'fileUpload', batch: true }).then(function (docTypes) {
                    // create an array of mimetype descriptions used to filter the dialog selection
                    // -- this seems to be problematic depending on browser implementation, removing for now:(
                    //$scope.uploadFileTypeFilter = _.map(docTypes, 'mimeType').join(',');

                    _.forEach(docTypes, function (docType) {
                        var entry = _.find(loadedDocumentTypes, { 'id': docType.idP });
                        if (entry) {
                            entry.extensions += ';' + docType.extension;
                        } else {
                            loadedDocumentTypes.push({ id: docType.idP, extensions: docType.extension });
                        }
                    });
                });
            }

            function validate() {
                $scope.customValidationMessages = [];                                

                // Skip initial validation
                if (isFirstValidation) {
                    isFirstValidation = false;
                    return;
                }                

                if ($scope.isReadOnlyControl) {
                    return;
                }

                if ($scope.isRequired && !$scope.uploadDisplayName) {
                    spFieldValidator.raiseValidationErrors($scope, ['A file is required.']);
                } else {
                    spFieldValidator.clearValidationErrors($scope);
                }
            }
        });

