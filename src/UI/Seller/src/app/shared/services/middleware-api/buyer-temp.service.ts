import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Inject, Injectable } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { HSBuyerPriceMarkup } from '@app-seller/models/buyer-markups.types'
import { AppConfig } from '@app-seller/models/environment.types'
import { OcTokenService } from '@ordercloud/angular-sdk'
import { HSBuyer } from '@ordercloud/headstart-sdk'

// WHOPLE FILE TO BE REPLACED BY SDK

@Injectable({
  providedIn: 'root',
})
export class BuyerTempService {
  constructor(
    private ocTokenService: OcTokenService,
    private http: HttpClient,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  private buildHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.ocTokenService.GetAccess()}`,
    })
  }

  async get(buyerID: string): Promise<HSBuyerPriceMarkup> {
    const url = `${this.appConfig.middlewareUrl}/buyer/${buyerID}`
    return await this.http
      .get<HSBuyerPriceMarkup>(url, { headers: this.buildHeaders() })
      .toPromise()
  }

  async create(superBuyer: HSBuyerPriceMarkup): Promise<HSBuyerPriceMarkup> {
    const url = `${this.appConfig.middlewareUrl}/buyer`
    return await this.http
      .post<HSBuyerPriceMarkup>(url, superBuyer, {
        headers: this.buildHeaders(),
      })
      .toPromise()
  }

  async save(
    buyerID: string,
    superBuyer: HSBuyerPriceMarkup
  ): Promise<HSBuyerPriceMarkup> {
    const url = `${this.appConfig.middlewareUrl}/buyer/${buyerID}`
    return await this.http
      .put<HSBuyerPriceMarkup>(url, superBuyer, {
        headers: this.buildHeaders(),
      })
      .toPromise()
  }
}
