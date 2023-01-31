import { BatchProcessSummary } from './BatchProcessSummary'
import { HSShipment } from './HSShipment'

export interface BatchProcessResult {
  Meta?: BatchProcessSummary
  SuccessfulList: HSShipment[]
  ProcessFailureList: BatchProcessFailure[]
  InvalidRowFailureList: DocumentRowError[]
}

export interface BatchProcessFailure {
  Shipment: HSShipment
  Error: string
}

export interface DocumentRowError {
  Row: number
  ErrorMessage: string
  Column: number
}
