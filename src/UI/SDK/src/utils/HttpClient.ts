import axios, { AxiosRequestConfig } from 'axios'
import tokenService from '../api/Tokens'
import Configuration from '../configuration'
import paramsSerializer from './paramsSerializer'
import OrderCloudError from './OrderCloudError'

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */

interface OcRequestConfig extends AxiosRequestConfig {
  impersonating?: boolean
  accessToken?: string
}
class HttpClient {
  constructor() {
    if (typeof axios === 'undefined') {
      throw new Error(
        'Missing required peer dependency axios. This must be installed and loaded before the Headstart SDK'
      )
    }

    this.get = this.get.bind(this)
    this.put = this.put.bind(this)
    this.post = this.post.bind(this)
    this.patch = this.patch.bind(this)
    this.delete = this.delete.bind(this)
    this._resolveToken = this._resolveToken.bind(this)
    this._buildRequestConfig = this._buildRequestConfig.bind(this)
    this._addTokenToConfig = this._addTokenToConfig.bind(this)
  }

  public get = async (
    path: string,
    config?: OcRequestConfig
  ): Promise<any> => {
    return await this.makeApiCall('get', path, config).catch(ex => {
      if (ex.response) {
        throw new OrderCloudError(ex)
      }
      throw ex
    })
  }

  public post = async (
    path: string,
    requestBody?: any,
    config?: OcRequestConfig
  ): Promise<any> => {
    config.data = requestBody
    return await this.makeApiCall('post', path, config).catch(ex => {
      if (ex.response) {
        throw new OrderCloudError(ex)
      }
      throw ex
    })
  }

  public put = async (
    path: string,
    requestBody?: any,
    config?: OcRequestConfig
  ): Promise<any> => {
    config.data = requestBody
    return await this.makeApiCall('put', path, config).catch(ex => {
      if (ex.response) {
        throw new OrderCloudError(ex)
      }
      throw ex
    })
  }

  public patch = async (
    path: string,
    requestBody?: any,
    config?: OcRequestConfig
  ): Promise<any> => {
    config.data = requestBody
    return await this.makeApiCall('patch', path, config).catch(ex => {
      if (ex.response) {
        throw new OrderCloudError(ex)
      }
      throw ex
    })
  }

  public delete = async (path: string, config: OcRequestConfig) => {
    return await this.makeApiCall('delete', path, config).catch(ex => {
      if (ex.response) {
        throw new OrderCloudError(ex)
      }
      throw ex
    })
  }

  private async makeApiCall(
    verb: 'get' | 'put' | 'post' | 'patch' | 'delete',
    path,
    config
  ) {
    const requestConfig = await this._buildRequestConfig(config)
    if (verb === 'put' || verb === 'post' || verb === 'patch') {
      const requestBody = requestConfig.data
      delete requestConfig.data
      const response = await axios[verb as string](
        `${Configuration.Get().baseApiUrl}${path}`,
        requestBody,
        requestConfig
      )
      return response.data
    } else {
      const response = await axios[verb as string](
        `${Configuration.Get().baseApiUrl}${path}`,
        requestConfig
      )
      return response.data
    }
  }

  // sets the token on every outgoing request, will attempt to
  // refresh the token if the token is expired and there is a refresh token set
  private async _addTokenToConfig(
    config: OcRequestConfig
  ): Promise<OcRequestConfig> {
    const token = this._resolveToken(config)
    const validToken = await tokenService.GetValidToken(token)
    config.headers.Authorization = `Bearer ${validToken}`
    return config
  }

  private _resolveToken(config: OcRequestConfig): string {
    let token
    if (config['accessToken']) {
      token = config['accessToken']
    } else if (config['impersonating']) {
      token = tokenService.GetImpersonationToken()
    } else {
      token = tokenService.GetAccessToken()
    }

    // remove these custom parameters that axios doesn't understand
    // we were storing on the axios config for simplicity
    delete config['accessToken']
    delete config['impersonating']
    return token
  }

  private _buildRequestConfig(
    config?: OcRequestConfig
  ): Promise<OcRequestConfig> {
    const defaultHeaders = {
      'Content-Type': 'application/json',
    }
    const configuredHeaders = config?.headers ? config.headers : defaultHeaders
    const sdkConfig = Configuration.Get()
    const requestConfig = {
      ...config,
      paramsSerializer,
      timeout: sdkConfig.timeoutInMilliseconds,
      headers: configuredHeaders
    }
    return this._addTokenToConfig(requestConfig)
  }
}

export default new HttpClient()
