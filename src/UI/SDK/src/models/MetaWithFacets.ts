import { ListFacet } from './ListFacet';

export interface MetaWithFacets {
    Facets?: ListFacet[]
    Page?: number
    PageSize?: number
    TotalCount?: number
    TotalPages?: number
    ItemRange?: number[]
}