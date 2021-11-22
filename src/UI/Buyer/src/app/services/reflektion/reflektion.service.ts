import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import {
  HSMeProduct,
  HeadStartSDK,
  HSLineItem,
} from '@ordercloud/headstart-sdk'
import {
  ListPageWithFacets,
  MetaWithFacets,
  OrderWorksheet,
  Product,
  ProductFacet,
  User,
} from 'ordercloud-javascript-sdk'
import { ProductSortOption } from 'src/app/components/products/sort-products/sort-products.component'
import { AppConfig } from 'src/app/models/environment.types'
import { ProductFilters } from 'src/app/models/filter-config.types'
import { CookieService } from 'ngx-cookie'
import {
  ReflektionBatchSearchResponse,
  ReflektionProduct,
  ReflektionSearchResponse,
} from './models/ReflektionSearchResponse'
import { JwtHelperService } from '@auth0/angular-jwt'

export const hsFrequentlyBoughtTogetherWidget = 'hs-frequently-bought-together'
export const hsSimilarProductsWidget = 'hs-similar-products'

@Injectable({
  providedIn: 'root',
})
export class ReflektionService {
  private reflektionTokenCookieName = `ordercloud.reflektion-token`
  private reflektionUuidCookieName = `ordercloud.reflektion-uuid`
  private jwtHelper = new JwtHelperService()

  constructor(
    private http: HttpClient,
    private appConfig: AppConfig,
    private cookieService: CookieService
  ) {}

  reflektionSortOptions: ProductSortOption[] = [
    { label: 'Name: A-Z', value: 'name-asc' },
    { label: 'Name: Z-A', value: 'name-desc' },
    { label: 'Price: High to Low', value: 'price-desc' },
    { label: 'Price: Low to High', value: 'price-asc' },
    { label: 'Rating: High to Low', value: 'review-desc' },
    { label: 'Rating: Low to High', value: 'review-asc' },
    { label: 'Reviews: High to Low', value: 'review-desc' },
    { label: 'Reviews: Low to High', value: 'review-asc' },
    { label: 'Featured', value: 'featured-desc' },
  ]

  async init(): Promise<void> {
    if (!this.getUuid()) {
      const uuid = window.rfk.uid()
      this.setUuid(uuid)
    }
    const token = this.getToken()
    if (!token || this.jwtHelper.isTokenExpired(token)) {
      const reflektionToken = await HeadStartSDK.Reflektion.GetToken()
      this.setToken(reflektionToken.accessToken)
    }
  }

  async listProducts(
    filters: any,
    userID?: string
  ): Promise<ListPageWithFacets<HSMeProduct>> {
    await this.init() // should be initialized already (base resolve service) but just making sure
    const reflektionResponse = await this.searchReflektion(
      filters.search,
      filters.sortBy,
      filters.page,
      filters?.filters,
      userID
    )
    const meProducts = {
      Meta: this.mapMeta(reflektionResponse),
      Items: reflektionResponse.content.product.value.map(
        this.mapProduct.bind(this)
      ),
    }
    return meProducts
  }

  async listAlsoViewedProducts(
    productID: string,
    userID?: string
  ): Promise<ListPageWithFacets<HSMeProduct>> {
    const body = {
      widget: {
        rfkid: 'hs-also-viewed-products',
      },
      context: {
        user: {
          user_id: userID || undefined, // error if null
          uuid: this.getUuid(),
        },
        page: {
          sku: [productID],
        },
      },
      content: {
        product: {},
      },
    }
    const response = await this.http
      .post<ReflektionSearchResponse>(
        `${this.appConfig.reflektionUrl}/api/search-rec/3`,
        body,
        {
          headers: { Authorization: this.getToken() },
        }
      )
      .toPromise()
    const meProducts = {
      Meta: this.mapMeta(response),
      Items: response.content.product.value.map(this.mapProduct.bind(this)),
    }
    return meProducts
  }

