import { Component, Input, Output, EventEmitter } from '@angular/core'
import { Validators, FormBuilder } from '@angular/forms'
import { StorefrontsService } from '../storefronts.service'
import { ApiClient } from '@ordercloud/angular-sdk'
import { faQuestionCircle } from '@fortawesome/free-solid-svg-icons'
import {
  debounceTime,
  distinctUntilChanged,
  switchMap,
  tap,
  map,
  filter,
} from 'rxjs/operators'
import { from, OperatorFunction } from 'rxjs'
import { Buyer, Buyers, Users } from 'ordercloud-javascript-sdk'
import { Observable } from 'rxjs'
import { HSApiClient } from '@ordercloud/headstart-sdk'
import { conditionalValidator } from '@app-seller/shared/validators/conditional-validator'
import { TypedFormGroup } from 'ngx-forms-typed'
import { ResourceUpdate } from '@app-seller/shared'

interface StoreFrontForm {
  Active?: boolean
  AppName?: string
  AccessTokenDuration?: number
  RefreshTokenDuration?: number
  AllowAnyBuyer?: boolean
  IsAnonBuyer?: boolean
  ['xp.AnonBuyerID']?: string // using this so we can determine which users to present when selecting DefaultContextUsername
  DefaultContextUserName?: string
}
type StoreFrontFormGroup = TypedFormGroup<StoreFrontForm>
@Component({
  selector: 'app-storefront-edit',
  templateUrl: './storefront-edit.component.html',
  styleUrls: ['./storefront-edit.component.scss'],
})
export class StorefrontEditComponent {
  faQuestionCircle = faQuestionCircle
  @Input()
  filterConfig
  @Input()
  set resourceInSelection(storefront: ApiClient) {
    this.selectedResource = storefront
    this.createStorefrontForm(storefront)
  }
  @Output()
  updateResource = new EventEmitter<ResourceUpdate>()
  isCreatingNew: boolean
  resourceForm: StoreFrontFormGroup
  selectedResource: ApiClient
  searchingBuyers = false

  constructor(
    public storefrontsService: StorefrontsService,
    private formBuilder: FormBuilder
  ) {
    this.isCreatingNew = this.storefrontsService.checkIfCreatingNew()
  }

  createStorefrontForm(storefront: HSApiClient): void {
    // TODO: figure out a way for users to control whether a storefront is anonymous-enabled
    // would be a great addition but we'll need to store that information somewhere besides apiClient.xp
    // because we need to know that information very early in app startup (before authentication takes place)
    // possibly store in blob storage and update when storefront is patched
    this.resourceForm = this.formBuilder.group({
      Active: [storefront.Active, Validators.required],
      AppName: [storefront.AppName, Validators.required],
      AccessTokenDuration: [
        storefront.AccessTokenDuration,
        Validators.required,
      ],
      RefreshTokenDuration: [
        storefront.RefreshTokenDuration,
        Validators.required,
      ],
      AllowAnyBuyer: [storefront.AllowAnyBuyer, Validators.required],
      // IsAnonBuyer: [storefront.IsAnonBuyer, Validators.required],
      DefaultContextUserName: [
        storefront?.DefaultContextUserName,
        Validators.required,
        // conditionalValidator(
        //   () => this.resourceForm.controls.IsAnonBuyer.value,
        //   Validators.required
        // ),
      ],
      ['xp.AnonBuyerID']: [
        storefront?.xp?.AnonBuyerID,
        Validators.required,
        // conditionalValidator(
        //   () => this.resourceForm.controls.IsAnonBuyer.value,
        //   Validators.required
        // ),
      ],
      ['xp.WebsiteUrl']: [storefront?.xp?.WebsiteUrl],
    }) as StoreFrontFormGroup
    // this.updateResource.emit({
    //   value: true,
    //   field: 'xp.IsStorefront',
    //   form: this.resourceForm,
    // })
    // this.resourceForm.controls.IsAnonBuyer.valueChanges.subscribe(() => {
    //   // If IsAnonBuyer changes we need to re-evaluate the validity of xp.AnonBuyerID and DefaultContextUserName
    //   // as those are required only if IsAnonBuyer is true
    //   this.resourceForm.controls['xp.AnonBuyerID'].updateValueAndValidity()
    //   this.resourceForm.controls.DefaultContextUserName.updateValueAndValidity()
    // })
  }

  updateResourceFromEvent(event: Event, field: string): void {
    const input = event.target as HTMLInputElement
    field === 'Active' || field === 'IsAnonBuyer'
      ? this.updateResource.emit({
          value: input.checked,
          field,
          form: this.resourceForm,
        })
      : this.updateResource.emit({
          value: input.value,
          field,
          form: this.resourceForm,
        })
  }

  searchBuyers: OperatorFunction<string, readonly Buyer[]> = (
    text$: Observable<string>
  ): Observable<Buyer[]> => {
    return text$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => (this.searchingBuyers = true)),
      switchMap((term) => {
        // changing buyer might mean the user they've selected is no longer part of that buyer, force them to reselect user
        this.resourceForm.controls.DefaultContextUserName.setValue(undefined)
        return from(Buyers.List({ search: term, pageSize: 10 })).pipe(
          map((listResponse) => {
            return listResponse.Items
          })
        )
      })
    )
  }

  searchUsers: OperatorFunction<string, readonly string[]> = (
    text$: Observable<string>
  ): Observable<string[]> => {
    return text$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => (this.searchingBuyers = true)),
      filter(() => !!this.resourceForm.controls['xp.AnonBuyerID'].value),
      switchMap((term) => {
        return from(
          Users.List(this.resourceForm.controls['xp.AnonBuyerID'].value, {
            search: term,
            pageSize: 10,
          })
        ).pipe(
          map((listResponse) => {
            return listResponse.Items.map((user) => user.Username)
          })
        )
      })
    )
  }

  buyerName = (buyer: Buyer): string => {
    if (typeof buyer === 'string') {
      return buyer
    } else {
      return buyer.Name
    }
  }

  selectBuyer(event: { item: Buyer; preventDefault: () => void }): void {
    event.preventDefault() // default behavior saves entire buyer object to model, we just want to set the ID
    this.resourceForm.controls['xp.AnonBuyerID'].setValue(event.item.ID)
  }
}
