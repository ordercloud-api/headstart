import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';
import { HSOrderReturn } from '../models';

export default class OrderReturns {

   /**
    * @param orderReturnId ID of the return to be completed
    */
    public async Complete(orderReturnId: string, accessToken?: string ): Promise<RequiredDeep<HSOrderReturn>> {
        return await httpClient.post(`/orderreturns/${orderReturnId}/complete`, {}, { params: accessToken} );
    }
}
