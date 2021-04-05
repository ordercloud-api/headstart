
export interface TaxCertificate {
    readonly ID?: number
    SignedDate?: string
    ExpirationDate?: string
    ExposureZoneName?: string
    Base64UrlEncodedPDF?: string
    readonly FileName?: string
    ExemptionNumber?: string
    readonly Expired?: boolean
}