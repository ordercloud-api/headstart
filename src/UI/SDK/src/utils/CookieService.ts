import Configuration from '../configuration'
import cookie from './CookieApi'

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
class CookieService {
  constructor() {
    this.get = this.get.bind(this)
    this.set = this.set.bind(this)
    this.remove = this.remove.bind(this)
  }

  public get(name: string): string {
    const configuration = Configuration.Get()
    const cookieName = configuration.cookieOptions.prefix + name
    return cookie.read(cookieName)
  }

  public set(name: string, cookieVal: string): void {
    const configuration = Configuration.Get()
    const cookieName = configuration.cookieOptions.prefix + name
    cookie.write(cookieName, cookieVal, configuration.cookieOptions)
  }

  public remove(name: string): void {
    const configuration = Configuration.Get()
    const cookieName = configuration.cookieOptions.prefix + name
    cookie.write(cookieName, undefined, configuration.cookieOptions)
  }
}

export default new CookieService()