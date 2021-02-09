import { HSLineItem, HSMeProduct } from "@ordercloud/headstart-sdk";
import { Address, BuyerProduct, LineItem, Supplier } from "ordercloud-javascript-sdk";

export interface LineItemGroupSupplier {
    supplier: Supplier
    shipFrom: Address
  }

  export enum LineItemStatus {
    Complete = 'Complete',
    Submitted = 'Submitted',
    Open = 'Open',
    Backordered = 'Backordered',
    Canceled = 'Canceled',
    CancelRequested = 'CancelRequested',
    Returned = 'Returned',
    ReturnRequested = 'ReturnRequested',
  }

    /**
 * LineItem with the full product details. Currently used in the cart page only.
 */
export interface LineItemWithProduct extends LineItem {
    Product?: HSMeProduct
  }

  export interface HSLineItemWithBuyerProduct {
    lineItem: HSLineItem;
    buyerProduct: BuyerProduct
  }