interface ApiError {
    ErrorCode: string
    Message: string
    Data: any
  }
  
  interface AuthError {
    error: string
    error_description: string
  }
  
  export default class OrderCloudError extends Error {
    isOrderCloudError: true
    request?: any
    response: any
    errors?: ApiError[] | AuthError
    status: number
    errorCode: string
    statusText: string
  
    constructor(ex) {
      const errors = safeParseErrors(ex) // extract ordercloud errors from response
      const error = errors?.[0] ?? errors // most of the time there is just one error
  
      super(getMessage(ex, error))
      this.isOrderCloudError = true
      this.errors = errors
      this.name = 'OrderCloudError'
      this.errorCode = getErrorCode(error)
      this.status = ex.response.status
      this.statusText = ex.response.statusText
      this.response = ex.response
      this.request = ex.request
    }
  }
  
  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  function safeParseErrors(ex): ApiError[] | AuthError | null {
    try {
      let str = ex?.response?.data
      if (!str) {
        return null
      }
      if (typeof str === 'object') {
        return str.Errors;
      }
      if (str && str.charCodeAt(0) === 65279) {
        // there seems to be a BOM character at the beginning
        // of this string that causes json parsing to fail
        str = str.substr(1)
      }
      const data = JSON.parse(str)
      const jsonData = data.Errors
      return Array.isArray(jsonData) ? jsonData : [jsonData] // make sure is array for consistency
    } catch (e) {
      return null
    }
  }
  
  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  const isApiError = (error: any): error is ApiError =>
    (error as ApiError).Data !== undefined
  
  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  function getMessage(ex, error?: ApiError | AuthError): string {
    if (!error) {
      return ex.response.statusText
    }
    if (isApiError(error)) {
      switch (error.ErrorCode) {
        case 'NotFound':
          return `${error.Data.ObjectType} ${error.Data.ObjectID} not found`
        default:
          return error.Message
      }
    } else {
      return error.error_description
    }
  }
  
  function getErrorCode(error?: ApiError | AuthError): string {
    if (!error) {
      return 'OrderCloudError'
    }
    if (isApiError(error)) {
      return error.ErrorCode
    } else {
      return error.error
    }
  }
