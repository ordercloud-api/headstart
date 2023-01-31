import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { HSOrderReturn } from '@ordercloud/headstart-sdk'
import { OrderReturns } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class OrderReturnService extends ResourceCrudService<HSOrderReturn> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      OrderReturns,
      currentUserService,
      '/order-returns',
      'orderreturns'
    )
  }
}
