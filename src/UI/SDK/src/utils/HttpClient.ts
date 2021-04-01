import axios, { AxiosRequestConfig } from 'axios'
import tokenService from '../api/Tokens'
import Configuration from '../configuration'
import Auth from '../api/Auth'
import paramsSerializer from './paramsSerializer'
import parseJwt from './ParseJwt'

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
class HttpClient {
  private _auth = new Auth()
  private _token = new tokenService()

  constructor() {
    // create a new instance so we avoid clashes with any
    // configurations done on default axios instance that
    // a consumer of this SDK might use
    if (typeof axios === 'undefined') {
      throw new Error(
        'Ordercloud is missing required peer dependency axios. This must be installed and loaded before the OrderCloud SDK'
      )
    }

    this.get = this.get.bind(this)
    this.put = this.put.bind(this)
    this.post = this.post.bind(this)
    this.patch = this.patch.bind(this)
    this.delete = this.delete.bind(this)
    this._buildRequestConfig = this._buildRequestConfig.bind(this)
    this._getToken = this._getToken.bind(this)
    this._isTokenExpired = this._isTokenExpired.bind(this)
    this._tokenInterceptor = this._tokenInterceptor.bind(this)
    this._tryRefreshToken = this._tryRefreshToken.bind(this)
  }

  public get = async (
    path: string,
    config?: AxiosRequestConfig
  ): Promise<any> => {
    const requestConfig = await this._buildRequestConfig(config)
    const response = await axios.get(
      `${Configuration.Get().baseApiUrl}${path}`,
      requestConfig
    )
    return response.data
  }

  public post = async (
    path: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<any> => {
    const requestConfig = await this._buildRequestConfig(config)
    const response = await axios.post(
      `${Configuration.Get().baseApiUrl}${path}`,
      data,
      requestConfig
    )
    return response.data
  }

  public put = async (
    path: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<any> => {
    const requestConfig = await this._buildRequestConfig(config)
    const response = await axios.put(
      `${Configuration.Get().baseApiUrl}${path}`,
      data,
      requestConfig
    )
    return response.data
  }

  public patch = async (
    path: string,
    data?: any,
    config?: AxiosRequestConfig
  ): Promise<any> => {
    const requestConfig = await this._buildRequestConfig(config)
    const response = await axios.patch(
      `${Configuration.Get().baseApiUrl}${path}`,
      data,
      requestConfig
    )
    return response.data
  }

  public delete = async (path: string, config: AxiosRequestConfig) => {
    const requestConfig = await this._buildRequestConfig(config)
    const response = await axios.delete(
      `${Configuration.Get().baseApiUrl}${path}`,
      requestConfig
    )
    return response.data
  }

  // sets the token on every outgoing request, will attempt to
  // refresh the token if the token is expired and there is a refresh token set
  private async _tokenInterceptor(
    config: AxiosRequestConfig
  ): Promise<AxiosRequestConfig> {
    let token = this._getToken(config)
    if (this._isTokenExpired(token)) {
      token = await this._tryRefreshToken(token)
    }
    config.headers.Authorization = `Bearer ${token}`
    return config
  }

  private _getToken(config: AxiosRequestConfig): string {
    let token
    if (config.params.accessToken) {
      token = config.params.accessToken
    } else if (config.params.impersonating) {
      token = this._token.GetImpersonationToken()
    } else {
      token = this._token.GetAccessToken()
    }

    // strip out axios params that we'vee hijacked for our own nefarious purposes
    delete config.params.accessToken
    delete config.params.impersonating
    return token
  }

  private _isTokenExpired(token: string): boolean {
    if (!token) {
      return true
    }
    const decodedToken = parseJwt(token)
    const currentSeconds = Date.now() / 1000
    const currentSecondsWithBuffer = currentSeconds - 10
    return decodedToken.exp < currentSecondsWithBuffer
  }

  private async _tryRefreshToken(accessToken: string): Promise<string> {
    const refreshToken = this._token.GetRefreshToken()
    if (!refreshToken) {
      return accessToken || ''
    }
    const sdkConfig = Configuration.Get()
    if (!accessToken && !sdkConfig.clientID) {
      return accessToken || ''
    }
    let clientID
    if (accessToken) {
      const decodedToken = parseJwt(accessToken)
      clientID = decodedToken.cid
    }
    if (sdkConfig.clientID) {
      clientID = sdkConfig.clientID
    }
    const refreshRequest = await this._auth.RefreshToken(refreshToken, clientID)
    return refreshRequest.access_token
  }

  private _buildRequestConfig(
    config?: AxiosRequestConfig
  ): Promise<AxiosRequestConfig> {
    const sdkConfig = Configuration.Get()
    const requestConfig = {
      ...config,
      paramsSerializer,
      timeout: sdkConfig.timeoutInMilliseconds,
      headers: {
        'Content-Type': 'application/json',
      },
    }
    return this._tokenInterceptor(requestConfig)
  }
}

export default new HttpClient()
