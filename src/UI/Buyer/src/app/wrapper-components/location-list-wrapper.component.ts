import { Component, OnInit } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { BuyerLocation } from '../models/buyer.types'

@Component({
  template: ` <ocm-location-list [locations]="locations"></ocm-location-list> `,
})
export class LocationListWrapperComponent implements OnInit {
  locations: BuyerLocation[]

  constructor(private activatedRoute: ActivatedRoute) {}

  ngOnInit(): void {
    this.locations = this.activatedRoute.snapshot.data.locations
  }
}
