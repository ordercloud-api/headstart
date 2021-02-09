import { environment } from 'src/environments/environment.local'
import { HSSupplier } from '@ordercloud/headstart-sdk'

export const SUPPLIER_LOGO_PATH_STRATEGY = 'SUPPLIER_LOGO_PATH_STRATEGY'

export function getSupplierLogoSmallUrl(
  supplier: HSSupplier,
  sellerID: string
): string {
  return `${environment.cmsUrl}/assets/${sellerID}/suppliers/${supplier.ID}/thumbnail?size=s`
}

export function getSupplierLogoMediumUrl(
  supplier: HSSupplier,
  sellerID: string
): string {
  return `${environment.cmsUrl}/assets/${sellerID}/suppliers/${supplier.ID}/thumbnail?size=m`
}
