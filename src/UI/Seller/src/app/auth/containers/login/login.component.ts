// angular
import { Component, OnInit, Inject } from '@angular/core'
import { FormBuilder, FormGroup } from '@angular/forms'
import { Router } from '@angular/router'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { ToastrService } from 'ngx-toastr'
import { AppConfig } from '@app-seller/models/environment.types'

@Component({
  selector: 'auth-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  form: FormGroup
  isAnon: boolean

  constructor(
    private currentUserService: CurrentUserService,
    private router: Router,
    private formBuilder: FormBuilder,
    private toasterService: ToastrService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  ngOnInit() {
    this.form = this.formBuilder.group({
      username: '',
      password: '',
      rememberMe: false,
    })
  }

  async onSubmit() {
    try {
      await this.currentUserService.login(
        this.form.get('username').value,
        this.form.get('password').value,
        this.form.get('rememberMe').value
      )
    } catch {
      this.toasterService.error('Invalid Login Credentials')
    }
    this.router.navigateByUrl('/home')
  }
}
