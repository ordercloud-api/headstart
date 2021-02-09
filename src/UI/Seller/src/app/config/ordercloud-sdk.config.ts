import { Configuration } from '@ordercloud/angular-sdk'
import { environment } from '../../environments/environment.local'

export const OcSDKConfig = (): Configuration => {
  return new Configuration({
    baseApiUrl: `${environment.orderCloudApiUrl}`,
    cookiePrefix: environment.appname.replace(/ /g, '_').toLowerCase(),
  })
}
