
export interface LineItemXp {
    StatusByQuantity?: Record<'Complete' | 'Submitted' | 'Open' | 'Backordered', number>
    ImageUrl?: string
    ShipMethod?: string
    SupplierComments?: string
}