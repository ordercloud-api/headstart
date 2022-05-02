import axios, { CancelToken } from 'axios'
import { AccessToken } from '../models/AccessToken'
import Configuration from '../configuration'
import { ApiRole } from 'ordercloud-javascript-sdk'
import serialize from '../utils/paramsSerializer'
import { RequiredDeep } from '../models/RequiredDeep'
import OrderCloudError from '../utils/OrderCloudError'

class Auth {
  constructor() {
    if (typeof axios === 'undefined') {
      throw new Error(
        'HeadstartSDK is missing required peer dependency axios. This must be installed and loaded before the Headstart SDK'
      )
    }

    /**
     * @ignore
     * not part of public api, don't include in generated docs
     */
    this.Anonymous = this.Anonymous.bind(this)
    this.ClientCredentials = this.ClientCredentials.bind(this)
    this.ElevatedLogin = this.ElevatedLogin.bind(this)
    this.Login = this.Login.bind(this)
    this.RefreshToken = this.RefreshToken.bind(this)
  }

  /**
   * @description this workflow is most appropriate for client apps where user is a human, ie a registered user
   *
   * @param username of the user logging in
   * @param password of the user logging in
   * @param client_id of the application the user is logging into
   * @param scope roles being requested - space delimited string or array
   * @param requestOptions.cancelToken Provide an [axios cancelToken](https://github.com/axios/axios#cancellation) that can be used to cancel the request.
   * @param requestOptions.requestType Provide a value that can be used to identify the type of request. Useful for error logs.
   */
  public async Login(
    username: string,
    password: string,
    clientID: string,
    scope: ApiRole[],
    requestOptions: {
      cancelToken?: CancelToken
      requestType?: string
    } = {}
  ): Promise<RequiredDeep<AccessToken>> {
    if (!Array.isArray(scope)) {
      throw new Error('scope must be a string array')
    }
    const body = {
      grant_type: 'password',
      username,
      password,
      client_id: clientID,
      scope: scope.join(' '),
    }
    const configuration = Configuration.Get()
    const response = await axios
      .post(`${configuration.orderCloudApiUrl}/oauth/token`, serialize(body), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: 'application/json',
        },
        ...requestOptions,
      })
      .catch(e => {
        if (e.response) {
          throw new OrderCloudError(e)
        }
        throw e
      })
    return response.data
  }

  /**
   * @description similar to login except client secret is also required, adding another level of security
   *
   * @param clientSecret of the application
   * @param username of the user logging in
   * @param password of the user logging in
   * @param clientID of the application the user is logging into
   * @param scope roles being requested - space delimited string or array
   * @param observe set whether or not to return the data Observable as the body, response or events. defaults to returning the body.
   * @param reportProgress flag to report request and response progress.
   * @param requestOptions.cancelToken Provide an [axios cancelToken](https://github.com/axios/axios#cancellation) that can be used to cancel the request.
   * @param requestOptions.requestType Provide a value that can be used to identify the type of request. Useful for error logs.
   */
  public async ElevatedLogin(
    clientSecret: string,
    username: string,
    password: string,
    clientID: string,
    scope: ApiRole[],
    requestOptions: {
      cancelToken?: CancelToken
      requestType?: string
    } = {}
  ): Promise<RequiredDeep<AccessToken>> {
    if (!Array.isArray(scope)) {
      throw new Error('scope must be a string array')
    }
    const body = {
      grant_type: 'password',
      scope: scope.join(' '),
      client_id: clientID,
      username,
      password,
      client_secret: clientSecret,
    }
    const configuration = Configuration.Get()
    const response = await axios
      .post(`${configuration.orderCloudApiUrl}/oauth/token`, serialize(body), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: 'application/json',
        },
        ...requestOptions,
      })
      .catch(e => {
        if (e.response) {
          throw new OrderCloudError(e)
        }
        throw e
      })
    return response.data
  }

  /**
   * @description this workflow is best suited for a backend system
   *
   * @param clientSecret of the application
   * @param clientID of the application the user is logging into
   * @param scope roles being requested - space delimited string or array
   * @param observe set whether or not to return the data Observable as the body, response or events. defaults to returning the body.
   * @param reportProgress flag to report request and response progress.
   * @param requestOptions.cancelToken Provide an [axios cancelToken](https://github.com/axios/axios#cancellation) that can be used to cancel the request.
   * @param requestOptions.requestType Provide a value that can be used to identify the type of request. Useful for error logs.
   */
  public async ClientCredentials(
    clientSecret: string,
    clientID: string,
    scope: ApiRole[],
    requestOptions: {
      cancelToken?: CancelToken
      requestType?: string
    } = {}
  ): Promise<RequiredDeep<AccessToken>> {
    if (!Array.isArray(scope)) {
      throw new Error('scope must be a string array')
    }
    const body = {
      grant_type: 'client_credentials',
      scope: scope.join(' '),
      client_id: clientID,
      client_secret: clientSecret,
    }
    const configuration = Configuration.Get()
    const response = await axios
      .post(`${configuration.orderCloudApiUrl}/oauth/token`, serialize(body), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: 'application/json',
        },
        ...requestOptions,
      })
      .catch(e => {
        if (e.response) {
          throw new OrderCloudError(e)
        }
        throw e
      })
    return response.data
  }

  /**
   * @description extend your users' session by getting a new access token with a refresh token. refresh tokens must be enabled in the dashboard
   *
   * @param refreshToken of the application
   * @param clientID of the application the user is logging into
   * @param requestOptions.cancelToken Provide an [axios cancelToken](https://github.com/axios/axios#cancellation) that can be used to cancel the request.
   * @param requestOptions.requestType Provide a value that can be used to identify the type of request. Useful for error logs.
   */
  public async RefreshToken(
    refreshToken: string,
    clientID: string,
    requestOptions: {
      cancelToken?: CancelToken
      requestType?: string
    } = {}
  ): Promise<RequiredDeep<AccessToken>> {
    const body = {
      grant_type: 'refresh_token',
      client_id: clientID,
      refresh_token: refreshToken,
    }
    const configuration = Configuration.Get()
    const response = await axios
      .post(`${configuration.orderCloudApiUrl}/oauth/token`, serialize(body), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: 'application/json',
        },
        ...requestOptions,
      })
      .catch(e => {
        if (e.response) {
          throw new OrderCloudError(e)
        }
        throw e
      })
    return response.data
  }

  /**
   * @description allow users to browse your catalog without signing in - must have anonymous template user set in dashboard
   *
   * @param clientID of the application the user is logging into
   * @param scope roles being requested - space delimited string or array
   * @param requestOptions.cancelToken Provide an [axios cancelToken](https://github.com/axios/axios#cancellation) that can be used to cancel the request.
   * @param requestOptions.requestType Provide a value that can be used to identify the type of request. Useful for error logs.
   */
  public async Anonymous(
    clientID: string,
    scope: ApiRole[],
    requestOptions: {
      cancelToken?: CancelToken
      requestType?: string
    } = {}
  ): Promise<RequiredDeep<AccessToken>> {
    if (!Array.isArray(scope)) {
      throw new Error('scope must be a string array')
    }
    const body = {
      grant_type: 'client_credentials',
      client_id: clientID,
      scope: scope.join(' '),
    }
    const configuration = Configuration.Get()
    const response = await axios
      .post(`${configuration.orderCloudApiUrl}/oauth/token`, serialize(body), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
          Accept: 'application/json',
        },
        ...requestOptions,
      })
      .catch(e => {
        if (e.response) {
          throw new OrderCloudError(e)
        }
        throw e
      })
    return response.data
  }
}

export default new Auth()
