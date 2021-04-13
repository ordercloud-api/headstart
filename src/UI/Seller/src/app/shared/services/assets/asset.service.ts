import { Injectable } from '@angular/core'
import { AssetType, DocumentAsset, ImageAsset } from '@app-seller/models/Asset.types'
import { FileHandle } from '@app-seller/models/file-upload.types'
import {
  HSProduct,
} from '@ordercloud/headstart-sdk'
import { Products, Product } from 'ordercloud-javascript-sdk'
import { MiddlewareAPIService } from '../middleware-api/middleware-api.service'
import { getAssetIDFromUrl } from './asset.helper'

@Injectable({
  providedIn: 'root',
})
export class AssetService {
  constructor(
    private middleware: MiddlewareAPIService
   ){}

  mapFileToFormData(file: FileHandle): FormData {
    const data = new FormData()
    Object.keys(file).forEach(key => {
      data.append(key, file[key])
    })
    return data;  
  }

  async uploadImageFiles(files: FileHandle[]): Promise<ImageAsset[]> {
    return await Promise.all(
      files.map(file => {
        return this.middleware.uploadImage(this.mapFileToFormData(file))
      })
    );
  }

  async uploadDocumentFiles(files: FileHandle[]): Promise<DocumentAsset[]> {
    return await Promise.all(
      files.map(file => {  
        return this.middleware.uploadDocument(this.mapFileToFormData(file))
      })
    );
  }

  async deleteAssetUpdateProduct(
    product: HSProduct,
    fileUrl: string, 
    assetType: AssetType): Promise<HSProduct> {
      const assetID = getAssetIDFromUrl(fileUrl)
      try {
        await this.middleware.deleteAsset(assetID)
        return await this.removeAssetFromProduct(product, assetID, assetType)
      } catch(err) {
        if(err?.status === 404 ) {
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
    if(assetType === 'image') {
      const newImages = (product?.xp as any)?.Images
      .filter(image => getAssetIDFromUrl(image.Url) !== assetID);
      patchObj = {
        xp: {
          Images: newImages
        }
      }
    } else {
      const newDocuments = (product?.xp as any)?.Documents
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

