import {
  Component,
  ChangeDetectorRef,
  NgZone,
  AfterViewChecked,
  OnInit,
} from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { StorefrontsService } from '../../storefronts/storefronts.service'
import { AssetUpload } from '@ordercloud/headstart-sdk'
import { JDocument } from '@ordercloud/cms-sdk'
import { StorefrontPageService } from '../storefront-page.service'
import { getPsHeight } from '@app-seller/shared/services/dom.helper'
import { REDIRECT_TO_FIRST_PARENT } from '@app-seller/layout/header/header.config'
import { v4 as guid } from 'uuid'
import { takeWhile } from 'rxjs/operators'
// import { ApiClients } from 'ordercloud-javascript-sdk';

@Component({
  selector: 'app-storefront-page-table',
  templateUrl: './storefront-page-table.component.html',
  styleUrls: ['./storefront-page-table.component.scss'],
})
export class StorefrontPageTableComponent
  extends ResourceCrudComponent<JDocument>
  implements AfterViewChecked, OnInit {
  route = 'pages'
  resourceHeight: number
  editorOptions: any
  assetFilterOptions: any
  constructor(
    private storefrontPageService: StorefrontPageService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    private storefrontsService: StorefrontsService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      storefrontPageService,
      router,
      activatedRoute,
      ngZone
    )
  }

  ngAfterViewChecked(): void {
    this.resourceHeight = getPsHeight('additional-item-resource')
  }

  ngOnInit(): any {
    super.ngOnInit() // call parent onInit so we don't overwrite it

    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    this.parentResourceIDSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((parentResourceID) => {
        if (parentResourceID) {
          // const apiClient = await ApiClients.Get(parentResourceID);
          this.setEditorOptions(parentResourceID)
          this.redirectToFirstParentIfNeeded(parentResourceID)

          // unlike content documents, assets are not scoped to an organization they are all visible to everyone
          // we will scope assets by api client by prefixing assets with the clientID and then filtering by that
          this.assetFilterOptions = {
            filters: {
              ID: `${parentResourceID}-*`,
            },
          }
        }
      })

    // we're passing down a function to the cms library
    // and need to bind 'this' from this class so we can access class methods/properties
    this.prefixAssetID = this.prefixAssetID.bind(this)
  }

  setEditorOptions(clientID: string): void {
    this.editorOptions = {}
    if (clientID === 'A5231DF1-2B00-4002-AB40-738A9E2CEC4B') {
      // eslint-disable-next-line @typescript-eslint/camelcase, camelcase
      this.editorOptions.content_css = [
        'https://marketplace-buyer-ui-test.azurewebsites.net/styles.f73343b62778747ba6b8.css',
      ]
    } else if (clientID === 'A2E62264-B6FB-4C25-8507-79A58A35BE85') {
      // eslint-disable-next-line @typescript-eslint/camelcase, camelcase
      this.editorOptions.content_css = [
        'https://sebrandsmarketplace-basecamp.azurewebsites.net/styles.e0ac2f58bbaf77939ee1.css',
      ]
    } else if (clientID === '997F5753-4C11-4F5F-9B40-6AC843638BA2') {
      // eslint-disable-next-line @typescript-eslint/camelcase, camelcase
      this.editorOptions.content_css = [
        'https://sebrandsmarketplace-bar.azurewebsites.net/styles.5b06344ef1424055c881.css',
      ]
    } else {
      // eslint-disable-next-line @typescript-eslint/camelcase, camelcase
      this.editorOptions.content_css = [
        'https://sebrandsmarketplace-anytime.azurewebsites.net/styles.5e6471eb0cec93cf2823.css',
      ]
    }
  }

  private async redirectToFirstParentIfNeeded(
    parentResourceID: string
  ): Promise<void> {
    if (parentResourceID === REDIRECT_TO_FIRST_PARENT) {
      await this.storefrontsService.listResources()
      this.ocService.selectParentResource(
        this.storefrontsService.resourceSubject.value.Items[0]
      )
    }
  }

  // unlike content documents, assets are not scoped to an organization they are all visible to everyone
  // we will scope assets by api client by prefixing assets with the clientID and then filtering by that
  prefixAssetID(asset: AssetUpload): Promise<AssetUpload> {
    if (!asset) return
    const prefixedID = `${this.parentResourceID}-${guid()}`
    asset.ID = prefixedID
    return Promise.resolve(asset)
  }
}
