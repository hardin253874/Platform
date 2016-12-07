// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, CodeMirror, sp, spResource, spEntity */

// this has grown to be a bfm. todo - tidy up

(function () {
    'use strict';

    angular
        .module('sp.common.ui.expressionEditor')
        .directive('spExpressionEditor', spExpressionEditor);

    function spExpressionEditor($q, $timeout, spCalcEngineService, spExpressionEditorService) {

        var directive = {
            restrict: 'E',
            replace: true,
            scope: {
                host: '=',
                context: '=',   // type ID or null
                params: '=',
                mode: '=', // 'full' or 'inline' with the latter the default
                options: '=',   // {expectedResultType: {dataType: e.g. spEntity.DataType.Bool},  disabled, choosers.{etc..} }
                onCompile: '&',
                onScriptChanged: '&'
            },
            templateUrl: 'expressionEditor/expressionEditor.tpl.html',
            require: 'ngModel',
            link: link
        };
        return directive;

        function link(scope, el, attrs, ngModel) {

            var codeMirror;

            var cmOptions = {
                mode: 'text/x-spql',
                lineNumbers: false,
                lineWrapping: true,
                matchBrackets: true,
                autofocus: false,
                readOnly: scope.options && scope.options.disabled
            };

            var isTestMode = sp.result(scope, '$root.__spTestMode');

            scope.entityTypeHints = {};
            scope.expressionTypes = {};
            scope.paramHints = {};
            scope.functions = spExpressionEditorService.functionList;

            function compileAsync(expression, suppressExpectedType) {
                return spCalcEngineService.compile(expression, {
                    context: scope.context,
                    params: scope.params,
                    host: scope.host,
                    expectedResultType: (suppressExpectedType ? null : sp.result(scope, 'options.expectedResultType'))
                });
            }

            function getCompiledExpressionEntityType(expression) {
                var expectedType = scope.options ? scope.options.expectedResultType : null;
                var key = expectedType + '|' + expression;
                if (scope.expressionTypes[key]) {
                    return scope.expressionTypes[key];
                }
                safeApply(function() {
                    scope.compileResult = null;
                });                
                return compileAsync(expression).then(function (result) {
                    // run once, with proper expected type, to get real script result                    
                    scope.compileResult = result;
                    return compileAsync(expression, true);
                }).then(function (result) {
                    // run again, without expected type, to take a guess at the auto-suggest
                    // needs a better solution, but auto-suggest needs a rework anyway
                    var expressionEntityTypeId = result.entityTypeId;
                    scope.expressionTypes[key] = expressionEntityTypeId;
                    return expressionEntityTypeId;
                });
            }

            function getEntityTypeHints(entityTypeId) {
                if (!entityTypeId) {
                    return [];
                }
                if (scope.entityTypeHints[entityTypeId]) {
                    return scope.entityTypeHints[entityTypeId];
                }
                return spCalcEngineService.getEntityTypeHints(entityTypeId, scope.host).then(function (hints) {
                    scope.entityTypeHints[entityTypeId] = hints;
                    return hints;
                });
            }

            function showPropHint(expression, external) {

                console.log('showPropHint: exp=%o, hints=%o, params=%o', expression, scope.entityTypeHints, scope.params);

                if (!expression) {
                    // show hints for the context type

                    if (scope.context) {
                        $q.when(getEntityTypeHints(scope.context))
                            .then(function (hints) {
                                if (hints && hints.length) {
                                    CodeMirror.showHint(codeMirror, myPropHint, {
                                        hints: hints,
                                        replace: true
                                    });
                                }
                            });
                    }

                    return;
                }

                $q.when(getCompiledExpressionEntityType(expression))
                    .then(getEntityTypeHints)
                    .then(function (hints) {
                        if (hints && hints.length) {
                            CodeMirror.showHint(codeMirror, myPropHint, {
                                hints: hints
                            });
                        }
                    })
                    .catch(function (err) {
                        console.log('showPropHint: err', err);
                    });
            }


            function showParamHint(skipOpening) {

                if (!scope.params && scope.context) {
                    console.log('showParamHint -> showPropHint', scope.context);
                    showPropHint();
                    return;
                }

                console.log('showParamHint', scope.entityTypeHints, scope.params);
                CodeMirror.showHint(codeMirror, myParamHint,
                    _.extend({ hints: _.map(scope.params, 'name') }, parameterDelimiters(skipOpening)));
            }

            function showHints() {
                logCm(codeMirror);

                var cur = codeMirror.getCursor();
                var token = codeMirror.getTokenAt(cur);
                var exp = { text: codeMirror.getValue() };

                //yuuuuuk, too much special case stuff going on

                if (scope.context && exp.text && _.last(exp.text.trim()) === '(') {
                    showPropHint();
                } else if (token.string && token.string.trim()) {
                    showPropHint(exp.text);
                } else if (scope.context) {
                    showPropHint();
                } else {
                    //scope.showParameters();
                    showParamHint();
                }
            }

            function compileExpression(expression) {
                $q.when(getCompiledExpressionEntityType(expression))
                    .then(getEntityTypeHints);
            }

            var debouncedCompile = _.debounce(compileExpression, 1000);            

            function handleParamsVisibility() {
                function hasClass(c, e) {
                    return new RegExp(c).test(e.className);
                }

                var parameterListEl = _.find(el.find('div'), _.partial(hasClass, 'parameter-list'));
                if (parameterListEl) {
                    if (codeMirror.hasFocus()) {
                        angular.element(parameterListEl).addClass('show');
                    } else {
                        angular.element(parameterListEl).removeClass('show');
                    }
                }
            }

            //notes - some odd things going on, no time to investigate now... to come back to
            // - hooking on the '[' doesn't seem to work in the on change event
            // - adding a key mapping on '.' doesn't work out.. seems to eat the .

            cmOptions.extraKeys = {
                "Tab": false,
                "Shift-Tab": false,
                "Ctrl-Space": showHints
            };
            codeMirror = CodeMirror.fromTextArea(el.find('textArea')[0], cmOptions);

            codeMirror.on("focus", function (cm) {
                console.assert(cm === codeMirror, 'not my codemirror');
                // timeout to give the editor a chance to get focus... cm.hasFocus() doesn't seem to be true yet
                $timeout(handleParamsVisibility, 0);
            });

            codeMirror.on("blur", function (cm) {
                console.assert(cm === codeMirror, 'not my codemirror');
                // timeout needed so the 'buttons' get the clicks
                $timeout(handleParamsVisibility, 200);
            });

            //                    codeMirror.on("cursorActivity", function (cm) {
            //                        console.log('expressionEditor: cursorActivity');
            //                        logCm(cm);
            //                    });

            codeMirror.on("change", function (cm, changeObj) {
                console.assert(cm === codeMirror, 'not my codemirror');

                // move value from the DOM back to the ngModelController                

                var newValue = cm.getValue();
                if (newValue !== ngModel.$viewValue) {
                    ngModel.$setViewValue(newValue);
                }                

                _.defer(function () {
                    codeMirror.refresh();
                    scope.$apply();
                });

                console.log('codeMirror.change: newValue %o origin %o', newValue, changeObj.origin);

                if (changeObj.origin !== 'setValue' || isTestMode) {
                    
                    if (scope.options && scope.options.onScriptChanged) {
                        scope.options.onScriptChanged(newValue);
                    }

                    var exp = { text: newValue };


                    if (isInExpression(cm) || isTestMode) {
                        if (_.last(exp.text) === '.') {
                            if (exp.text.length > 1) {
                                showPropHint(exp.text.substring(0, exp.text.length - 1));
                            } else {
                                showPropHint();
                            }
                        } else if (_.last(exp.text) === '[') {
                            // this isn't working... something to with the [ char being a bit special
                            console.warn('expression-editor: didn\'t think this was working');
                            showParamHint(true /*skipOpening*/);
                        } else if (_.trim(exp.text)) {
                            debouncedCompile(exp.text);
                        } else if (!_.trim(exp.text)) {
                            scope.compileResult = {
                                expression: "",
                                error: null
                            };
                        }
                    } else if (_.last(newValue) === '[') {
                        // this isn't working... something to with the [ char being a bit special
                        console.warn('expression-editor: didnt think this was working');
                        showParamHint();
                    }
                }

                _.extend(scope.debug, { changeObj: changeObj });
            });

            ngModel.$render = function () {
                // move the view value from the ngModelController to the DOM
                codeMirror.setValue(ngModel.$viewValue);
                _.extend(scope.debug, { viewValue: ngModel.$viewValue });
            };

            // CodeMirror expects a string, so make sure it gets one. This does not change the model.
            ngModel.$formatters.push(function (value) {
                if (angular.isUndefined(value) || value === null) {
                    return '';
                }
                if (angular.isObject(value) || angular.isArray(value)) {
                    throw new Error('cannot use an object or an array as a model');
                }
                return value;
            });

            scope.parameterClicked = function (param) {
                console.log('clicked');
                var delims = parameterDelimiters();
                codeMirror.replaceSelection(delims.opening + param.name + delims.closing);
                codeMirror.focus();
            };
            scope.functionClicked = function (name) {
                if (!name) return;
                codeMirror.replaceSelection(name);
                codeMirror.focus();
            };

            scope.memberClicked = function (name) {
                codeMirror.replaceSelection('.[' + name + ']');
                codeMirror.focus();
            };

            scope.showFunctions = function () {
                var chooser = sp.result(scope.options, 'choosers.functionChooser');
                if (chooser && chooser.chooserFn) {
                    $q.when(chooser.chooserFn()).then(function (name) {
                        if (name) scope.functionClicked(name);
                    });
                } else {
                    scope.visualState.showFunctionsList = !scope.visualState.showFunctionsList;
                }
            };

            scope.showParameters = function () {
                var chooser = sp.result(scope.options, 'choosers.parameterChooser');
                if (chooser && chooser.chooserFn) {
                    $q.when(chooser.chooserFn()).then(function (name) {
                        if (name) {
                            scope.parameterClicked({ name: name });
                        }
                    });
                } else {
                    scope.visualState.showParametersList = !scope.visualState.showParametersList;
                }
            };

            scope.canShowResources = function () {
                return !!sp.result(scope.options, 'choosers.resourceChooser');
            };

            scope.showResources = function () {
                var chooser = sp.result(scope.options, 'choosers.resourceChooser');
                if (chooser && chooser.chooserFn) {
                    $q.when(chooser.chooserFn()).then(function (name) {
                        if (name) scope.parameterClicked({ name: name });
                    });
                }
            };

            scope.canShowProperties = function() {
                return !!(sp.result(scope.options, 'choosers.propertyChooser') &&
                (codeMirror.getValue() || scope.context));
            };

            scope.showProperties = function () {
                var chooser = sp.result(scope.options, 'choosers.propertyChooser');

                if (chooser && chooser.chooserFn) {

                    var expression = codeMirror.getValue();

                    if (!expression) {
                        if (scope.context) {
                            $q.when(chooser.chooserFn(scope.context)).then(function (name) {
                                if (name) scope.memberClicked(name);
                            });
                        }
                        return;
                    }

                    $q.when(getCompiledExpressionEntityType(expression))
                        .then(function (resourceTypeId) {
                            $q.when(chooser.chooserFn(resourceTypeId)).then(function (name) {
                                if (name) scope.memberClicked(name);
                            });
                        });
                }
            };

            scope.showHints = showHints;

            scope.$watch('params', function (params, prev) {

                //                        console.log('watch params %o %o %o', ngModel.$viewValue, params, prev, params === prev);

                spCalcEngineService.compileParameterExpressions(params, params).then(function (results) {

                    //                            console.log('compileParams results %o => %o', params, results);

                    _.forEach(params, function (p) {
                        if (!p.typeName && p.expression) {
                            var result = _.find(results, { expression: p.expression });
                            p.typeName = result.resultType;
                            p.entityTypeId = result.entityTypeId;
                        }
                    });
                });
            });

            scope.$watch('options.expectedResultType', function (params, prev) {
                var expression = codeMirror.getValue();
                if (expression) {
                    getCompiledExpressionEntityType(expression);
                }
            });

            scope.$watch('compileResult', function () {
                // scope.compileResult = object returned from WebApi. See EvalResult.
                if (scope.compileResult) {
                    if (scope.options && scope.options.onCompile) {
                        scope.options.onCompile({ result: scope.compileResult });
                    }
                }
            });

            function safeApply(fn) {
                if (!scope.$root.$$phase) {
                    // digest or apply not in progress
                    scope.$apply(fn);
                } else {
                    // digest or apply already in progress
                    fn();
                }
            }

            var throttledRefresh = _.throttle(function() {
                safeApply(function() {
                    codeMirror.refresh();
                });
            }, 100);

            scope.$on('sp.app.ui-refresh', function () {
                throttledRefresh();
            });

            scope.$on('sp.view.setDefaultFocus', function () {
                codeMirror.focus();
            });

            scope.$on('$destroy', function () {
                if (codeMirror.state.completionActive) {
                    codeMirror.state.completionActive.close();
                }
            });

            scope.visualState = {};
            scope.debug = {};

        }

        // end link(..)


        function findExpRange(cm, cur) {
            var line = cm.getLine(cur.line);
            var start = line.lastIndexOf('{{', cur.ch);
            var end = line.indexOf('}}', cur.ch);
            var range = {
                from: start < 0 ? start : start + 2,
                to: start < 0 ? -1 : end >= 0 ? end : line[line.length - 1] !== '}' ? line.length : line.length - 1
            };
            if (range.from >= 0) {
                range.text = line.substring(range.from, range.to);
                range.ch = Math.min(cur.ch - range.from, range.to - range.from);
            }
            return range;
        }

        function isInExpression(cm) {
            return cm.getModeAt(cm.getCursor()).name === 'spql';
        }

        function logCm(cm) {
            var cur = cm.getCursor();
            var token = cm.getTokenAt(cur);
            var priorToken = token.start > 0 ? cm.getTokenAt({ line: cur.line, ch: token.start - 1 }) : null;
            var priorChar = cm.getValue().substring(token.start - 1, token.start);
            var mode = cm.getMode(), innerMode = cm.getModeAt(cur);
            var line = cm.getLine(cur.line);

            var expRange = cm.getModeAt(cur).name !== "null" ? findExpRange(cm, cur) : { from: -1, to: -1 };

            console.log('codemirror: line %s ch %s token %o %o %o, mode=%s %o, innerMode=%s %o, line %s, exp from',
                cur.line, cur.ch, token.string, token.type, token,
                mode.name, mode, innerMode.name, innerMode,
                line, expRange);
            if (priorToken) console.log('codemirror...more: prior token %o %o %o, prior char %o',
                priorToken.string, priorToken.type, priorToken,
                priorChar);
        }

        /*
         * This function is to return a list of hints (label and value) and the replacement location if chosen.
         * It will be called by Codemirror in a number of circumstances...
         * - when the user has entered a dot following a parameter
         *  -> token is the . and prior is the parameter
         * - when the user has clicked on showHint with cursor immediately following a parameter
         *  -> token is the parameter, prior is something else
         *  => need to prefix with a .
         * - when the user has entered a dot after a parameter and started typing
         *  -> the token is what they have typed, without the dot, and the prev is the parameter
         * - the user types a [ when expression is empty
         *  -> the token is empty, the prior is undefined
         */
        function myPropHint(cm, options) {
            var cur = cm.getCursor();
            var token = cm.getTokenAt(cur);
            var priorToken = token.start > 0 ? cm.getTokenAt({ line: cur.line, ch: token.start - 1 }) : null;
            var priorChar = cm.getValue().substring(token.start - 1, token.start);
            var from = token.start, to = token.end;
            var prefix = '';
            var list = options.hints;
            if (token.string && token.string !== '.' && token.string !== '(' && token.string[token.string.length - 1] !== ']') {
                var strToMatch = token.string.trim().toLowerCase();
                list = _.filter(options.hints || [], function (p) {
                    return p.toLowerCase().indexOf(strToMatch) === 0;
                });
            }
            //        logCm(cm);
            console.log('myPropHint: token %o %o prior %o %o', token && token.string, token, priorToken && priorToken.string, priorToken);
            console.log('myPropHint: value %o prev char %o', cm.getValue(), priorChar);
            if (!options.replace && (token.string === '.' || token.string[token.string.length - 1] === ']')) {
                from = to; // don't remove existing token
            }
            if (token.string && token.string[token.string.length - 1] === ']') {
                prefix = '.';
            }
            //        console.log('myPropHint: token %o cur %o list %o prefix %o', token, cur, list, prefix);

            return {
                list: _.map(list, function (p) {
                    return {
                        text: prefix + wrapParam(p, parameterDelimiters()),
                        displayText: p
                    };
                }),
                from: CodeMirror.Pos(cur.line, from),
                to: CodeMirror.Pos(cur.line, to)
            };
        }


        function parameterDelimiters(skipOpening) {
            return {
                skipOpening: !!skipOpening,
                opening: '[',
                closing: ']'
            };
        }

        function wrapParam(p, options) {
            var opening = options.skipOpening ? '' : options.opening || '[';
            var closing = options.closing || ']';
            return opening + p + closing;
        }

        function myParamHint(cm, options) {
            var cur = cm.getCursor();
            var token = cm.getTokenAt(cur);
            var from = token.start, to = token.end;
            var list = [];
            if (token.string !== '.') {
                var strToMatch = token.string && token.string.trim().toLowerCase();
                //            if (strToMatch && strToMatch[0] === '[') strToMatch = strToMatch.substring(1);
                //            console.log('matching on ', strToMatch);
                if (strToMatch) {
                    list = _.filter(options.hints || [], function (p) {
                        return p.toLowerCase().indexOf(strToMatch) === 0;
                    });
                }
                if (!list.length || !strToMatch) {
                    list = options.hints;
                    from = to; // don't remove existing token
                }
            }
            //        logCm(cm);
            //        console.log('myParamHint: token %o cur %o list %o', token, cur, list);
            return {
                list: _.map(list, function (p) {
                    return {
                        text: wrapParam(p, options),
                        displayText: p
                    };
                }),
                from: CodeMirror.Pos(cur.line, from),
                to: CodeMirror.Pos(cur.line, to)
            };
        }
    }

})();
