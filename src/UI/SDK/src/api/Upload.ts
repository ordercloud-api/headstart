import httpClient from '../utils/HttpClient'
import { AssetUpload } from '../models/AssetUpload'
import { Asset } from '../models/Asset'
import { RequiredDeep } from '../models/RequiredDeep'

export default class Upload {
  constructor() {
    this.UploadAsset = this.UploadAsset.bind(this)
  }

  async UploadAsset(
    asset: AssetUpload,
    accessToken?: string
  ): Promise<RequiredDeep<Asset>> {
    const form = new FormData()
    for (const prop in asset) {
      if (asset.hasOwnProperty(prop)) {
        form.append(prop, asset[prop])
      }
    }
    return await httpClient.post(`/assets`, form, { params: { accessToken } })
  }
}
