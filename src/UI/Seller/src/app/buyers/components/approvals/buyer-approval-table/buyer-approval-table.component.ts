import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { ApprovalRule } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerApprovalService } from '../buyer-approval.service'
import { BuyerService } from '../../buyers/buyer.service'

@Component({
  selector: 'app-buyer-approval-table',
  templateUrl: './buyer-approval-table.component.html',
  styleUrls: ['./buyer-approval-table.component.scss'],
})
export class BuyerApprovalTableComponent extends ResourceCrudComponent<ApprovalRule> {
  constructor(
    private buyerApprovalService: BuyerApprovalService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    private buyerService: BuyerService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      buyerApprovalService,
      router,
      activatedroute,
      ngZone
    )
  }
}
