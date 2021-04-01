import Configuration from '../configuration'

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
class CookieService {
  constructor() {
    this.get = this.get.bind(this)
    this.set = this.set.bind(this)
    this.buildCookieString = this.buildCookieString.bind(this)
    this.remove = this.remove.bind(this)
  }

  public get(cookieName: string): string {
    const rows = document.cookie.split(';')
    for (const row of rows) {
      const [key, val] = row.split('=')
      const cookieKey = decodeURIComponent(key.trim().toLowerCase())
      if (cookieKey === cookieName.toLowerCase()) {
        return decodeURIComponent(val)
      }
    }
    return ''
  }

  public set(cookieName: string, cookieVal: string): void {
    document.cookie = this.buildCookieString(cookieName, cookieVal)
  }

  public remove(cookieName: string): void {
    document.cookie = this.buildCookieString(cookieName, undefined)
  }

  private buildCookieString(name: string, value?: string) {
    const options = Configuration.Get().cookieOptions || {}
    let expires
    if (!value) {
      expires = new Date('Thu, 01 Jan 1970 00:00:00 GMT')
      value = ''
    } else {
      // set expiration of cookie longer than token
      // so we can parse clientid from token to perform refresh when token has expired
      expires = new Date()
      expires.setFullYear(expires.getFullYear() + 1)
    }

    let str = encodeURIComponent(name) + '=' + encodeURIComponent(value)
    str += options.domain ? ';domain=' + options.domain : ''
    str += expires ? ';expires=' + expires.toUTCString() : ''
    str += options.secure ? ';secure' : ''
    str += options.samesite ? ';samesite=' + options.samesite : ''

    return str
  }
}

export default new CookieService()
