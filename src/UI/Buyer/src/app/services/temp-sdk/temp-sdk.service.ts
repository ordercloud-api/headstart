import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { ListPageWithFacets, Tokens } from 'ordercloud-javascript-sdk'
import {
  ListArgs,
  HSOrder,
  SuperHSMeProduct,
  HSMeProduct,
} from '@ordercloud/headstart-sdk'
import { ListPage } from '@ordercloud/headstart-sdk'
import { AppConfig } from 'src/app/models/environment.types'

// WHOPLE FILE TO BE REPLACED BY SDK
@Injectable({
  providedIn: 'root',
})
export class TempSdk {
  constructor(private http: HttpClient, private appConfig: AppConfig) {}

  buildHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
  }

  createHttpParams(args: ListArgs): HttpParams {
    let params = new HttpParams()
    Object.entries(args).forEach(([key, value]) => {
      if (key !== 'filters' && value) {
        params = params.append(key, value.toString())
      }
    })
    Object.entries(args.filters).forEach(([key, value]) => {
      if ((typeof value !== 'object' && value) || (value && value.length)) {
        params = params.append(key, value.toString())
      }
    })
    return params
  }

  async listMeProducts(
    args: ListArgs
  ): Promise<ListPageWithFacets<HSMeProduct>> {
    const url = `${this.appConfig.middlewareUrl}/me/products`
    return await this.http
      .get<ListPageWithFacets<HSMeProduct>>(url, {
        headers: this.buildHeaders(),
        params: this.createHttpParams(args),
      })
      .toPromise()
  }

  async listReflektionProducts(userID: string, page: number, searchTerm: string) : Promise<ListPageWithFacets<HSMeProduct>> {
    var body = {
      "n_item": 20,
      "page_number": page,
      "data": {
          "query": {
              "keyphrase": {
                  "value": [
                    searchTerm
                  ]
              }
          },
          "context": {
              "user": {
                  "uuid": userID
              }
          },
          "content": {
              "product": {}
          },
          "force_v2_specs": true
      }
    };
    var resp = await this.http.post<any>("https://api-staging.rfksrv.com/search-rec/12353-150015332/3?", body, { headers: { Authorization: "01-14c9627a-35d9c17fa8dd9b1b627d4890fcab45dcec5df4ab"} }).toPromise();

    var products = resp.content.product.value.map(p => {
      return {
        ID: p.id,
        Name: p.name,
        QuantityMultiplier: 1,
        PriceSchedule: {
          MinQuantity: 1,
          PriceBreaks: [{
            Quantity: 1,
            Price: p.price
          }]
        },
        xp: {
          Currency: "USD",
          Images: [{
            Url: p.image_url
          }]
        }
      }
    })
    return { 
      Meta: {
        TotalPages: resp.total_page,
        PageSize: resp.content.product.n_item,
        TotalCount: resp.content.product.total_item,
        Page: page,
        Facets: []
      },
      Items: products
    };
  }

  async getMeProduct(id: string): Promise<SuperHSMeProduct> {
    const url = `${this.appConfig.middlewareUrl}/me/products/${id}`
    return await this.http
      .get<SuperHSMeProduct>(url, { headers: this.buildHeaders() })
      .toPromise()
  }

  async getSupplierFilterConfig(): Promise<ListPage<any>> {
    const url = `${this.appConfig.middlewareUrl}/supplierfilterconfig`
    return await this.http
      .get<ListPage<any>>(url, {
        headers: this.buildHeaders(),
      })
      .toPromise()
  }

  async deleteLineItem(orderID: string, lineItemID: string): Promise<void> {
    const url = `${this.appConfig.middlewareUrl}/order/${orderID}/lineitems/${lineItemID}`
    return await this.http
      .delete<void>(url, { headers: this.buildHeaders() })
      .toPromise()
  }

  async applyAutomaticPromotionsToOrder(orderID: string): Promise<HSOrder> {
    const url = `${this.appConfig.middlewareUrl}/order/${orderID}/applypromotions`
    return await this.http
      .post<HSOrder>(url, { headers: this.buildHeaders() })
      .toPromise()
  }
}
