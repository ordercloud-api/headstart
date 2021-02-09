import { Component, Input, Output, EventEmitter } from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { Router } from '@angular/router'
import { StorefrontsService } from '../storefronts.service'
import { ApiClient } from '@ordercloud/angular-sdk'
@Component({
  selector: 'app-storefront-edit',
  templateUrl: './storefront-edit.component.html',
  styleUrls: ['./storefront-edit.component.scss'],
})
export class StorefrontEditComponent {
  @Input()
  filterConfig
  @Input()
  set resourceInSelection(storefront: ApiClient) {
    this.selectedResource = storefront
    this.createStorefrontForm(storefront)
  }
  @Output()
  updateResource = new EventEmitter<any>()
  isCreatingNew: boolean
  resourceForm: FormGroup
  selectedResource: ApiClient

  constructor(public storefrontsService: StorefrontsService) {
    this.isCreatingNew = this.storefrontsService.checkIfCreatingNew()
  }

  createStorefrontForm(storefront: ApiClient) {
    this.resourceForm = new FormGroup({
      Active: new FormControl(storefront.Active),
      AppName: new FormControl(storefront.AppName, Validators.required),
      AccessTokenDuration: new FormControl(storefront.AccessTokenDuration),
      RefreshTokenDuration: new FormControl(storefront.RefreshTokenDuration),
      AllowAnyBuyer: new FormControl(storefront.AllowAnyBuyer),
    })
  }

  updateResourceFromEvent(event: any, field: string): void {
    field === 'Active'
      ? this.updateResource.emit({ value: event.target.checked, field })
      : this.updateResource.emit({ value: event.target.value, field })
  }
}
