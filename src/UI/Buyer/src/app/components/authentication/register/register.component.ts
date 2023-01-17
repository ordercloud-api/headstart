import { Component, OnInit } from '@angular/core'
import { UntypedFormGroup, Validators, UntypedFormControl } from '@angular/forms'
import { MeUser } from 'ordercloud-javascript-sdk'
import { AppConfig } from 'src/app/models/environment.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import {
  ValidateName,
  ValidateEmail,
  ValidatePhone,
  ValidateStrongPassword,
  ValidateFieldMatches,
} from '../../../validators/validators'

@Component({
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class OCMRegister implements OnInit {
  form: UntypedFormGroup
  appName: string
  loading = false

  constructor(
    private context: ShopperContextService,
    public appConfig: AppConfig
  ) { }

  // TODO: validation isn't working
  ngOnInit(): void {
    this.appName = this.context.appSettings.appname
    this.form = new UntypedFormGroup({
      Username: new UntypedFormControl('', Validators.required),
      FirstName: new UntypedFormControl('', [Validators.required, ValidateName]),
      LastName: new UntypedFormControl('', [Validators.required, ValidateName]),
      Email: new UntypedFormControl('', [Validators.required, ValidateEmail]),
      Phone: new UntypedFormControl('', ValidatePhone),
      Password: new UntypedFormControl('', [
        Validators.required,
        ValidateStrongPassword,
      ]),
      ConfirmPassword: new UntypedFormControl('', [
        Validators.required,
        ValidateFieldMatches('Password'),
      ]),
    })
  }

  async onSubmit(): Promise<void> {
    if (!this.context.appSettings.anonymousShoppingEnabled) {
      throw new Error("User registration is not enabled")
    } else {
      try {
        this.loading = true
        const me: MeUser = this.form.value
        me.Active = true
        await this.context.authentication.register(me)
        this.context.router.toHome()
      } finally {
        this.loading = false
      }

    }
  }
}
