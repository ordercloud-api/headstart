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
