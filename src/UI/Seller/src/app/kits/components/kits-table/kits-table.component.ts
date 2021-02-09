import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { KitService } from '@app-seller/kits/kits.service'
import { HSKitProduct } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'app-kits-table',
  templateUrl: './kits-table.component.html',
  styleUrls: ['./kits-table.component.scss'],
})
export class KitsTableComponent extends ResourceCrudComponent<HSKitProduct> {
  constructor(
    kitService: KitService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, kitService, router, activatedRoute, ngZone)
  }
}
