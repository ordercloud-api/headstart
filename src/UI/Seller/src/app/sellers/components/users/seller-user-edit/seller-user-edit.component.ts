import { Component, Input, Output, EventEmitter } from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { UserGroupAssignment, User } from '@ordercloud/angular-sdk'
import { ValidateEmail } from '@app-seller/validators/validators'
import { SellerUserService } from '@app-seller/sellers/seller-admin.service'
@Component({
  selector: 'app-seller-user-edit',
  templateUrl: './seller-user-edit.component.html',
  styleUrls: ['./seller-user-edit.component.scss'],
})
export class SellerUserEditComponent {
  @Input()
  filterConfig
  @Input()
  set resourceInSelection(sellerUser: User) {
    this.createSellerUserForm(sellerUser)
  }
  @Output()
  updateResource = new EventEmitter<any>()
  @Output()
  isCreatingNew: boolean
  resourceForm: FormGroup
  constructor(public sellerUserService: SellerUserService) {
    this.isCreatingNew = this.sellerUserService.checkIfCreatingNew()
  }

  createSellerUserForm(user: User) {
    this.resourceForm = new FormGroup({
      Active: new FormControl(user.Active),
      Username: new FormControl(user.Username, Validators.required),
      FirstName: new FormControl(user.FirstName, Validators.required),
      LastName: new FormControl(user.LastName, Validators.required),
      Email: new FormControl(user.Email, [Validators.required, ValidateEmail]),
    })
  }
  updateResourceFromEvent(event: any, field: string): void {
    field === 'Active'
      ? this.updateResource.emit({ value: event.target.checked, field })
      : this.updateResource.emit({ value: event.target.value, field })
  }
}
