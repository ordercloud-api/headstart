import { HSOrder } from '@ordercloud/headstart-sdk'
import { OrderType } from '../models/order.types'

export const isQuoteOrder = (order?: HSOrder): boolean => {
  return order?.xp?.OrderType === OrderType.Quote
}
