import { SdkConfiguration } from './models'

class Configuration {
  private config: SdkConfiguration = {
    baseApiUrl: 'https://marketplace-api-qa.azurewebsites.net',
    baseAuthUrl: 'https://auth.ordercloud.io/oauth/token',
    timeoutInMilliseconds: 120 * 1000,
    clientID: null,
    cookieOptions: {
      samesite: 'lax', // browser default
      secure: false,
      domain: null,
    },
  }

  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  constructor() {
    this.Set = this.Set.bind(this)
    this.Get = this.Get.bind(this)
  }

  Set(config: SdkConfiguration): void {
    this.config = { ...this.config, ...config }
  }

  Get(): SdkConfiguration {
    return this.config
  }
}

export default new Configuration()
