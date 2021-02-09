import { Component, ChangeDetectorRef, NgZone, OnInit } from '@angular/core'
import { ProductFacet } from '@ordercloud/angular-sdk'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { FacetService } from '@app-seller/facets/facet.service'
import { FormGroup, FormControl, Validators } from '@angular/forms'

@Component({
  selector: 'app-facet-table',
  templateUrl: './facet-table.component.html',
  styleUrls: ['./facet-table.component.scss'],
})
export class FacetTableComponent
  extends ResourceCrudComponent<ProductFacet>
  implements OnInit {
  constructor(
    facetService: FacetService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, facetService, router, activatedRoute, ngZone)
  }

  // static filters that should apply to all headstart orgs, custom filters for specific applications can be
  // added to the filterconfig passed into the resourcetable in the future
  filterConfig = {
    Filters: [],
  }

  // Overwrite create resource function to add XpPath
  async createNewResource(): Promise<void> {
    // dataIsSaving indicator is used in the resource table to conditionally tell the
    // submit button to disable
    // Add XpPath from Facet.Name
    this.updatedResource.XpPath = `Facets.${this.updatedResource.ID}`
    try {
      this.dataIsSaving = true
      const newResource = await this.ocService.createNewResource(
        this.updatedResource
      )
      this.selectResource(newResource)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }
}
