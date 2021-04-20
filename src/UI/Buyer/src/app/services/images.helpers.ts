import { HSLineItem, HSMeProduct } from '@ordercloud/headstart-sdk'
import { CurrentUser } from '../models/profile.types'

export const getPrimaryImageUrl = (
  product: HSMeProduct,
  user: CurrentUser
): string => {
  const images = product?.xp?.Images
  return images && images.length ? images[0].Url : '/assets/product.jpg'
}

export const getPrimaryLineItemImage = (
  lineItemID: string,
  lineItems: HSLineItem[],
  user: CurrentUser
): string => {
  const li = lineItems.find((item) => item.ID === lineItemID)
  return getPrimaryImageUrl(li.Product, user)
}
