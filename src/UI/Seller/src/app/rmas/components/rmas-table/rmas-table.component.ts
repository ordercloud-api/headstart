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
        Display: 'Type',
        Path: 'Type',
        Items: [
          { Text: 'Cancellation', Value: 'Cancellation' },
          { Text: 'Return', Value: 'Return' },
        ],
        Type: 'Dropdown',
      },
      {
        Display: 'Status',
        Path: 'Status',
        Items: [
          { Text: 'Requested', Value: 'Requested' },
          { Text: 'Processing', Value: 'Processing' },
          { Text: 'Approved', Value: 'Approved' },
          { Text: 'Complete', Value: 'Complete' },
          { Text: 'Denied', Value: 'Denied' },
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
