import { ReportFilters } from './ReportFilters';

export interface ReportTemplate {
    TemplateID?: string
    SellerID?: string
    ReportType?: 'BuyerLocation' | 'SalesOrderDetail' | 'PurchaseOrderDetail' | 'LineItemDetail'
    Name?: string
    Description?: string
    Headers?: string[]
    Filters?: ReportFilters
    AvailableToSuppliers?: boolean
}