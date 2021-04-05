import { TransactionXP } from './TransactionXP';

export interface HSPaymentTransaction {
    xp?: TransactionXP
    ID?: string
    Type?: string
    DateExecuted?: string
    Amount?: number
    Succeeded?: boolean
    ResultCode?: string
    ResultMessage?: string
}