import { ClaimResolutionStatuses } from './ClaimResolutionStatuses';

export interface ClaimsSummary {
    HasClaims?: boolean
    HasUnresolvedClaims?: boolean
    Resolutions?: ClaimResolutionStatuses[]
}