import { ListFilter } from './ListFilter';

export interface CosmosListOptions {
    PageSize?: number
    ContinuationToken?: string
    Filters?: ListFilter[]
    Sort?: string
    SortDirection?: 'ASC' | 'DESC'
    Search?: string
    SearchOn?: string
}