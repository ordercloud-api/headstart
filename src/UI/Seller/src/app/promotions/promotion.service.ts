import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { Promotion } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import {
  HSPromoType,
  HSPromoEligibility,
  PromotionXp,
  MinRequirementType,
  HSBogoType,
} from '@app-seller/models/promo-types'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { Promotions } from 'ordercloud-javascript-sdk'
import { HSSupplier } from '@ordercloud/headstart-sdk'

@Injectable({
  providedIn: 'root',
})
export class PromotionService extends ResourceCrudService<Promotion> {
  emptyResource = {
    ID: null,
    Name: '',
    Description: '',
    LineItemLevel: false,
    Code: '',
    RedemptionLimit: null,
    RedemptionLimitPerUser: null,
    FinePrint: '',
    StartDate: '',
    ExpirationDate: '',
    EligibleExpression: 'true',
    ValueExpression: '',
    CanCombine: true,
    AllowAllBuyers: true,
    xp: {
      Type: HSPromoType.Percentage,
      Value: null,
      AppliesTo: HSPromoEligibility.EntireOrder,
      ScopeToSupplier: false,
      Supplier: null,
      SKUs: [],
      Automatic: false,
      MinReq: {
        Type: null,
        Int: null,
      },
      BOGO: {
        Type: HSBogoType.Percentage,
        Value: null,
        BuySKU: {
          SKU: null,
          Qty: null,
        },
        GetSKU: {
          SKU: null,
          Qty: null,
        },
      },
      MaxShipCost: null,
      Buyers: [],
    },
  }

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      Promotions,
      currentUserService,
      '/promotions',
      'promotions'
    )
  }
  buildEligibleExpression(
    safeXp: PromotionXp,
    selectedSupplier?: HSSupplier
  ): string {
    let eligibleExpression = ''
    const skuArr = safeXp?.SKUs?.map((sku) => `item.ProductID = '${sku}'`)
    switch (safeXp?.AppliesTo) {
      case HSPromoEligibility.SpecificSupplier:
        eligibleExpression = `item.SupplierID = '${selectedSupplier?.ID}'`
        break
      case HSPromoEligibility.SpecificSKUs:
        skuArr.forEach(
          (exp, i) =>
            (eligibleExpression =
              i === 0
                ? `${eligibleExpression} ${exp}`
                : `${eligibleExpression} or ${exp}`)
        )
        break
      default:
        // NOTE: The default expression is "true", which will allow the promotion to be applied to any order
        // in the platform.  This is selected if no eligibilty requirements are defined during the create
        // process
        eligibleExpression = 'true'
        break
    }
    if (safeXp?.Type === 'BOGO') {
      eligibleExpression = this.buildBogoEligibleExpression(safeXp)
    }
    switch (safeXp?.MinReq?.Type) {
      case MinRequirementType.MinPurchase:
        if (safeXp?.AppliesTo === HSPromoEligibility.SpecificSupplier) {
          eligibleExpression = `${eligibleExpression} and items.total(SupplierID = '${selectedSupplier?.ID}') >= ${safeXp?.MinReq?.Int}`
        } else if (safeXp?.AppliesTo === HSPromoEligibility.SpecificSKUs) {
          eligibleExpression = `${eligibleExpression} and Order.Subtotal >= ${safeXp?.MinReq?.Int}`
        } else {
          eligibleExpression = `${eligibleExpression} and Order.Subtotal >= ${safeXp?.MinReq?.Int}`
        }
        break
      case MinRequirementType.MinItemQty:
        if (safeXp.AppliesTo === HSPromoEligibility.SpecificSupplier) {
          eligibleExpression = `${eligibleExpression} and items.Quantity(SupplierID = '${selectedSupplier?.ID}') >= ${safeXp?.MinReq?.Int}`
        } else if (safeXp?.AppliesTo === HSPromoEligibility.SpecificSKUs) {
          eligibleExpression = `${eligibleExpression} and Order.LineItemCount >= ${safeXp?.MinReq?.Int}`
        } else {
          eligibleExpression = `${eligibleExpression} and Order.LineItemCount >= ${safeXp?.MinReq?.Int}`
        }
        break
    }
    if (safeXp?.MaxShipCost) {
      safeXp?.MinReq?.Type
        ? (eligibleExpression = `Order.ShippingCost < ${safeXp?.MaxShipCost}`)
        : (eligibleExpression = `Order.ShippingCost < ${safeXp?.MaxShipCost}`)
    }
    return eligibleExpression.trim()
  }

  buildValueExpression(
    safeXp: PromotionXp,
    selectedSupplier?: HSSupplier
  ): string {
    let valueExpression = 'Order.Subtotal'
    switch (safeXp?.AppliesTo) {
      case HSPromoEligibility.SpecificSupplier:
        valueExpression = 'item.LineSubtotal'
        if (safeXp?.Type === HSPromoType.Percentage) {
          valueExpression = `item.LineSubtotal * ${safeXp?.Value / 100}`
        }
        if (safeXp?.Type === HSPromoType.FixedAmount) {
          valueExpression = `${safeXp?.Value} / items.count(SupplierID = '${selectedSupplier?.ID}')`
        }
        break
      case HSPromoEligibility.SpecificSKUs:
        if (safeXp?.Type === HSPromoType.Percentage) {
          valueExpression = `item.LineSubtotal * ${safeXp?.Value / 100}`
        }
        if (safeXp?.Type === HSPromoType.FixedAmount) {
          valueExpression = `item.Quantity * ${safeXp?.Value}`
        }
        break
      default:
        if (safeXp?.Type === HSPromoType.Percentage) {
          valueExpression = `items.total(Product.xp.PromotionEligible=\'true\') * ${
            safeXp.Value / 100
          }`
        }
        if (safeXp?.Type === HSPromoType.FixedAmount) {
          valueExpression = `${safeXp?.Value}`
        }
        break
    }
    if (safeXp?.Type === 'FreeShipping') {
      valueExpression = 'Order.ShippingCost'
    }
    if (safeXp?.Type === 'BOGO') {
      valueExpression = this.buildBogoValueExpression(safeXp).trim()
    }
    return valueExpression.trim()
  }

  buildBogoValueExpression(safeXp: PromotionXp): string {
    const buySKU = safeXp?.BOGO?.BuySKU?.SKU
    const buyQty = safeXp?.BOGO?.BuySKU?.Qty
    const getSKU = safeXp?.BOGO?.GetSKU?.SKU
    const getQty = safeXp?.BOGO?.GetSKU?.Qty
    const percentOff = safeXp?.BOGO?.Value / 100
    const buyAndGetQty = Number(buyQty) + Number(getQty)
    let valueExpression: string
    if (buySKU === getSKU) {
      if (safeXp?.BOGO?.Type === HSBogoType.Percentage) {
        valueExpression = `((items.quantity(ProductID='${buySKU}')-(items.quantity(ProductID='${buySKU}')%${buyAndGetQty}))/${buyAndGetQty})*${getQty}*((items.total(ProductID='${getSKU}')/items.quantity(ProductID='${getSKU}')))*${percentOff}`
      } else if (safeXp?.BOGO?.Type === HSBogoType.FixedAmount) {
        valueExpression = `((items.quantity(ProductID='${buySKU}')-items.quantity(ProductID='${buySKU}')%${buyAndGetQty})/${buyAndGetQty})*${safeXp?.BOGO?.Value}`
      }
    } else {
      if (safeXp?.BOGO?.Type === HSBogoType.Percentage) {
        valueExpression = `min(((items.quantity(ProductID='${buySKU}')-((items.quantity(ProductID='${buySKU}')%${buyQty})))/${buyQty})*${getQty},items.quantity(ProductID='${getSKU}')-(items.quantity(ProductID='${getSKU}')%${getQty}))*((items.total(ProductID='${getSKU}'))/items.quantity(ProductID='${getSKU}'))*${percentOff}`
      } else if (safeXp?.BOGO?.Type === HSBogoType.FixedAmount) {
        valueExpression = `min(((items.quantity(ProductID='${buySKU}')-((items.quantity(ProductID='${buySKU}')%${buyQty})))/${buyQty})*${getQty},items.quantity(ProductID='${getSKU}')-(items.quantity(ProductID='${getSKU}')%${getQty}))/${getQty}*${safeXp?.BOGO?.Value}`
      }
    }

    return valueExpression?.trim()
  }

  buildBogoEligibleExpression(safeXp: PromotionXp): string {
    const buySKU = safeXp?.BOGO?.BuySKU?.SKU
    const buySKUQty = safeXp?.BOGO?.BuySKU?.Qty
    const getSKU = safeXp?.BOGO?.GetSKU?.SKU
    const getSKUQty = safeXp?.BOGO?.GetSKU?.Qty
    let eligibleExpression: string
    if (buySKU === getSKU) {
      eligibleExpression = `items.quantity(ProductID='${buySKU}')>=${
        buySKUQty + getSKUQty
      }`
    } else {
      eligibleExpression = `items.quantity(ProductID='${buySKU}')>= ${buySKUQty} and items.quantity(ProductID='${getSKU}')>=${getSKUQty}`
    }
    return eligibleExpression.trim()
  }
}
