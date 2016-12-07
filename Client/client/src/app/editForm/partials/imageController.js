// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {

    angular.module('app.editForm.imageController', [
        'mod.app.editForm', 'sp.common.fileUpload', 'mod.common.spWebService',
        'mod.common.ui.spDialogService', 'mod.common.ui.spImageViewerDialog', 'mod.common.spXsrf',
        'mod.common.spFileDownloadService'
    ]);

    angular.module('app.editForm.imageController')
        .factory('imageService', imageService)
        .factory('imageUploadService', imageUploadService)
        .factory('imagePickerService', imagePickerService)
        .controller('imageController', ImageController);

    function imageService(spEditForm) {
        'ngInject';

        return {
            initImageViewModel
        };

        function initImageViewModel(ctrl) {
            // check if this control is being used as an image relationship or as a field control on a custom image form
            const isRelationship = ctrl.formControl.hasRelationship('console:relationshipToRender') && ctrl.formControl.relationshipToRender !== null;
            const isField = ctrl.formControl.hasRelationship('console:fieldToRender') && ctrl.formControl.fieldToRender !== null;

            ctrl.isCustomImageForm = isField;
            ctrl.fieldToRender = ctrl.formControl.fieldToRender;
            ctrl.isReadOnlyControl = ctrl.formControl.readOnlyControl;

            if (isField) {
                spEditForm.commonFieldControlInit(ctrl.fieldToRender);
            }

            if (isRelationship) {
                // we handle our own validation because it acts as a field or a relationship.
                // When the commonFieldControlInit gets a validator option, this should go away
                spEditForm.commonRelFormControlInit(ctrl, {validator: null});
            }

            // image height, width
            ctrl.thumbnailSizeId = -1;
            ctrl.thumbnailScalingId = -1;
            ctrl.thumbnailWidth = '100%';
            ctrl.thumbnailHeight = '150px';
            ctrl.browseBtnWidth = '120px';
            ctrl.contextMenuBtnWidth = '20px';
            ctrl.nameInputWidth = 'auto';

            // other flags
            ctrl.canUploadImage = false;
            ctrl.imageAvailable = false;
            ctrl.buttonText = 'Browse';
        }
    }

    function imageUploadService(spEntityService, spUploadManager) {
        'ngInject';

        return {
            initFileUploadViewModel
        };

        function initFileUploadViewModel(ctrl) {
            ctrl.imageInstantUpload = !ctrl.isCustomImageForm;
            ctrl.imageUploadSession = spUploadManager.createUploadSession('image');
            ctrl.imageUploadSession.suppressErrors = true; // errors handled by image controller
            ctrl.imageUploadMessage = "";
            ctrl.imageUploadImagePreviewSrc = null;
            ctrl.imageUploadFilter = spUtils.imageFileTypeFilter; // 'image/*';
            ctrl.imageUploadOnFileUploadComplete = function (fileName, fileUploadId) {
                ctrl.imageUploadFileName = fileName;
                ctrl.imageUploadFileId = fileUploadId;
                return createNewImageResource(ctrl, fileName, fileUploadId);
            };
        }

        function createNewImageResource(ctrl, imageName, imageHash) {

            // Assumes the standard rel control init stuff has been called
            if (!ctrl.relationshipToRender) {
                return;
            }

            const newEntityType = ctrl.relationshipToRender.getToType()._id._id;
            const entity = spEntity.createEntityOfType(newEntityType);

            entity.name = imageName;
            entity.registerField('core:fileDataHash', spEntity.DataType.String);
            entity.setFileDataHash(imageHash);

            const fileExtension = spUploadManager.getFileExtension(imageName);
            if (fileExtension) {
                entity.registerField('core:fileExtension', spEntity.DataType.String);
                entity.setFileExtension(fileExtension);
            }

            return spEntityService.putEntity(entity).then(function (id) {
                //TODO - we shouldn't be doing the following things directly to the entity
                // replace temp id with actual id
                // and update the datastate to unchanged as this entity has already been saved
                entity._id._id = id;
                entity.dataState = spEntity.DataStateEnum.Unchanged;

                ctrl.formData.setLookup(ctrl.relationshipToRender.idP, entity);
                ctrl.selectedEntity = getSelectedEntity(ctrl);

            }, function (error) {
                console.error('failed to upload file', error);
            });
        }

        function getSelectedEntity(ctrl) {
            return ctrl.formData.getLookup(ctrl.relationshipToRender.id());
        }
    }

    function imagePickerService(spEntityService, spDialogService) {
        'ngInject';

        return {
            chooseExistingImage
        };

        /**
         * Show a picker dialog for image entities and return the chosen.
         * Returns a promise for the selected image entity.
         */
        function chooseExistingImage({templateReport, entityType, selectedEntity}) {

            const reportOptions = {
                reportId: !spUtils.isNullOrUndefined(templateReport) ? templateReport.idP : 0,
                multiSelect: false,
                isEditMode: false,
                selectedItems: null,
                entityTypeId: entityType,
                isInPicker: true
            };

            const defaults = {
                templateUrl: 'entityPickers/entityReportPicker/spEntityReportPicker_modal.tpl.html',
                controller: ImagePickerModelInstanceController,
                windowClass: 'modal inlineRelationPickerDialog', // todo: rename the class to something more meaningful
                resolve: {
                    reportOptions: () => reportOptions
                },
            };

            // return the promise
            return spDialogService.showDialog(defaults, {}).then(function (result) {

                const newSelection = _.first(result);

                // skip out if nothing chosen
                if (!newSelection) return null;

                // skip out if selection is unchanged
                if (selectedEntity && selectedEntity.idP === newSelection.eid) return null;

                // get an entity object for the selected entity id (with name, type etc)
                // and return the promise for it
                return spEntityService.getEntity(newSelection.eid, 'name', {hint: 'imgCtrl', batch: false});
            });
        }

        function ImagePickerModelInstanceController($scope, $uibModalInstance, reportOptions) {
            'ngInject';

            $scope.model = {reportOptions};

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
        }
    }

    function ImageController($scope, $q, spFieldValidator, spEditForm, spUploadManager, spWebService,
                             spNavService, spImageViewerDialog, spXsrf, spFileDownloadService, spAlertsService,
                             imageService, imageUploadService, imagePickerService) {
        'ngInject';

        let skipValidation = false;

        // check if this control is being used as an image relationship or as a field control on a custom image form
        const isRelationship = $scope.formControl.hasRelationship('console:relationshipToRender') && $scope.formControl.relationshipToRender !== null;

        imageService.initImageViewModel($scope);
        imageUploadService.initFileUploadViewModel($scope);

        $scope.imageUploadBusyIndicatorOptions = {
            type: 'progressBar',
            text: 'Uploading...',
            placement: 'window',
            isBusy: false,
            percent: 0
        };

        $scope.imageSaveBusyIndicatorOptions = {
            type: 'progressBar',
            text: 'Uploading...',
            placement: 'window',
            isBusy: false,
            percent: 100
        };

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

        spEditForm.getTemplateReport().then(report => {
            $scope.templateReport = report;
        });

        // This is called before the form is saved.
        // When the case is a custom form upload the image
        // and assign the hash to the entity so that it can be saved.
        $scope.formControl.handlePreSave = function (entity) {

            if (!$scope.isCustomImageForm || !$scope.canUploadImage || _.isEmpty($scope.imageUploadSession.files)) {
                return $q.when(entity);
            }

            return $scope.imageUploadSession.uploadFiles().then(function (result) {
                var fileExtension;

                if (!result || !result.length) {
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
            }, function (error) {
                throw new Error('An error occured during the image upload. Error: ' + error);
            }).finally(function () {
                // recreate the session so we can send again
                $scope.imageUploadSession = spUploadManager.createUploadSession('image');
            });
        };

        // provide a function to validate the model is correct. Used when saving
        $scope.formControl.spValidateControl = function (entity) {
            validate();
            return $scope.customValidationMessages.length === 0;
        };

        // browse
        $scope.browse = function () {
            if (isRelationship && getSelectedEntity($scope)) {
                viewImageInfo();
            }
        };

        // style/width/height
        $scope.popImageDimensions = function () {
            const winHeight = $(window).height();
            const winWidth = $(window).width();
            return {
                'max-height': Math.round(winHeight * 0.9) + 'px',
                'max-width': Math.round(winWidth * 0.9) + 'px'
            };
        };

        // image div
        $scope.imgDivDimensions = function () {
            return {
                width: parseFloat($scope.thumbnailWidth) + 'px',
                height: parseFloat($scope.thumbnailHeight) + 'px'
            };
        };

        // input controls container div
        $scope.inputCtrlsContainerDivWidth = function () {
            return {width: $scope.thumbnailWidth};
        };

        // browse button
        $scope.browseButtonWidth = function () {
            return {width: '63px'};
        };

        // contextMenu button width
        $scope.contextMenuButtonWidth = function () {
            return {width: '20px'};
        };

        // image Name Input Width
        $scope.imageNameInputWidth = function () {
            return {width: ($scope.thumbnailWidth - 85) + 'px'};  // 55 + 32 + 2
        };

        $scope.getImageStyle = function () {
            return $scope.isCustomImageForm && $scope.imageUploadImagePreviewSrc && $scope.imageUrl ?
                {height: '150px'} : {};
        };

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

        $scope.openModal = function () {
            if (!$scope.popImageUrl) {
                return;
            }

            spImageViewerDialog.showModalDialog({
                imageUrl: $scope.popImageUrl
            });
        };

        $scope.$watch('imageUploadImagePreviewSrc', function (value) {
            // Set the url to the preview url
            if (value &&
                $scope.isCustomImageForm &&
                $scope.canUploadImage) {
                $scope.imageAvailable = true;
                $scope.imageUrl = value;
                $scope.popImageUrl = value;
            }
        });

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
                if (skipValidation === true) {
                    skipValidation = false;
                }
                else {
                    validate();
                }
            }
        });

        $scope.$on("validateForm", function (event) {
            validate();
        });

        $scope.$on('formControlUpdated', function () {
            spEditForm.commonRelFormControlInit($scope);
        });

        $scope.$watch('imageUploadSession.fileNames[0]', function (value) {
            if (!$scope.isCustomImageForm || !$scope.canUploadImage) {
                return;
            }

            if (value) {
                $scope.displayString = value;
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

        function updateImageRelatedData(selectedEntity) {
            if (selectedEntity !== null) {
                const id = selectedEntity._id._id;

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

        function displayName(entity) {
            return entity ? entity.getName() : '';
        }

        function buttonText(entity) {
            return entity ? 'Details' : 'Browse';
        }

        function getSelectedEntity(scope) {
            return scope.formData && scope.formData.getLookup($scope.relationshipToRender.id());
        }

        function updateImageSizeAndScale() {

            if ($scope.formControl) {
                var size = $scope.formControl.getLookup('console:thumbnailSizeSetting');
                if (size) {
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
                if (scale) {
                    $scope.thumbnailScalingId = scale._id._id;
                }
            }
        }

        function updateFormId() {
            if ($scope.relationshipToRender) {
                // get toType
                const toType = $scope.relationshipToRender.getLookup('core:toType');
                if (toType.hasRelationship('console:defaultEditForm')) {
                    $scope.formId = toType.getLookup('console:defaultEditForm').id();//._id;
                }
            }
        }

        function getImageUrl(id) {
            if (!id) return '';

            let url = spWebService.getWebApiRoot() + '/spapi/data/v1/image/thumbnail/' + id;

            if ($scope.thumbnailSizeId > 0 && $scope.thumbnailScalingId > 0) {
                url += '/' + $scope.thumbnailSizeId + '/' + $scope.thumbnailScalingId;
            } else {
                url += '/console-smallThumbnail/core-scaleImageProportionally';
            }

            return spXsrf.addXsrfTokenAsQueryString(url);
        }

        function getPopImageUrl(id) {
            return id ?
                spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/image/' + id) :
                '';
        }

        function getImageDownloadUrl(id) {
            return spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/image/download/' + (id));
        }

        function findExistingImage() {
            return imagePickerService.chooseExistingImage($scope)
                .then(entity => {
                    if (entity) setImageEntity(entity);
                });
        }

        function clearImage() {
            setImageEntity(null);
        }

        function setImageEntity(entity) {
            // update form data, may be null
            $scope.formData.setLookup($scope.relationshipToRender.idP, entity);

            // set selectedEntity based on the new related entity
            $scope.selectedEntity = getSelectedEntity($scope);
        }

        function viewImageInfo() {
            if ($scope.formId) {
                spNavService.navigateToChildState('viewForm', $scope.selectedEntity.id(), {formId: $scope.formId});
            }
        }

        function downloadImage() {
            const entity = getSelectedEntity($scope);
            if (entity !== null && entity._id._id > 0) {
                spFileDownloadService.downloadFile(getImageDownloadUrl(entity._id._id));
            }
            else {
                spAlertsService.addAlert('No image available to download', 'error');
            }
        }
    }

})();