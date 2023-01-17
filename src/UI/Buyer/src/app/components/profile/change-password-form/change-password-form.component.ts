import { Component, OnInit } from '@angular/core'
import { faTimes } from '@fortawesome/free-solid-svg-icons'
import { UntypedFormGroup, Validators, UntypedFormControl } from '@angular/forms'
import { MeUser } from 'ordercloud-javascript-sdk'
import {
  ValidateStrongPassword,
  ValidateFieldMatches,
} from '../../../validators/validators'
import { ToastrService } from 'ngx-toastr'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './change-password-form.component.html',
  styleUrls: ['./change-password-form.component.scss'],
})
export class OCMChangePasswordForm implements OnInit {
  form: UntypedFormGroup
  me: MeUser
  faTimes = faTimes

  constructor(
    private toasterService: ToastrService,
    private context: ShopperContextService
  ) {}

  ngOnInit(): void {
    this.setForm()
  }

  setForm(): void {
    this.form = new UntypedFormGroup({
      currentPassword: new UntypedFormControl('', Validators.required),
      newPassword: new UntypedFormControl('', [
        Validators.required,
        ValidateStrongPassword,
      ]),
      confirmNewPassword: new UntypedFormControl('', [
        ValidateFieldMatches('newPassword'),
        Validators.required,
      ]),
    })
  }

  resetFormValues(): void {
    this.form.controls.currentPassword.setValue('')
    this.form.controls.newPassword.setValue('')
    this.form.controls.confirmNewPassword.setValue('')
  }

  async updatePassword(): Promise<void> {
    const { newPassword, currentPassword } = this.form.value
    await this.context.authentication.validateCurrentPasswordAndChangePassword(
      newPassword,
      currentPassword
    )
    this.toasterService.success('Password Changed')
    this.resetFormValues()
  }
}
