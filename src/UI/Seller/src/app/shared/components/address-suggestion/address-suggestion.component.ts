import { Component, EventEmitter, Input, Output } from '@angular/core'
import { ListPage, BuyerAddress } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'address-suggestion',
  templateUrl: './address-suggestion.component.html',
  styleUrls: ['./address-suggestion.component.scss'],
})
export class AddressSuggestionComponent {
  @Input()
  suggestedAddresses: ListPage<BuyerAddress>
  @Output()
  selectedAddress = new EventEmitter<BuyerAddress>()
  activeAddress: BuyerAddress
  constructor() {}

  setActiveAddress(address) {
    this.activeAddress = address
    this.selectedAddress.emit(address)
  }
}
