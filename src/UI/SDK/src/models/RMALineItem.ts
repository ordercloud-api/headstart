export interface RMALineItem {
    ID?: string
    QuantityRequested?: number
    QuantityProcessed?: number
    Status?: 'Requested' | 'Processing' | 'Approved' | 'Complete' | 'Denied' | 'PartialQtyApproved' | 'PartialQtyComplete'
    Reason?: string
    Comment?: string
    PercentToRefund?: number
    RefundableViaCreditCard?: boolean
    IsResolved?: boolean
    IsRefunded?: boolean
    LineTotalRefund?: number
}