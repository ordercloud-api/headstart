/**
 * See for reserved type options https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-a-custom-event-to-sitecore-cdp.html 
 * */ 
export type CDPEventType = "ADD" | "IDENTITY" | "ORDER_CHECKOUT" | "SEARCH" | "VIEW" | "CLEAR_CART" | string;

export interface SitecoreCDPEvent {
    channel: "WEB" | "MOBILE_WEB" | "MOBILE_APP";
    type: CDPEventType;
    language: string;
    currency: string;
    page: string;
    pos: string;
    browser_id: string
}

/**
 * The SEARCH event captures the user's action of searching for a product.
 * https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-a-search-event-to-sitecore-cdp.html
 */
export interface SitecoreCDPSearchEvent extends SitecoreCDPEvent {
    product_name: string;
    product_type: string;
}

/** 
 * Capture IDENTITY events wherever in the site that the guest provides data that might help identify them. It is common for a single browser session to have multiple IDENTITY events. 
 * https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-an-identity-event-to-sitecore-cdp.html
 * */
export interface SitecoreCDPIdentifyEvent extends SitecoreCDPEvent {
    identifiers: { provider: string, id: string};
    email?: string;
    title?: string;
    firstname?: string;
    lastname?: string; 
    gender?: string;
    dob?: string;
    mobile?: string;
    phone?: string;
    street?: string;
    city?: string;
    state?: string;
    country?: string;
    postal_code?: string;
}

/**
 * The ADD event captures the product details when a user adds the product(s) to their online cart.
 * https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-an-add-event-to-sitecore-cdp.html
 */
export interface SitecoreCDPAddEvent extends SitecoreCDPEvent {
    product: {
        type: string;
        item_id: string;
        name: string;
        orderedAt: string;
        quantity: number;
        price: number;
        productId: string;
        currency: string;
        originalPrice: number;
        referenceId: string;
    }
}


/**
 * Before you can send an ORDER_CHECKOUT event, the guest must be identified by sending an IDENTITY event. 
 * https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-an-order-checkout-event-to-sitecore-cdp.html
 */
export interface SitecoreCDPPurchaseEvent extends SitecoreCDPEvent {
    order: {
        referenceId: string;
        orderedAt: string;
        status: "PURCHASED";
        currencyCode: string;
        price: number;
        paymentType: "Card";
        cardType?: string;
        orderItems: {
            type?: string,
            referenceId?: string;
            orderedAt?: string;
            status?:"PURCHASED";
            currencyCode?: string;
            price?: number;
            name?: string;
            productId?: string;
            quantity?: number;
        }[]
    }
}