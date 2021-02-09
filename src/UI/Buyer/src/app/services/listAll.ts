import { flatten, range } from 'lodash'
import { Meta } from 'ordercloud-javascript-sdk'

interface ListPage<T> {
  Items?: T[]
  Meta?: Meta
}

/**
 * @description returns all items from all pages for an ordercloud list function
 *
 * @param listFn the ordercloud function that will be called repeatedly
 * until all items have been retrieved (not invoked)
 * @param listArgs any arguments to the function should be passed in
 * as separate parameters
 *
 * @example
 * listAll(this.ocProductsService, {filters: {'xp.Color': 'Red'}});
 */
export async function listAll<T = any>(
  service: any,
  listFunc: (...args: any) => Promise<ListPage<T>>,
  ...listArgs: any[]
): Promise<ListPage<T>> {
  // get or create filters obj if it doesnt exist
  listFunc = listFunc.bind(service)
  const hasFiltersObj = typeof listArgs[listArgs.length - 1] === 'object'
  const filtersObj = hasFiltersObj ? listArgs.pop() : {}

  // set page and pageSize
  filtersObj.page = 1
  filtersObj.pageSize = 100

  const result1 = await listFunc(...listArgs, filtersObj)
  const additionalPages = range(2, result1?.Meta.TotalPages + 1)

  const requests = additionalPages.map((page: number) =>
    listFunc(...listArgs, { ...filtersObj, page })
  )
  const results: ListPage<T>[] = await Promise.all(requests)
  // combine and flatten items for all list calls
  return {
    Items: flatten([result1, ...results].map((r) => r.Items)),
    Meta: result1.Meta,
  }
}
