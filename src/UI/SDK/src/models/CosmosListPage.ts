import { CosmosMeta } from './CosmosMeta'

export interface CosmosListPage<TItem> {
  Items?: TItem[]
  Meta?: CosmosMeta
}
