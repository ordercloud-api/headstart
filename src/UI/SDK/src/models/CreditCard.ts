
export interface CreditCard {
    ID?: string
    Token?: string
    readonly DateCreated?: string
    CardType?: string
    PartialAccountNumber?: string
    CardholderName?: string
    ExpirationDate: string
}