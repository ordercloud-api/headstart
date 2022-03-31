import { SdkConfiguration } from './models'

class Configuration {
  private defaultConfig: SdkConfiguration = {
    baseApiUrl: 'https://headstartdemo-middleware.azurewebsites.net',
    orderCloudApiUrl: 'https://sandboxapi.ordercloud.io',
    timeoutInMilliseconds: 120 * 1000, // 120 seconds by default
    clientID: null,
    cookieOptions: {
      samesite: 'lax', // browser default
      secure: false,
      domain: null,
      prefix: 'ordercloud',
      path: '/', // accessible on any path in the domain
    },
  }
  private config: SdkConfiguration = cloneDeep(this.defaultConfig)

  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  constructor() {
    this.Set = this.Set.bind(this)
    this.Get = this.Get.bind(this)
  }

  Set(config: SdkConfiguration): void {
    this.config = { ...this.defaultConfig, ...this.config, ...(config || {}) }
    this.config.cookieOptions = {
      ...this.defaultConfig.cookieOptions,
      ...this.config.cookieOptions,
      ...(config?.cookieOptions || {}),
    }
  }

  Get(): SdkConfiguration {
    return this.config
  }
}

// takes an object and creates a new one with same properties/values
// useful so we don't mutate original object
function cloneDeep(obj: any): any {
  return JSON.parse(JSON.stringify(obj))
}

export default new Configuration()
