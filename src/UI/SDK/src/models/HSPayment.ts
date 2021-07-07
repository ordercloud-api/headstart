import { PaymentXP } from './PaymentXP';
import { Payment } from 'ordercloud-javascript-sdk';
import { TransactionXP } from '.';

export type HSPayment = Payment<PaymentXP, TransactionXP>