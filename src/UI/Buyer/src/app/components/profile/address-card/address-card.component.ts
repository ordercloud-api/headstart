import { Component, Input, ViewEncapsulation } from '@angular/core'
import { Address } from 'ordercloud-javascript-sdk'

@Component({
  templateUrl: './address-card.component.html',
  styleUrls: ['./address-card.component.scss'],
  encapsulation: ViewEncapsulation.ShadowDom,
})
export class OCMAddressCard {
  @Input() address = {} as Address
  @Input() highlight?: boolean

  // make into pipe?
  getFullName(address: Address): string {
    const fullName = `${address?.FirstName || ''} ${address?.LastName || ''}`
    return fullName.trim()
  }
}
