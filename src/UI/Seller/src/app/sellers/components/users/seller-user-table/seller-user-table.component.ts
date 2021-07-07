import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { User } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { SellerUserService } from '@app-seller/sellers/seller-admin.service'
@Component({
  selector: 'app-seller-user-table',
  templateUrl: './seller-user-table.component.html',
  styleUrls: ['./seller-user-table.component.scss'],
})
export class SellerUserTableComponent extends ResourceCrudComponent<User> {
  constructor(
    sellerUserService: SellerUserService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, sellerUserService, router, activatedroute, ngZone)
  }
}
