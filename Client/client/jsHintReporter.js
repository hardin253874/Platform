"use strict";

module.exports = {
    reporter: function (res) {

        res.forEach(function (r) {
            var file = r.file;
            var err = r.error;
            var str = "";

            str = file + "(" + err.line + "," + err.character + "): error CS1519: " + err.reason + " ('" + err.evidence.trim() + "')\n";

            process.stderr.write(str);
        });
    }
};