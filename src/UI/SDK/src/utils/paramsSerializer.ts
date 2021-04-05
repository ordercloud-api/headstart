/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
export default function ParamSerializer(params: {
  [key: string]: any
}): string {
  const valuesArray: string[] = []

  // serialize filters first, they are handled specially
  if (params.filters) {
    const filters = flattenFiltersObject(params.filters)
    for (const key in filters) {
      const filterVal = filters[key]
      if (Array.isArray(filterVal)) {
        filterVal.forEach(val =>
          valuesArray.push(`${key}=${encodeURIComponent(val)}`)
        )
      } else if (filterVal) {
        valuesArray.push(`${key}=${encodeURIComponent(filterVal)}`)
      }
    }
    delete params.filters
  }

  // serialize the rest of the params
  for (const key in params) {
    const val = params[key]
    if (val) {
      valuesArray.push(`${key}=${encodeURIComponent(val)}`)
    }
  }

  return valuesArray.length ? `${valuesArray.join('&')}` : ''
}

/**
 * @ignore
 * not part of public api, don't include in generated docs
 *
 * build a flattened filters object  where each key is the dot-referenced property
 * to filter and the value is the value to filter by
 * this ultimately gets sent to ordercloud as a query param
 */
function flattenFiltersObject(filters) {
  const result = {}
  for (const key in filters) {
    inspectProp(filters[key], key, result)
  }
  return result
}

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
function inspectProp(propVal, propName, result) {
  const isObject = Object.prototype.toString.call(propVal) === '[object Object]'
  if (isObject) {
    for (const key in propVal) {
      inspectProp(propVal[key], `${propName}.${key}`, result)
    }
  } else {
    if (propVal === null) {
      throw new Error(
        `Null is not a valid filter prop. Use negative filter "!" combined with wildcard filter "*" to define a filter for the absence of a value. \nex: an order list call with { xp: { hasPaid: '!*' } } would return a list of orders where xp.hasPaid is null or undefined\nhttps://ordercloud.io/features/advanced-querying#filtering`
      )
    }
    result[propName] = propVal
  }
}
