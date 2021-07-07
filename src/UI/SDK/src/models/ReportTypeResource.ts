
export interface ReportTypeResource {
    ID?: 'BuyerLocation' | 'SalesOrderDetail' | 'PurchaseOrderDetail' | 'LineItemDetail'
    Name?: string
    ReportCategory?: string
    AvailableToSuppliers?: boolean
    Value?: string
    AdHocFilters?: string[]
}