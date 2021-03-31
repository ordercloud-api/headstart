// https://github.com/ordercloud-api/oc-codegen#hooks-
import {
  Model,
  Param,
  PostFormatModelHook,
  PostFormatOperationHook,
  FilterModelsHook,
  Operation,
} from '@ordercloud/oc-codegen'

const filterModels: FilterModelsHook = function(model) {
  // instead of the many similar list models that the spec spits out
  // we're going to consolidate into two list types with type parameters ListPage and ListPageFacet
  // for example ListCreditCard becomes ListPage<CreditCard>
  return !model.isList
}

const postFormatModel: PostFormatModelHook = function(model, models) {
  // add model.typeParams and prop.typeParams
  inspectModelForTypeParams(models, model, model)

  // add model.hasTypeParams and prop.hasTypeParams
  model['hasTypeParams'] = Boolean(model['typeParams'].length)
  model.properties.forEach(prop => {
    prop['hasTypeParams'] = Boolean(prop['typeParams'].length)
  })

  // add prop.typescriptType to props on model
  model.properties.forEach(prop => {
    prop['typescriptType'] = findTypeForModelProps(prop, model)
  })

  // RETURN MODEL - THIS IS IMPORTANT
  return model
}

const postFormatOperation: PostFormatOperationHook = function(operation) {
  // add prop.typescriptType to props on operations
  operation.allParams.forEach(param => {
    param['typescriptType'] = findTypeForOperationProps(param, operation)
  })

  operation.queryParams.forEach(param => {
    param['typescriptType'] = findTypeForOperationProps(param, operation)
  })

  if (operation['verb'] === 'get' && operation.returnType?.startsWith('List')) {
    operation['isList'] = true
    //operation['returnType'] = operation.returnType.substring(4)
  }

  if (operation.isList) {
    // instead of the many similar list models that the spec spits out
    // we're going to consolidate into two list types with type parameters ListPage and ListPageFacet
    // for example ListCreditCard becomes ListPage<CreditCard>
    let newImports = [...operation.fileImports]
    operation.fileImports.forEach(fileImport => {
      if (fileImport === operation.returnType) {
        if (operation.isFacetList) {
          newImports = [...newImports, 'ListPageFacet', operation.baseType]
        } else {
          newImports = [...newImports, 'ListPage', operation.baseType]
        }
        // remove the old list type
        newImports = newImports.filter(i => i !== fileImport)
      }
      return fileImport
    })
    operation.fileImports = [...new Set(newImports)] // unique array
  }

  const operationExpectsBody: boolean =
    operation.verb == 'post' ||
    operation.verb == 'put' ||
    operation.verb == 'patch'

  if (operationExpectsBody && operation.bodyParam == null) {
    operation.hasBodyParam = true
    operation.bodyParam = { name: '{}' } as Param
  }

  // RETURN OPERATION - THIS IS IMPORTANT
  return operation
}

module.exports = {
  filterModels,
  postFormatModel,
  postFormatOperation,
}

/******************
 * BEGIN HELPER METHODS *
 * ****************
 */

const javascriptTypes = {
  'integer': 'number',
  'object': 'any',
  'string': 'string',
  'boolean': 'boolean',
}

function findTypeForOperationProps(prop: Param, operation: Operation) {
  if (!prop) {
    return 'void'
  }

  if (prop.name === 'filters') {
    // we're updating the behavior of filters so that we get better type inference
    // instead of accepting dot-referenced xp values such as { 'xp.color': 'red' }
    // we will now expect { xp: { color: 'red' } }
    prop.description =
      'An object whose keys match the model, and the values are the values to filter by'
    return `Filters<Required<${operation.baseType}>>`
  }

  if (prop.isEnum) {
    // using backticks here so we can write quotes
    return prop.enum.map(p => `'${p}'`).join(' | ')
  }

  const jsType = javascriptTypes[prop.type] || prop.type

  if (prop.isArray) {
    return prop.isCustomType ? `${prop.type}[]` : `${jsType}[]`
  }

  if (!prop.hasRequiredFields && prop.isCustomType) {
    return prop.type
  }

  return jsType
}

function findTypeForModelProps(prop: Param, model: Model) {
  if (!prop) {
    return 'void'
  }

  // if (prop.name === 'xp') {
  //   return `T${model.name}Xp`
  // }

  if (model.isList && prop.name === 'Items') {
    return `T${model.baseType}[]`
  }

  if (prop.isEnum) {
    // using backticks here so we can write quotes
    return prop.enum.map(p => `'${p}'`).join(' | ')
  }

  const typeParams = prop['hasTypeParams']
    ? `<${prop['typeParams'].join(',')}>`
    : ''
  const jsType = javascriptTypes[prop.type] || prop.type

  if (prop.isArray) {
    return prop.isCustomType ? `${prop.type + typeParams}[]` : `${jsType}[]`
  }

  if (!prop.hasRequiredFields && prop.isCustomType) {
    return prop.type + typeParams
  }

  return jsType
}

function inspectModelForTypeParams(
  allModels: Model[],
  rootModel: Model,
  inspectModel: Model,
  rootProp?: Param,
  parentProp?: Param
) {
  if (!inspectModel['typeParams']) {
    inspectModel['typeParams'] = []
  }
  inspectModel.properties.forEach(prop => {
    if (!prop['typeParams']) {
      prop['typeParams'] = []
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
      const toInspect = allModels.find(model => {
        return model.name === prop.type || model.type === prop.type
      })

      if (!toInspect) {
        console.log(prop)
        throw new Error(`Unable to find next model to inspect for ${prop.type}`)
      }
      if (!rootProp && !parentProp) {
        inspectModelForTypeParams(allModels, rootModel, toInspect, prop)
      } else if (rootProp && !parentProp) {
        inspectModelForTypeParams(
          allModels,
          rootModel,
          toInspect,
          rootProp,
          prop
        )
      }
    }
  })
}
