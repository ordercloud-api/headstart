import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import {
  Address,
  BuyerAddress,
  LineItem,
  ListPage,
  Me,
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
import { listAll } from 'src/app/services/listAll'
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
  isAnon: boolean

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
  ) { }

  async ngOnInit(): Promise<void> {
    this.isAnon = this.context.currentUser.isAnonymous();
    if(this.isAnon) {
      this.showNewAddress();
    }
    this.spinner.hide()
    this.selectedShippingAddress = this.lineItems?.Items[0].ShippingAddress
    await this.ListAddressesForShipping()
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
    if (!this.selectedBuyerLocation && !this.isAnon) {
      throw new Error('Please select a location for this order')
    }
    try {
      this.spinner.show()
      if (this.isAnon) {
        await this.handleAnonShippingAddress(newShippingAddress)
      } else {
        this.order = await this.context.order.checkout.setBuyerLocationByID(
          this.selectedBuyerLocation?.ID
        )
        this.handleLoggedInShippingAddress(newShippingAddress)
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

  async handleAnonShippingAddress(newShippingAddress: Address<any>): Promise<void> {
    if (newShippingAddress != null) {
      this.selectedShippingAddress = newShippingAddress;
    }
    this.context.order.checkout.setOneTimeAddress((this.selectedShippingAddress as Address), 'shipping')
    this.continue.emit()
  }

  async handleLoggedInShippingAddress(newShippingAddress: Address<any>): Promise<void> {
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
  }

  addressFormChanged(address: BuyerAddress): void {
    this.selectedShippingAddress = address
  }

  showNewAddress(): void {
    this.showNewAddressForm = true
    this.selectedShippingAddress = null
    this.suggestedAddresses = []
  }

  private async ListAddressesForShipping() {
    const buyerLocationsFilter = {
      filters: { Editable: 'false' }
    }
    const shippingAddressesFilter = {
      filters: { Shipping: 'true' }
    }
    this.existingBuyerLocations = await listAll(Me, Me.ListAddresses, buyerLocationsFilter)
    this.homeCountry = this.existingBuyerLocations?.Items[0]?.Country || 'US'
    if (this.existingBuyerLocations?.Items.length === 1) {
      this.selectedBuyerLocation = this.selectedShippingAddress = this.existingBuyerLocations.Items[0]
    }

    this.existingShippingAddresses = await listAll(Me, Me.ListAddresses, shippingAddressesFilter)
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
