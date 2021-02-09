/**
 * All the OC API "List" endpoints share a common pagination, searching, and sorting functionality.
 *
 * Use this as a super class for table or list components that need those features.
 */
export abstract class BaseBrowse {
  requestOptions = { search: undefined, page: undefined, sortBy: undefined }

  constructor() {}

  pageChanged(page: number) {
    Object.assign(this.requestOptions, { page: page })
    this.loadData()
  }

  searchChanged(searchStr: string) {
    Object.assign(this.requestOptions, { search: searchStr, page: undefined })
    this.loadData()
  }

  sortChanged(sortStr: string) {
    Object.assign(this.requestOptions, { sortBy: sortStr, page: undefined })
    this.loadData()
  }

  // Override this method in the sub class
  abstract loadData()
}
