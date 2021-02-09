import { Component } from '@angular/core'
import { ActivatedRoute } from '@angular/router'

@Component({
  template: `
    <ocm-location-management
      [locationID]="locationID"
    ></ocm-location-management>
  `,
})
export class LocationManagementWrapperComponent {
  locationID: string
  constructor(private activatedRoute: ActivatedRoute) {
    this.locationID = this.activatedRoute.snapshot.params.locationID
  }
}
