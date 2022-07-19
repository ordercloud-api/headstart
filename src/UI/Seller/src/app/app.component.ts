import { Component, OnInit } from '@angular/core'
import { AppStateService } from '@app-seller/shared'
import axios, { AxiosError, AxiosResponse } from 'axios'
import { Observable } from 'rxjs'
import { AppAuthService } from './auth/services/app-auth.service'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  isLoggedIn$: Observable<boolean>

  constructor(
    private appStateService: AppStateService,
    private appAuthService: AppAuthService
  ) {
    this.isLoggedIn$ = this.appStateService.isLoggedIn
  }

  ngOnInit(): void {
    this.useAxiosInterceptors()
  }

  useAxiosInterceptors(): void {
    // axios interceptors allow us to do something before or after a request or response
    // https://github.com/ordercloud-api/ordercloud-javascript-sdk#interceptors
    // here we're logging the user out if the response is forbidden or unauthorized

    axios.interceptors.response.use(
      (response) => response,
      (error: AxiosError) => {
        if (
          error?.response?.status &&
          (error.response.status === 401 || error.response.status === 403)
        ) {
          this.appAuthService.logout()
        }
        return Promise.reject(error)
      }
    )
  }
}
