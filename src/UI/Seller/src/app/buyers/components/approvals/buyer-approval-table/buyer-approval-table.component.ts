import { Component, ChangeDetectorRef, NgZone, OnInit } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerApprovalService } from '../buyer-approval.service'
import { BuyerService } from '../../buyers/buyer.service'
import { takeWhile } from 'rxjs/operators'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'
import { ApprovalRule, ListPage, UserGroup, UserGroups } from 'ordercloud-javascript-sdk'


@Component({
  selector: 'app-buyer-approval-table',
  templateUrl: './buyer-approval-table.component.html',
  styleUrls: ['./buyer-approval-table.component.scss'],
})
export class BuyerApprovalTableComponent 
  extends ResourceCrudComponent<ApprovalRule> implements OnInit {
    existingLocations: ListPage<UserGroup>
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

  async ngOnInit(): Promise<any> {
    super.ngOnInit() // call parent onInit so we don't overwrite it
    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    this.parentResourceIDSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(async (parentResourceID) => {
        if (parentResourceID && parentResourceID !== '!') {
          this.existingLocations = await HeadStartSDK.Services.ListAll(
            UserGroups, 
            UserGroups.List, 
            parentResourceID,
            {filters: {'xp.Type': 'BuyerLocation'}}
          )
        }
      })
  }
}
