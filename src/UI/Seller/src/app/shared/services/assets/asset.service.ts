import { Injectable } from '@angular/core'
import { FileHandle } from '@app-seller/models/file-upload.types'
import {
  AssetType,
  DocumentAsset,
  HeadStartSDK,
  HSProduct, ImageAsset,
} from '@ordercloud/headstart-sdk'
import { Products, Product } from 'ordercloud-javascript-sdk'
import { MiddlewareAPIService } from '../middleware-api/middleware-api.service'
import { getAssetIDFromUrl } from './asset.helper'

@Injectable({
  providedIn: 'root',
})
export class AssetService {
  constructor(
  ) { }

  async uploadImageFiles(files: FileHandle[]): Promise<ImageAsset[]> {
    return await Promise.all(
      files.map(file => {
        return HeadStartSDK.Assets.CreateImage({
          File: file.File,
          Filename: file.Filename
        })
      })
    );
  }

  async uploadDocumentFiles(files: FileHandle[]): Promise<DocumentAsset[]> {
    return await Promise.all(
      files.map(file => {
        return HeadStartSDK.Assets.CreateDocument({
          File: file.File,
          Filename: file.Filename
        })
      })
    );
  }

  async deleteAssetUpdateProduct(
    product: HSProduct,
    fileUrl: string,
    assetType: AssetType): Promise<HSProduct> {
    const assetID = getAssetIDFromUrl(fileUrl)
    try {
      await HeadStartSDK.Assets.Delete(assetID)
      return await this.removeAssetFromProduct(product, assetID, assetType)
    } catch (err) {
      if (err?.status === 404) {
        //  If the asset was not found on the delete request. Still delete asset from product
        return await this.removeAssetFromProduct(product, assetID, assetType)
      } else {
        throw err
      }
    }
  }

  async removeAssetFromProduct(
    product: any,
    assetID: string,
    assetType: AssetType): Promise<Product> {
    let patchObj: Partial<Product>
    if (assetType === 'image') {
      const newImages = product?.xp?.Images
        .filter(image => getAssetIDFromUrl(image.Url) !== assetID);
      patchObj = {
        xp: {
          Images: newImages
        }
      }
    } else {
      const newDocuments = product?.xp?.Documents
        .filter(doc => getAssetIDFromUrl(doc.Url) !== assetID);
      patchObj = {
        xp: {
          Documents: newDocuments
        }
      }
    }
    return await Products.Patch(product.ID, patchObj)
  }
}

