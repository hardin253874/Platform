// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spSurveySectionsEditor', [
        'mod.common.spMobile',
        'mod.common.spEntityService',
        'mod.common.alerts',
        'mod.app.editFormServices',
        'mod.common.ui.spEditFormDialog',
        'sp.common.fieldValidator',
        'mod.app.formBuilder.services.spFormBuilderService',
        'spApps.reportServices',
        'mod.app.navigationProviders',
        'mod.common.spCachingCompile'])
           .directive('spSurveySectionsEditor', function ($q, $parse, $timeout, $location, $anchorScroll, spMobileContext, spEntityService, spAlertsService, spDialogService, spEditForm, spEditFormDialog, spFieldValidator, spFormBuilderService, spReportService, spNavService, spCachingCompile) {
               return {
                   restrict: 'AE',
                   replace: false,
                   transclude: false,
                   scope: {
                       formControl: '=',
                       parentControl: '=?',
                       formData: '=',
                       formMode: '=?',
                       isInTestMode: '=?',
                       isReadOnly: '=?',
                       isInDesign: '=?'
                   },
                   link: function (scope, element) {

                       var co = 'name, description, choiceOptionOrder, choiceOptionValue';
                       var qr = 'name, description, isOfType.{alias, name, inherits.alias}, questionId, questionOrder, questionWeight, questionAllowAttachments, questionAllowNotes, inLibrary.{name, description}, choiceQuestionChoiceSet.{name, choiceOptionSetChoices.{' + co + '}}, questionCategories.{name, description}, numericQuestionIsInteger, choiceQuestionIsMulti';

                       var saving = false;

                       scope.isMobile = spMobileContext.isMobile;
                       scope.canModify = false;
                       scope.canDelete = false;
                       scope.canCreateSections = false;
                       scope.canCreateQuestions = false;
                       scope.canViewLibraries = true; //false;

                       /////
                       // Control setup.
                       /////
                       scope.model = {
                           sections: [],
                           showContextMenu: false,
                           contextMenu: {
                               menuItems: []
                           },
                           dropOptions: {
                               supportTouchEvents: true,
                               onAllowDrop: onAllowDrop,
                               onDragOver: onDragOver,
                               onDragLeave: onDragLeave,
                               onDragEnter: onDragEnter,
                               onDrop: onDrop
                           },
                           dragOptions: {
                               supportTouchEvents: true,
                               onDragStart: onDragStart,
                               onDragEnd: onDragEnd
                           }
                       };

                       scope.relationshipToRender = null;
                       
                       var placeholder;
                       var container;
                       var relLibrary;

                       scope.$watch("formControl", function() {
                           if (scope.formControl) {
                               scope.relationshipToRender = scope.formControl.relationshipToRender;

                               placeholder = angular.element(element.find('.drag-placeholder')[0]);
                               placeholder.remove();

                               container = angular.element(element.find('.ordered-form-rows-area')[0]);

                               if (!relLibrary || relLibrary.id < 0) {
                                   spEntityService.getEntity('core:inLibrary', 'name').then(function (rel) {
                                       if (rel) {
                                           relLibrary = { id: rel.idP, isReverse: true };
                                       }
                                   });
                               }

                               ////
                               //// Check whether it is worth showing the library dialog
                               ////
                               //spReportService.getReportData('core:questionModalReport', { startIndex: 0, pageSize: 1, displayPageSize: 1 }).then(function (data) {
                               //    scope.canViewLibraries = (sp.result(data, 'gdata.length', 0) > 0);
                               //});                                   

                               //
                               // Perform a security check to manually disable features based on user access
                               //
                               var types = ['core:choiceQuestion', 'core:numericQuestion', 'core:textQuestion', 'core:surveySection'];

                               scope.canCreateQuestions = false;
                               scope.model.contextMenu.menuItems.length = 0;

                               spEntityService.getEntities(types, 'alias, canCreateType').then(function (typeEntities) {
                                   if (typeEntities) {
                                       _.forEach(typeEntities, function (t) {
                                           switch (t.nsAlias) {
                                               case 'core:numericQuestion':
                                                   if (t.canCreateType === true) {
                                                       scope.model.contextMenu.menuItems.push({ text: 'Numeric Question', icon: 'assets/images/16x16/add.svg', type: 'click', click: "newQuestion('numeric')" });
                                                   }
                                                   break;
                                               case 'core:textQuestion':
                                                   if (t.canCreateType === true) {
                                                       scope.model.contextMenu.menuItems.push({ text: 'Text Question', icon: 'assets/images/16x16/add.svg', type: 'click', click: "newQuestion('text')" });
                                                   }
                                                   break;
                                               case 'core:choiceQuestion':
                                                   if (t.canCreateType === true) {
                                                       scope.model.contextMenu.menuItems.push({ text: 'Choice Question', icon: 'assets/images/16x16/add.svg', type: 'click', click: "newQuestion('choice')" });
                                                   }
                                                   break;
                                               case 'core:surveySection':
                                                   scope.canCreateSections = t.canCreateType === true;
                                                   break;
                                           }
                                       });
                                   }

                                   scope.canCreateQuestions = sp.result(scope.model, 'contextMenu.menuItems.length', 0) > 0;
                               });
                           }
                       });

                       scope.$watch("formData", function () {
                           if (scope.formData) {
                               scope.canModify = scope.formData.canModify || false;
                               scope.canDelete = scope.formData.canDelete || false;

                               if (scope.formData.dataState === spEntity.DataStateEnum.Create) {

                                   //
                                   // Creates an empty collection ready to accept survey contents
                                   //
                                   createSections();
                               } else {

                                   //
                                   // Retrieves all the components this control is to work with
                                   //
                                   getSections();
                               }
                           }
                       });

                       //
                       // Retrieves the relationship being displayed or edited.
                       //
                       function getRelationship()  {
                           var rel = null;
                           if (scope.relationshipToRender) {
                               var relId = { id: scope.relationshipToRender.eid(), isReverse: false };
                               rel = scope.formData.getRelationship(relId);
                           }
                           return rel;
                       }

                       //
                       // Gets the full information about sections and questions belonging to the survey and adds them to the form data.
                       //
                       function getSections() {
                           return spEntityService.getEntity(scope.formData.idP, 'name, surveySections.{name, isOfType.alias, surveySectionOrder, surveyBreak, surveyPage, surveyQuestions.{' + qr + '}}').then(function (survey) {

                               // attach to the correct graph on these entities for history and change management
                               _.forEach(survey.surveySections, function (section) {
                                   section._setGraph(scope.formData._graph);
                               });

                               scope.formData.setRelationship(scope.relationshipToRender.idP, []);
                               scope.formData.setRelationship(scope.relationshipToRender.idP, survey.surveySections);

                               scope.model.sections = getRelationship();
                           });
                       }

                       //
                       // Creates sections container for adding the contents of the survey.
                       //
                       function createSections() {

                           var relContainer = scope.formData.registerRelationship({ id: scope.relationshipToRender.eid(), isReverse: false });
                           relContainer.autoCardinality();

                           scope.formData.setRelationship(scope.relationshipToRender.idP, []);

                           scope.model.sections = getRelationship();
                       }

                       //
                       // Applies changes made to a question object into the given model.
                       //
                       function applyChanges(model, question) {

                           // Put back into the correct lookup. (See "WHY?!" below)
                           question.inLibrary = model.getLookup(relLibrary);

                           question.name = model.name;
                           question.description = model.description;
                           question.questionId = model.questionId;
                           question.questionOrder = model.questionOrder;
                           question.questionWeight = model.questionWeight;
                           question.questionAllowAttachments = model.questionAllowAttachments;
                           question.questionAllowNotes = model.questionAllowNotes;
                           question.questionCategories = model.questionCategories;

                           var type = sp.result(question.type, 'nsAlias') || sp.result(question.isOfType, '0.nsAlias');
                           
                           if (type === 'core:numericQuestion') {
                               question.numericQuestionIsInteger = model.numericQuestionIsInteger;
                           }

                           if (type === 'core:choiceQuestion') {
                               question.choiceQuestionIsMulti = model.choiceQuestionIsMulti;
                               question.choiceQuestionChoiceSet = model.choiceQuestionChoiceSet;
                               question.choiceQuestionChoiceSet.choiceOptionSetChoices = model.choiceQuestionChoiceSet.choiceOptionSetChoices;
                           }
                       }

                       function updateSurveyPaging() {
                           var page = 1;
                           var sections = [];
                           var setPageFn = function(s) { s.surveyPage = page; };

                           var orderedSections = _.orderBy(scope.model.sections, 'surveySectionOrder');

                           _.each(orderedSections, function (section) {
                               if (section.surveyBreak) {
                                   _.each(sections, setPageFn);
                                   if (sections.length) {
                                       page++;
                                       sections.length = 0;
                                   }
                               } else {
                                   sections.push(section);
                               }
                           });

                           _.each(sections, setPageFn);
                           sections.length = 0;
                       }

                       //
                       // Save the form data for updates that are allow in view mode by this control.
                       //
                       function saveFormDataInViewMode() {
                           updateSurveyPaging();
                           if ((scope.formMode === spEditForm.formModes.view) && (scope.isReadOnly === true) && (scope.canModify === true)) {
                               saving = true;
                               spEditForm.saveFormData(scope.formData).then(getSections, function (error) {
                                   spAlertsService.addAlert(spEditForm.formatSaveErrorMessage(error), { severity: spAlertsService.sev.Error });
                               }).finally(function() {
                                   saving = false;
                               });
                           }
                       }

                       //
                       // Applies changes made to the model back to the question based on the callback return.
                       //
                       function optionallyApplyChanges(model, question, readonly, callback) {
                           var bm = model.graph.history.addBookmark();
                           var res = callback();
                           var handleResult = function (result) {
                               if (result === false) {
                                   bm.undo();
                               } else {
                                   bm.endBookmark();
                                   if (readonly !== true) {
                                       applyChanges(model, question);
                                   }
                               }
                               return result;
                           };
                           if (res && res.then) {
                               // handle promises                    
                               return $q.when(res).then(handleResult);
                           } else {
                               handleResult(res);
                               return null;
                           }
                       }

                       //
                       // Opens the dialog to edit a question
                       //
                       function showQuestionProperties(question, inEditMode) {

                           // Determine which form to load into the modal dialog.
                           var form = null;
                           if (question) {
                               var type = sp.result(question.type, 'nsAlias') || sp.result(question.isOfType, '0.nsAlias');
                               switch (type) {
                                   case 'core:numericQuestion':
                                       form = 'core:numericQuestionModalForm';
                                       break;
                                   case 'core:textQuestion':
                                       form = 'core:textQuestionModalForm';
                                       break;
                                   case 'core:choiceQuestion':
                                       form = 'core:choiceQuestionModalForm';
                                       break;
                               }
                           }

                           if (form) {
                               //
                               // Attempt at client side clone.
                               //
                               var q = spEntity.fromJSON({});
                               
                               spEntity.augment(q, question);
                               q._id._id = question.idP;
                               q.dataState = question.dataState;

                               // WHY?! Forms don't use reverse aliases? BUG: #27736
                               var rel = q.getRelationshipContainer('core:inLibrary');
                               if (rel) {
                                   q.registerLookup(relLibrary);
                                   q.setLookup(relLibrary, q.inLibrary);
                               }

                               var noEdits = ((scope.isReadOnly === true || scope.canModify === false) && !inEditMode);
                               var options = {
                                   title: 'Question Properties',
                                   entity: q,
                                   form: form,
                                   formMode: noEdits ? spEditForm.formModes.view : spEditForm.formModes.edit,
                                   optionsEnabled: false,
                                   saveEntity: false
                               };

                               return optionallyApplyChanges(q, question, noEdits, _.partial(spEditFormDialog.showDialog, options));
                           }

                           return $q.when();
                       }

                       //
                       // Creates a new empty section.
                       //
                       scope.newSection = function (creatingQuestion, sectionName) {
                           if (saving && !creatingQuestion) {
                               return null;
                           }

                           var maxOrder = 0;
                           if (scope.model.sections.length) {
                               maxOrder = _.max(_.map(scope.model.sections, 'surveySectionOrder')) + 1;
                           }

                           var section = spEntity.fromJSON({
                               typeId: 'surveySection',
                               name: jsonString(sectionName),
                               surveySectionOrder: jsonInt(maxOrder),
                               surveyPage: jsonInt(1),
                               surveyQuestions: jsonRelationship()
                           });
                           
                           if (!creatingQuestion) {
                               scope.model.sections.add(section);
                               saveFormDataInViewMode();
                           }

                           var hash = 'survey-section-' + section.idP;

                           $timeout(function () {
                               $location.hash(hash);
                               $anchorScroll();
                           }, 0);

                           return section;
                       };

                       //
                       // Creates a page break section.
                       //
                       scope.newBreak = function() {
                           if (saving) {
                               return null;
                           }

                           var maxOrder = 0;
                           if (scope.model.sections.length) {
                               maxOrder = _.max(_.map(scope.model.sections, 'surveySectionOrder')) + 1;
                           }

                           var sectionBreak = spEntity.fromJSON({
                               typeId: 'surveySection',
                               name: jsonString(),
                               surveySectionOrder: jsonInt(maxOrder),
                               surveyQuestions: jsonRelationship(),
                               surveyBreak: jsonBool(true)
                           });

                           scope.model.sections.add(sectionBreak);
                           saveFormDataInViewMode();

                           var hash = 'survey-break-' + sectionBreak.idP;

                           $timeout(function () {
                               $location.hash(hash);
                               $anchorScroll();
                           }, 0);

                           return sectionBreak;
                       };

                       //
                       // Creates a new question and adds it to the last section.
                       // If no section exists, or if the last section is a page break, it will create one. 
                       //
                       scope.newQuestion = function(type) {
                           var maxOrder = 0;
                           var lastSection = getAddingSection();
                           if (lastSection.surveyQuestions.length) {
                               maxOrder = _.max(_.map(lastSection.surveyQuestions, 'questionOrder')) + 1;
                           }

                           var json = {
                               typeId: type + 'Question',
                               name: jsonString(),
                               description: jsonString(),
                               questionId: jsonString(),
                               questionOrder: jsonDecimal(maxOrder),
                               questionWeight: jsonDecimal(),
                               questionAllowAttachments: jsonBool(),
                               questionAllowNotes: jsonBool(),
                               questionCategories: jsonRelationship(),
                               inLibrary: jsonLookup()
                           };

                           if (type === 'numeric') {
                               json.numericQuestionIsInteger = jsonBool();
                           }

                           if (type === 'choice') {
                               json.choiceQuestionIsMulti = jsonBool();
                               json.choiceQuestionChoiceSet = spEntity.fromJSON({
                                   typeId: 'choiceOptionSet',
                                   name: jsonString('New Set'),
                                   description: jsonString(),
                                   choiceOptionSetChoices: jsonRelationship()
                               });
                           }

                           var question = spEntity.fromJSON(json);

                           showQuestionProperties(question, true).then(function (result) {
                               if (result === true) {
                                   lastSection.surveyQuestions.add(question);
                                   if (!_.includes(scope.model.sections, lastSection)) {
                                       scope.model.sections.add(lastSection);
                                   }
                               }

                               saveFormDataInViewMode();

                               var hash = 'survey-question-' + question.idP;

                               $timeout(function () {
                                   $location.hash(hash);
                                   $anchorScroll();
                               }, 0);
                           });
                           //return question;
                       };

                       //
                       // Removes a section from the survey.
                       //
                       scope.removeSection = function (section) {
                           if (section) {

                               // delete any relevant questions
                               _.forEach(section.surveyQuestions.slice(0), function(question) {
                                   scope.removeQuestion(section, question, true);
                               });

                               // remove and delete the section
                               scope.model.sections.remove(section);

                               // mark section for deletion also
                               section.dataState = spEntity.DataStateEnum.Delete;

                               saveFormDataInViewMode();
                           }
                       };

                       //
                       // Removes a question from the survey 
                       //
                       scope.removeQuestion = function (section, question, removingSection) {
                           if (question) {
                               if (question.inLibrary) {

                                   // just remove
                                   if (section && section.surveyQuestions) {
                                       section.surveyQuestions.remove(question);
                                   }

                               } else {

                                   // delete
                                   if (section && section.surveyQuestions) {
                                       section.surveyQuestions.deleteEntity(question);
                                   } else {
                                       question.dataState = spEntity.DataStateEnum.Delete;
                                   }
                               }

                               if (!removingSection) {
                                   saveFormDataInViewMode();
                               }
                           }
                       };

                       //
                       // Edits the properties of a given question in a modal dialog.
                       //
                       scope.editQuestion = function(question) {
                           showQuestionProperties(question, true).then(saveFormDataInViewMode);
                       };

                       //
                       // Opens a picker for questions that belong to a library.
                       //
                       scope.addQuestion = function () {
                           var modalInstanceCtrl = ['$scope', '$uibModalInstance', 'outerOptions', function ($scope, $uibModalInstance, outerOptions) {
                               $scope.model = {
                                   reportOptions: outerOptions
                               };
                               $scope.ok = function () {
                                   $scope.isModalOpened = false;
                                   $uibModalInstance.close($scope.model.reportOptions);
                               };
                               $scope.$on('spReportEventGridDoubleClicked', function (event) {
                                   event.stopPropagation();
                                   $scope.ok();
                               });
                               $scope.cancel = function () {
                                   $scope.isModalOpened = false;
                                   $uibModalInstance.dismiss('cancel');
                               };
                               $scope.model.reportOptions.cancelDialog = $scope.cancel;
                               $scope.isModalOpened = true;
                           }];

                           // store which questions are already included
                           var existing = _(scope.model.sections).flatMap('surveyQuestions').filter(function(q) {
                               return q.inLibrary;
                           }).value();

                           var libraryReportOptions = {
                               reportId: 'core:questionModalReport',
                               multiSelect: true,
                               isEditMode: false,
                               newButtonInfo: {},
                               isInPicker: true,
                               isMobile: scope.isMobile,
                               fastRun: true
                           };

                           var defaults = {
                               templateUrl: 'entityPickers/entityCompositePicker/spEntityCompositePickerModal.tpl.html',
                               controller: modalInstanceCtrl,
                               windowClass: 'modal inlineRelationPickerDialog',
                               resolve: {
                                   outerOptions: function () {
                                       return libraryReportOptions;
                                   }
                               }
                           };

                           var options = {};
                           
                           spDialogService.showDialog(defaults, options).then(function (result) {
                               if (libraryReportOptions.selectedItems) {

                                   // discover the newly added questions
                                   var added = _(libraryReportOptions.selectedItems).differenceWith(existing, function(s, e) {
                                       return s.eid === e.idP;
                                   }).map('eid').value();

                                   if (added.length) {
                                       spEntityService.getEntities(added, qr).then(function (questionEntities) {
                                           if (questionEntities && questionEntities.length) {

                                               var addFn = function(q) {
                                                   var maxOrder = 0;

                                                   // insert all added question into the appropriate sections
                                                   var addToSection = getAddingSection(q);
                                                   if (addToSection.surveyQuestions.length) {
                                                       maxOrder = _.max(_.map(addToSection.surveyQuestions, 'questionOrder')) + 1;
                                                   }

                                                   q.questionOrder = maxOrder;
                                                   addToSection.surveyQuestions.add(q);

                                                   if (!_.includes(scope.model.sections, addToSection)) {
                                                       scope.model.sections.add(addToSection);
                                                   }
                                               };

                                               // do questions with no categories or more than one category first so they get moved into the last section or a single new one
                                               _.each(_.filter(questionEntities, function (q) { return q && (!q.questionCategories || q.questionCategories.length !== 1); }), addFn);
                                               _.each(_.filter(questionEntities, function (q) { return q && q.questionCategories && q.questionCategories.length === 1; }), addFn);
                                               
                                               saveFormDataInViewMode();
                                           }
                                       });
                                   }
                               }
                           });
                       };
                       
                       //
                       // Gets the section that a question belongs to.
                       //
                       function getSection(question) {
                           return _.find(scope.model.sections, function (s) {
                               return s.surveyQuestions.indexOf(question) >= 0;
                           });
                       }

                       //
                       // Gets or creates the section that the given question should be added to.
                       //
                       function getAddingSection(question) {
                           var section = _.maxBy(scope.model.sections, 'surveySectionOrder');
                           
                           // if adding a library question, that has a single (1) category, then move it to or create
                           // a section that reflects that category.
                           if (question && question.inLibrary && question.questionCategories.length === 1) {
                               var sectionName = sp.result(question.questionCategories, '0.name');

                               if (sectionName && sectionName.length) {
                                   section = _.find(scope.model.sections, function(s) { return s.name && s.name === sectionName; }) || scope.newSection(true, sectionName);
                               }
                           } else {

                               if (!section || section.surveyBreak) {
                                   section = scope.newSection(true);
                               }
                           }
                           
                           return section;
                       }

                       /////
                       // Support callbacks for editable labels.
                       /////
                       scope.isValidSectionName = function(oldName, newName) {
                           if (newName && oldName && newName.toLowerCase() === oldName.toLowerCase()) {
                               return true;
                           }
                           if (!newName) {
                               return false;
                           }
                           return true;
                       };

                       scope.filterSectionNameCharacters = function(event) {
                           var e = event.originalEvent || event;
                           if (e.shiftKey) {
                               switch (e.which) {
                                   case 188: // <
                                   case 190: // >
                                       e.stopPropagation();
                                       e.preventDefault();
                                       return false;
                               }
                           }
                           return true;
                       };

                       scope.changeSectionNameValidate = function (value) {
                           if (value) {
                               return value.replace(/[<>]+/g, '');
                           }
                           return value;
                       };

                       scope.updateSectionName = function() {
                           saveFormDataInViewMode();
                       };

                       /////
                       // Drag n Drop.
                       /////
                       var dropBefore = false;
                       var dropTargets = []; // shouldn't get too big(?)

                       //
                       // Is the node element marked as an ordered row
                       //
                       function isRow(node) {
                           if (!node || !node.classList) {
                               return false;
                           }
                           return node.classList.contains('ordered-form-row');
                       }

                       function isRowContainer(node) {
                           if (!node || !node.classList) {
                               return false;
                           }
                           return node.classList.contains('ordered-form-rows');
                       }

                       function isRowContainerArea(node) {
                           if (!node || !node.classList) {
                               return false;
                           }
                           return node.classList.contains('ordered-form-rows-area');
                       }

                       function isSection(data) {
                           if (!data || (!data.isOfType && !data.type)) {
                               return false;
                           }

                           if (sp.result(data.type, 'nsAlias') === 'core:surveySection') {
                               return true;
                           }

                           var aliases = _.map(data.isOfType, 'nsAlias');

                           //console.log(aliases.join());

                           return aliases.indexOf('core:surveySection') >= 0;
                       }

                       function isQuestion(data) {
                           if (!data || (!data.isOfType && !data.type)) {
                               return false;
                           }

                           var t = sp.result(data.type, 'nsAlias');
                           if ((t === 'core:numericQuestion') || (t === 'core:textQuestion') || (t === 'core:choiceQuestion')) {
                               return true;
                           }

                           var aliases = [];
                           _.forEach(data.isOfType, function(t) {
                               aliases = _.map(t.inherits, 'nsAlias').concat(aliases);
                               aliases.push(t.nsAlias);
                           });

                           //console.log(aliases.join());

                           return aliases.indexOf('core:surveyQuestion') >= 0;
                       }

                       //
                       // Examines and compares both drag and drop information to determine if a drop is permitted.
                       //
                       function canDrop(source, target, dragData, dropData) {
                           if (!scope.canModify) {
                               return false;
                           }

                           var isArea = isRowContainerArea(target);

                           if (!dragData || !dropData || dragData === dropData) {
                               return isArea === true;
                           }

                           var section = isSection(dragData);
                           var question = isQuestion(dragData);
                           var toSection = isSection(dropData);
                           var toQuestion = isQuestion(dropData);

                           // dragging a question row
                           if (isRow(source[0]) && question) {

                               // may be dropped on a question or a section (if the section is not a page break)
                               return (isRow(target) && toQuestion) || (isRowContainer(target) && toSection && !dropData.surveyBreak);
                           } else {

                               // dragging a section container
                               if (isRowContainer(source[0]) && section) {

                                   // may be dropped on a question, section or the encompassing area
                                   return (isRow(target) && toQuestion) || (isRowContainer(target) && toSection) || isArea;
                               }
                           }

                           return false;
                       }

                       //
                       // Should any insert go before or after the target node
                       //
                       function shouldInsertBefore(event, target) {
                           var mY = event.offsetY || event.layerY;
                           var tH = target.clientHeight;
                           var tY = target.clientTop;
                           //console.log('mY:' + mY + ' tY:' + tY + ' tH:' + tH);
                           dropBefore = mY < ((tY + tH) / 2);
                           return dropBefore;
                       }

                       function onAllowDrop(source, target, dragData, dropData) {
                           return canDrop(source, target, dragData, dropData);
                       }

                       function onDragStart(event, data) {
                           if (!scope.canModify) {
                               return;
                           }

                           var evt = event.originalEvent || event;
                           if (evt && evt.currentTarget) {
                               var height = evt.currentTarget.clientHeight;

                               var dragging = $(evt.currentTarget);
                               
                               $timeout(function() {
                                   dragging.addClass("drag-source");

                                   placeholder.height(height);
                               }, 0);

                               evt.currentTarget.parentNode.insertBefore(placeholder[0], evt.currentTarget.nextSibling);
                           }

                           evt.stopPropagation();
                       }

                       function onDragEnd(event, data) {
                           var evt = event.originalEvent || event;
                           if (evt && evt.currentTarget) {
                               var dragging = $(evt.currentTarget);
                               $timeout(function () { dragging.removeClass("drag-source"); }, 0);
                           }

                           if (placeholder) {
                               placeholder.remove();
                           }

                           evt.stopPropagation();
                       }
                       
                       function onDragEnter(event, source, target, dragData, dropData) {
                           if (dropTargets.indexOf(target) < 0) {
                               dropTargets.push(target);
                           }

                           if (dropTargets.indexOf(container[0]) < 0) {
                               dropTargets.push(container[0]);
                           }

                           if (canDrop()) {
                               var evt = event.originalEvent || event;
                               evt.preventDefault();
                           }
                       }

                       function onDragLeave(event, source, target, dragData, dropData) {
                           //if (target === container[0]) {
                           //    console.log('leaving outer container');
                           //}

                           var i = dropTargets.indexOf(target);
                           if (i >= 0) {
                               dropTargets.splice(i, 1);
                           }

                           var dropping = $(target);
                           dropping.removeClass("drag-target");
                           
                           if (!dropTargets.length) {
                               placeholder.remove();
                           }
                       }
                       
                       //
                       // Handles events where draggable items are moved over droppable ones.
                       //
                       function onDragOver(event, source, target, dragData, dropData) {
                           if (!canDrop(source, target, dragData, dropData)) {
                               return true;
                           }

                           var dropping = $(target);

                           var evt = event.originalEvent || event;

                           var placeholderNode = placeholder[0];

                           // row is being dragged
                           if (isRow(source[0])) {

                               // over another row
                               if (isRow(target)) {

                                   // insert placeholder before or after the target row
                                   if (shouldInsertBefore(evt, target)) {
                                       target.parentNode.insertBefore(placeholderNode, target);
                                   } else {
                                       target.parentNode.insertBefore(placeholderNode, target.nextSibling);
                                   }
                               } else {

                                   // over a row container
                                   if (isRowContainer(target)) {

                                       dropBefore = false;

                                       // insert placeholder only if there are no rows in the container already
                                       var rowcount = dropping.find('> .ordered-form-row').length;
                                       if (rowcount === 0) {
                                           target.appendChild(placeholderNode);
                                       }
                                   }
                               }
                           } else {

                               // container is being dragged
                               if (isRowContainer(source[0])) {

                                   // over a row
                                   if (isRow(target)) {

                                       dropBefore = false;

                                       // insert placeholder after the container that holds this row
                                       var rowContainer = target.parentNode;
                                       if (rowContainer) {
                                           rowContainer.parentNode.insertBefore(placeholderNode, rowContainer.nextSibling);
                                       }
                                   } else {

                                       // over a row container
                                       if (isRowContainer(target)) {

                                           // insert the placeholder before or after the target container
                                           if (shouldInsertBefore(evt, target)) {
                                               target.parentNode.insertBefore(placeholderNode, target);
                                           } else {
                                               target.parentNode.insertBefore(placeholderNode, target.nextSibling);
                                           }
                                       }
                                   }
                               }
                           }
                           
                           dropping.addClass('drag-target');

                           evt.preventDefault();
                           evt.stopPropagation();
                           return false;
                       }

                       //
                       // Handles a drop event.
                       //
                       function onDrop(event, source, target, dragData, dropData) {
                           if (!canDrop(source, target, dragData, dropData)) {
                               return true;
                           }

                           var evt = event.originalEvent || event;

                           var dropping = $(target);

                           try {

                               var targetSection = isSection(dropData) ? dropData : getSection(dropData);

                               var newIndex = 0;

                               // Dropping a question
                               if (isQuestion(dragData)) {

                                   // Try and order the questions as they have been rendered (excluding the one being dragged if necessary)
                                   var orderedQuestions = _(targetSection.surveyQuestions).filter(function (q) { return dragData !== q; }).orderBy(['questionOrder', 'questionId', 'name']).value();

                                   // Get the index of the placeholder communicating where the item is to be inserted
                                   var c = dropping;
                                   if (isRow(target)) {
                                       c = $(target.parentNode);
                                   }

                                   newIndex = c.find('> .drag-item').not('.drag-source').index(placeholder[0]);

                                   // (Re-)order all the questions in this section to reflect the new positioning (NOTE: this will also affect library questions)
                                   var n = 0;
                                   _.forEach(orderedQuestions, function(q) {
                                       q.questionOrder = n >= newIndex ? n + 1 : n;
                                       n++;
                                   });

                                   dragData.questionOrder = newIndex;

                                   var sourceSection = getSection(dragData);

                                   if (sourceSection !== targetSection) {
                                       sourceSection.surveyQuestions.remove(dragData);
                                       targetSection.surveyQuestions.add(dragData);
                                   }

                                   saveFormDataInViewMode();

                               } else {

                                   // Dropping a section
                                   if (isSection(dragData)) {
                                       
                                       // Try and order the sections as they have been rendered (excluding the one being dragged)
                                       var orderedSections = _(scope.model.sections).filter(function (s) { return dragData !== s; }).orderBy('surveySectionOrder').value();

                                       newIndex = $(container).find('> .drag-item').not('.drag-source').index(placeholder[0]);

                                       // (Re-)order all the questions in this section to reflect the new positioning (NOTE: this will also affect library questions)
                                       var m = 0;
                                       _.forEach(orderedSections, function (s) {
                                           s.surveySectionOrder = m >= newIndex ? m + 1 : m;
                                           m++;
                                       });

                                       dragData.surveySectionOrder = newIndex;

                                       saveFormDataInViewMode();
                                   }
                               }

                           } finally {
                               // clean up
                               dropBefore = false;

                               if (dropTargets) {
                                   dropTargets.length = 0;
                               }

                               if (placeholder) {
                                   placeholder.remove();
                               }

                               if (dropping) {
                                   dropping.removeClass('drag-target');
                               }

                               evt.stopPropagation();
                               return false;
                           }
                       }

                       scope.getHeaderClass = function() {
                           return scope.isMobile ? 'headerPanel-right' : 'headerPanel-left';
                       };

                       /////
                       // Control sizing and placement.
                       /////
                       scope.$on('gather', function (event, callback) {
                           callback(scope.formControl, scope.parentControl, element);
                       });

                       var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spSurveySectionsEditor/spSurveySectionsEditor.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());