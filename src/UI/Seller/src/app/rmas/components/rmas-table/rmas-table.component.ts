import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { RMAService } from '@app-seller/rmas/rmas.service'
import { RMA } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'app-rmas-table',
  templateUrl: './rmas-table.component.html',
  styleUrls: ['./rmas-table.component.scss'],
})
export class RMATableComponent extends ResourceCrudComponent<RMA> {
  continuationToken: string
  constructor(
    rmaService: RMAService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, rmaService, router, activatedRoute, ngZone)
  }

  async handleScrollEnd() {
    if ((this.resourceList?.Meta as any)?.ContinuationToken) {
      const continuationToken = [
        {
          ContinuationToken: `${
            (this.resourceList?.Meta as any)?.ContinuationToken
          }`,
        },
      ]
      const nextRMARecords = await this.ocService.list(continuationToken)
      this.resourceList = {
        Meta: nextRMARecords?.Meta,
        Items: [...this.resourceList.Items, ...nextRMARecords.Items],
      }
    }
  }

  filterConfig = {
    Filters: [
      {
        Display: 'ADMIN.FILTERS.TYPE',
        Path: 'Type',
        Items: [
          { Text: 'ADMIN.RMAS.CANCELLATION', Value: 'Cancellation' },
          { Text: 'ADMIN.RMAS.RETURN', Value: 'Return' },
        ],
        Type: 'Dropdown',
      },
      {
        Display: 'ADMIN.FILTERS.STATUS',
        Path: 'Status',
        Items: [
          { Text: 'ADMIN.RMAS.REQUESTED', Value: 'Requested' },
          { Text: 'ADMIN.RMAS.PROCESSING', Value: 'Processing' },
          { Text: 'ADMIN.RMAS.APPROVED', Value: 'Approved' },
          { Text: 'ADMIN.RMAS.COMPLETE', Value: 'Complete' },
          { Text: 'ADMIN.RMAS.DENIED', Value: 'Denied' },
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
