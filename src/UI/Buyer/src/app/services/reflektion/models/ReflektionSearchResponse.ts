export interface ReflektionSearchResponse {
  widget: ReflektionWidgetDetail
  total_item: number
  rid: string
  dt: number
  errors: ReflektionError[]
  url: string
  ts: number
  content: {
    product: ReflektionContentProductDetail
  }
  page_number: number
  total_page: number
  n_item: number
  query2id: {
    keyphrase: string
  }
  facet: any
}

export interface ReflektionBatchSearchResponse {
  errors: ReflektionError[]
  batch: {
    widget: ReflektionWidgetDetail
    total_item: number
    content: {
      product: ReflektionContentProductDetail
    }
    page_number: number
    total_page: number
    n_item: number
  }[]
  url: string
  ts: number
  rid: string
  dt: string
}

export interface ReflektionContentProductDetail {
  total_item: number
  n_item: number
  value: ReflektionProduct[]
}
export interface ReflektionWidgetDetail {
  rfkid: string
  used_in: string
  variation_id: string
  type: string
}

export interface ReflektionError {
  message: string
  code: number
  type: string
  severity: string
  details: any
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
