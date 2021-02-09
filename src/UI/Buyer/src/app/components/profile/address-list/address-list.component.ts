import { Component, Input, OnInit } from '@angular/core'
import { faEdit, faTrashAlt } from '@fortawesome/free-regular-svg-icons'
import { faArrowLeft, faPlus } from '@fortawesome/free-solid-svg-icons'
import { BuyerAddress, ListPage } from 'ordercloud-javascript-sdk'
import { ToastrService } from 'ngx-toastr'
import { getSuggestedAddresses } from '../../../services/address-suggestion.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { ModalState } from 'src/app/models/shared.types'

@Component({
  templateUrl: './address-list.component.html',
  styleUrls: ['./address-list.component.scss'],
})
export class OCMAddressList implements OnInit {
  @Input() addresses: ListPage<BuyerAddress>
  faPlus = faPlus
  faArrowLeft = faArrowLeft
  faTrashAlt = faTrashAlt
  faEdit = faEdit
  currentAddress: BuyerAddress
  requestOptions: {
    page?: number
    search?: string
    filters?: {
      [key: string]: string | string[]
    }
  } = {
    page: undefined,
    search: undefined,
    filters: { ['Editable']: 'true' },
  }
  resultsPerPage = 8
  areYouSureModal = ModalState.Closed
  showCreateAddressForm = false
  isLoading = false
  suggestedAddresses: BuyerAddress[]
  homeCountry: string
  _addressError: string
  constructor(
    private context: ShopperContextService,
    private toasterService: ToastrService
  ) {}

  ngOnInit(): void {
    this.reloadAddresses()
  }

  reset(): void {
    this.currentAddress = {}
  }

  showAddAddress(): void {
    this.currentAddress = null
    this.showCreateAddressForm = true
  }

  showEditAddress(address: BuyerAddress): void {
    this.currentAddress = address
    this.showCreateAddressForm = true
  }

  showAreYouSure(address: BuyerAddress): void {
    this.currentAddress = address
    this.areYouSureModal = ModalState.Open
  }

  closeAreYouSure(): void {
    this.currentAddress = null
    this.areYouSureModal = ModalState.Closed
  }

  dismissEditAddressForm(): void {
    this.currentAddress = null
    this.showCreateAddressForm = false
    this.suggestedAddresses = null
  }

  async addressFormSubmitted(address: BuyerAddress): Promise<void> {
    window.scrollTo(0, null)
    try {
      if (this.currentAddress?.ID) {
        await this.updateAddress(address)
      } else {
        await this.addAddress(address)
      }
    } catch (e) {
      if (e?.response?.data?.Message) {
        this._addressError = e?.response?.data?.Message
      } else {
        throw e
      }
    }
  }

  addressFormChanged(address: BuyerAddress): void {
    this.currentAddress = address
  }

  async deleteAddress(address: BuyerAddress): Promise<void> {
    this.areYouSureModal = ModalState.Closed
    await this.context.addresses.delete(address.ID)
    this.addresses.Items = this.addresses.Items.filter(
      (a) => a.ID !== address.ID
    )
  }

  updateRequestOptions(newOptions: { page?: number; search?: string }): void {
    this.requestOptions = Object.assign(this.requestOptions, newOptions)
    this.reloadAddresses()
  }

  protected refresh(): void {
    this.currentAddress = null
    this.reloadAddresses()
  }

  private async addAddress(address: BuyerAddress): Promise<void> {
    try {
      address.Shipping = true
      address.Billing = true
      const newAddress = await this.context.addresses.create(address)
      this.addresses.Items = [...this.addresses.Items, newAddress]
      this.showCreateAddressForm = false
      this.suggestedAddresses = null
      this.refresh()
    } catch (ex) {
      this.suggestedAddresses = getSuggestedAddresses(ex)
      if (!(this.suggestedAddresses?.length >= 1)) {
        throw ex
      } else {
        this.currentAddress = null
      }
    }
  }

  private async updateAddress(address: BuyerAddress): Promise<any> {
    try {
      address.Shipping = true
      address.Billing = true
      ;(address as any).ID = this.currentAddress.ID
      await this.context.addresses.edit(address.ID, address)
      this.showCreateAddressForm = false
      this.suggestedAddresses = null
      this.refresh()
    } catch (ex) {
      this.currentAddress = null
      this.suggestedAddresses = getSuggestedAddresses(ex)
    }
  }

  private async reloadAddresses(): Promise<void> {
    this.isLoading = true
    this.addresses = await this.context.addresses.list(this.requestOptions)
    if (!this.homeCountry) {
      const buyerLocations = await this.context.addresses.listBuyerLocations()
      this.homeCountry = buyerLocations.Items[0]?.Country
    }
    this.isLoading = false
  }
}
