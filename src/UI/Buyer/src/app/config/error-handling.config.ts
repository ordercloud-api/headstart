/* eslint-disable max-lines-per-function */
import { ErrorHandler, Inject, Injector, Injectable } from '@angular/core'
import { ToastrService } from 'ngx-toastr'

/**
 * this error handler class extends angular's ErrorHandler
 * in order to automatically format ordercloud error messages
 * and display them in toastr
 */
@Injectable({
  providedIn: 'root',
})
export class AppErrorHandler extends ErrorHandler {
  constructor(@Inject(Injector) private readonly injector: Injector) {
    super()
  }

  public handleError(ex: any): void {
    this.displayError(ex)
    super.handleError(ex)
  }

  /**
   * use this to display error message
   * but continue exection of code
   */
  public displayError(ex: any): void {
    let message = ''
    if (ex.promise && ex.rejection) {
      ex = ex.rejection
    }
    if (ex?.error?.Errors?.length) {
      const e = ex.errors[0]
      if (e.Data && e.Data.WebhookName) {
        // webhook error
        message = e.Data.body
      } else if (e.ErrorCode === 'NotFound') {
        message = `${e.Data.ObjectType} ${e.Data.ObjectID} not found.`
      } else {
        message = e.Message
      }
    } else if (ex?.response?.data?.Message) {
      message = ex.response.data.Message
    } else if (ex?.error?.Message) {
      message = ex.error.Message
      // eslint-disable-next-line camelcase
    } else if (ex?.error?.error_description) {
      message = ex.error.error_description
    } else if (ex.error) {
      message = ex.error
    } else if (ex.message) {
      message = ex.message
    } else {
      message = 'An error occurred'
    }
    if (typeof message === 'object') {
      message = JSON.stringify(message)
    }
    if (
      message === 'Token refresh attempt not possible' ||
      message === 'Access token is invalid or expired.'
    ) {
      // display user friendly error
      message = 'Your session has expired. Please log in.'
    }
    this.toastrService.error(message, 'Error', { onActivateTick: true })
  }

  /**
   * Need to get ToastrService from injector rather than constructor injection to avoid cyclic dependency error
   */
  private get toastrService(): ToastrService {
    return this.injector.get(ToastrService)
  }
}
