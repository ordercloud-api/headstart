
export interface BuyerAddressXP {
    Accessorials?: 'DestinationInsideDelivery' | 'DestinationLiftGate' | 'LimitedAccessDelivery' | 'ResidentialDelivery'
    Email?: string
    LocationID?: string
    AvalaraCertificateID?: number
    AvalaraCertificateExpiration?: string
    OpeningDate?: string
    BillingNumber?: string
    Status?: string
    LegalEntity?: string
    PrimaryContactName?: string
}