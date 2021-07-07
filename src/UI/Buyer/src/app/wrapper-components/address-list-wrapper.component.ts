import { Component, OnInit } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { ListPage } from '@ordercloud/headstart-sdk'
import { Address } from 'ordercloud-javascript-sdk'

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
