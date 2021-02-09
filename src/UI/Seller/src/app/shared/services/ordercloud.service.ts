import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Inject, Injectable } from '@angular/core'
import { applicationConfiguration } from '../../config/app.config'
import { OcTokenService } from '@ordercloud/angular-sdk'
import { AppConfig } from '@app-seller/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class OrderCloudService {
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

  resetPassword(body: unknown, headerToUse?: HttpHeaders): boolean {
    const headerResult: HttpHeaders =
      headerToUse == null ? headerToUse : this.headers.headers

    const url = `${this.appConfig.orderCloudApiUrl}/v1/me/password`
    this.http.post(url, body, { headers: headerResult }).subscribe(
      () => {
        return true
      },
      (error) => {
        throw error
      }
    )
    return false
  }
}
