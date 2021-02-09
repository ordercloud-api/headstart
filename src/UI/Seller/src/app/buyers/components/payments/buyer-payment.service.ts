import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerCreditCard, OcCreditCardService } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { BUYER_SUB_RESOURCE_LIST } from '../buyers/buyer.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { CreditCards } from 'ordercloud-javascript-sdk'

// TODO - this service is only relevent if you're already on the supplier details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class BuyerPaymentService extends ResourceCrudService<BuyerCreditCard> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      CreditCards,
      currentUserService,
      '/buyers',
      'buyers',
      BUYER_SUB_RESOURCE_LIST,
      'payments'
    )
  }
}
