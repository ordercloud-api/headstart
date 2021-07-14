export interface RMALog {
    Status?: 'Requested' | 'Denied' | 'Processing' | 'Approved' | 'Complete'
    Date?: string
    AmountRefunded?: number
    FromUserID?: string
}