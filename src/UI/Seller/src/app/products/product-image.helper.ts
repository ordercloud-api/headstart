import { environment } from 'src/environments/environment.local'
import {
  HSProduct,
  HSLineItem,
} from '@ordercloud/headstart-sdk'

export const IMAGE_HOST_URL =
  'https://s3.dualstack.us-east-1.amazonaws.com/staticcintas.eretailing.com/images/product'
export const PLACEHOLDER_URL = 'http://placehold.it/300x300'
export const PRODUCT_IMAGE_PATH_STRATEGY = 'PRODUCT_IMAGE_PATH_STRATEGY'

export function getProductSmallImageUrl(
  product: HSProduct,
  sellerID: string
): string {
  return `${environment.cmsUrl}/assets/${sellerID}/products/${product.ID}/thumbnail?size=s`
}

export function getProductMediumImageUrl(
  product: HSProduct,
  sellerID: string
): string {
  return `${environment.cmsUrl}/assets/${sellerID}/products/${product.ID}/thumbnail?size=m`
}

export const getPrimaryLineItemImage = (
  lineItemID: string,
  lineItems: HSLineItem[],
  sellerID: string
): string => {
  const li = lineItems.find((item) => item.ID === lineItemID)
  return getProductMediumImageUrl(li.Product, sellerID)
}
