import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core'
import { ListPage, BuyerAddress, Address } from '@ordercloud/angular-sdk'
import { ResourceCrudComponent } from '../resource-crud/resource-crud.component'

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
