import { AppConfig } from "../../models/environment.types";
import { Injectable } from '@angular/core'
import { HSLineItem, HSProduct } from "@ordercloud/headstart-sdk";

// mootrack() is defined in a script loaded in loadMoosendTracker()
declare var mootrack: any;

// These requests should not be awaited, they are fire-and-forget.

/** Track commerce events and forward to SiteCore Send https://www.sitecore.com/products/send */
@Injectable({
    providedIn: 'root',
})
export class SitecoreSendTrackingService {
    private userIdentified = false; 
    // This is defined in the seed process in the middleware
    private readonly anonymousUserEmail = "default-buyer-user@test.com"

    constructor(private appConfig: AppConfig) { 
        if (!appConfig.useSitecoreSend) { return; }

        this.loadMoosendTracker();
            
        mootrack('init', appConfig.sitecoreSendWebsiteID);
    }

    identify(email: string): void {
        if (!this.appConfig.useSitecoreSend) { return; }
        // this is not a real user
        if (email === this.anonymousUserEmail) { return; }

        mootrack('identify', email);
        this.userIdentified = true;
    }

    viewProduct(product: HSProduct): void {
        if (!this.appConfig.useSitecoreSend || !this.userIdentified) { return; }

        let p = this.MapProduct(product);
        mootrack('PAGE_VIEWED', [p]);
    }

    addToCart(lineItem: HSLineItem): void {
        if (!this.appConfig.useSitecoreSend || !this.userIdentified) { return; }

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

    purchase(lineItems: HSLineItem[]): void {
        if (!this.appConfig.useSitecoreSend || !this.userIdentified) { return; }

        let products = lineItems.map(this.MapProductFromLi);
        mootrack('trackOrderCompleted', products);
    }

    customEvent(key: string, data: any): void {
        if (!this.appConfig.useSitecoreSend || !this.userIdentified) { return; }

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

    private GetUrl(productID: string): string {
        return `${this.appConfig.baseUrl}/products/${productID}`;
    }

    private loadMoosendTracker():void  {
        var node = document.createElement("script");
        node.type = "text/javascript";
        node.async = true;
        // Mootracker installation https://help.moosend.com/hc/en-us/articles/115002454009-How-can-I-install-website-tracking-by-using-the-JS-tracking-library-
        node.innerHTML = "!function(t,n,e,o,a){function d(t){var n=~~(Date.now()/3e5),o=document.createElement(e);o.async=!0,o.src=t+\"?ts=\"+n;var a=document.getElementsByTagName(e)[0];a.parentNode.insertBefore(o,a)}t.MooTrackerObject=a,t[a]=t[a]||function( ){return t[a].q?void t[a].q.push(arguments):void(t[a].q=[arguments])},window .attachEvent?window.attachEvent(\"onload\",d.bind(this,o) ):window.addEventListener(\"load\",d.bind(this,o),!1)}(window,document,\"script\",\"//cdn.stat-track.com/statics/moosend-tracking.min.js\",\"mootrack\");";
        document.getElementsByTagName('head')[0].appendChild(node);
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