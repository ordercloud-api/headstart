# Sitecore Send Integration For OrderCloud Headstart

## Scope of this integration
This integration tracks user interactions on the Storefront and forwards these events to Sitecore Send. 

Events 
- User Identify 
- View Product Detail Page
- Add To Cart
- Place Order


## Sitecore Send Basics 
[Sitecore Send](https://www.sitecore.com/products/send) is an email campaign automation tool. Create targeted and relevant communications from beautiful templates and an easy drag-and-drop experience while leveraging all your visitors' data. A great use-case for commerce is abandoned cart emails to retarget users after they indicate buying interest but do not convert. 

See [this guide](https://developers.sitecore.com/learn/integrations/send-oc) for integrating OrderCloud and Send. 


## Steps to use
- Set up the headstart application. This is process is throughly documented [here](https://github.com/ordercloud-api/headstart#initial-setup).
- Sign up for Sitecore Send, create a new "website" for the storefront and copy the ID.  
- Set up FE buyer environment variables (in Buyer/src/assets/appConfigs) required for Send. 
```json
  "useSitcoreSend": true,
  "sitcoreSendWebsiteID": "xxxxxxxxxxxxxxxxxxxxxxxx",
```

