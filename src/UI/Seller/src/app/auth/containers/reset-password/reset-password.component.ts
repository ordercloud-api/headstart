import { ForgottenPassword } from 'ordercloud-javascript-sdk'
// angular
import { Component, Inject, OnInit } from '@angular/core'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { Router, ActivatedRoute } from '@angular/router'

// angular libs
import { ToastrService } from 'ngx-toastr'

// ordercloud
import { AppConfig, AppFormErrorService } from '@app-seller/shared'

import {
  ValidateFieldMatches,
  ValidateStrongPassword,
} from '@app-seller/validators/validators'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { TypedFormGroup } from 'ngx-forms-typed'

interface ResetPasswordForm {
  username: string
  password: string
  passwordConfirm: string
}

@Component({
  selector: 'auth-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: TypedFormGroup<ResetPasswordForm>
  username: string
  token: string

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private toasterService: ToastrService,
    private formErrorService: AppFormErrorService,
    @Inject(applicationConfiguration) protected appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    this.resetPasswordForm = new FormGroup({
      username: new FormControl(
        this.activatedRoute.snapshot.queryParams.username as string
      ),
      password: new FormControl('', [
        Validators.required,
        ValidateStrongPassword,
      ]),
      passwordConfirm: new FormControl('', [
        Validators.required,
        ValidateFieldMatches('password'),
      ]),
    }) as TypedFormGroup<ResetPasswordForm>
  }

  async onSubmit(): Promise<void> {
    if (this.resetPasswordForm.status === 'INVALID') {
      return
    }
    try {
      await ForgottenPassword.ResetPasswordByVerificationCode(
        this.activatedRoute.snapshot.queryParams.code as string,
        {
          ClientID: this.appConfig.clientID,
          Username: this.activatedRoute.snapshot.queryParams.username as string,
          Password: this.resetPasswordForm?.get('password')?.value as string,
        }
      )
      this.toasterService.success('Password Reset Successfully')
      void this.router.navigateByUrl('/login')
    } catch {
      this.toasterService.error('Unable to reset password')
    }
  }

  // control visibility of password mismatch error
  protected passwordMismatchError(): boolean {
    return this.formErrorService.hasPasswordMismatchError(
      this.resetPasswordForm.controls.passwordConfirm
    )
  }
}
