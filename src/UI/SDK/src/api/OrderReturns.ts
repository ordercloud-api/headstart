import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';
import { HSOrderReturn } from '../models';
import { OrderReturnItem } from 'ordercloud-javascript-sdk';
import { LineItemReturnCalculation } from 'ordercloud-javascript-sdk/dist/models/LineItemReturnCalculation';

export default class OrderReturns {

   /**
    * @param orderReturnId ID of the return to be completed
    */
    public async Complete(orderReturnId: string, accessToken?: string ): Promise<RequiredDeep<HSOrderReturn>> {
        return await httpClient.post(`/orderreturns/${orderReturnId}/complete`, {}, { params: accessToken} );
    }

    /**
    * @param orderId ID of the order to be calculated
    */
    public async Calculate(orderId: string, itemsToReturn: OrderReturnItem[] , accessToken?: string ): Promise<RequiredDeep<LineItemReturnCalculation[]>> {
        return await httpClient.post(`/orderreturns/${orderId}/calculate`, itemsToReturn, { params: accessToken} );
    }
}
