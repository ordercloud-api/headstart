// angular
import { Component, OnInit } from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { Router } from '@angular/router'
import { ToastrService } from 'ngx-toastr'
import { ForgottenPassword } from 'ordercloud-javascript-sdk'
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
    private router: Router,
    public appConfig: AppConfig,
    private toastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this.appName = this.context.appSettings.appname
    this.form = new FormGroup({
      email: new FormControl(''),
      username: new FormControl(''),
    })
  }

  async onSubmit(): Promise<void> {
    const username = (this.form.get('username').value as string) || null
    const email = (this.form.get('email').value as string) || null
    await ForgottenPassword.SendVerificationCode({
      Email: username ? null : email, // if you provide both username and email to the API you get no results
      Username: username,
      ClientID: this.appConfig.clientID,
      URL: window.location.origin,
    })
    this.toastrService.success('Password Reset Email Sent!')
    void this.router.navigateByUrl('/login')
  }
}
