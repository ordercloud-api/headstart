import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import {
  faQuestionCircle,
} from '@fortawesome/free-solid-svg-icons'
import {
  Address,
  BuyerAddress,
  LineItem,
  ListPage,
  Me,
} from 'ordercloud-javascript-sdk'
import {
  HeadStartSDK,
  HSOrder,
  HSAddressBuyer,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'

import { getSuggestedAddresses } from '../../../services/address-suggestion.helper'
import { NgxSpinnerService } from 'ngx-spinner'
import { ErrorMessages } from '../../../services/error-constants'
import { flatten as _flatten } from 'lodash'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { TranslateService } from '@ngx-translate/core'
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
  faQuestionCircle = faQuestionCircle
  existingShippingAddresses: ListPage<BuyerAddress>
  selectedShippingAddress: BuyerAddress
  showNewAddressForm = false
  suggestedAddresses: BuyerAddress[]
  existingBuyerLocations: ListPage<BuyerAddress>
  selectedBuyerLocation: BuyerAddress
  homeCountry: string
  tooltip: string

  constructor(
    private context: ShopperContextService,
    private spinner: NgxSpinnerService,
    private translate: TranslateService
  ) { }

  async ngOnInit(): Promise<void> {
    this.isAnon = this.context.currentUser.isAnonymous();
    this.tooltip = this.translate.instant('CHECKOUT.CHECKOUT_ADDRESS.BUYER_LOCATION_TOOLTIP')
    if(this.isAnon) {
      this.showNewAddress();
    }
    this.spinner.hide()
    this.selectedShippingAddress = this.lineItems?.Items[0].ShippingAddress
    await this.ListAddressesForShipping()
    await this.listSavedBuyerLocations()
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

  handleFormDismissed(): void {
    this.showNewAddressForm = false
    this.selectedShippingAddress = this.lineItems?.Items[0].ShippingAddress
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

  private async listSavedBuyerLocations(): Promise<void> {
    const listOptions = {
      page: 1,
      pageSize: 100,
    }
    this.existingBuyerLocations = await this.context.addresses.listBuyerLocations(
      listOptions, true
    )
    this.homeCountry = this.existingBuyerLocations?.Items[0]?.Country || 'US'
    if( this.existingBuyerLocations?.Items?.length === 1) {
      this.selectedBuyerLocation = this.selectedShippingAddress = this.existingBuyerLocations.Items[0]
    }
  }

  async saveAddressesAndContinue(
    newShippingAddress: Address = null
  ): Promise<void> {
    try {
      this.spinner.show()
      if (this.isAnon) {
        await this.handleAnonShippingAddress(newShippingAddress)
      } else {
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
      this.selectedShippingAddress = await this.validateNewShippingAddress(newShippingAddress)
    } if(this.selectedShippingAddress) {
      this.context.order.checkout.setOneTimeAddress((this.selectedShippingAddress as Address), 'shipping')
      this.continue.emit()
    } else {
      // not able to create address - display suggestions to user
      this.spinner.hide()
    }
  }

  async handleLoggedInShippingAddress(newShippingAddress: Address<any>): Promise<void> {
    if (!this.selectedBuyerLocation) {
      throw new Error('Please select a location for this order')
    }
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
    const [buyerLocations, existingShippingAddresses] = await Promise.all([
      Me.ListAddresses(buyerLocationsFilter),
      HeadStartSDK.Services.ListAll(Me, Me.ListAddresses, shippingAddressesFilter)
    ])
    this.homeCountry = buyerLocations?.Items[0]?.Country || 'US'
    this.existingShippingAddresses = existingShippingAddresses
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
      return this.handleAddressError(ex);
    }
  }

  private async validateNewShippingAddress(address: BuyerAddress): Promise<HSAddressBuyer> {
    try {
      const validatedAddress = await this.context.addresses.validateAddress(address)
      return validatedAddress
    } catch (ex) {
      return this.handleAddressError(ex)
    }
  }

  private handleAddressError(ex: any): null {
    this.suggestedAddresses = getSuggestedAddresses(ex)
    if (!(this.suggestedAddresses?.length >= 1)) throw ex
    return null // set this.selectedShippingAddress
  }
}
