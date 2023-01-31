import { RequiredDeep, Tokens } from 'ordercloud-javascript-sdk'
import { Component, Inject } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import {
  AssetType,
  AssetUpload,
  BatchProcessResult,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { getPsHeight } from '@app-seller/shared/services/dom.helper'
import { NgxSpinnerService } from 'ngx-spinner'
import { AppConfig } from '@app-seller/models/environment.types'
import { FileHandle } from '@app-seller/models/file-upload.types'
import { Products } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'upload-shipments',
  templateUrl: './upload-shipments.component.html',
  styleUrls: ['./upload-shipments.component.scss'],
})
export class UploadShipmentsComponent {
  constructor(
    @Inject(applicationConfiguration) private appConfig: AppConfig,
    private spinner: NgxSpinnerService
  ) {
    this.contentHeight = getPsHeight('base-layout-item')
  }

  files: FileHandle[] = []
  contentHeight = 0
  showUploadSummary = false
  batchProcessResult: RequiredDeep<BatchProcessResult>
  showResults = false

  async downloadTemplate(): Promise<void> {
    const file = 'Shipment_Import_Template.xlsx'
    const sharedAccessSignature = await this.getSharedAccessSignature(file)
    const uri = `${this.appConfig.blobStorageUrl}/downloads/Shipment_Import_Template.xlsx${sharedAccessSignature}`
    const link = document.createElement('a')
    link.download = file
    link.href = uri
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  private async getSharedAccessSignature(fileName: string): Promise<string> {
    return await HeadStartSDK.Reports.GetSharedAccessSignature(fileName)
  }

  async manualFileUpload(event, fileType: string): Promise<void> {
    this.showUploadSummary = true
    this.showResults = false
    this.spinner.show()
    const accessToken = Tokens.GetAccessToken()
    let asset: AssetUpload = {}

    if (fileType === 'staticContent') {
      if (
        event?.File !== null &&
        event?.File !== undefined &&
        !Array.isArray(event)
      ) {
        asset = {
          Active: true,
          Title: 'document',
          File: event.File,
          FileName: 'shipments_to_process',
        } as AssetUpload
      } else {
        const mappedFiles: FileHandle[] = Array.from(event).map(
          (file: File) => {
            asset = {
              Active: true,
              Title: 'document',
              File: file,
              FileName: 'shipments_to_process',
            } as AssetUpload
            return { File: file, URL, Filename: 'shipments_to_process' }
          }
        )
      }

      const formData = new FormData()

      for (const prop in asset) {
        if (asset.hasOwnProperty(prop)) {
          formData.append(prop, asset[prop])
        }
      }

      const result = await HeadStartSDK.Shipments.UploadShipments(formData)
      if (result !== null) {
        this.batchProcessResult = result
        this.spinner.hide()
        this.showResults = true
      }
    }
  }

  async uploadAsset(
    productID: string,
    file: FileHandle,
    assetType: AssetType
  ): Promise<any> {
    if (assetType === 'image') {
      const [imageData, currentProduct] = await Promise.all([
        HeadStartSDK.Assets.CreateImage({
          File: file.File,
          Filename: file.Filename,
        }),
        Products.Get(productID),
      ])
      const patchObj = {
        xp: {
          Images: [...(currentProduct?.xp?.Images || []), imageData],
        },
      }
      return await Products.Patch(productID, patchObj)
    } else {
      const [documentData, currentProduct] = await Promise.all([
        HeadStartSDK.Assets.CreateDocument({
          File: file.File,
          Filename: file.Filename,
        }),
        Products.Get(productID),
      ])
      const patchObj = {
        xp: {
          Documents: [...(currentProduct?.xp?.Documents || []), documentData],
        },
      }
      return await Products.Patch(productID, patchObj)
    }
  }

  getColumnHeader(columnNumber: number): string {
    //Ensure number is within the amount of columns on the Excel sheet.
    if (columnNumber >= 1 && columnNumber <= 16) {
      return ShipmentImportColumnHeader[columnNumber]
    }
  }

  stageDocument(event): void {
    console.log(event)
  }
}

export enum ShipmentImportColumnHeader {
  OrderID = 1,
  LineItemID = 2,
  QuantityShipped = 3,
  ShipmentID = 4,
  BuyerID = 5,
  Shipper = 6,
  DateShipped = 7,
  DateDelivered = 8,
  TrackingNumber = 9,
  Cost = 10,
  FromAddressID = 11,
  ToAddressID = 12,
  Account = 13,
  XpService = 14,
  ShipmentComment = 15,
  ShipmentLineItemComment = 16,
}
