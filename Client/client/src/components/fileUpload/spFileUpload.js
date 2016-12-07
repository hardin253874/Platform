// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, $, sp, console */

(function () {
    'use strict';

    angular.module('sp.common.fileUpload', [
        'mod.common.spWebService',
        'mod.common.spXsrf',
        'mod.common.alerts',
        'mod.common.spWebService']);

    angular.module('sp.common.fileUpload')
        .factory('spUploadManager', uploadManager)
        .directive('spFileUpload', fileUploadDirective);

    /* @ngInject */
    function uploadManager($rootScope, $q, spXsrf, spAlertsService, spWebService) {

        var exports = {
            createUploadSession: createUploadSession,
            getFileExtension: getFileExtension
        };

        function getFileExtension(fileName) {
            if (!fileName) {
                return null;
            }

            var fileExtension = null,
                lastDotIndex = fileName.lastIndexOf(".");

            if (lastDotIndex !== -1) {
                fileExtension = fileName.substring(lastDotIndex);
            }

            return fileExtension;
        }

        function UploadSession(allowMultipleFiles) {
            var that = this;

            this.files = [];
            this.fileNames = [];
            this.uploader = null;
            this.deferredUpload = null;
            this.progress = 0;          // progress in percentage
            this.allowMultipleFiles = allowMultipleFiles;
            this.suppressErrors = false;
        }

        UploadSession.prototype.clear = function () {
            this.files.length = 0;
            this.fileNames.length = 0;
        };

        /**
         * Return a promise upload the current files
         **/
        UploadSession.prototype.uploadFiles = function () {
            if (this.deferredUpload) {
                throw new Error('UploadSession: Attempted upload a session a second time.');
            }

            this.deferredUpload = $q.defer();

            if (this.files.length === 0) {      // if there is nothing to upload, then the promise is already completed.
                this.deferredUpload.resolve({});
            }

            //this.deferredUpload.reject('ffffff');


            if (this.uploader) { // we are in a position to upload
                startUpload(this);
            } else {
                console.log("UploadSession: we are not yet in a position to start the uploade, the uploader has not been set. The promise will wait until one has been set.");
            }

            return this.deferredUpload.promise;
        };

        UploadSession.prototype.attachUploader = function (newUploader) {
            var that = this;

            if (this.uploader === newUploader) { // attaching to the same uploader so ignore
                return;
            }

            function safeApply() {
                if (!$rootScope.$$phase) {
                    $rootScope.$apply();
                }
            }

            // for information on these settings https://github.com/blueimp/jQuery-File-Upload/wiki/Options
            var uri = spWebService.getWebApiRoot() + '/spapi/data/v2/file';
            if (that.expectedType) {
                uri += '?type=' + that.expectedType;
            }
            var uploadUrl = spXsrf.addXsrfTokenAsQueryString(uri);
            newUploader.fileupload({
                dataType: 'json',
                url: uploadUrl,
                //maxChunkSize: 10000000, // 10 MB

                add: function (e, data) {
                    if (!that.allowMultipleFiles) {
                        that.clear();
                    }

                    that.files.push(data);
                    updateFileNames(that);
                    safeApply();
                },
                clear: function () {
                    that.clear();
                    safeApply();
                },
                files: function () {
                    return that.fileNames;
                },
                upload: function () {
                    _.each(that.files, function (file) {
                        file.submit();
                    });
                    that.clear();
                    safeApply();
                },
                progressall: function (e, data) {
                    that.progress = parseInt(data.loaded / data.total * 100, 10);
                    safeApply();
                },
                done: function (e, data) {
                    console.log("spFileUpload: Upload succeeded.");

                    that.progress = 100;

                    that.deferredUpload.resolve(data.result);
                    safeApply();
                },
                fail: function (e, data) {                    
                    console.error("spFileUpload: Upload failed.", data);
                    //about bug 28027 if file upload fails the error message is poor
                    //use alertService to raise error message, otherwise the form error message will be raisedn by empty content.
                    var msg = "File upload failed.";
                    if (sp.result(data, "jqXHR.status") === 403) {
                        msg = "File type was disallowed. Configure file types in administration.";
                    }
                    if (!that.suppressErrors) {                        
                        spAlertsService.addAlert(msg, { severity: spAlertsService.sev.Error });
                    }
                    that.progress = 100;
                    that.deferredUpload.reject(data.errorThrown);
                    safeApply();
                }
            });

            this.uploader = newUploader;


            if (this.deferredUpload) {
                startUpload(this);
            }
        };

        function startUpload(session) {
            _.map(session.files, function (file) {
                file.submit();
            });
        }

        function updateFileNames(session) {
            session.fileNames.length = 0;
            _.each(session.files, function (file) {
                session.fileNames.push(file.files[0].name);
            });
        }

        function createUploadSession(expectedType) {
            var session = new UploadSession();
            session.expectedType = expectedType; // can be undefined
            return session;
        }

        return exports;
    }

    /* @ngInject */
    function fileUploadDirective(spUploadManager, $window) {
        return {
            restrict: 'E',
            scope: {
                uploadSession: '=',
                busyIndicatorOptions: '=',
                message: '=',
                imagePreviewSrc: '=?',
                filter: '@',
                callback: '=',
                instantUpload: '=?',
                showFileName: '=?',
                buttonLabel: '=?',
                buttonImageUrl: '=?',
                displayName: '=?'
            },
            templateUrl: 'fileUpload/spFileUpload.tpl.html',
            link: function (scope, element, attrs) {


                scope.fileDisplayName = scope.displayName ? scope.displayName : '';
                scope.btnLabel = 'Upload';
                scope.btnImageUrl = null;
                var uploader = $(element);

                scope.getFilter = function getFilter() {
                    // To make the upload function working in MS Edge, disable the spreetSheet filter now. Remove if MS fixes the Edge bug.
                    var isEdgeBrowser = $window.navigator.userAgent.indexOf('Edge') > -1;
                    return isEdgeBrowser ? '' : scope.filter;
                };


                scope.$watch('uploadSession', function (value) {

                    if (value) {
                        value.attachUploader(uploader);
                    }

                });

                // as soon as there is a file, upload it
                scope.$watch('uploadSession.fileNames[0]', function (value) {
                    if (value) {

                        // reset error message and image preview source
                        reset();

                        // update file display name
                        updateDisplayName(value);

                        // upload file
                        if (angular.isUndefined(scope.instantUpload) || scope.instantUpload === true) {

                            scope.upload();

                            // show progress bar
                            scope.busyIndicatorOptions.isBusy = true;
                        }

                        // generate image preview
                        if (scope.filter && scope.filter === sp.imageFileTypeFilter) {
                            generateImagePreview();
                        }
                    }
                });

                scope.$watch('uploadSession.progress', function (value) {
                    if (value && scope.busyIndicatorOptions) {
                        scope.busyIndicatorOptions.percent = value;
                    }
                });

                scope.$watch('buttonLabel', function (value) {
                    if (value) {
                        scope.btnLabel = value;
                    }
                });

                scope.$watch('buttonImageUrl', function (value) {
                    if (value) {
                        scope.btnImageUrl = value;
                    }
                });

                scope.$watch('filter', function (value) {
                    // To make the upload function working in MS Edge, disable the spreetSheet filter now. Remove if MS fixes the Edge bug.
                    var isEdgeBrowser = $window.navigator.userAgent.indexOf('Edge') > -1;
                    scope.filterEx = isEdgeBrowser ? '' : scope.filter;
                });

                scope.upload = function () {
                    scope.uploadSession.uploadFiles().then(
                        function (result) {
                            if (angular.isDefined(scope.message)) {
                                scope.message = 'Success: ' + result[0].fileName;
                            }
                            if (angular.isDefined(scope.callback)) {
                                scope.callback(result[0].fileName, result[0].hash);
                                console.log('filename: ', result[0].fileName);
                            }
                        }, function (error) {
                            if (angular.isDefined(scope.message)) {
                                scope.errorMessage = scope.message = 'Failed with error: ' + error;

                                //DO NOT window.alert!
                                //alert('Failed with error: ' + error);
                                console.error('Failed with error: ' + error);

                                // reset the file display name
                                updateDisplayName('');
                            }
                        })
                        .finally(function () { // recreate the session so we can send again
                            scope.uploadSession = spUploadManager.createUploadSession(scope.uploadSession.expectedType);

                            // hide progress bar
                            scope.busyIndicatorOptions.isBusy = false;
                        });
                    scope.$on("resetFileName", function (event) {
                        updateDisplayName('');
                    });
                };

                function updateDisplayName(name) {
                    scope.fileDisplayName = name;
                }

                function generateImagePreview() {
                    if (scope.uploadSession && scope.uploadSession.fileNames && scope.uploadSession.fileNames.length > 0) {
                        var file = scope.uploadSession.files[0].files[0];

                        var reader = new FileReader();

                        reader.onload = function (e) {

                            if (angular.isDefined(scope.imagePreviewSrc)) {
                                scope.$apply(function () {
                                    scope.imagePreviewSrc = e.target.result;
                                });
                            }
                        };

                        reader.readAsDataURL(file);
                    }
                }

                function reset() {
                    scope.errorMessage = '';
                    scope.imagePreviewSrc = null;
                }
            }
        };
    }
}());