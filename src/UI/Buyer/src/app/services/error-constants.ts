import { AxiosError } from 'axios'
import { Promotion } from 'ordercloud-javascript-sdk'
import { MiddlewareError, InventoryErrorData } from '../models/error.types'

export class ErrorMessages {
  public static get orderNotAccessibleError(): string {
    return 'You no longer have access to this order'
  }
}

export const ErrorCodes = {
  Order: {
    CannotSubmitBadStatus: 'Order.CannotSubmitBadStatus',
    CannotSubmitUncalculatedOrder: 'Order.CannotSubmitUncalculatedOrder',
    CannotSubmitWithUnaccceptedPayments:
      'Order.CannotSubmitWithUnaccceptedPayments',
  },
  Payment: {
    FailedToVoidAuthorization: 'Payment.FailedToVoidAuthorization',
  },
  Inventory: {
    Insufficient: 'Inventory.Insufficient',
  },
  OrderSubmit: {
    AlreadySubmitted: 'OrderSubmit.AlreadySubmitted',
    MissingShippingSelections: 'OrderSubmit.MissingShippingSelections',
    OrderCloudValidationError: 'OrderSubmit.OrderCloudValidationError',
  },
  InternalServerError: 'InternalServerError',
}

export function extractMiddlewareError(
  exception: AxiosError
): MiddlewareError | null {
  if (exception?.response?.data) {
    return exception.response.data as MiddlewareError
  }
  return null
}

export function isInventoryError(
  error: MiddlewareError
): error is MiddlewareError<InventoryErrorData> {
  return error.ErrorCode === ErrorCodes.Inventory.Insufficient
}

// order hasn't hit order calc endpoint
export function isUncalculatedOrderError(
  error: MiddlewareError
): error is MiddlewareError<null> {
  return error.ErrorCode === ErrorCodes.Order.CannotSubmitUncalculatedOrder
}

export function isPromotionError(
  error: MiddlewareError
): error is MiddlewareError<Promotion> {
  return error.ErrorCode.startsWith('Promotion')
}

export function isBadStatusError(
  error: MiddlewareError
): error is MiddlewareError<null> {
  return error.ErrorCode === ErrorCodes.Order.CannotSubmitBadStatus
}

export function getPaymentError(errorReason: string): string {
  const reason = errorReason.replace('AVS', 'Address Verification') // AVS isn't likely something to be understood by a layperson
  return `The authorization for your payment was declined by the processor due to: "${reason}". Please reenter your information or use a different card`
}
