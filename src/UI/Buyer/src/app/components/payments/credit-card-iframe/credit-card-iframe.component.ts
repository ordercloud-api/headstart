import { Component, EventEmitter, OnInit, Output } from '@angular/core'
import { DomSanitizer, SafeUrl } from '@angular/platform-browser'
import { AppConfig } from 'src/app/models/environment.types'

@Component({
  templateUrl: './credit-card-iframe.component.html',
  styleUrls: ['./credit-card-iframe.component.scss'],
})
export class OCMCreditCardIframe implements OnInit {
  @Output() ccEntered = new EventEmitter()

  creditCardIframeUrl: SafeUrl

  constructor(private appConfig: AppConfig, private sanitizer: DomSanitizer) {}

  ngOnInit(): void {
    this.creditCardIframeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
      `${this.appConfig.creditCardIframeUrl}?invalidinputevent=true&css=.error%7Bcolor%3Ared%3Bborder-color%3Ared%3B%7D`
    )

    // eslint-disable-next-line @typescript-eslint/no-this-alias
    const component = this
    window.addEventListener(
      'message',
      function (event: any) {
        const response = JSON.parse(event.data)
        component.ccEntered.emit(response)
      },
      false
    )
  }
}
