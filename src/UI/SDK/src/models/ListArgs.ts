import { Filters } from './Filters'

export interface ListArgs<T = any> {
  search?: string
  searchOn?: string[]
  sortBy?: string[]
  page?: number
  pageSize?: number
  filters?: Filters<Required<T>>
}
