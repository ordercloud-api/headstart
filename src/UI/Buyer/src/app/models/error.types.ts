export interface MiddlewareError<TData = any> {
  ErrorCode: string
  Message: string
  Data: TData
}

export interface InventoryErrorData {
  LineItemID: string
  ProductID: string
  QuantityAvailable: number
  QuantityRequested: number
  VariantID: string
}

export interface ErrorDisplayData {
  code?: string
  title?: string
  message?: string
  buttonText?: string
}

export interface ErrorTypes {
  [key: string]: ErrorDisplayData
}
