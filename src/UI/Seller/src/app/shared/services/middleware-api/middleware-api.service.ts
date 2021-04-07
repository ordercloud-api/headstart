/* eslint-disable @typescript-eslint/no-unsafe-return */
/* eslint-disable @typescript-eslint/restrict-template-expressions */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/restrict-plus-operands */
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Inject, Injectable } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig } from '@app-seller/models/environment.types'
import { OcTokenService, Order } from '@ordercloud/angular-sdk'
import {
  ListPage,
  SuperHSProduct,
  BatchProcessResult,
  SupplierFilterConfigDocument,
  SuperHSShipment,
  HSSupplier,
  AssetUpload,
} from '@ordercloud/headstart-sdk'
import { Observable } from 'rxjs'

@Injectable({
  providedIn: 'root',
})
// TODO: replace these manually written API calls with the headstart sdk
export class MiddlewareAPIService {
  readonly headers = {
    headers: new HttpHeaders({
      Authorization: `Bearer ${this.ocTokenService.GetAccess()}`,
    }),
  }
  constructor(
    private ocTokenService: OcTokenService,
    private http: HttpClient,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  async acknowledgeQuoteOrder(orderID: string): Promise<Order> {
    const url = `${this.appConfig.middlewareUrl}/order/acknowledgequote/${orderID}`
    return await this.http.post<Order>(url, this.headers).toPromise()
  }

  async isLocationDeletable(locationID: string): Promise<boolean> {
    const url = `${this.appConfig.middlewareUrl}/supplier/candelete/${locationID}`
    return await this.http.get<boolean>(url, this.headers).toPromise()
  }

  async updateSupplier(
    supplierID: string,
    supplier: HSSupplier
  ): Promise<HSSupplier> {
    const url = `${this.appConfig.middlewareUrl}/supplier/${supplierID}`
    return await this.http.patch(url, supplier, this.headers).toPromise()
  }

  async getSupplierFilterConfig(): Promise<
    ListPage<SupplierFilterConfigDocument>
  > {
    const url = `${this.appConfig.middlewareUrl}/supplierfilterconfig`
    return await this.http
      .get<ListPage<SupplierFilterConfigDocument>>(url, this.headers)
      .toPromise()
  }

  async getSupplierData(supplierOrderID: string): Promise<any> {
    const url = `${this.appConfig.middlewareUrl}/supplier/orderdetails/${supplierOrderID}`
    return await this.http.get<any>(url, this.headers).toPromise()
  }

  async patchLineItems(
    superShipment: SuperHSShipment,
    headers: HttpHeaders
  ): Promise<any> {
    return await this.http
      .post(this.appConfig.middlewareUrl + '/shipment', superShipment, {
        headers,
      })
      .toPromise()
  }

  batchShipmentUpload(
    formData: FormData,
    headers: HttpHeaders
  ): Observable<BatchProcessResult> {
    return this.http.post(
      this.appConfig.middlewareUrl + '/shipment/batch/uploadshipment',
      formData,
      { headers }
    )
  }

  uploadProductImage(productID: string, asset: AssetUpload) {
    const formData = new FormData()
    formData.append('ID', asset.ID)
    formData.append('Url', asset.Url) 
    formData.append('Title', asset.Title)
    formData.append('File', asset.File)
    return this.http.put(
      this.appConfig.middlewareUrl + `/products/images/${productID}`, 
      formData, 
      this.headers
    ).toPromise()
  }
}
