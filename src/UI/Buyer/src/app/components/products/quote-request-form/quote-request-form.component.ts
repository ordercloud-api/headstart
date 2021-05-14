import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import {
  ValidateEmail,
  ValidateName,
  ValidatePhone,
} from 'src/app/validators/validators'
import {
  QuoteOrderInfo,
  HSAddressBuyer,
} from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { CurrentUser } from 'src/app/models/profile.types'

@Component({
  templateUrl: './quote-request-form.component.html',
  styleUrls: ['./quote-request-form.component.scss'],
})
export class OCMQuoteRequestForm implements OnInit {
  quoteRequestForm: FormGroup
  requestOptions: {
    pageSize?: number
  } = {
    pageSize: 100,
  }
  myBuyerLocations: HSAddressBuyer[]
  isAnon: boolean
  //todo revert type to QuoteOrderInfo
  @Output() formSubmitted = new EventEmitter<{ user: any }>()
  @Output() formDismissed = new EventEmitter()

  private currentUser: CurrentUser
  constructor(private context: ShopperContextService) {}

  async ngOnInit(): Promise<void> {
    this.isAnon = this.context.currentUser.isAnonymous()
    await this.getMyBuyerLocations()
    this.setForms()
  }

  @Input() set CurrentUser(user: CurrentUser) {
    this.currentUser = user
  }

  async getMyBuyerLocations(): Promise<void> {
    const addresses = await this.context.addresses.list(this.requestOptions)
    this.myBuyerLocations = addresses.Items
  }

  onLocationChange(value: string) {
    const selectedIndex = parseInt(value.substring(0,1))
    this.quoteRequestForm.patchValue({"ShippingAddressId": this.myBuyerLocations[selectedIndex].ID})
  }

  setForms(): void {
    this.quoteRequestForm = new FormGroup({
      FirstName: new FormControl(this.getAttribute('FirstName'), [
        Validators.required,
        ValidateName,
      ]),
      LastName: new FormControl(this.getAttribute('LastName'), [
        Validators.required,
        ValidateName,
      ]),
      BuyerLocation: new FormControl(
        this.myBuyerLocations[0]?.AddressName || '',
        !this.isAnon ? [Validators.required] : null
      ),
      Phone: new FormControl(this.getAttribute('Phone') || '', [
        Validators.required,
        ValidatePhone,
      ]),
      Email: new FormControl(this.getAttribute('Email') || '', [
        Validators.required,
        ValidateEmail,
      ]),
      Comments: new FormControl(''),
      ShippingAddressId: new FormControl(this.myBuyerLocations[0]?.ID || '')
    })
  }

  getAttribute(key: string): string {
    if(this.isAnon || !this.currentUser) {
      return ''
    } else {
      return this.currentUser[key] || ''
    }
  }

  onSubmit(): void {
    if (this.quoteRequestForm.status === 'INVALID') return
    this.formSubmitted.emit({
      user: this.quoteRequestForm.value,
    })
  }

  dismissForm(): void {
    this.formDismissed.emit()
  }
}
