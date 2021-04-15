import { Component, Input, Output, EventEmitter } from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { UserGroupAssignment, User } from '@ordercloud/angular-sdk'
import { BuyerUserService } from '../buyer-user.service'
import { ValidateEmail } from '@app-seller/validators/validators'
import { GeographyConfig } from '@app-seller/shared/models/supported-countries.constant'
import { AppFormErrorService, SupportedCountries } from '@app-seller/shared'
import { UserGroupTypes } from '@app-seller/shared/components/user-group-assignments/user-group-assignments.constants'
@Component({
  selector: 'app-buyer-user-edit',
  templateUrl: './buyer-user-edit.component.html',
  styleUrls: ['./buyer-user-edit.component.scss'],
})
export class BuyerUserEditComponent {
  @Input()
  filterConfig
  @Input()
  set resourceInSelection(buyerUser: User) {
    this.selectedResource = buyerUser
    this.createBuyerUserForm(buyerUser)
  }
  @Output()
  updateResource = new EventEmitter<FormGroup>()
  @Output()
  userGroupAssignments = new EventEmitter<UserGroupAssignment[]>()
  isCreatingNew: boolean
  resourceForm: FormGroup
  selectedResource: User
  countryOptions: SupportedCountries[]
  isUserAssignedToGroups: boolean
  constructor(
    public buyerUserService: BuyerUserService,
    public appFormErrorService: AppFormErrorService
  ) {
    this.isCreatingNew = this.buyerUserService.checkIfCreatingNew()
    this.countryOptions = GeographyConfig.getCountries()
  }

  createBuyerUserForm(user: User) {
    this.resourceForm = new FormGroup({
      Active: new FormControl(user.Active || false),
      Username: new FormControl(user.Username, Validators.required),
      FirstName: new FormControl(user.FirstName, Validators.required),
      LastName: new FormControl(user.LastName, Validators.required),
      Email: new FormControl(user.Email, [Validators.required, ValidateEmail]),
      Country: new FormControl(user.xp?.Country, Validators.required),
      BuyerGroupAssignments: new FormControl(null, this.isCreatingNew ? [Validators.required] : null),
      PermissionGroupAssignments: new FormControl()
    });
  }

  toggleActive(event: Event) {
    this.resourceForm.controls['Active'].setValue((event.target as HTMLInputElement).checked)
    this.updateResourceFromEvent()
  }

  updateResourceFromEvent(): void {
    this.updateResource.emit(this.resourceForm)
  }

  addUserGroupAssignments(event): void {
    if (event.UserGroupType === UserGroupTypes.BuyerLocation) {
      this.resourceForm.controls["BuyerGroupAssignments"]?.setValue(event.Assignments)
    } else if (event.UserGroupType === UserGroupTypes.UserPermissions) {
      this.resourceForm.controls["PermissionGroupAssignments"]?.setValue(event.Assignments)
    }
    this.updateResource.emit(this.resourceForm)
  }

  userHasAssignments(event: boolean): void {
    this.isUserAssignedToGroups = event
    if (event && !this.isCreatingNew) {
      this.resourceForm.controls.Country.disable()
    }
    if (!event && !this.isCreatingNew) {
      this.resourceForm.controls.Country.enable()
    }
  }

  hasValidEmailError = (): boolean =>
    this.appFormErrorService.hasInvalidIdError(this.resourceForm.get('Email'))
}
