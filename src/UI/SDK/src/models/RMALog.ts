import { CommerceRole } from "ordercloud-javascript-sdk"

export interface RMALog {
    Status?: 'Requested' | 'Denied' | 'Processing' | 'Approved' | 'Complete'
    Date?: string
    AmountRefunded?: number
    FromUserID?: string
    FromUserType?: CommerceRole
}