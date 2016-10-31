// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('app.editForm.imageController', ['mod.app.editForm', 'sp.common.fileUpload', 'mod.common.spWebService', 'mod.common.ui.spDialogService', 'mod.common.ui.spImageViewerDialog', 'mod.common.spXsrf', 'mod.common.spFileDownloadService'])
    .controller('imageController',
        function ($scope, $http, spFieldValidator, spEditForm, spUploadManager, spWebService, spEntityService, spNavService, spDialogService, spImageViewerDialog, spXsrf, $timeout, $q, spFileDownloadService) {
            'use strict';

            //---- init ----//
            spEditForm.getTemplateReport(function (report) {
                $scope.templateReport = report;
            });
            // chk if this control is being used as an image relationship or as a field control on a custom image form
            var isRelationship = ($scope.formControl.hasRelationship('console:relationshipToRender') && $scope.formControl.getRelationshipToRender() !== null);
            $scope.isCustomImageForm = ($scope.formControl.hasRelationship('console:fieldToRender') && $scope.formControl.getFieldToRender() !== null);
            //var relToRender = $scope.formControl.hasRelationship('console:relationshipToRender') && $scope.formControl.getRelationshipToRender() !== null ? $scope.formControl.getRelationshipToRender() : null;
            $scope.fieldToRender = $scope.formControl.hasRelationship('console:fieldToRender') && $scope.formControl.getFieldToRender() !== null ? $scope.formControl.getFieldToRender() : null;
            $scope.isReadOnlyControl = $scope.formControl.hasField('console:readOnlyControl') ? $scope.formControl.getReadOnlyControl() : null;
           
            if ($scope.isCustomImageForm) {     // i.e. field
                spEditForm.commonFieldControlInit($scope.fieldToRender);
            }

            if (isRelationship) {
                spEditForm.commonRelFormControlInit($scope, { validator: null });           // we handle our own validation because it acts as a field or a relationship. When the commonFieldControlInit gets a validator option, this should go away

            }
            
            // image height, width
            $scope.thumbnailSizeId = -1;
            $scope.thumbnailScalingId = -1;
            $scope.thumbnailWidth = '100%';
            $scope.thumbnailHeight = '150px';
            $scope.browseBtnWidth = '120px';
            $scope.contextMenuBtnWidth = '20px';
            $scope.nameInputWidth = 'auto';

            // other flags
            $scope.canUploadImage = false;
            $scope.imageAvailable = false;
            $scope.buttonText = 'Browse';            
            var skipValidation = false;

            
            // image upload
            $scope.imageUploadSession = spUploadManager.createUploadSession();
            $scope.imageUploadMessage = "";
            $scope.imageUploadImagePreviewSrc = null;
            $scope.imageUploadFilter = spUtils.imageFileTypeFilter; // 'image/*';
            // When it's a custom form don't upload instantly
            $scope.imageInstantUpload = !$scope.isCustomImageForm;            

            // image upload callback
            $scope.imageUploadOnFileUploadComplete = function (fileName, fileUploadId) {
                $scope.imageUploadFileName = fileName;
                $scope.imageUploadFileId = fileUploadId;
                
                createNewImageResource(fileName,fileUploadId);
            };
            
            $scope.$watch('imageUploadImagePreviewSrc', function (value) {
                // Set the url to the prevew url
                if (value &&
                    $scope.isCustomImageForm &&
                    $scope.canUploadImage) {
                    $scope.imageAvailable = true;
                    // image url
                    $scope.imageUrl = value;
                    // popup image url
                    $scope.popImageUrl = value;
                }
            });

            // image upload busy indicator
            $scope.imageUploadBusyIndicatorOptions = {
                type: 'progressBar',
                text: 'Uploading...',
                placement: 'window',
                isBusy: false,
                percent: 0
            };
            
            // image save busy indicator
            $scope.imageSaveBusyIndicatorOptions = {
                type: 'progressBar',
                text: 'Uploading...',
                placement: 'window',
                isBusy: false,
                percent: 100
            };
            
            // watch formMode
            $scope.$watch("formMode", function () {

                $scope.isReadOnlyControl = $scope.formControl && ($scope.formControl.readOnlyControl === true);

                if ($scope.formMode && $scope.formMode === 'edit' && $scope.isReadOnlyControl !== true) {
                    $scope.canUploadImage = true;
                } else if ($scope.formMode && $scope.formMode === 'view') {
                    $scope.canUploadImage = false;
                    $scope.isReadOnlyControl = true;
                }

                updateFormControlSettings();
            });

            // watch formData
            $scope.$watch("formData", function () {
                if ($scope.formData) {
                    
                    if (isRelationship) {
                        updateImageSizeAndScale();
                        updateFormId();
                        skipValidation = true;  // should skip validation when first time form data is loaded
                        $scope.selectedEntity = getSelectedEntity($scope);
                        $scope.canModifyDenied = sp.result($scope.formData, 'canModify') === false;
                    }
                    else if ($scope.isCustomImageForm) {
                        
                        if ($scope.formData.dataState !== spEntity.DataStateEnum.Create) {
                            $scope.imageAvailable = true;
                            $scope.imageUploadImagePreviewSrc = null;
                            // image url
                            $scope.imageUrl = getImageUrl($scope.formData.id());
                            // popup image url
                            $scope.popImageUrl = getPopImageUrl($scope.formData.id());

                            // download image url
                            $scope.downloadImageUrl = getImageDownloadUrl($scope.formData.id());
                        } else {
                            $scope.imageAvailable = false;                                                        
                            $scope.imageUploadImagePreviewSrc = null;
                            $scope.imageUrl = '';
                            $scope.popImageUrl = '';
                        }
                        
                        $scope.selectedEntity = $scope.formData;                        

                        // image name
                        $scope.displayString = $scope.formData.getField($scope.fieldToRender.id());

                        updateFormControlSettings();
                    }
                }
            });


            function updateFormControlSettings() {
                if (!$scope.formMode || !$scope.formData || !$scope.isCustomImageForm) {
                    return;
                }

                if ($scope.formMode === 'edit') {
                    if ($scope.formData.dataState === spEntity.DataStateEnum.Create) {
                        // If we are creating a new image then set the appropriate flags
                        $scope.canUploadImage = true;
                        $scope.isReadOnlyControl = false;
                    } else {
                        // Image is not being created.
                        $scope.canUploadImage = false;
                        $scope.isReadOnlyControl = true;
                    }   
                }
            }                


            // watch selectedEntity
            $scope.$watch("selectedEntity", function () {
                if (angular.isDefined($scope.selectedEntity)) {
                    // skip if used in custom image form 
                    if ($scope.isCustomImageForm === true) {
                        return;
                    }

                    // update display string
                    $scope.displayString = displayName($scope.selectedEntity);
                    
                    // update button text
                    $scope.buttonText = buttonText($scope.selectedEntity);
                    
                    // update image related data
                    updateImageRelatedData($scope.selectedEntity);
                    
                    // should skip validation when first time form data is loaded
                    if(skipValidation === true) {
                        skipValidation = false;
                    }
                    else {
                        // validate
                        validate();
                    }
                }
            });
            
            // validate
            $scope.$on("validateForm", function (event) {                                
                validate();
            });
            
            $scope.$on('formControlUpdated', function () {
                spEditForm.commonRelFormControlInit($scope);
            });

            $scope.$watch('imageUploadSession.fileNames[0]', function(value) {
                if (!$scope.isCustomImageForm || !$scope.canUploadImage) {
                    return;
                }

                if (value) {
                    $scope.displayString = value;
                }                
            });

            // This is called before the form is saved.
            // When the case is a custom form upload the image
            // and assign the has to the entity so that it can be saved.
            $scope.formControl.handlePreSave = function (entity) {
                var deferred;

                deferred = $q.defer();
                deferred.resolve(entity);

                if (!$scope.isCustomImageForm ||
                    !$scope.canUploadImage ||
                    !$scope.imageUploadSession.files ||
                    !$scope.imageUploadSession.files.length) {
                    return deferred.promise;
                }                
                
                return $scope.imageUploadSession.uploadFiles().then(function(result) {
                    var fileExtension;

                    if (!result ||
                        !result.length) {
                        throw new Error('An error occured during the image upload.');
                    }

                    $scope.imageUploadFileName = result[0].fileName;
                    $scope.imageUploadFileId = result[0].hash;
                    $scope.displayString = result[0].fileName;

                    entity.name = $scope.displayString;
                    entity.setField('core:fileDataHash', result[0].hash, spEntity.DataType.String);                        

                    fileExtension = spUploadManager.getFileExtension(result[0].fileName);
                    if (fileExtension) {
                        entity.registerField('core:fileExtension', spEntity.DataType.String);
                        entity.setFileExtension(fileExtension);    
                    }

                    return entity;
                }, function(error) {
                    throw new Error('An error occured during the image upload. Error ' + error);
                }).finally(function() {
                    // recreate the session so we can send again
                    $scope.imageUploadSession = spUploadManager.createUploadSession();
                });                
            };

            function validate() {
                $scope.customValidationMessages = [];

                if (isRelationship) {
                    spEditForm.validateRelationshipControl($scope, getSelectedEntity($scope));
                } else if ($scope.isCustomImageForm &&
                           $scope.canUploadImage) {
                    if (!$scope.imageUploadSession.files || !$scope.imageUploadSession.files.length) {
                        spFieldValidator.raiseValidationErrors($scope, ['An image is required.']);
                    }
                    else {
                        spFieldValidator.clearValidationErrors($scope);
                    }
                }
            }
            
            // provide a function to validate the model is correct. Used when saving
            $scope.formControl.spValidateControl = function (entity) {
                validate();
                return $scope.customValidationMessages.length === 0;
            };
            
            // browse  
            $scope.browse = function () {
                var entity = null;

                if (isRelationship) {
                    if ($scope.formData) {
                        entity = $scope.formData.getLookup($scope.relationshipToRender.id());
                    }

                    if (entity !== null) {
                        viewImageInfo();
                    }
                }                
            };

            // ---- private functions ----//
            
            // helpers
            
            function updateImageRelatedData(selectedEntity) {
                if (selectedEntity !== null) {
                    var id = selectedEntity._id._id;

                    // imageAvailable flag
                    $scope.imageAvailable = true;

                    // image url
                    $scope.imageUrl = getImageUrl(id);

                    // popup image url
                    $scope.popImageUrl = getPopImageUrl(id);
                }
                else {
                    $scope.imageUrl = '';
                    $scope.popImageUrl = '';
                    $scope.imageAvailable = false;
                }
            }           
            
            function createNewImageResource(imageName, imageHash) {
                // Sanity check
                if ($scope.isCustomImageForm || !isRelationship) {
                    return;
                }            
               
                var fileExtension;

                var newEntityType = $scope.relationshipToRender.getToType()._id._id; //$scope.relationshipToRender.toType;                
                
                var entity = spEntity.createEntityOfType(newEntityType).setName(imageName);

                entity.registerField('core:fileDataHash', spEntity.DataType.String);
        
                entity.setFileDataHash(imageHash);                

                fileExtension = spUploadManager.getFileExtension(imageName);                
                if (fileExtension) {
                    entity.registerField('core:fileExtension', spEntity.DataType.String);
                    entity.setFileExtension(fileExtension);    
                }                

                spEntityService.putEntity(entity).then(function (id) {

                    // replace temp id with actual id
                    entity._id._id = id;

                    // update the datastate to unchanged as this entity has already been saved 
                    entity.dataState = spEntity.DataStateEnum.Unchanged;   
                    
                    // update form data
                    $scope.formData.setLookup($scope.relationshipToRender.id(), entity);

                    // set selectedEntity
                    $scope.selectedEntity = getSelectedEntity($scope);                           

                }, function (error) {
                    window.alert(error);
                })
                .finally(function () {
                    // TODO: hide progress bar
                    //scope.busyIndicatorOptions.isBusy = false;
                });
            }
            
            function displayName(entity) {
                return entity ? entity.getName() : '';
            }
            
            function buttonText(entity) {
                return entity ? 'Details' : 'Browse';
            }

            function getSelectedEntity(scope) {
                return scope.formData.getLookup($scope.relationshipToRender.id());
            }
            
            function updateImageSizeAndScale() {
                
                if ($scope.formControl) {
                    var size = $scope.formControl.getLookup('console:thumbnailSizeSetting');
                    if(size) {
                        $scope.thumbnailSizeId = size._id._id;
                        
                        var height = size.getField('console:thumbnailHeight');
                        if (height && _.isNumber(height)) {
                            $scope.thumbnailHeight = height;
                        }
                        var width = size.getField('console:thumbnailWidth');
                        if (width && _.isNumber(width)) {
                            $scope.thumbnailWidth = width;
                        }
                    }
                    
                    var scale = $scope.formControl.getLookup('console:thumbnailScalingSetting');
                    if(scale) {
                        $scope.thumbnailScalingId = scale._id._id;
                    }
                }
            }
            
            function updateFormId() {

                if ($scope.relationshipToRender) {
                    // get toType
                    var toType = $scope.relationshipToRender.getLookup('core:toType');
                    if (toType.hasRelationship('console:defaultEditForm')) {
                        $scope.formId = toType.getLookup('console:defaultEditForm').id();//._id;
                    }
                }
            }

            // style/width/height
            $scope.popImageDimensions = function () {
                var style = {};
                
                var winHeight = $(window).height();
                var winWidth = $(window).width();
                style['max-height'] = Math.round(winHeight * 0.9) + 'px';
                style['max-width'] = Math.round(winWidth * 0.9) + 'px';
                return style;
            };
            
            // image div
            $scope.imgDivDimensions = function () {
                var style = {};

                style.width = parseFloat($scope.thumbnailWidth) + 'px';
                style.height = parseFloat($scope.thumbnailHeight) + 'px';
                return style;
            };
            
            // input controls container div
            $scope.inputCtrlsContainerDivWidth = function () {
                var style = {};

                style.width = $scope.thumbnailWidth;
                return style;
            };
            
            // browse button
            $scope.browseButtonWidth = function () {
                var style = {};

                style.width = '63px';
                return style;
            };
            
            // contextMenu button width
            $scope.contextMenuButtonWidth = function () {
                var style = {};

                style.width = '20px';
                return style;
            };
            
            // image Name Input Width
            $scope.imageNameInputWidth = function () {
                var style = {};

                style.width = ($scope.thumbnailWidth - 85) + 'px';  // 55 + 32 + 2
                return style;
            };

            $scope.getImageStyle = function() {
                var style = {};
                                
                if ($scope.isCustomImageForm &&                    
                    $scope.imageUploadImagePreviewSrc &&
                    $scope.imageUrl) {                    
                    style.height = '150px';
                }                

                return style;
            };
            
            function getImageUrl(id) {

                if(!id) {
                    return '';
                }
                
                if ($scope.thumbnailSizeId > 0 && $scope.thumbnailScalingId > 0) {
                    return spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/image/thumbnail/' + (id) + '/' + $scope.thumbnailSizeId + '/' + $scope.thumbnailScalingId);
                } else {
                    return spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/image/thumbnail/' + (id) + '/console-smallThumbnail/core-scaleImageProportionally');
                }                                
            }
            
            function getPopImageUrl(id) {

                if (!id) {
                    return '';
                }

                return spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/image/' + (id));
            }

            function getImageDownloadUrl(id) {

                return spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/image/download/' + (id));
            }
            
            //--- context menu actions ---//
            
           
            var findExistingImage = function () {
                // picker Report 
                var reportOptions = {
                    reportId: !spUtils.isNullOrUndefined($scope.templateReport) ? $scope.templateReport.idP : 0,
                    multiSelect: false,
                    isEditMode: false,
                    selectedItems: null,
                    entityTypeId: $scope.entityType,
                    isInPicker: true
                };

                var defaults = {
                    templateUrl: 'entityPickers/entityReportPicker/spEntityReportPicker_modal.tpl.html',
                    controller: 'imagePickerModelInstanceController',
                    windowClass: 'modal inlineRelationPickerDialog', // todo: rename the class to something more meaningful
                    resolve: {
                        reportOptions: function () {
                            return reportOptions;
                        }
                    },
                };

                var options = {};

                spDialogService.showDialog(defaults, options).then(function (result) {
                    if (result && result.length > 0) {
                        if (spUtils.isNullOrUndefined($scope.selectedEntity) || $scope.selectedEntity.idP !== result[0].eid) {
                            spEntityService.getEntity(result[0].eid, 'name', { hint:'imgCtrl', batch: false }).then(function (entity) {
                                if (!entity) {
                                    return;
                                }

                                // update form data
                                $scope.formData.setLookup($scope.relationshipToRender.id(), entity);
                                
                                // set selectedEntity
                                $scope.selectedEntity = getSelectedEntity($scope);
                            });
                        }
                    }
                });
            };
            
            function clearImage() {
                // clear selected image
                $scope.formData.setLookup($scope.relationshipToRender.id(), null);
                
                // set selectedEntity
                $scope.selectedEntity = getSelectedEntity($scope);
            }
            
            function viewImageInfo() {
                var params = {};
                if ($scope.formId) {
                    params.formId = $scope.formId;

                    spNavService.navigateToChildState('viewForm', $scope.selectedEntity.id(), params);
                }
            }
            
            function downloadImage() {

                var entity = getSelectedEntity($scope);
                if (entity !== null && entity._id._id > 0) {
                    spFileDownloadService.downloadFile(getImageDownloadUrl(entity._id._id));
                }
                else {
                    window.alert('No image available to download');
                }
            }
            
            $scope.downloadImageFromCustomImageForm = function () {
                if ($scope.downloadImageUrl) {                    
                    spFileDownloadService.downloadFile($scope.downloadImageUrl);
                }
            };

            
            $scope.menuItemClick = function (value) {
                $scope.isMenu1Open = false; // close the context menu
                switch (value) {
                    case "FindExisting":
                        findExistingImage();
                        break;
                    case "Clear":
                        clearImage();
                        break;
                    case "ViewInfo":
                        viewImageInfo();
                        break;
                    case "Download":
                        downloadImage();
                        break;
                    default:
                        window.alert('none matched');
                        break;
                }

            };           
            
            // contextMenu
            $scope.contextMenu = {
                menuItems: [
                   {
                        text: 'Find Existing',
                        icon: 'assets/images/16x16/view.svg',
                        type: 'click',
                        click: 'menuItemClick(\'FindExisting\')',
                        disabled: 'canModifyDenied'
                    },
                    {
                        text: 'Clear',
                        icon: 'assets/images/16x16/delete.svg',
                        type: 'click',
                        click: 'menuItemClick(\'Clear\')',
                        disabled: '!imageAvailable || canModifyDenied'
                    },
                    {
                        type: 'divider'
                    },
                    {
                        text: 'View Details',
                        icon: 'assets/images/16x16/viewDetail.svg',
                        type: 'click',
                        click: 'menuItemClick(\'ViewInfo\')',
                        disabled: '!imageAvailable'
                    },
                    {
                        text: 'Download Image',
                        icon: 'assets/images/16x16/download.svg',
                        type: 'click',
                        click: 'menuItemClick(\'Download\')',
                        disabled: '!imageAvailable'
                    }
                ]
            };

            $scope.openModal = function () {              
                if (!$scope.popImageUrl) {
                    return;
                }

                spImageViewerDialog.showModalDialog({
                    imageUrl: $scope.popImageUrl
                });
            };


        }).controller('imagePickerModelInstanceController',function($scope, $uibModalInstance, reportOptions) {
            $scope.model = {};
            $scope.model.reportOptions = reportOptions;

            $scope.ok = function () {
                $uibModalInstance.close($scope.model.reportOptions.selectedItems);
            };
            
            $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
            };
            
            $scope.$on('spReportEventGridDoubleClicked', function (event, selectedItems) {
                event.stopPropagation();
                $scope.ok();
            });
        });