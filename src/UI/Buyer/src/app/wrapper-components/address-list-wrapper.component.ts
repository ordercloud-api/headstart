import { Component, OnInit } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { ListPage, Address } from '@ordercloud/headstart-sdk'

@Component({
  template: `<ocm-address-list [addresses]="addresses"></ocm-address-list> `,
})
export class AddressListWrapperComponent implements OnInit {
  addresses: ListPage<Address>

  constructor(private activatedRoute: ActivatedRoute) {}

  ngOnInit(): void {
    this.addresses = this.activatedRoute.snapshot.data.addresses
  }
}
