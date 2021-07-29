import httpClient from '../utils/HttpClient';
import { HSApiClient} from '../models';

export default class Storefronts {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.DeployStoreFront = this.DeployStoreFront.bind(this);
    }

   /**
    * @param apiClient
    */
    public async DeployStoreFront(apiClient: HSApiClient): Promise<void> {
        return await httpClient.post('/storefronts/deploy', apiClient, { params: {} })
    }
}
