import { Component, Input } from '@angular/core'
import { Address } from 'ordercloud-javascript-sdk'
import { faDownload } from '@fortawesome/free-solid-svg-icons'

import {
  HSAddressBuyer,
  TaxCertificate,
} from '@ordercloud/headstart-sdk'
import { GeographyConfig } from '../../../config/geography.class'
import { downloadBase64String } from 'src/app/services/download.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { AppConfig } from 'src/app/models/environment.types'
import { CertificateFormOutput } from 'src/app/models/profile.types'

@Component({
  templateUrl: './location-management.component.html',
  styleUrls: ['./location-management.component.scss'],
})
export class OCMLocationManagement {
  faDownload = faDownload
  address: HSAddressBuyer = {}
  certificate: TaxCertificate = {}
  showCertificateForm = false
  userCanAdminResaleCert = false
  userCanAdminPermissions = false
  userCanViewLocationOrders = false
  _locationID = ''
  @Input() set locationID(locationID: string) {
    this._locationID = locationID
    this.userCanAdminResaleCert = this.context.currentUser.hasLocationAccess(
      this._locationID,
      'ResaleCertAdmin'
    )
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
    if (this.userCanAdminResaleCert) {
      this.certificate = await this.context.addresses.getCertificate(
        this._locationID
      )
    }
  }

  // make into pipe?
  getFullName(address: Address): string {
    const fullName = `${address?.FirstName || ''} ${address?.LastName || ''}`
    return fullName.trim()
  }

  async saveCertificate(formOutput: CertificateFormOutput): Promise<void> {
    // TODO - how do tax certs work in Canada?
    const state = GeographyConfig.getStates('US').find(
      (s) => this.address.State === s.abbreviation
    )
    const certificate: TaxCertificate = {
      ExpirationDate: formOutput.ExpirationDate,
      SignedDate: formOutput.SignedDate,
      Base64UrlEncodedPDF: formOutput.Base64UrlEncodedPDF,
      ExposureZoneName: state.label,
      ExemptionNumber: this.address.ID,
    }
    this.certificate = await this.context.addresses.createCertificate(
      this.address.ID,
      certificate
    )
    this.dismissCertificateForm()
  }

  downloadCertificate(): void {
    downloadBase64String(
      this.certificate.Base64UrlEncodedPDF,
      'application/pdf',
      this.certificate.FileName
    )
  }

  dismissCertificateForm(): void {
    this.showCertificateForm = false
  }

  revealCertificateForm(): void {
    this.showCertificateForm = true
  }

  hasCertificate(): boolean {
    return this.certificate.FileName !== null && this.certificate.FileName !== undefined
  }
}
