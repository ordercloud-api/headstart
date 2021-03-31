import { MetaWithFacets } from './MetaWithFacets'

export interface ListPageFacet<TItem> {
  Items?: TItem[]
  Meta?: MetaWithFacets
}
