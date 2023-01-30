import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Supplier, SupplierUsers, Suppliers } from 'ordercloud-javascript-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms'
import { get as _get } from 'lodash'
import {
  ValidateRichTextDescription,
  ValidateEmail,
  RequireCheckboxesToBeChecked,
} from '@app-seller/validators/validators'
import { SupplierService } from '../supplier.service'
import { HeadStartSDK, HSSupplier } from '@ordercloud/headstart-sdk'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'

function createSupplierForm(supplier: HSSupplier) {
  return new UntypedFormGroup({
    ID: new UntypedFormControl({
      value: supplier.ID,
      disabled: !this.isCreatingNew || this.isSupplierUser,
    }),
    Name: new UntypedFormControl(supplier.Name, Validators.required),
    Description: new UntypedFormControl(
      _get(supplier, 'xp.Description'),
      ValidateRichTextDescription
    ),
    // need to figure out strucure of free string array
    // StaticContentLinks: new FormControl(_get(supplier, 'xp.StaticContentLinks'), Validators.required),
    SupportContactName: new UntypedFormControl(
      (_get(supplier, 'xp.SupportContact') &&
        _get(supplier, 'xp.SupportContact.Name')) ||
        ''
    ),
    SupportContactEmail: new UntypedFormControl(
      (_get(supplier, 'xp.SupportContact') &&
        _get(supplier, 'xp.SupportContact.Email')) ||
        '',
      ValidateEmail
    ),
    SupportContactPhone: new UntypedFormControl(
      (_get(supplier, 'xp.SupportContact') &&
        _get(supplier, 'xp.SupportContact.Phone')) ||
        ''
    ),
    Active: new UntypedFormControl({
      value: supplier.Active,
      disabled: this.isSupplierUser,
    }),
    SyncFreightPop: new UntypedFormControl({
      value: supplier.xp?.SyncFreightPop || false,
      disabled: this.isSupplierUser,
    }),
    Currency: new UntypedFormControl(
      {
        value: _get(supplier, 'xp.Currency'),
        disabled: !this.isCreatingNew || this.isSupplierUser,
      },
      Validators.required
    ),
    ProductTypes: new UntypedFormGroup(
      {
        Standard: new UntypedFormControl({
          value: supplier.xp?.ProductTypes?.includes('Standard') || false,
          disabled: this.isSupplierUser,
        }),
        Quote: new UntypedFormControl({
          value: supplier.xp?.ProductTypes?.includes('Quote') || false,
          disabled: this.isSupplierUser,
        }),
      },
      RequireCheckboxesToBeChecked()
    ),
    FreeShippingEnabled: new UntypedFormControl(
      supplier.xp?.FreeShippingThreshold != null
    ),
    FreeShippingThreshold: new UntypedFormControl(supplier.xp?.FreeShippingThreshold),
    Categories: new UntypedFormControl({
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
    private middleWareApiService: MiddlewareAPIService
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
    const supplierFilterConfig =
      await this.middleWareApiService.getSupplierFilterConfig()
    const filterConfig = {
      Filters: supplierFilterConfig.Items.map((filter) => filter.Doc),
    }
    this.filterConfig = filterConfig
  }

  async createNewResource() {
    try {
      this.dataIsSaving = true
      // Create Supplier
      const supplier = (await this.supplierService.createNewResource(
        this.updatedResource
      )) as HSSupplier
      const patchObj: Partial<HSSupplier> = {
        xp: {},
      }
      if (this.file) {
        // Upload their logo, if there is one.  Then, patch supplier xp
        const imgUrls = await HeadStartSDK.Assets.CreateImage({
          File: this.file,
        })
        patchObj.xp.Image = imgUrls
      }
      // Default the NotificationRcpts to initial user
      const users = await SupplierUsers.List(supplier.ID)
      patchObj.xp.NotificationRcpts = [users.Items[0].Email]
      const patchedSupplier: HSSupplier = await Suppliers.Patch(
        supplier.ID,
        patchObj
      )
      this.updateResourceInList(patchedSupplier)
      this.selectResource(patchedSupplier)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  updateResourceInList(supplier: Supplier): void {
    const index = this.resourceList?.Items?.findIndex(
      (item) => item.ID === supplier.ID
    )
    if (index !== -1) {
      this.resourceList.Items[index] = supplier
    }
  }
}
