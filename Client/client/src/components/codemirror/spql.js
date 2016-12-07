// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* eslint-disable */
/*global CodeMirror */

CodeMirror.defineMode("spql", function (config, parserConfig) {
    "use strict";

    var functions = parserConfig.functions || {},
        atoms = parserConfig.atoms || { "false": true, "true": true, "null": true },
        builtin = parserConfig.builtin || {},
        keywords = parserConfig.keywords || {},
        operatorChars = parserConfig.operatorChars || /^[*+\-%<>!=&|~^]/,
        support = parserConfig.support || {},
        hooks = parserConfig.hooks || {},
        dateSQL = parserConfig.dateSQL || { "date": true, "time": true, "timestamp": true };

    function tokenBase(stream, state) {
        var ch = stream.next();

        // call hooks from the mime type
        if (hooks[ch]) {
            var result = hooks[ch](stream, state);
            if (result !== false) { return result; }
        }

        if (ch.charCodeAt(0) > 47 && ch.charCodeAt(0) < 58) {
            // numbers
            stream.match(/^[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?/);
            return "number";
        } else if (ch == "'") {
            // strings
            state.tokenize = tokenLiteral(ch);
            return state.tokenize(stream, state);
        } else if (/^[\(\),\;\[\]]/.test(ch)) {
            // no highlightning
            return null;
        } else if (ch == "#" || (ch == "-" && stream.eat("-") && stream.eat(" "))) {
            // 1-line comments
            stream.skipToEnd();
            return "comment";
        } else if (ch == "/" && stream.eat("*")) {
            // multi-line comments
            state.tokenize = tokenComment;
            return state.tokenize(stream, state);
        } else if (ch == ".") {
            // .1 for 0.1
            if (support.zerolessFloat && stream.match(/^(?:\d+(?:e\d*)?|\d*e\d+)/i)) {
                return "number";
            }
        } else if (operatorChars.test(ch)) {
            // operators
            stream.eatWhile(operatorChars);
            return null;
        } else {
            stream.eatWhile(/^[_\w\d]/);
            var word = stream.current().toLowerCase();
            // dates (standard SQL syntax)
            if (dateSQL.hasOwnProperty(word) && (stream.match(/^( )+'[^']*'/) || stream.match(/^( )+"[^"]*"/))) {
                return "number";
            }
            if (atoms.hasOwnProperty(word)) { return "atom"; }
            if (builtin.hasOwnProperty(word)) { return "builtin"; }
            if (keywords.hasOwnProperty(word)) { return "keyword"; }
            if (functions.hasOwnProperty(word)) { return "string-2"; }
            return null;
        }
    }

    // 'string', with char specified in quote escaped by '\'
    function tokenLiteral(quote) {
        return function (stream, state) {
            var escaped = false, ch;
            while ((ch = stream.next()) != null) {
                if (ch == quote && !escaped) {
                    state.tokenize = tokenBase;
                    break;
                }
                escaped = !escaped && ch == "\\";
            }
            return "string";
        };
    }
    function tokenComment(stream, state) {
        while (true) {
            if (stream.skipTo("*")) {
                stream.next();
                if (stream.eat("/")) {
                    state.tokenize = tokenBase;
                    break;
                }
            } else {
                stream.skipToEnd();
                break;
            }
        }
        return "comment";
    }

    function pushContext(stream, state, type) {
        state.context = {
            prev: state.context,
            indent: stream.indentation(),
            col: stream.column(),
            type: type
        };
    }

    function popContext(state) {
        state.indent = state.context.indent;
        state.context = state.context.prev;
    }

    return {
        startState: function () {
            return { tokenize: tokenBase, context: null };
        },

        token: function (stream, state) {
            if (stream.sol()) {
                if (state.context && state.context.align == null) {
                    state.context.align = false;
                }
            }
            if (stream.eatSpace()) { return null; }

            var style = state.tokenize(stream, state);
            if (style == "comment") { return style; }

            if (state.context && state.context.align == null) {
                state.context.align = true;
            }

            var tok = stream.current();
            if (tok == "(") {
                pushContext(stream, state, ")");
            }
            else if (tok == "[") {
                pushContext(stream, state, "]");
            }
            else if (state.context && state.context.type == tok) {
                popContext(state);
            }
            return style;
        },

        indent: function (state, textAfter) {
            var cx = state.context;
            if (!cx) { return CodeMirror.Pass; }
            if (cx.align) { return cx.col + (textAfter.charAt(0) == cx.type ? 0 : 1); }
            else { return cx.indent + config.indentUnit; }
        }
    };
});

(function () {
    "use strict";

    // `identifier`
    function hookIdentifier(stream) {
        var ch;
        while ((ch = stream.next()) != null) {
            if (ch == "]" && !stream.eat("]")) { return "variable-2"; }
        }
        return null;
    }

    // variable token
    function hookVar(stream) {
        // variables
        // @@ and prefix
        if (stream.eat("@")) {
            stream.match(/^session\./);
            stream.match(/^local\./);
            stream.match(/^global\./);
        }

        if (stream.eat("'")) {
            stream.match(/^.*'/);
            return "variable-2";
        } else if (stream.eat('"')) {
            stream.match(/^.*"/);
            return "variable-2";
        } else if (stream.eat("`")) {
            stream.match(/^.*`/);
            return "variable-2";
        } else if (stream.match(/^[0-9a-zA-Z$\.\_]+/)) {
            return "variable-2";
        }
        return null;
    }

    // short functions keyword token
    function hookClient(stream) {
        // \g, etc
        return stream.match(/^[a-zA-Z]\b/) ? "variable-2" : null;
    }

    function set(str) {
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) {
            obj[words[i]] = true;
        }
        return obj;
    }

    // this is based on Peter Raganitsch's 'plsql' mode
    CodeMirror.defineMIME("text/x-spql", {
        name: "spql",
        functions: set("convert context iif isnull all abs ceiling exp floor log log log10 power round sign square sqrt charindex charindex left len replace right substring tolower toupper datefromparts timefromparts datetimefromparts year month day hour minute second getdate getdatetime gettime dateadd datediff datename dayofyear quarter week weekday dayofyear count any every max min sum avg stdev join "),
        keywords: set("let select where order by asc desc like is"),
        builtin: set("int decimal currency percent date time datetime bool string"),
        atoms: set("and or not false true null unknown"),
        operatorChars: /^[*+\-%<>!=~]/,
        dateSQL: set("date time timestamp"),
        hooks: {
            "@": hookVar,
            "[": hookIdentifier,
            "\\": hookClient
        }
    });

}());
