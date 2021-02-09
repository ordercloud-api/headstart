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