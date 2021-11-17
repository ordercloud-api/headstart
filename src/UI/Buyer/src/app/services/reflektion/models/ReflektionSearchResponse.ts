export interface ReflektionSearchResponse {
  content: {
    product: {
      total_item: number
      n_item: number
      value: ReflektionProduct[]
    }
  }
  total_item: number
  page_number: number
  total_page: number
  url: string
  rid: string
  dt: number
  query2id: {
    keyphrase: string
  }
  n_item: number
  ts: number
}

export interface ReflektionProduct {
  sku: string
  skuid: string
  name: string
  color: string
  on_sale: number
  price: string
  category_names: string[]
  brand: string
  product_url: string
  all_category_names: string[]
  image_url: string
  final_price: number
  breadcrumbs: string
  product_group: number
  id: number
  size: string
}
