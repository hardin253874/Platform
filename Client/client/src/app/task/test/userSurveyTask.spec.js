// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Console|userSurveyTask|spec:', function() {
    "use strict";

    var controller;
    var $scope;
    var _taskSingleSectionSingleQuestion;
    var _taskTwoSectionSingleQuestions;

    beforeEach(module('mod.app.userSurveyTask'));

    beforeEach(inject(function (spUserSurveyTaskService) {
       var section1 = createSection('Section 1');
       var section2 = createSection('Section 2');


       _taskSingleSectionSingleQuestion = createTask([createTextAnswer(section1)]);
       _taskTwoSectionSingleQuestions = createTask([createTextAnswer(section1), createTextAnswer(section2)]);

    }));

    function createTask(answers) {
        var result = spEntity.fromJSON({
            typeId: 'userSurveyTask',
            name: 'dummy task',
            userTaskIsComplete: false,
            userSurveyTaskSurveyResponse: {
                surveyStartedOn: jsonString(null),
                surveyCompletedOn: jsonString(null),
                surveyAnswers: [ jsonString(null)],
                campaignForResults: {
                    surveyClosesOn: jsonString(null)
                }
            }
        });

        result.userSurveyTaskSurveyResponse.surveyAnswers = answers;

        return result;
    }

    function createTextAnswer(section, order, questionId) {
        var result = spEntity.fromJSON({
            questionBeingAnswered: {
                typeId: 'core:textQuestion',
                name: 'Question 1',
                questionId: questionId ? questionId : jsonString(null),
                questionOrder: order ? order : jsonString(null)
            },
            questionInSection: jsonString(null),
            surveyAnswerString: jsonString(null),
            surveyAnswerNumber: jsonString(null),
            surveyAnswerSingleChoice: jsonLookup(),
            surveyAnswerMultiChoice: [],
            surveyAnswerNotes: jsonString(null),
            surveyAnswerAttachments: jsonString(null)
        });

        result.questionInSection = section;

        return result;
    }

    function createSection(sectionName, order) {
        return spEntity.fromJSON({
            typeId: 'userSurveySection',
            name: sectionName,
            surveySectionOrder: order ? order : jsonString(null)
        });
    }

    describe('spUserSurveyTaskService', function () {

        it('spUserSurveyTaskService  was created ok', inject(function (spUserSurveyTaskService) {
            expect(spUserSurveyTaskService).toBeTruthy();
        }));

        it('createDisplayStructure from single section single question', inject(function (spUserSurveyTaskService) {
            var displayStructure = spUserSurveyTaskService.createDisplayStructure(_taskSingleSectionSingleQuestion);
            expect(displayStructure).toBeTruthy();
            expect(displayStructure.length).toBe(1);
            expect(displayStructure[0].answers).toBeTruthy();
            expect(displayStructure[0].answers.length).toBe(1);
        }));

        it('createDisplayStructure from two sections single questions', inject(function (spUserSurveyTaskService) {
            var displayStructure = spUserSurveyTaskService.createDisplayStructure(_taskTwoSectionSingleQuestions);
            expect(displayStructure).toBeTruthy();
            expect(displayStructure.length).toBe(2);
            expect(displayStructure[0].answers).toBeTruthy();
            expect(displayStructure[0].answers.length).toBe(1);
            expect(displayStructure[1].answers).toBeTruthy();
            expect(displayStructure[1].answers.length).toBe(1);
        }));


        it('createDisplayStructure sections ordered by name', inject(function (spUserSurveyTaskService) {
            var sectionA = createSection('A');
            var sectionB = createSection('B');
            var sectionC = createSection('C');

            var task = createTask([createTextAnswer(sectionA), createTextAnswer(sectionC), createTextAnswer(sectionB)]);
            var displayStructure = spUserSurveyTaskService.createDisplayStructure(task);

            expect(displayStructure[0].name).toBe('A');
            expect(displayStructure[1].name).toBe('B');
            expect(displayStructure[2].name).toBe('C');
        }));

        it('createDisplayStructure sections ordered by order', inject(function (spUserSurveyTaskService) {
            var section1 = createSection('A', 1);
            var section2 = createSection('C', 2);
            var section3 = createSection('B', 3);

            var task = createTask([createTextAnswer(section1), createTextAnswer(section3), createTextAnswer(section2)]);
            var displayStructure = spUserSurveyTaskService.createDisplayStructure(task);

            expect(displayStructure[0].name).toBe('A');
            expect(displayStructure[1].name).toBe('C');
            expect(displayStructure[2].name).toBe('B');
        }));

        it('createDisplayStructure questions ordered by order', inject(function (spUserSurveyTaskService) {
            var section1 = createSection('A', 1);
            var answer1 = createTextAnswer(section1, 1, 'q3');
            var answer2 = createTextAnswer(section1, 2, 'q2');
            var answer3 = createTextAnswer(section1, 3, 'q1');

            var task = createTask([answer1, answer3, answer2]);
            var displayStructure = spUserSurveyTaskService.createDisplayStructure(task);

            expect(displayStructure[0].answers[0].idP).toBe(answer1.idP);
            expect(displayStructure[0].answers[1].idP).toBe(answer2.idP);
            expect(displayStructure[0].answers[2].idP).toBe(answer3.idP);
        }));

        it('createDisplayStructure questions ordered by questionId', inject(function (spUserSurveyTaskService) {
            var section1 = createSection('A', 1);
            var answer1 = createTextAnswer(section1, null, 'q1');
            var answer2 = createTextAnswer(section1, null, 'q2');
            var answer3 = createTextAnswer(section1, null, 'q3');

            var task = createTask([answer1, answer3, answer2]);
            var displayStructure = spUserSurveyTaskService.createDisplayStructure(task);

            expect(displayStructure[0].answers[0].idP).toBe(answer1.idP);
            expect(displayStructure[0].answers[1].idP).toBe(answer2.idP);
            expect(displayStructure[0].answers[2].idP).toBe(answer3.idP);
        }));
    });



    //
    // Controller
    //

    describe('UserSurveyTaskController', function () {


        beforeEach(inject(function ($injector, $controller, $rootScope, $stateParams, $q, spUserSurveyTaskService) {

            spUserSurveyTaskService.getTask = function (taskId) {
                return $q.when(_taskSingleSectionSingleQuestion);
            };
            $scope = $rootScope.$new();
            $stateParams.eid = '111';
            controller = $controller('UserSurveyTaskController', { $scope: $scope, spUserSurveyTaskService: spUserSurveyTaskService });

        }));


        // test is not great. replacing a method on the service doesn't really work
        it('userSurveyTask controller was created ok', inject(function () {
            console.log('controller scope', $scope);
            expect(controller).toBeTruthy();
        }));
    });
});
