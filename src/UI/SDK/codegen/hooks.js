var __read = (this && this.__read) || function (o, n) {
    var m = typeof Symbol === "function" && o[Symbol.iterator];
    if (!m) return o;
    var i = m.call(o), r, ar = [], e;
    try {
        while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
    }
    catch (error) { e = { error: error }; }
    finally {
        try {
            if (r && !r.done && (m = i["return"])) m.call(i);
        }
        finally { if (e) throw e.error; }
    }
    return ar;
};
var __spreadArray = (this && this.__spreadArray) || function (to, from) {
    for (var i = 0, il = from.length, j = to.length; i < il; i++, j++)
        to[j] = from[i];
    return to;
};
var filterModels = function (model) {
    // instead of the many similar list models that the spec spits out
    // we're going to consolidate into two list types with type parameters ListPage and ListPageFacet
    // for example ListCreditCard becomes ListPage<CreditCard>
    return !model.isList;
};
var postFormatModel = function (model, models) {
    // add model.typeParams and prop.typeParams
    inspectModelForTypeParams(models, model, model);
    // add model.hasTypeParams and prop.hasTypeParams
    model['hasTypeParams'] = Boolean(model['typeParams'].length);
    model.properties.forEach(function (prop) {
        prop['hasTypeParams'] = Boolean(prop['typeParams'].length);
    });
    // add prop.typescriptType to props on model
    model.properties.forEach(function (prop) {
        prop['typescriptType'] = findTypeForModelProps(prop, model);
    });
    // RETURN MODEL - THIS IS IMPORTANT
    return model;
};
var postFormatOperation = function (operation) {
    var _a;
    // add prop.typescriptType to props on operations
    operation.allParams.forEach(function (param) {
        param['typescriptType'] = findTypeForOperationProps(param, operation);
    });
    operation.queryParams.forEach(function (param) {
        param['typescriptType'] = findTypeForOperationProps(param, operation);
    });
    if (operation['verb'] === 'get' && ((_a = operation.returnType) === null || _a === void 0 ? void 0 : _a.startsWith('List'))) {
        operation['isList'] = true;
        //operation['returnType'] = operation.returnType.substring(4)
    }
    if (operation.isList) {
        // instead of the many similar list models that the spec spits out
        // we're going to consolidate into two list types with type parameters ListPage and ListPageFacet
        // for example ListCreditCard becomes ListPage<CreditCard>
        var newImports_1 = __spreadArray([], __read(operation.fileImports));
        operation.fileImports.forEach(function (fileImport) {
            if (fileImport === operation.returnType) {
                if (operation.isFacetList) {
                    newImports_1 = __spreadArray(__spreadArray([], __read(newImports_1)), ['ListPageFacet', operation.baseType]);
                }
                else {
                    newImports_1 = __spreadArray(__spreadArray([], __read(newImports_1)), ['ListPage', operation.baseType]);
                }
                // remove the old list type
                newImports_1 = newImports_1.filter(function (i) { return i !== fileImport; });
            }
            return fileImport;
        });
        operation.fileImports = __spreadArray([], __read(new Set(newImports_1))); // unique array
    }
    var operationExpectsBody = operation.verb == 'post' ||
        operation.verb == 'put' ||
        operation.verb == 'patch';
    if (operationExpectsBody && operation.bodyParam == null) {
        operation.hasBodyParam = true;
        operation.bodyParam = { name: '{}' };
    }
    // RETURN OPERATION - THIS IS IMPORTANT
    return operation;
};
module.exports = {
    filterModels: filterModels,
    postFormatModel: postFormatModel,
    postFormatOperation: postFormatOperation,
};
/******************
 * BEGIN HELPER METHODS *
 * ****************
 */
var javascriptTypes = {
    'integer': 'number',
    'object': 'any',
    'string': 'string',
    'boolean': 'boolean',
};
function findTypeForOperationProps(prop, operation) {
    if (!prop) {
        return 'void';
    }
    if (prop.name === 'filters') {
        // we're updating the behavior of filters so that we get better type inference
        // instead of accepting dot-referenced xp values such as { 'xp.color': 'red' }
        // we will now expect { xp: { color: 'red' } }
        prop.description =
            'An object whose keys match the model, and the values are the values to filter by';
        return "Filters<Required<" + operation.baseType + ">>";
    }
    if (prop.isEnum) {
        // using backticks here so we can write quotes
        return prop.enum.map(function (p) { return "'" + p + "'"; }).join(' | ');
    }
    var jsType = javascriptTypes[prop.type] || prop.type;
    if (prop.isArray) {
        return prop.isCustomType ? prop.type + "[]" : jsType + "[]";
    }
    if (!prop.hasRequiredFields && prop.isCustomType) {
        return prop.type;
    }
    return jsType;
}
function findTypeForModelProps(prop, model) {
    if (!prop) {
        return 'void';
    }
    // if (prop.name === 'xp') {
    //   return `T${model.name}Xp`
    // }
    if (model.isList && prop.name === 'Items') {
        return "T" + model.baseType + "[]";
    }
    if (prop.isEnum) {
        // using backticks here so we can write quotes
        return prop.enum.map(function (p) { return "'" + p + "'"; }).join(' | ');
    }
    var typeParams = prop['hasTypeParams']
        ? "<" + prop['typeParams'].join(',') + ">"
        : '';
    var jsType = javascriptTypes[prop.type] || prop.type;
    if (prop.isArray) {
        return prop.isCustomType ? prop.type + typeParams + "[]" : jsType + "[]";
    }
    if (!prop.hasRequiredFields && prop.isCustomType) {
        return prop.type + typeParams;
    }
    return jsType;
}
function inspectModelForTypeParams(allModels, rootModel, inspectModel, rootProp, parentProp) {
    if (!inspectModel['typeParams']) {
        inspectModel['typeParams'] = [];
    }
    inspectModel.properties.forEach(function (prop) {
        if (!prop['typeParams']) {
            prop['typeParams'] = [];
        }
        // if (prop.isXp) {
        //   if (!rootProp) {
        //     const typeParam = `T${inspectModel.name}Xp`
        //     rootModel['typeParams'].unshift(typeParam)
        //     prop['typeParams'].unshift(typeParam)
        //   } else {
        //     let typeParam = parentProp
        //       ? `T${parentProp.name}Xp`
        //       : `T${rootProp.name}Xp`
        //     if (typeParam === 'TItemsXp') {
        //       typeParam = `T${rootProp.type}Xp`
        //       rootModel['typeParams'].unshift(typeParam)
        //       rootProp['typeParams'].unshift(typeParam)
        //     } else {
        //       rootModel['typeParams'].push(typeParam)
        //       rootProp['typeParams'].push(typeParam)
        //     }
        //   }
        // }
        if (prop.isCustomType) {
            var toInspect = allModels.find(function (model) {
                return model.name === prop.type || model.type === prop.type;
            });
            if (!toInspect) {
                console.log(prop);
                throw new Error("Unable to find next model to inspect for " + prop.type);
            }
            if (!rootProp && !parentProp) {
                inspectModelForTypeParams(allModels, rootModel, toInspect, prop);
            }
            else if (rootProp && !parentProp) {
                inspectModelForTypeParams(allModels, rootModel, toInspect, rootProp, prop);
            }
        }
    });
}
export {};
