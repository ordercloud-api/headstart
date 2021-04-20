// angular
import { Component, OnInit, Inject } from '@angular/core'
import { FormBuilder, FormGroup } from '@angular/forms'
import { Router } from '@angular/router'

// angular libs
import { ToastrService } from 'ngx-toastr'

// ordercloud
import { ForgottenPassword } from 'ordercloud-javascript-sdk'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig } from '@app-seller/models/environment.types'

@Component({
  selector: 'auth-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
})
export class ForgotPasswordComponent implements OnInit {
  resetEmailForm: FormGroup

  constructor(
    private router: Router,
    private formBuilder: FormBuilder,
    private toasterService: ToastrService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    this.resetEmailForm = this.formBuilder.group({ email: '', username: '' })
  }

  async onSubmit(): Promise<void> {
    const email = (this.resetEmailForm.get('email').value as string) || null
    const username =
      (this.resetEmailForm.get('username').value as string) || null
    await ForgottenPassword.SendVerificationCode({
      ClientID: this.appConfig.clientID,
      Email: username ? null : email, // if you provide both username and email to the API you get no results
      Username: username,
      URL: window.location.origin,
    })
    this.toasterService.success('Password Reset Email Sent!')
    void this.router.navigateByUrl('/login')
  }
}
