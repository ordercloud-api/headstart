export interface ReflektionProductSearchResponse {
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
  n_item: number,
  suggestion: ReflektionSearchSuggestions
  query2id: {
    keyphrase: string
  }
  facet: any
}

export type ReflektionProductDetailWidgetResponse =
  ReflektionBatchResponse<ReflektionBatchProduct>

export type ReflektionHomeWidgetResponse = ReflektionBatchResponse<
  ReflektionBatchAppearance | ReflektionBatchProduct
>

export interface ReflektionBatchResponse<T> {
  errors?: ReflektionError[]
  url: string
  ts: number
  rid: string
  dt: string
  batch: T[]
}

export interface ReflektionBatchAppearance {
  widget: ReflektionWidgetDetail
  appearance: {
    templates: {
      html: {
        devices: {
          pc: {
            content: string
          }
        }
      }
      css: {
        devices: {
          pc: {
            content: string
          }
        }
      }
    }
    css_names: string[]
    html_names: string[]
  }
}

export interface ReflektionBatchProduct {
  widget: ReflektionWidgetDetail
  total_item: number
  content: {
    product: ReflektionContentProductDetail
  }
  page_number: number
  total_page: number
  n_item: number
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

export interface ReflektionSearchSuggestions {
  category?: ReflektionSearchSuggestion[]
  keyphrase?: ReflektionSearchSuggestion[]
  trending_category?: ReflektionSearchSuggestion[]
}

export interface ReflektionSearchSuggestion {
  id: string
  in_content: "product" | void
  text: string
  url?: string
}
