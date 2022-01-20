import { QuoteOrderInfo } from './QuoteOrderInfo';
import { ClaimsSummary } from './ClaimsSummary';
import { HSAddressBuyer } from './HSAddressBuyer';
import { ShipMethodSupplierView } from './ShipMethodSupplierView';

export interface OrderXp {
    ExternalTaxTransactionID?: string
    ShipFromAddressIDs?: string[]
    SupplierIDs?: string[]
    NeedsAttention?: boolean
    StopShipSync?: boolean
    OrderType?: 'Standard' | 'Quote'
    QuoteOrderInfo?: QuoteOrderInfo
    Returns?: ClaimsSummary
    Cancelations?: ClaimsSummary
    Currency?: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN'
    SubmittedOrderStatus?: 'Open' | 'Completed' | 'Canceled'
    ApprovalNeeded?: string
    ShippingStatus?: 'Shipped' | 'PartiallyShipped' | 'Canceled' | 'Processing' | 'Backordered'
    ClaimStatus?: 'NoClaim' | 'Pending' | 'Complete'
    PaymentMethod?: string
    ShippingAddress?: HSAddressBuyer
    SelectedShipMethodsSupplierView?: ShipMethodSupplierView[]
    IsResubmitting?: boolean
	HasSellerProducts?: boolean
    QuoteStatus?: 'NeedsBuyerReview' | 'NeedsSellerReview'
    QuoteSellerContactEmail?: string
    QuoteBuyerContactEmail?: string
    QuoteSubmittedDate?: string
    QuoteSupplierID?: string
}