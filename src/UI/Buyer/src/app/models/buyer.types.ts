import { HSAddressBuyer, HSMeProduct } from "@ordercloud/headstart-sdk";

export interface BuyerLocation {
    location?: HSAddressBuyer
  }

  //    these can be replaced with sdk
export interface BuyerRequestForInfo {
    FirstName: string
    LastName: string
    Email: string
    Phone: string
    Comments: string
}

export interface ContactSupplierBody {
    Product: HSMeProduct
    BuyerRequest: BuyerRequestForInfo
}