import { HSLocationUserGroup } from "@ordercloud/headstart-sdk";
import { MeUser } from "ordercloud-javascript-sdk";
import { CurrenySymbol } from "./currency.types";


  export interface DecodedOCToken {
    /**
     * the ordercloud username
     */
    usr: string
  
    /**
     * the client id used when making token request
     */
    cid: string
  
    /**
     * helpful for identifying user types in an app
     * that may have both types
     */
    usrtype: 'admin' | 'buyer'
  
    /**
     * list of security profile roles that this user
     * has access to, read more about security profile roles
     * [here](https://developer.ordercloud.io/documentation/platform-guides/authentication/security-profiles)
     */
    role: string[] // TODO: add security profile roles to the sdk
  
    /**
     * the issuer of the token - should always be https://auth.ordercloud.io
     */
    iss: string
  
    /**
     * the audience - who should be consuming this token
     * this should always be https://api.ordercloud.io (the ordercloud api)
     */
    aud: string
  
    /**
     * expiration of the token (in seconds) since the
     * UNIX epoch (January 1, 1970 00:00:00 UTC)
     */
    exp: number
  
    /**
     * point at which token was issued (in seconds) since the
     * UNIX epoch (January 1, 1970 00:00:00 UTC)
     */
    nbf: number
  
    /**
     * the order id assigned to the anonymous user,
     * this value will *only* exist for anonymous users
     */
    orderid?: string
  }

  export interface CurrentUser extends MeUser {
    FavoriteProductIDs: string[]
    FavoriteOrderIDs: string[]
    UserGroups: HSLocationUserGroup[]
    Currency: CurrenySymbol
  }