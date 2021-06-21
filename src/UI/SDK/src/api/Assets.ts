import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';
import { DocumentAsset, FileData, ImageAsset } from '../models';

export default class Assets {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.CreateImage = this.CreateImage.bind(this);
        this.Delete = this.Delete.bind(this);
    }

   /**
    * @param assetData required: File
    */
    public async CreateImage(assetData: FileData): Promise<RequiredDeep<ImageAsset>> {
        return await httpClient.post('/assets/image', this.mapFileToFormData(assetData), { params: {} })
    }

    public async CreateDocument(assetData: FileData): Promise<RequiredDeep<DocumentAsset>> {
        return await httpClient.post('/assets/document', this.mapFileToFormData(assetData), { params: {} })
    }

    /**
    * @param assetID id of the asset that you want to delete. The id is the finaly parameter of the asset Url.
    */
    public async Delete(assetID: string): Promise<void> {
        return await httpClient.delete('/assets/' + assetID, { params: {} })
    }

    private mapFileToFormData(file: FileData) {
        const data = new FormData()
        Object.keys(file).forEach(key => {
            data.append(key, file[key])
        })
        return data;
    }

}
