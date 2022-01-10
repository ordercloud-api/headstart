import { Component, Input } from '@angular/core'
import { Address } from 'ordercloud-javascript-sdk'
import { faDownload } from '@fortawesome/free-solid-svg-icons'

import {
  HSAddressBuyer,
} from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { AppConfig } from 'src/app/models/environment.types'

@Component({
  templateUrl: './location-management.component.html',
  styleUrls: ['./location-management.component.scss'],
})
export class OCMLocationManagement {
  faDownload = faDownload
  address: HSAddressBuyer = {}
  userCanAdminPermissions = false
  userCanViewLocationOrders = false
  _locationID = ''
  @Input() set locationID(locationID: string) {
    this._locationID = locationID
    this.userCanAdminPermissions = this.context.currentUser.hasLocationAccess(
      this._locationID,
      'PermissionAdmin'
    )
    this.userCanViewLocationOrders = this.context.currentUser.hasLocationAccess(
      this._locationID,
      'ViewAllOrders'
    )
    this.getLocationManagementDetails()
  }

  constructor(
    private context: ShopperContextService,
    private appConfig: AppConfig
  ) {}

  toLocationOrders(): void {
    this.context.router.toOrdersByLocation({ location: this._locationID })
  }

  async getLocationManagementDetails(): Promise<void> {
    this.address = await this.context.addresses.get(this._locationID)
  }

  // make into pipe?
  getFullName(address: Address): string {
    const fullName = `${address?.FirstName || ''} ${address?.LastName || ''}`
    return fullName.trim()
  }
}
