import { CookieOptions } from '../models'

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
class CookieApi {
  public write(
    name: string,
    value: string = undefined,
    options: CookieOptions
  ) {
    document.cookie = this.buildCookieString(name, value, options)
  }

  public read(name: string) {
    const rows = document.cookie.split(';')
    for (const row of rows) {
      const [key, val] = row.split('=')
      const cookieKey = decodeURIComponent(key.trim().toLowerCase())
      if (cookieKey === name.toLowerCase()) {
        return decodeURIComponent(val)
      }
    }
    return ''
  }

  // dont use this method - only making public so I can test the cookie string
  // document.cookie only returns name=value pairs, not expiration or any other options set on create
  public buildCookieString(
    name: string,
    value: string = undefined,
    options: CookieOptions
  ) {
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
    str += options.path ? ';path=' + options.path : ''

    // per http://www.ietf.org/rfc/rfc2109.txt browser must allow at minimum 4096 bytes per cookie
    const cookieLength = str.length + 1
    if (cookieLength > 4096) {
      console.warn(
        `Cookie ${name} possibly not set or overflowed because it was too large! (${cookieLength} > 4096 bytes)`
      )
    }

    return str
  }
}

export default new CookieApi()