import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { OrderReturnService } from '@app-seller/order-returns/order-returns.service'
import { HSOrderReturn } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'order-return-table',
  templateUrl: './order-return-table.component.html',
  styleUrls: ['./order-return-table.component.scss'],
})
export class OrderReturnTableComponent extends ResourceCrudComponent<HSOrderReturn> {
  continuationToken: string
  constructor(
    orderReturnService: OrderReturnService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, orderReturnService, router, activatedRoute, ngZone)
  }

  filterConfig = {
    Filters: [
      {
        Display: 'ADMIN.FILTERS.STATUS',
        Path: 'Status',
        Items: [
          {
            Text: 'ADMIN.ORDER_RETURNS.STATUSES.AWAITING_APPROVAL',
            Value: 'AwaitingApproval',
          },
          { Text: 'ADMIN.ORDER_RETURNS.STATUSES.OPEN', Value: 'Open' },
          {
            Text: 'ADMIN.ORDER_RETURNS.STATUSES.COMPLETED',
            Value: 'Completed',
          },
          { Text: 'ADMIN.ORDER_RETURNS.STATUSES.CANCELED', Value: 'Canceled' },
        ],
        Type: 'Dropdown',
      },
      {
        Display: 'ADMIN.FILTERS.FROM_DATE',
        Path: 'from',
        Type: 'DateFilter',
      },
      {
        Display: 'ADMIN.FILTERS.TO_DATE',
        Path: 'to',
        Type: 'DateFilter',
      },
    ],
  }
}
