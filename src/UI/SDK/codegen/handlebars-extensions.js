"use strict";
function handlebarsExt(Handlebars) {
    // converts /buyers/{buyerID} to /buyers/${buyerID} so template literal take in parameters
    Handlebars.registerHelper('parameterizePath', function (path) {
        if (!path) {
            return '';
        }
        return path.replace(/{/g, '${');
    });
    Handlebars.registerHelper('commaSeparate', function (fields) {
        return fields.join(', ');
    });
    Handlebars.registerHelper('commaSeparateWithDefaultAny', function (types) {
        return types.map(function (t) { return t + " = any"; }).join(', ');
    });
}
module.exports = handlebarsExt;
