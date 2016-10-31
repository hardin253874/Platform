// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spQuestionAttachments', spQuestionAttachments);
            
            
    function spQuestionAttachments(spDialogService, spUploadManager, spXsrf) {
        return { 
            restrict: 'E',
            replace: true,
            scope: {
                attachments: '=',
                readOnly: '='
            },
            templateUrl: 'task/directives/spQuestionAttachments.tpl.html',
            link: function (scope, elem, attrs) {

                var dialogModel = {};

                scope.deleteAttachment = deleteAttachment;
                scope.getAttachmentUrl = getAttachmentUrl;


                // set up the upload button
                scope.fileUploadSession = spUploadManager.createUploadSession();
                scope.fileUploadMessage = "Uploading";

                scope.fileUploadBusyIndicatorOptions = {
                    type: 'window',
                    text: 'Uploading...',
                    placement: 'element',
                    isBusy: false,
                    percent: 0
                };

                scope.uploadButtonLabel = 'Browse';
                //scope.uploadDisplayName = '';
                //scope.downloadUri = '';
                scope.fileUpdateImagePreviewSrc = '';
                scope.fileUploadOnFileUploadComplete = createAttachment;

                function deleteAttachment(attachment) {
                    scope.attachments.deleteEntity(attachment);
                }

                function createAttachment(fileName, fileUploadId) {
                    var attachment = spEntity.fromJSON({
                        typeId: 'core:surveyAttachment',
                        name: fileName,
                        fileDataHash: fileUploadId
                    });

                    scope.attachments.add(attachment);

                    return attachment;
                }

                function getAttachmentUrl(attachment) {
                    return spXsrf.addXsrfTokenAsQueryString('/spapi/data/v2/file/' + attachment.fileDataHash);
                }
            }
        };
    }
    

}());