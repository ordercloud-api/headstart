
export interface OrderPromotion {
    readonly Amount?: number
    readonly LineItemID?: string
    ID?: string
    LineItemLevel?: boolean
    Code?: string
    Name?: string
    RedemptionLimit?: number
    RedemptionLimitPerUser?: number
    readonly RedemptionCount?: number
    Description?: string
    FinePrint?: string
    StartDate?: string
    ExpirationDate?: string
    EligibleExpression?: string
    ValueExpression?: string
    CanCombine?: boolean
    AllowAllBuyers?: boolean
}