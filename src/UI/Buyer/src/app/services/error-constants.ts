import { AxiosError } from 'axios'
import { Promotion } from 'ordercloud-javascript-sdk'
import {
  MiddlewareError,
  InventoryErrorData,
  ErrorTypes,
  ErrorDisplayData,
} from '../models/error.types'

export class ErrorMessages {
  public static get orderNotAccessibleError(): string {
    return 'You no longer have access to this order'
  }
}

export const ErrorCodes: ErrorTypes = {
  FailedToVoidAuthorization: {
    code: 'Payment.FailedToVoidAuthorization',
    title: 'ERRORS.FAILED_TO_VOID_AUTHORIZATION.TITLE',
    message: 'ERRORS.FAILED_TO_VOID_AUTHORIZATION.MESSAGE',
    buttonText: 'ERRORS.FAILED_TO_VOID_AUTHORIZATION.BUTTONTEXT',
  },
  Insufficient: {
    code: 'Inventory.Insufficient',
    title: 'ERRORS.INSUFFICIENT.TITLE',
    buttonText: 'ERRORS.INSUFFICIENT.BUTTONTEXT',
  },
  AlreadySubmitted: {
    code: 'OrderSubmit.AlreadySubmitted',
    title: 'ERRORS.ALREADY_SUBMITTED.TITLE',
    buttonText: 'ERRORS.ALREADY_SUBMITTED.BUTTONTEXT',
  },
  MissingShippingSelections: {
    code: 'OrderSubmit.MissingShippingSelections',
    title: 'ERRORS.MISSING_SHIPPING_SELECTIONS.TITLE',
    message: 'ERRORS.MISSING_SHIPPING_SELECTIONS.MESSAGE',
    buttonText: 'ERRORS.MISSING_SHIPPING_SELECTIONS.BUTTONTEXT',
  },
  OrderCloudValidationError: {
    code: 'OrderSubmit.OrderCloudValidationError',
  },
  CannotSubmitBadStatus: {
    code: 'OrderSubmit.CannotSubmitBadStatus',
  },
  CannotSubmitUncalculatedOrder: {
    code: 'OrderSubmit.CannotSubmitUncalculatedOrder',
  },
  CannotSubmitWithUnaccceptedPayments: {
    code: 'Order.CannotSubmitWithUnaccceptedPayments',
  },
  InternalServerError: {
    code: 'InternalServerError',
  },
  CreditCardAuth: {
    title: 'ERRORS.CREDIT_CARD_AUTH.TITLE',
    buttonText: 'ERRORS.CREDIT_CARD_AUTH.BUTTONTEXT',
  },
}

export function extractMiddlewareError(
  exception: AxiosError
): MiddlewareError | null {
  // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
  if (exception?.response?.data?.Errors?.length) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    return exception.response.data.Errors[0] as MiddlewareError
  }
  return null
}

export function isInventoryError(
  error: MiddlewareError
): error is MiddlewareError<InventoryErrorData> {
  return error.ErrorCode === ErrorCodes.Inventory.code
}

// order hasn't hit order calc endpoint
export function isUncalculatedOrderError(
  error: MiddlewareError
): error is MiddlewareError<null> {
  return error.ErrorCode === ErrorCodes.CannotSubmitUncalculatedOrder.code
}

export function isPromotionError(
  error: MiddlewareError
): error is MiddlewareError<Promotion> {
  return error.ErrorCode.startsWith('Promotion')
}

export function isBadStatusError(
  error: MiddlewareError
): error is MiddlewareError<null> {
  return error.ErrorCode === ErrorCodes.CannotSubmitBadStatus.code
}
