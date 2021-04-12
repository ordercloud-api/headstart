import {
  HSProduct,
  HSLineItem,
} from '@ordercloud/headstart-sdk'

export const IMAGE_HOST_URL =
  'https://s3.dualstack.us-east-1.amazonaws.com/staticcintas.eretailing.com/images/product'
export const PLACEHOLDER_URL = 'http://placehold.it/300x300'
export const PRODUCT_IMAGE_PATH_STRATEGY = 'PRODUCT_IMAGE_PATH_STRATEGY'

export function getProductSmallImageUrl(
  product: HSProduct
): string {
  const images = (product?.xp as any)?.Images
  return images && images.length ? images[0].ThumbnailUrl : ''
}

export function getProductMediumImageUrl(
  product: HSProduct,
): string {
  const images = (product?.xp as any)?.Images
  return images && images.length ? images[0].Url : ''
}

export const getPrimaryLineItemImage = (
  lineItemID: string,
  lineItems: HSLineItem[],
): string => {
  const li = lineItems.find((item) => item.ID === lineItemID)
  return getProductMediumImageUrl(li.Product)
}

export const getImageIDFromUrl = (url: string): string => {
    const split = url.split("/")
    return split[split.length-1]
  }