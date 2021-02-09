import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { BuyerAddress, ListPage, Address } from 'ordercloud-javascript-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { SupplierAddressService } from '../supplier-address.service'
import { SupplierService } from '../../suppliers/supplier.service'

@Component({
  selector: 'app-supplier-location-table',
  templateUrl: './supplier-location-table.component.html',
  styleUrls: ['./supplier-location-table.component.scss'],
})
export class SupplierLocationTableComponent extends ResourceCrudComponent<Address> {
  suggestedAddresses: ListPage<BuyerAddress>
  selectedAddress: Address
  canBeDeleted: boolean

  constructor(
    private supplierAddressService: SupplierAddressService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    private supplierService: SupplierService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      supplierAddressService,
      router,
      activatedroute,
      ngZone
    )
  }

  handleAddressSelect(address) {
    this.updatedResource = address
  }

  discardChanges(): void {
    this.suggestedAddresses = null
    this.setUpdatedResourceAndResourceForm(this.resourceInSelection)
  }

  determineIfDeletable(value: boolean): void {
    this.canBeDeleted = value
  }

  async updateExistingResource(): Promise<void> {
    try {
      this.dataIsSaving = true
      const updatedResource = await this.ocService.updateResource(
        this.updatedResource.ID,
        this.updatedResource
      )
      this.resourceInSelection = this.ocService.copyResource(updatedResource)
      this.setUpdatedResourceAndResourceForm(updatedResource)
      this.suggestedAddresses = null
      this.dataIsSaving = false
    } catch (ex) {
      this.suggestedAddresses = this.ocService.getSuggestedAddresses(
        ex?.response?.data
      )
      throw ex?.response?.data?.Message
    } finally {
      this.dataIsSaving = false
    }
  }

  async createNewResource(): Promise<void> {
    try {
      this.dataIsSaving = true
      const newResource = await this.ocService.createNewResource(
        this.updatedResource
      )
      this.selectResource(newResource)
      this.suggestedAddresses = null
      this.dataIsSaving = false
    } catch (ex) {
      this.suggestedAddresses = this.ocService.getSuggestedAddresses(
        ex?.response?.data
      )
      throw ex?.response?.data?.Message
    } finally {
      this.dataIsSaving = false
    }
  }
}
