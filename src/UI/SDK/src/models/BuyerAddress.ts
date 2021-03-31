
export interface BuyerAddress {
    readonly ID?: string
    Shipping?: boolean
    Billing?: boolean
    readonly Editable?: boolean
    readonly DateCreated?: string
    CompanyName?: string
    FirstName?: string
    LastName?: string
    Street1?: string
    Street2?: string
    City?: string
    State?: string
    Zip?: string
    Country?: string
    Phone?: string
    AddressName?: string
}