import { Component, ChangeDetectorRef, NgZone, Inject } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import {
  Supplier,
  OcSupplierUserService,
  OcSupplierService,
} from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { get as _get } from 'lodash'
import {
  ValidateRichTextDescription,
  ValidateEmail,
  RequireCheckboxesToBeChecked,
} from '@app-seller/validators/validators'
import { SupplierService } from '../supplier.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import {
  HeadStartSDK,
  HSSupplier,
} from '@ordercloud/headstart-sdk'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { AppConfig } from '@app-seller/models/environment.types'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { Suppliers } from 'ordercloud-javascript-sdk'

function createSupplierForm(supplier: HSSupplier) {
  return new FormGroup({
    ID: new FormControl({
      value: supplier.ID,
      disabled: !this.isCreatingNew || this.isSupplierUser,
    }),
    Name: new FormControl(supplier.Name, Validators.required),
    Description: new FormControl(
      _get(supplier, 'xp.Description'),
      ValidateRichTextDescription
    ),
    // need to figure out strucure of free string array
    // StaticContentLinks: new FormControl(_get(supplier, 'xp.StaticContentLinks'), Validators.required),
    SupportContactName: new FormControl(
      (_get(supplier, 'xp.SupportContact') &&
        _get(supplier, 'xp.SupportContact.Name')) ||
      ''
    ),
    SupportContactEmail: new FormControl(
      (_get(supplier, 'xp.SupportContact') &&
        _get(supplier, 'xp.SupportContact.Email')) ||
      '',
      ValidateEmail
    ),
    SupportContactPhone: new FormControl(
      (_get(supplier, 'xp.SupportContact') &&
        _get(supplier, 'xp.SupportContact.Phone')) ||
      ''
    ),
    Active: new FormControl({
      value: supplier.Active,
      disabled: this.isSupplierUser,
    }),
    SyncFreightPop: new FormControl({
      value: supplier.xp?.SyncFreightPop || false,
      disabled: this.isSupplierUser,
    }),
    Currency: new FormControl(
      {
        value: _get(supplier, 'xp.Currency'),
        disabled: !this.isCreatingNew || this.isSupplierUser,
      },
      Validators.required
    ),
    ProductTypes: new FormGroup(
      {
        Standard: new FormControl({
          value: supplier.xp?.ProductTypes?.includes('Standard') || false,
          disabled: this.isSupplierUser,
        }),
        Quote: new FormControl({
          value: supplier.xp?.ProductTypes?.includes('Quote') || false,
          disabled: this.isSupplierUser,
        }),
      },
      RequireCheckboxesToBeChecked()
    ),
    FreeShippingEnabled: new FormControl(
      supplier.xp?.FreeShippingThreshold != null
    ),
    FreeShippingThreshold: new FormControl(supplier.xp?.FreeShippingThreshold),
    Categories: new FormControl({
      value: _get(supplier, 'xp.Categories', []),
      disabled: this.isSupplierUser,
    }),
  })
}

@Component({
  selector: 'app-supplier-table',
  templateUrl: './supplier-table.component.html',
  styleUrls: ['./supplier-table.component.scss'],
})
export class SupplierTableComponent extends ResourceCrudComponent<Supplier> {
  filterConfig: {}
  file: File
  constructor(
    private supplierService: SupplierService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    ngZone: NgZone,
    private middleWareApiService: MiddlewareAPIService,
    private appAuthService: AppAuthService,
    @Inject(applicationConfiguration) private appConfig: AppConfig,
    private ocSupplierUserService: OcSupplierUserService,
    private ocSupplierService: OcSupplierService
  ) {
    super(
      changeDetectorRef,
      supplierService,
      router,
      activatedroute,
      ngZone,
      createSupplierForm
    )
    this.router = router
    this.setUpfilter()
  }

  handleStagedFile(event: File): void {
    this.file = event
  }

  async setUpfilter(): Promise<void> {
    await this.buildFilterConfig()
  }

  async buildFilterConfig(): Promise<void> {
    const supplierFilterConfig = await this.middleWareApiService.getSupplierFilterConfig()
    const filterConfig = {
      Filters: supplierFilterConfig.Items.map((filter) => filter.Doc),
    }
    this.filterConfig = filterConfig
  }

  async createNewResource() {
    try {
      this.dataIsSaving = true
      // Create Supplier
      const supplier = await this.supplierService.createNewResource(
        this.updatedResource
      )
      let patchObj: Partial<Supplier> = {
        xp: {}
      }
      if (this.file) {
        // Upload their logo, if there is one.  Then, patch supplier xp
        const imgUrls = await HeadStartSDK.Assets.CreateImage({
          File: this.file
        })
        patchObj.xp.Image = imgUrls
      }
      // Default the NotificationRcpts to initial user
      const users = await this.ocSupplierUserService
        .List(supplier.ID)
        .toPromise()
      patchObj.xp.NotificationRcpts = [users.Items[0].Email]
      const patchedSupplier: HSSupplier = await this.ocSupplierService
        .Patch(supplier.ID, patchObj)
        .toPromise()
      this.updateResourceInList(patchedSupplier)
      this.selectResource(patchedSupplier)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  updateResourceInList(supplier: Supplier): void {
    const index = this.resourceList?.Items?.findIndex(item => item.ID === supplier.ID)
    if (index !== -1) {
      this.resourceList.Items[index] = supplier
    }
  }
}
