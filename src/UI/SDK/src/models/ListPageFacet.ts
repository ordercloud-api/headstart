import { MetaWithFacets } from "ordercloud-javascript-sdk";

export interface ListPageFacet<TItem> {
  Items?: TItem[]
  Meta?: MetaWithFacets
}
