import httpClient from '../utils/HttpClient'

export default class Support {
  private impersonating: boolean = false

  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  constructor() {
    this.SubmitCase = this.SubmitCase.bind(this)
  }

  /**
   * @param supportCase
   * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
   */
  public async SubmitCase(
    supportCase: FormData,
    accessToken?: string
  ): Promise<void> {
    const impersonating = this.impersonating
    this.impersonating = false
    return await httpClient.post(`/support/submitcase`, supportCase, {
      params: { accessToken, impersonating },
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  }

  /**
   * @description
   * enables impersonation by calling the subsequent method with the stored impersonation token
   *
   * @example
   * Support.As().List() // lists Support using the impersonated users' token
   */
  public As(): this {
    this.impersonating = true
    return this
  }
}
