import { AppConfig } from "../models/environment.types";
import { Injectable } from '@angular/core'
import { HSLineItem, HSProduct } from "@ordercloud/headstart-sdk";

// mootrack() is defined in a script tag in index.html
declare var mootrack: any;

// These requests should not be awaited, they are fire-and-forget.


/** Track commerce events and forward to Moosend https://moosend.com/ */
@Injectable({
    providedIn: 'root',
})
export class MooTrackService {
    userIdentified = false; 
    // This is defined in the seed process in the middleware
    readonly anonymousUserEmail = "default-buyer-user@test.com"

    constructor(private appConfig: AppConfig) { 
        if (!appConfig.useMoosend) { return; }
            
        mootrack('init', appConfig.moosendWebsiteID);
    }

    identify(email: string) {
        if (!this.appConfig.useMoosend) { return; }
        // this is not a real user
        if (email === this.anonymousUserEmail) { return; }

        mootrack('identify', email);
        this.userIdentified = true;
    }

    viewProduct(product: HSProduct) {
        if (!this.appConfig.useMoosend || !this.userIdentified) { return; }

        let p = this.MapProduct(product);
        mootrack('PAGE_VIEWED', [p]);
    }

    addToCart(lineItem: HSLineItem) {
        if (!this.appConfig.useMoosend || !this.userIdentified) { return; }

        let product = this.MapProductFromLi(lineItem)
        mootrack('trackAddToOrder', 
            product.itemCode, 
            product.itemPrice, 
            product.itemUrl, 
            product.itemQuantity, 
            product.itemTotalPrice, 
            product.itemName, 
            product.itemImage
        );
    }

    purchase(lineItems: HSLineItem[]) {
        if (!this.appConfig.useMoosend || !this.userIdentified) { return; }

        let products = lineItems.map(this.MapProductFromLi);
        mootrack('trackOrderCompleted', products);
    }

    customEvent(key: string, data: any) {
        if (!this.appConfig.useMoosend || !this.userIdentified) { return; }

        mootrack(key, data);
    }

    private MapProductFromLi(lineItem: HSLineItem): MoosendProduct {
        return {
            itemCode: lineItem.Product.ID,
            itemName: lineItem.Product.Name,
            itemPrice: lineItem.UnitPrice,
            itemQuantity: lineItem.Quantity,
            itemTotalPrice: lineItem.LineTotal,
            itemImage: lineItem.Product.xp.Images[0].Url,
            itemUrl: this.GetUrl(lineItem.Product.ID),
        }
    }

    private MapProduct(product: HSProduct): MoosendProduct {
        return {
            itemCode: product.ID,
            itemName: product.Name,
            itemImage: product.xp.Images[0].Url,
            itemUrl: this.GetUrl(product.ID),
        }
    }

    private GetUrl(productID: string) {
        return `${this.appConfig.baseUrl}/products/${productID}`;
    }
}

export interface MoosendProduct {
    itemCode?: string,
    itemPrice?: number,
    itemUrl?: string,
    itemQuantity?: number,
    itemTotalPrice?: number,
    itemName?: string,
    itemImage?: string,
    itemCategory?: string,
    itemManufacturer?: string
}