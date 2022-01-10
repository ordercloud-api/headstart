import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { BuyerAddress, ListPage, Address } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import { BuyerLocationService } from '../buyer-location.service'
import { BuyerService } from '../../buyers/buyer.service'

@Component({
  selector: 'app-buyer-location-table',
  templateUrl: './buyer-location-table.component.html',
  styleUrls: ['./buyer-location-table.component.scss'],
})
export class BuyerLocationTableComponent extends ResourceCrudComponent<BuyerAddress> {
  suggestedAddresses: ListPage<Address>
  selectedAddress: Address

  constructor(
    private buyerLocationService: BuyerLocationService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    private buyerService: BuyerService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      buyerLocationService,
      router,
      activatedroute,
      ngZone
    )
  }
}
