
export enum ReportType {
  BuyerLocation,
  SalesOrderDetail,
}

export interface ReportFilters {
  BuyerID: string[]
  Country: string[]
  State: string[]
  Status: string[]
  Type: string[]
}

export interface ReportTypeResource {
  ID: ReportType
  Name: string
  ReportCategory: string
}
