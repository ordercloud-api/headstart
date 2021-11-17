import { ReflektionAccessToken } from '..'
import httpClient from '../utils/HttpClient'
import cookies from '../utils/CookieService'

export default class Reflektion {
  private impersonating: boolean = false
  private reflektionTokenCookieName = `ordercloud.reflektion-token`
  private reflektionUuidCookieName = `ordercloud.reflektion-uuid`

  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  constructor() {
    this.GenerateToken = this.GenerateToken.bind(this)
    this.GetToken = this.GetToken.bind(this)
    this.SetToken = this.SetToken.bind(this)
    this.SetUuid = this.SetUuid.bind(this)
    this.GetUuid = this.Getuuid.bind(this)
  }

  /**
   * @param orderID ID of the order.
   * @param paymentUpdateRequest
   * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
   */
  public async GenerateToken(
    accessToken?: string
  ): Promise<ReflektionAccessToken> {
    const impersonating = this.impersonating
    this.impersonating = false
    return await httpClient.get(`/reflektion/token`, {
      params: { accessToken, impersonating },
    })
  }

  public GetToken(): string {
    return cookies.get(this.reflektionTokenCookieName)
  }

  public SetToken(token: string): void {
    cookies.set(this.reflektionTokenCookieName, token)
  }

  public GetUuid(): string {
    return cookies.get(this.reflektionTokenCookieName)
  }

  public SetUuid(token: string): void {
    cookies.set(this.reflektionTokenCookieName, token)
  }

  public async Search(
    reflectionUrl: string,
    page: number,
    search: string
  ): Promise<any> {
    const body = this.buildSearchRequest()
  }

  private buildSearchRequest(
    uuid: string,
    search: string,
    sortBy: string[],
    page: number
  ) {
    const sortArray = (sortBy || []).map(value => {
      const [name, order] = value.split('-')
      return { name, order }
    })
    return {
      data: {
        n_item: 20,
        page_number: Number(page),
        query: {
          keyphrase: {
            value: [search ?? ''],
          },
        },
        context: {
          user: {
            uuid: uuid,
          },
        },
        sort: {
          value: sortArray,
        },
        content: {
          product: {},
        },
        force_v2_specs: true,
      },
    }
  }

  /**
   * @description
   * enables impersonation by calling the subsequent method with the stored impersonation token
   *
   * @example
   * Payments.As().List() // lists Payments using the impersonated users' token
   */
  public As(): this {
    this.impersonating = true
    return this
  }
}
