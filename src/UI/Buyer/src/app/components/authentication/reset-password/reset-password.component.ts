// angular
import { Component, OnInit } from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
// angular libs

// ordercloud
import { ForgottenPassword, PasswordReset } from 'ordercloud-javascript-sdk'
import {
  ValidateStrongPassword,
  ValidateFieldMatches,
} from '../../../validators/validators'
import { ToastrService } from 'ngx-toastr'
import { ActivatedRoute } from '@angular/router'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { AppConfig } from 'src/app/models/environment.types'

@Component({
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
})
export class OCMResetPassword implements OnInit {
  form: FormGroup
  username: string
  code: string
  appName: string

  constructor(
    private toasterService: ToastrService,
    private context: ShopperContextService,
    private activatedRoute: ActivatedRoute,
    public appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    const urlParams = this.activatedRoute.snapshot.queryParams
    this.code = urlParams['code'] as string
    this.username = urlParams['username'] as string
    this.appName = this.context.appSettings.appname
    this.form = new FormGroup({
      username: new FormControl(this.username),
      password: new FormControl('', [
        Validators.required,
        ValidateStrongPassword,
      ]),
      passwordConfirm: new FormControl('', [
        Validators.required,
        ValidateFieldMatches('password'),
      ]),
    })
  }

  async onSubmit(): Promise<void> {
    if (this.form.status === 'INVALID') {
      return
    }
    const password = this.form.get('password').value as string
    const config: PasswordReset = {
      ClientID: this.appConfig.clientID,
      Username: this.username,
      Password: password,
    }
    await ForgottenPassword.ResetPasswordByVerificationCode(this.code, config)
    this.toasterService.success('Password Reset', 'Success')
    this.context.router.toLogin()
  }
}
