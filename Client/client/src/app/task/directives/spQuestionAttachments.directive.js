// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask')
            .directive('spQuestionAttachments', spQuestionAttachments);
            
            
    function spQuestionAttachments(spDialogService, spUploadManager, spXsrf, spWebService) {
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
                    var attachmentName = fileName;

                    var attachmentExtension = spUploadManager.getFileExtension(fileName);
                    if (attachmentExtension && _.endsWith(attachmentName, attachmentExtension)) {
                        attachmentName = attachmentName.slice(0, -(attachmentExtension.length));
                    }

                    var attachment = spEntity.fromJSON({
                        typeId: 'core:surveyAttachment',
                        name: attachmentName,
                        fileDataHash: fileUploadId,
                        fileExtension: attachmentExtension
                    });

                    scope.attachments.add(attachment);

                    return attachment;
                }

                function getAttachmentUrl(attachment) {
                    return spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v2/file/' + attachment.fileDataHash);
                }
            }
        };
    }
    

}());