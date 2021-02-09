import { Component } from '@angular/core'
import {
  faPhone,
  faQuestionCircle,
  faFileAlt,
} from '@fortawesome/free-solid-svg-icons'
import { ToastrService, ActiveToast } from 'ngx-toastr'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './app-footer.component.html',
  styleUrls: ['./app-footer.component.scss'],
})
export class OCMAppFooter {
  faPhone = faPhone
  faQuestionCircle = faQuestionCircle
  faFileAlt = faFileAlt

  constructor(
    private context: ShopperContextService,
    private toastrService: ToastrService
  ) {}

  toTermsAndConditions(): void {
    this.context.router.toRoute('/policy')
  }

  toSupport(): void {
    this.context.router.toRoute('/customer-service')
  }

  toFAQ(): void {
    this.context.router.toRoute('/faq')
  }

  submitClaim(): ActiveToast<any> {
    return this.toastrService.warning('Roadmap: navigate to claims page.')
  }
}
