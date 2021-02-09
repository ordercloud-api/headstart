import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import {
  Address,
  BuyerAddress,
  LineItem,
  ListPage,
} from 'ordercloud-javascript-sdk'
import {
  HSOrder,
  HSAddressBuyer,
} from '@ordercloud/headstart-sdk'

import { getSuggestedAddresses } from '../../../services/address-suggestion.helper'
import { NgxSpinnerService } from 'ngx-spinner'
import { ErrorMessages } from '../../../services/error-constants'
import { flatten as _flatten } from 'lodash'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
// TODO - Make this component "Dumb" by removing the dependence on context service
// and instead have it use inputs and outputs to interact with the CheckoutComponent.
// Goal is to get all the checkout logic and state into one component.

@Component({
  templateUrl: './checkout-address.component.html',
  styleUrls: ['./checkout-address.component.scss'],
})
export class OCMCheckoutAddress implements OnInit {
  @Input() order: HSOrder
  @Input() lineItems: ListPage<LineItem>
  @Output() continue = new EventEmitter()
  @Output() handleOrderError = new EventEmitter()
  _addressError: string

  readonly NEW_ADDRESS_CODE = 'new'
  existingBuyerLocations: ListPage<BuyerAddress>
  selectedBuyerLocation: BuyerAddress
  existingShippingAddresses: ListPage<BuyerAddress>
  selectedShippingAddress: BuyerAddress
  showNewAddressForm = false
  suggestedAddresses: BuyerAddress[]
  homeCountry: string

  constructor(
    private context: ShopperContextService,
    private spinner: NgxSpinnerService
  ) {}

  async ngOnInit(): Promise<void> {
    this.spinner.hide()
    this.selectedShippingAddress = this.lineItems?.Items[0].ShippingAddress
    await this.listSavedShippingAddresses()
    await this.listSavedBuyerLocations()
  }

  onBuyerLocationChange(buyerLocationID: string): void {
    this.selectedBuyerLocation = this.existingBuyerLocations.Items.find(
      (location) => buyerLocationID === location.ID
    )
    const shippingAddress = this.existingShippingAddresses.Items.find(
      (location) => location.ID === this.selectedBuyerLocation.ID
    )
    if (shippingAddress) {
      this.selectedShippingAddress = shippingAddress
    }
  }

  onShippingAddressChange(shippingAddressID: string): void {
    this.showNewAddressForm = shippingAddressID === this.NEW_ADDRESS_CODE
    this.selectedShippingAddress = this.existingShippingAddresses.Items.find(
      (address) => shippingAddressID === address.ID
    )
    const shippingAddress = this.existingShippingAddresses.Items.find(
      (address) => address.ID === this.selectedShippingAddress?.ID
    )
    if (shippingAddress) {
      this.selectedShippingAddress = shippingAddress
    }
  }

  async saveAddressesAndContinue(
    newShippingAddress: Address = null
  ): Promise<void> {
    if (!this.selectedBuyerLocation) {
      throw new Error('Please select a location for this order')
    }
    try {
      this.spinner.show()
      this.order = await this.context.order.checkout.setBuyerLocationByID(
        this.selectedBuyerLocation?.ID
      )
      if (newShippingAddress != null) {
        this.selectedShippingAddress = await this.saveNewShippingAddress(
          newShippingAddress
        )
      }

      if (this.selectedShippingAddress) {
        await this.context.order.checkout.setShippingAddressByID(
          this.selectedShippingAddress
        )
        this.continue.emit()
      } else {
        // not able to create address - display suggestions to user
        this.spinner.hide()
      }
    } catch (e) {
      if (e?.message === ErrorMessages.orderNotAccessibleError) {
        this.handleOrderError.emit(e.message)
      } else if (e?.response?.data?.Message) {
        this._addressError = e?.response?.data?.Message
      } else {
        throw e
      }
      this.spinner.hide()
    }
  }

  addressFormChanged(address: BuyerAddress): void {
    this.selectedShippingAddress = address
  }

  showNewAddress(): void {
    this.showNewAddressForm = true
    this.selectedShippingAddress = null
    this.suggestedAddresses = []
  }

  private async listSavedBuyerLocations(): Promise<void> {
    const listOptions = {
      page: 1,
      pageSize: 100,
    }
    this.existingBuyerLocations = await this.context.addresses.listBuyerLocations(
      listOptions
    )
    this.homeCountry = this.existingBuyerLocations?.Items[0]?.Country || 'US'
    if (this.existingBuyerLocations?.Meta.TotalPages <= 1) {
      if (this.existingBuyerLocations?.Items.length === 1) {
        this.selectedBuyerLocation = this.selectedShippingAddress = this.existingBuyerLocations.Items[0]
      }
    } else {
      let requests = []
      for (
        let page = 2;
        page <= this.existingBuyerLocations.Meta.TotalPages;
        page++
      ) {
        listOptions.page = page;
        // Hack to avoid page being mutated after the request has been added to the queue
        const copiedListOptions = JSON.parse(JSON.stringify(listOptions));
        requests = [
          ...requests,
          this.context.addresses.listBuyerLocations(copiedListOptions),
        ]
      }
      return await Promise.all(requests).then((response) => {
        this.existingBuyerLocations.Items = [
          ...this.existingBuyerLocations.Items,
          ..._flatten(response.map((r) => r.Items)),
        ]
      })
    }
  }

  private async listSavedShippingAddresses(): Promise<void> {
    const listOptions = {
      page: 1,
      pageSize: 100
    }
    this.existingShippingAddresses = await this.context.addresses.listShippingAddresses(listOptions);
    if (this.existingShippingAddresses?.Meta.TotalPages > 1) {
      let requests = []
      for (
        let page = 2;
        page <= this.existingShippingAddresses.Meta.TotalPages;
        page++
      ) {
        listOptions.page = page;
        // Hack to avoid page being mutated after the request has been added to the queue
        const copiedListOptions = JSON.parse(JSON.stringify(listOptions));
        requests = [
          ...requests,
          this.context.addresses.listShippingAddresses(copiedListOptions),
        ]
      }
      return await Promise.all(requests).then((response) => {
        this.existingShippingAddresses.Items = [
          ...this.existingShippingAddresses.Items,
          ..._flatten(response.map((r) => r.Items)),
        ]
      })
    }
  }

  private async saveNewShippingAddress(
    address: BuyerAddress
  ): Promise<HSAddressBuyer> {
    address.Shipping = true
    address.Billing = false
    try {
      const savedAddress = await this.context.addresses.create(address)
      return savedAddress
    } catch (ex) {
      this.suggestedAddresses = getSuggestedAddresses(ex)
      if (!(this.suggestedAddresses?.length >= 1)) throw ex
      return null // set this.selectedShippingAddress
    }
  }
}
