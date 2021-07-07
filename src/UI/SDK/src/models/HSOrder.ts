import { OrderXp } from './OrderXp';
import { Order } from 'ordercloud-javascript-sdk';
import { UserXp } from './UserXp';
import { BuyerAddressXP } from './BuyerAddressXP';

export type HSOrder = Order<OrderXp, UserXp, BuyerAddressXP>