  async getProductDetailWidgetData(
    productID: string,
    userID?: string
  ): Promise<{
    [hsFrequentlyBoughtTogetherWidget]: HSMeProduct[]
    [hsSimilarProductsWidget]: HSMeProduct[]
  }> {
    const body = {
      data: {
        batch: [
          {
            widget: {
              rfkid: hsFrequentlyBoughtTogetherWidget,
            },
          },
          {
            widget: {
              rfkid: hsSimilarProductsWidget,
            },
          },
        ],
        context: {
          page: {
            sku: [productID],
          },
          user: {
            uuid: this.getUuid(),
            userID: userID || undefined,
          },
        },
        content: {
          product: {},
        },
      },
    }
    const response = await this.http
      .post<ReflektionBatchSearchResponse>(
        `${this.appConfig.reflektionUrl}/api/search-rec/3`,
        body,
        {
          headers: { Authorization: this.getToken() },
        }
      )
      .toPromise()
    return {
      [hsFrequentlyBoughtTogetherWidget]: response.batch
        .find((b) => b.widget.rfkid === hsFrequentlyBoughtTogetherWidget)
        .content.product.value.map(this.mapProduct.bind(this)),
      [hsSimilarProductsWidget]: response.batch
        .find((b) => b.widget.rfkid === hsSimilarProductsWidget)
        .content.product.value.map(this.mapProduct.bind(this)),
    }
  }

  /**
   *
   * @param view where the add to cart happened
   * @param productID the ID of the product that was added to cart
   */
  trackAddToCart(view: 'pdp' | 'qview' | 'cart', lineItem: HSLineItem): void {
    this.trackReflectionEvent('a2c', view, {
      products: {
        sku: lineItem.ProductID,
        price: lineItem.UnitPrice,
        quantity: lineItem.Quantity,
      },
    })
  }

  /**
   *
   * @param view where the product or products were seen
   * @param products the list of products that were seen
   */
  trackProductView(
    view: 'home' | 'pdp' | 'confirm' | 'cart' | 'category' | 'search' | 'other',
    products: Product[]
  ): void {
    this.trackReflectionEvent('view', view, {
      products: products.map((p) => ({ sku: p.ID })),
    })
  }

  // used to track a widget, this should only be called
  // if the widget is in the user's view port
  trackWidgetView(rfkid: string): void {
    this.trackReflectionEvent('widget', 'appear', { rfkid })
  }

  trackUserLogin(user: User): void {
    this.trackReflectionEvent('user', 'login', {
      context: {
        user: {
          id: user.ID,
          email: user.Email,
        },
      },
    })
  }

  // use this to track anytime user information is updated
  trackUserInfo(user: User): void {
    this.trackReflectionEvent('user', 'info', {
      context: {
        user: {
          id: user.ID,
          email: user.Email,
        },
      },
    })
  }

  trackOrderSubmit(worksheet: OrderWorksheet, isAnonymous: boolean): void {
    const user = worksheet.Order.FromUser
    const lineItems = worksheet.LineItems
    this.trackReflectionEvent('order', 'confirm', {
      context: {
        user: isAnonymous
          ? undefined
          : {
              id: user.ID,
              email: user.Email,
              address: worksheet.Order.BillingAddress,
            },
      },
      products: lineItems.map((li) => ({
        sku: li.ProductID,
        quantity: li.Quantity,
        price: li.LineTotal,
        price_original: li.LineSubtotal,
      })),
      checkout: {
        order_id: worksheet.Order.ID,
        subtotal: worksheet.Order.Subtotal,
        total: worksheet.Order.Total,
      },
    })
  }

  private trackReflectionEvent(type: string, name: string, value: any = {}) {
    window.rfk.push([
      'trackEvent',
      {
        type,
        name,
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        value,
      },
    ])
  }

  private mapProduct(reflektionProduct: ReflektionProduct): HSMeProduct {
    const product = {
      ID: reflektionProduct.sku,
      Name: reflektionProduct.name,
      QuantityMultiplier: 1,
      PriceSchedule: {
        Name: '',
        MinQuantity: 1,
        PriceBreaks: [
          {
            Quantity: 1,
            Price: Number(reflektionProduct.final_price),
          },
        ],
      },
      xp: {
        Currency: 'USD',
        Images: [
          {
            Url: reflektionProduct.image_url,
          },
        ],
      },
    }
    return product as HSMeProduct
  }

  private getToken(): string {
    return this.cookieService.get(this.reflektionTokenCookieName)
  }

  private setToken(token: string): void {
    this.cookieService.put(this.reflektionTokenCookieName, token)
  }

  private getUuid(): string {
    return this.cookieService.get(this.reflektionUuidCookieName)
  }

