import { HSLineItem, HSSupplier, ProductXp } from '@ordercloud/headstart-sdk'
import { LineItemProduct } from 'ordercloud-javascript-sdk'

export const IMAGE_HOST_URL =
  'https://s3.dualstack.us-east-1.amazonaws.com/staticcintas.eretailing.com/images/product'
export const PLACEHOLDER_URL = 'https://via.placeholder.com/300x300'
export const PRODUCT_IMAGE_PATH_STRATEGY = 'PRODUCT_IMAGE_PATH_STRATEGY'
export const SUPPLIER_LOGO_PATH_STRATEGY = 'SUPPLIER_LOGO_PATH_STRATEGY'
export const PLACEHOLDER_PRODUCT_IMAGE = '../../../../assets/product.jpg'

export function getProductSmallImageUrl(
  product: LineItemProduct<ProductXp>
): string {
  const images = product?.xp?.Images
  return images && images.length
    ? images[0]?.ThumbnailUrl
      ? images[0]?.ThumbnailUrl
      : images[0].Url
    : PLACEHOLDER_URL
}

export function getProductMediumImageUrl(
  product: LineItemProduct<ProductXp>
): string {
  const images = product?.xp?.Images
  return images && images.length ? images[0].Url : PLACEHOLDER_PRODUCT_IMAGE
}

export function getSupplierLogoSmallUrl(supplier: HSSupplier): string {
  return supplier.xp?.Image?.ThumbnailUrl || PLACEHOLDER_URL
}

export function getSupplierLogoMediumUrl(supplier: HSSupplier): string {
  return supplier?.xp?.Image?.Url || ''
}

export function getPrimaryLineItemImage(
  lineItemID: string,
  lineItems: HSLineItem[]
): string {
  const li = lineItems.find((item) => item.ID === lineItemID)
  return getProductMediumImageUrl(li.Product)
}

export function getAssetIDFromUrl(url: string): string {
  const split = url.split('/')
  return split[split.length - 1]
}
