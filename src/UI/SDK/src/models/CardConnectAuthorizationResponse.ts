import { BinInfo } from './BinInfo';

export interface CardConnectAuthorizationResponse {
    token?: string
    account?: string
    retref?: string
    amount?: number
    expiry?: string
    merchid?: string
    avsresp?: string
    cvvresp?: string
    signature?: string
    bintype?: string
    commcard?: string
    emv?: string
    binInfo?: BinInfo
    authcode?: string
    respcode?: string
    respproc?: string
    respstat?: string
    resptext?: string
}