  private setUuid(uuid: string): void {
    this.cookieService.put(this.reflektionUuidCookieName, uuid)
  }

  private async searchReflektion(
    search: string,
    sortBy: string[],
    page: number,
    filters?: any,
    userID?: string
  ): Promise<ReflektionSearchResponse> {
    const body = this.buildReflektionSearchRequest(
      search,
      sortBy,
      page,
      filters,
      userID
    )
    return await this.http
      .post<ReflektionSearchResponse>(
        `${this.appConfig.reflektionUrl}/api/search-rec/3`,
        body,
        {
          headers: { Authorization: this.getToken() },
        }
      )
      .toPromise()
  }

  private buildReflektionSearchRequest(
    search: string,
    sortBy: string[],
    page: number,
    filters?: any,
    userID?: string
  ) {
    const sortArray = (sortBy || []).map((value) => {
      const [name, order] = value.split('-')
      return { name, order }
    })
    const categoryFilter = filters?.categoryID ? [filters.categoryID] : []
    const allFilters = this.buildFilters(categoryFilter, filters)

    let searchRequest = {
      data: {
        n_item: 20,
        page_number: Number(page),
        query: {
          keyphrase: {
            value: [search ?? ''],
          },
        },
        widget: {
          rfkid: 'rfkid_7',
        },
        suggestion: {
          keyphrase: {
            max: 1,
          },
        },
        request_for: ['query'],
        context: {
          user: {
            user_id: userID || undefined, // error if null
            uuid: this.getUuid(),
          },
        },
        sort: {
          value: sortArray,
        },
        facet: {
          all: true,
        },
        filter: allFilters,
        content: {
          product: {},
        },
        force_v2_specs: true,
      },
    }
    searchRequest = this.mapFacetFilter(filters, searchRequest)
    return searchRequest
  }

  private mapFacetFilter(filter: any, searchRequest: any): any {
    if (filter['xp.Facets.gender']) {
      const genderSplit = filter['xp.Facets.gender'].split('|')
      searchRequest.data.filter.genders = {
        value: genderSplit,
      }
    }
    return searchRequest
  }

  private mapMeta(response: ReflektionSearchResponse): MetaWithFacets {
    const Page = response.page_number
    const PageSize = response.content.product.n_item
    const TotalCount = response.content.product.total_item
    const itemRangeStart = (Page - 1) * PageSize + 1
    const itemRangeEnd = Math.max(itemRangeStart + PageSize, TotalCount)
    const meta = {
      Facets: response?.facet ? this.mapFacets(response.facet) : [],
      Page,
      PageSize,
      TotalCount,
      TotalPages: response.total_page,
      ItemRange: [itemRangeStart, itemRangeEnd],
    }
    return meta
  }

  private mapFacets(facets: any): any {
    // Hard-coding which facets from this Reflektion environment to display for a demo
    const facetsToDisplay = ['genders', 'price', 'size']
    const facetMap = Object.entries(facets)
    const result = []
    for (const [facetName, facet] of facetMap) {
      if (facetsToDisplay.includes(facetName)) {
        const values = []
        for (const value of facet['value']) {
          values.push({
            Value: value.text,
            Count: value.count,
          })
        }
        result.push({
          Name: facet['display_name'],
          XpPath: 'Facets.' + facetName,
          Values: values,
        })
      }
    }
    return result
  }

  private buildFilters(categoryFilter: any[], filters: any) {
    const allFilters = {
      all_category_ids: {
        value: categoryFilter,
      },
    }

    const filterProperties = Object.entries(filters)
    for (const [key, value] of filterProperties) {
      if (key.startsWith('xp.Facets.price')) {
        const keySplit = key.split('.')
        const priceValues = { value: [] }
        const valueSplit = (value as string).split('|')
        for (const value of valueSplit) {
          const priceSplit = value.split(' - ')
          priceValues.value.push({
            min: parseInt(priceSplit[0]),
            max: parseInt(priceSplit[1]),
          })
        }
        allFilters[keySplit[keySplit.length - 1]] = priceValues
      } else if (key.startsWith('xp.Facets')) {
        const keySplit = key.split('.')
        const valueSplit = { value: (value as string).split('|') }
        allFilters[keySplit[keySplit.length - 1]] = valueSplit
      }
    }

    return allFilters
  }
}
