import { User } from './User';

export interface OrderApproval {
    readonly ApprovalRuleID?: string
    readonly ApprovingGroupID?: string
    readonly Status?: 'Pending' | 'Approved' | 'Declined'
    readonly AllowResubmit?: boolean
    readonly DateCreated?: string
    readonly DateCompleted?: string
    readonly Approver?: User
    readonly Comments?: string
}