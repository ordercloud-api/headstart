import { Component } from '@angular/core'
import { AppStateService } from '@app-seller/shared'
import { Observable } from 'rxjs'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  isLoggedIn$: Observable<boolean>

  constructor(private appStateService: AppStateService) {
    this.isLoggedIn$ = this.appStateService.isLoggedIn
  }
}
