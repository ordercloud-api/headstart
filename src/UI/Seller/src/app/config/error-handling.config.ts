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
    if (ex.rejection) {
      ex = ex.rejection
    }
    let message = ''
    if (ex?.errors?.Errors?.length) {
      const e = ex.errors.Errors[0]
      if (e.Data && e.Data.WebhookName) {
        // webhook error
        message = e.Data.body
      } else if (e.ErrorCode === 'NotFound') {
        message = `${e.Data.ObjectType} ${e.Data.ObjectID} not found.`
      } else {
        message = e.Message
      }
    } else if (ex?.errors?.['error_description']) {
      message = ex.errors['error_description']
    } else if (ex?.errors?.[0]) {
      message = ex?.errors?.[0]?.Message
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
