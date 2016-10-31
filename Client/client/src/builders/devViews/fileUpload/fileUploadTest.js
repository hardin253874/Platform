// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.fileUploadTest', ['sp.common.fileUpload'])
        .controller('FileUploadCtrl',
            ['$scope', 'spUploadManager',
                function($scope, uploadManager) {

                    $scope.uploadSession = uploadManager.createUploadSession();
                    $scope.message = "";
                    $scope.imagePreviewSrc = null;
                    $scope.spreadSheetFilter = spUtils.spreadsheetFileTypeFilter;// 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,.csv';
                    $scope.imageFilter = spUtils.imageFileTypeFilter;// 'image/*';
                    
                    $scope.busyIndicatorOptions = {
                        type: 'progressBar',
                        text: 'Uploading...',
                        placement: 'window',
                        isBusy: false,
                        percent:0
                    };
                    
                    $scope.onFileUploadComplete = function(fileName, fileUploadId) {
                        $scope.fileName = fileName;
                        $scope.fileId = fileUploadId;
                    };
                    
                    
                    // image upload
                    $scope.imageUploadSession = uploadManager.createUploadSession();
                    $scope.imageUploadMessage = "";
                    $scope.imageUploadImagePreviewSrc = null;
                    $scope.imageUploadFilter = spUtils.imageFileTypeFilter;// 'image/*';


                    $scope.imageUploadBusyIndicatorOptions = {
                        type: 'progressBar',
                        text: 'Uploading...',
                        placement: 'window',
                        isBusy: false,
                        percent: 0
                    };

                    $scope.imageUploadOnFileUploadComplete = function (fileName, fileUploadId) {
                        $scope.imageUploadFileName = fileName;
                        $scope.imageUploadFileId = fileUploadId;
                    };
                    
                    
                    // file upload
                    $scope.fileUploadSession = uploadManager.createUploadSession();
                    $scope.fileUploadMessage = "";
                    $scope.fileUploadImagePreviewSrc = null;

                    $scope.fileUploadBusyIndicatorOptions = {
                        type: 'progressBar',
                        text: 'Uploading...',
                        placement: 'element',
                        isBusy: false,
                        percent: 0
                    };

                    $scope.fileUploadOnFileUploadComplete = function (fileName, fileUploadId) {
                        $scope.fileUploadFileName = fileName;
                        $scope.fileUploadFileId = fileUploadId;
                    };

                    //$scope.upload = function() {
                    //    $scope.uploadSession.uploadFiles().then(
                    //        function(result) {
                    //            $scope.message = 'Success: ' + result[0];

                    //        }, function(error) {
                    //            $scope.message = 'Failed with error: ' + error;
                    //        })
                    //        .finally(function() { // recreate the session so we can send again
                    //            $scope.uploadSession = uploadManager.createUploadSession();

                    //        });
                    //};

                }]);


}());