// Copyright 2011-2015 Global Software Innovation Pty Ltd
/*global angular, console, $, _, html2canvas, Globalize, FileReader */

(function () {
    'use strict';

    angular.module('mod.feedback', [
        'sp.common.loginService', 'mod.common.spWebService', 'mod.common.spEntityService',
        'mod.common.alerts', 'mod.featureSwitch'
    ]);

    angular.module('mod.feedback')
        .directive('spFeedback', spFeedbackDirective)
        .controller('FeedbackFormController', FeedbackFormController)
        .factory('spFeedbackService', spFeedbackService)
        .directive('spFeedbackFileUpload', spFeedbackFileUpload);

    /* @ngInject */
    function spFeedbackService($document, $http, $rootScope, $q, $location, $timeout,
                               spLoginService, spWebService, spAlertsService, spEntityService,
                               rnFeatureSwitch) {

        var consoleListenersAdded = false;

        var exports = {
            messages: [],
            showForm: false,
            screenshotCanvas: null,
            log: log,
            error: error,
            postFeedback: postFeedback,
            requestEmail: requestEmail
        };

        init();

        return exports;

        function init() {
            $rootScope.$on('sp.showFeedbackForm', showFeedbackForm);
            $rootScope.$on('sp.enableLogCapture', enableLogCapture);

            // always listen for errors and post directly to the server
            console.addListener('error', error);

            // turn on log capture too ready for when we post an issue/do feedback/submit a bug/whatever
            if (rnFeatureSwitch.isFeatureOn('submitABug')) {
                enableLogCapture();
            }
        }

        function log() {
            exports.messages.push(messageFromArguments.apply(null, arguments));
            if (exports.messages.length > 300) {
                exports.messages = exports.messages.slice(exports.messages.length - 50);
            }
        }

        function error() {
            return postError.apply(null, arguments);
        }

        function messageFromArguments() {
            var dateStr = Globalize.format(new Date(), 'yyyy-MM-dd HH:mm:ssz');
            return _.reduce(_.flattenDeep(arguments), (result, e) => result + e, dateStr + ': ');
        }

        function postError() {
            if (spLoginService.getAuthenticatedIdentity()) {
                var message = messageFromArguments.apply(null, arguments);
                var url = spWebService.getWebApiRoot() + '/spapi/data/v1/console/reporterror?error=' + message;
                var args = {headers: spWebService.getHeaders()};
                $http.get(url, args);
            }
        }

        function postFeedback({email, phone, comments, attachments}) {
            if (!spLoginService.getAuthenticatedIdentity())
                return;

            attachments = _.map(attachments, function (a) {
                var nameParts = a.name.split('.');
                return {
                    name: _.initial(nameParts).join('.'),
                    ext: _.last(nameParts),
                    data: a.data && a.data.replace(/^data[:]image\/(png|jpg|jpeg)[;]base64,/i, '') || ''
                };
            });

            comments = (comments || 'user submitted issue') + '\r\n\r\n' + $location.absUrl();
            var body = $document[0].body;
            comments += '\r\nplatform version: ' +
                body.getAttribute('data-server-version') + ' ' +
                body.getAttribute('data-client-version');

            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/console/feedback';
            var args = {headers: spWebService.getHeaders()};
            var imageDataUrl = exports.screenshotCanvas ? exports.screenshotCanvas.toDataURL() : null;
            var imageData = imageDataUrl && imageDataUrl.replace(/^data[:]image\/(png|jpg|jpeg)[;]base64,/i, '') || '';

            var data = {
                email: email,
                phone: phone,
                comments: comments,
                messages: exports.messages,
                attachments: attachments.concat([{name: 'automatic-screenshot', data: imageData, ext: 'png'}])
            };

            $http.post(url, data, args)
                .then(result => {
                    console.log('Submitting feedback done: ' + result);
                    spAlertsService.addAlert('Issue submitted. You should receive a follow up email shortly.');
                })
                .catch(result =>
                    console.log('error submitting feedback: ' + (result && result.status || ''))
                );
        }

        /**
         * Return a promise for the email address for the given person.
         */
        function requestEmail() {
            if (!spLoginService.getAuthenticatedIdentity() || !spLoginService.accountHolderId)
                return;

            return spEntityService.getEntity('core:person', 'fields.name')
                .then(function (personType) {
                    var field = _.find(personType.fields, f => f.name === 'Business email');
                    return field && spEntityService.getEntity(spLoginService.accountHolderId, 'name,#' + field.idP)
                            .then(e => {
                                var email = e.getField(field.idP);
                                return email;
                            });
                });
        }

        function enableLogCapture() {
            //todo - support turning off too!

            if (!consoleListenersAdded) {
                consoleListenersAdded = true;

                // for this to work we have monkey-patched console...
                if (console.addListener) {
                    console.addListener('log', log);
                    console.addListener('warn', log);
                    console.addListener('error', log);
                }
            }
        }

        function showFeedbackForm() {
            setAppData('feedbackLabel', 'working...');
            // use $timeout so the UI gets updated
            $timeout(() => {
                $q.when(showFeedbackFormImpl())
                    .finally(() => setAppData('feedbackLabel', 'Submit issue/Feature request'));
            });
        }

        function setAppData(key, value) {
            $rootScope.appData = $rootScope.appData || {};
            $rootScope.appData[key] = value;
        }

        function showFeedbackFormImpl() {

            // ensure capture is enabled.... need to work out a better way for this all to hang together....
            enableLogCapture();

            if (exports.showForm) return;

            exports.screenshotCanvas = null;

            // take screenshot before showing any kind of feedback form
            console.warn('Please ignore any immediately following messages about refusing to load images');

            // use $q.when to ensure is a promise of the type we expect
            return $q.when(takeScreenshot()).then(canvas => {
                exports.screenshotCanvas = canvas;
                exports.showForm = true;
            }).finally(() => {
                console.timeEnd('screenshot');
            });
        }

        function takeScreenshot() {
            if (!html2canvas)
                return '';

            return html2canvas(document.body);
        }
    }

    /* @ngInject */
    function spFeedbackDirective() {
        return {
            restrict: 'E',
            templateUrl: 'feedback/feedback.tpl.html',
            controller: FeedbackFormController,
            controllerAs: 'ctrl',
            bindToController: true,
            scope: {}
        };
    }

    /* @ngInject */
    function FeedbackFormController($scope, spFeedbackService, $q) {

        $scope.model = null; // initialised in reset()

        $scope.isVisible = isVisible;
        $scope.getStyle = getStyle;
        $scope.cancelClick = cancelClick;
        $scope.submitClick = submitClick;

        $scope.$watch(() => spFeedbackService.showForm, showFormChanged);
        $scope.$on('feedbackFileUploadSelected', feedbackFileUploadSelected);

        reset();

        function reset() {
            $scope.model = {
                comments: '',
                email: '',
                phone: '',
                attachments: []
            };
        }

        function showFormChanged(show) {
            if (show) {
                reset();
                $q.when(spFeedbackService.requestEmail())
                    .then(email => $scope.model.email = email);
            }
        }

        function feedbackFileUploadSelected(event, selectedFile) {
            if (selectedFile) {
                var fileReader = new FileReader();
                fileReader.onloadend = e => {
                    $scope.model.attachments.push({name: selectedFile.name, data: e.target.result});
                };
                fileReader.readAsDataURL(selectedFile);
            }
        }

        function cancelClick() {
            spFeedbackService.showForm = false;
            reset();
        }

        function submitClick() {
            spFeedbackService.postFeedback($scope.model);
            spFeedbackService.showForm = false;
            reset();
        }

        function getStyle() {
            var style = {};

            if (!isVisible()) return style;

            style.height = ($(window).height() / 2) + 350 + 'px';
            style.width = ($(window).width() / 2) + 'px';

            return style;
        }

        function isVisible() {
            return spFeedbackService.showForm;
        }
    }

    /* @ngInject */
    function spFeedbackFileUpload() {
        return {
            scope: {},
            link: link
        };

        function link(scope, el, attrs) {
            el.bind('change', function (event) {
                var files = event.target.files;
                for (var i = 0; i < files.length; i++) {
                    scope.$emit("feedbackFileUploadSelected", files[i]);
                }
            });
        }
    }

}());