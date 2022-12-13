import { OrderReturn } from 'ordercloud-javascript-sdk'

export interface HSOrderReturn extends OrderReturn<OrderReturnXp> {}

export interface OrderReturnXp {
    SellerComments?: string
    SubmittedStatusDetails?: ReturnEventDetails
    ApprovedStatusDetails?: ReturnEventDetails
    DeclinedStatusDetails?: ReturnEventDetails
    CompletedStatusDetails?: ReturnEventDetails
    CanceledStatusDetails?: ReturnEventDetails
}

export interface ReturnEventDetails {
    ProcessedByName?: string
    ProcessedByUserId?: string
    ProcessedByCompanyId?: string // empty if Admin, otherwise SupplierID if supplier and BuyerID if buyer
    RefundAmount?: number
}