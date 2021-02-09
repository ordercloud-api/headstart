// angular
import { Component, OnInit } from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
// angular libs

// ordercloud
import { PasswordReset, TokenPasswordReset } from 'ordercloud-javascript-sdk'
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
  token: string
  appName: string

  constructor(
    private toasterService: ToastrService,
    private context: ShopperContextService,
    private activatedRoute: ActivatedRoute,
    public appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    // TODO - figure out how to access url.
    const urlParams = this.activatedRoute.snapshot.queryParams
    this.token = urlParams['token']
    // this.username = urlParams['user'];
    this.appName = this.context.appSettings.appname
    this.form = new FormGroup({
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

    const config: TokenPasswordReset = {
      NewPassword: this.form.get('password').value,
    }
    await this.context.authentication.resetPassword(this.token, config)
    this.toasterService.success('Password Reset', 'Success')
    this.context.router.toLogin()
  }
}
