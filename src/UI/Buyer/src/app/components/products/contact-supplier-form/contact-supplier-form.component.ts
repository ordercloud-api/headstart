import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import {
  ValidateEmail,
  ValidateName,
  ValidatePhone,
} from 'src/app/validators/validators'
import { HSAddressBuyer } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { CurrentUser } from 'src/app/models/profile.types'

@Component({
  templateUrl: './contact-supplier-form.component.html',
  styleUrls: ['./contact-supplier-form.component.scss'],
})
export class OCMContactSupplierForm implements OnInit {
  contactSupplierForm: FormGroup
  myBuyerLocations: HSAddressBuyer[]
  requestOptions: {
    pageSize?: number
  } = {
    pageSize: 100,
  }
  @Output() contactFormSubmitted = new EventEmitter<{ formData: any }>()
  @Output() contactFormDismissed = new EventEmitter()

  private currentUser: CurrentUser
  constructor(private context: ShopperContextService) {}

  async ngOnInit(): Promise<void> {
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

  setForms(): void {
    this.contactSupplierForm = new FormGroup({
      FirstName: new FormControl(this.currentUser?.FirstName || '', [
        Validators.required,
        ValidateName,
      ]),
      LastName: new FormControl(this.currentUser?.LastName || '', [
        Validators.required,
        ValidateName,
      ]),
      BuyerLocation: new FormControl(
        this.myBuyerLocations[0]?.AddressName || '',
        [Validators.required]
      ),
      Email: new FormControl(this.currentUser?.Email || '', [
        Validators.required,
        ValidateEmail,
      ]),
      Phone: new FormControl(this.currentUser?.Phone || '', [
        Validators.required,
        ValidatePhone,
      ]),
      Comments: new FormControl('', Validators.required),
    })
  }

  onSubmit(): void {
    if (this.contactSupplierForm.status === 'INVALID') return
    this.contactFormSubmitted.emit({
      formData: this.contactSupplierForm.value,
    })
  }

  dismissForm(): void {
    this.contactFormDismissed.emit()
  }
}
