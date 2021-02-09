import { SafeUrl } from '@angular/platform-browser'
import { Shipment } from '@ordercloud/angular-sdk'

export interface BatchProcessResult {
  ProcessFailureList: BatchProcessFailure[]
  InvalidRowFailureList: DocumentRowError[]
  SuccessfulList: Shipment[]
  Meta: BatchProcessSummary
}

export interface BatchProcessFailure {
  Shipment: Shipment
  Error: string
}

export interface DocumentRowError {
  Row: number
  ErrorMessage: string
  Column: number
}

export interface BatchProcessSummary {
  TotalCount: number
  SuccessfulCount: number
  ProcessFailureListCount: number
  DocumentFailureListCount: number
}


export interface FileHandle {
  File: File
  URL: SafeUrl
  Filename?: string
}