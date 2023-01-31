import { ReportType } from "./ReportType"

export interface ReportTypeResource {
    ID?: ReportType
    Name?: string
    ReportCategory?: string
    AvailableToSuppliers?: boolean
    Value?: string
    AdHocFilters?: string[]
}