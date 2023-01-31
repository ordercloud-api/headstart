import { ReportFilters } from './ReportFilters';
import { ReportType } from './ReportType';

export interface ReportTemplate {
    TemplateID?: string
    SellerID?: string
    ReportType?: ReportType
    Name?: string
    Description?: string
    Headers?: string[]
    Filters?: ReportFilters
    AvailableToSuppliers?: boolean
}