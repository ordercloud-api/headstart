import { Component, Input, Output, EventEmitter } from '@angular/core'
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms'
import { User } from 'ordercloud-javascript-sdk'
import { ValidateEmail } from '@app-seller/validators/validators'
import { SellerUserService } from '@app-seller/sellers/seller-admin.service'
@Component({
  selector: 'app-seller-user-edit',
  templateUrl: './seller-user-edit.component.html',
  styleUrls: ['./seller-user-edit.component.scss'],
})
export class SellerUserEditComponent {
  @Input() filterConfig
  @Input()
  set resourceInSelection(sellerUser: User) {
    this.user = sellerUser
    this.createSellerUserForm(sellerUser)
  }
  @Output()
  updateResource = new EventEmitter<any>()
  @Output()
  user: User;
  isCreatingNew: boolean
  resourceForm: UntypedFormGroup
  constructor(public sellerUserService: SellerUserService) {
    this.isCreatingNew = this.sellerUserService.checkIfCreatingNew()
  }

  createSellerUserForm(user: User) {
    this.resourceForm = new UntypedFormGroup({
      Active: new UntypedFormControl(user.Active),
      Username: new UntypedFormControl(user.Username, Validators.required),
      FirstName: new UntypedFormControl(user.FirstName, Validators.required),
      LastName: new UntypedFormControl(user.LastName, Validators.required),
      Email: new UntypedFormControl(user.Email, [Validators.required, ValidateEmail]),
    })
  }
  updateResourceFromEvent(event: any, field: string): void {
    field === 'Active'
      ? this.updateResource.emit({ value: event.target.checked, field })
      : this.updateResource.emit({ value: event.target.value, field })
  }
}
