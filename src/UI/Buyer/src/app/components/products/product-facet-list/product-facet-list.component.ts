import { Component, Input } from '@angular/core'
import { ListFacet } from 'ordercloud-javascript-sdk'

@Component({
  templateUrl: './product-facet-list.component.html',
  styleUrls: ['./product-facet-list.component.scss'],
})
export class OCMProductFacetList {
  @Input() facetList: ListFacet[]
}
