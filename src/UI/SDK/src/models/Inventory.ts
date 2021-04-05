
export interface Inventory {
    Enabled?: boolean
    NotificationPoint?: number
    VariantLevelTracking?: boolean
    OrderCanExceed?: boolean
    QuantityAvailable?: number
    readonly LastUpdated?: string
}