// angular
import { Component, OnInit } from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { AppConfig } from 'src/app/models/environment.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
})
export class OCMForgotPassword implements OnInit {
  form: FormGroup
  appName: string

  constructor(
    private context: ShopperContextService,
    public appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    this.appName = this.context.appSettings.appname
    this.form = new FormGroup({ email: new FormControl('') })
  }

  async onSubmit(): Promise<void> {
    const email = this.form.get('email').value
    await this.context.authentication.forgotPasssword(email)
  }
}
