import { Component, Input, Output, EventEmitter, OnDestroy, OnInit } from '@angular/core'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { HSBuyer } from '@ordercloud/headstart-sdk'
import { BuyerService } from '../buyer.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { Router } from '@angular/router'
import { isEqual as _isEqual } from 'lodash'
import { HSBuyerData, HSBuyerPriceMarkup } from '@app-seller/models/buyer.types'
import { OcAddressService, OcImpersonationConfigService } from '@ordercloud/angular-sdk'
import { Subscription } from 'rxjs'
import { ResourceFormUpdate } from '@app-seller/shared'
import { CatalogsTempService } from '@app-seller/shared/services/middleware-api/catalogs-temp.service'
@Component({
  selector: 'app-buyer-edit',
  templateUrl: './buyer-edit.component.html',
  styleUrls: ['./buyer-edit.component.scss'],
})
export class BuyerEditComponent implements OnDestroy {
  resourceForm: FormGroup
  showImpersonation: boolean
  isCreatingNew = false
  areChanges = false
  dataIsSaving = false
  impersonationSubscription: Subscription
  helperMessage: string
  helperAction: string
  helperLink: string

  _superBuyerStatic: HSBuyerPriceMarkup
  _superBuyerEditable: HSBuyerPriceMarkup

  @Input()
  labelSingular: string
  @Input()
  set orderCloudBuyer(buyer: HSBuyer) {
    if (buyer.ID) {
      this.getBuyerData(buyer.ID)
      this.handleSelectedBuyerChange(buyer)
    } else {
      this.refreshBuyerData(this.buyerService.emptyResource)
    }
  }

  @Output()
  resourceDelete = new EventEmitter<any>()

  constructor(
    private buyerService: BuyerService,
    private ocImpersonationService: OcImpersonationConfigService,
    private router: Router,
    private buyerTempService: BuyerTempService,
    private appAuthService: AppAuthService,
    private hsCatalogService: CatalogsTempService,
    private addressService: OcAddressService
  ) { }

  async getBuyerData(buyerID: string): Promise<void> {
    const [catalogs, addresses] = await Promise.all([
      this.hsCatalogService.list(buyerID), 
    this.addressService.List(buyerID).toPromise()])
    if(!catalogs?.Items || catalogs.Items?.length === 0) {
      this.helperMessage = "Looks like this Buyer has no Catalogs."
      this.helperAction = "Set Catalogs now"
      this.helperLink = `/buyers/${buyerID}/catalogs/new`
    } else if(!addresses || addresses.Items?.length === 0) {
      this.helperMessage = "Looks like this Buyer has no Buyer Groups."
      this.helperAction = "Set Buyer Groups now"
      this.helperLink = `/buyers/${buyerID}/locations/new`
    }
  }

  updateResourceFromEvent(event: any, field: string): void {
    let resourceUpdate: ResourceFormUpdate;
    if (field === "ImpersonatingEnabled") {
      resourceUpdate = {
        field: 'ImpersonationConfig',
        value: this.showImpersonation ? null : this._superBuyerStatic?.ImpersonationConfig,
        form: this.resourceForm
      }
      this.showImpersonation = !this.showImpersonation
    } else {
      const value =
        field === 'Buyer.Active' ? event.target.checked : event.target.value
      resourceUpdate = {
        field,
        value,
        form: this.resourceForm
      }
    }
    this._superBuyerEditable = this.buyerService.getUpdatedEditableResource<HSBuyerPriceMarkup>(
      resourceUpdate,
      this._superBuyerEditable
    )
    this.checkForChanges()
  }

  createBuyerForm(superBuyer: any): void {
    const { Buyer, Markup, ImpersonationConfig } = superBuyer;
    this.showImpersonation = ImpersonationConfig && ImpersonationConfig !== null

    this.resourceForm = new FormGroup({
      Name: new FormControl(Buyer.Name, Validators.required),
      Active: new FormControl(Buyer.Active),
      Markup: new FormControl(Markup.Percent),
      ChiliPublishFolder: new FormControl(Buyer.xp.ChiliPublishFolder),
      ImpersonatingEnabled: new FormControl(this.showImpersonation),
      URL: new FormControl((Buyer.xp as any).URL),
      ClientID: new FormControl(ImpersonationConfig?.ClientID)
    })
    this.setImpersonationValidator()
  }

  setImpersonationValidator(): void {
    const url = this.resourceForm.get('URL')
    const ClientID = this.resourceForm.get('ClientID')
    this.impersonationSubscription = this.resourceForm
      .get('ImpersonatingEnabled')
      .valueChanges.subscribe((impersonation) => {
        if (impersonation) {
          url.setValidators([Validators.required])
          ClientID.setValidators([Validators.required, Validators.minLength(36)])
        }
      })
  }

  async handleSelectedBuyerChange(buyer: HSBuyer): Promise<void> {
    const superHSBuyer = await this.buyerTempService.get(buyer.ID)
    this.refreshBuyerData(superHSBuyer)
  }

  refreshBuyerData(superBuyer: HSBuyerPriceMarkup): void {
    this.createBuyerForm(superBuyer)
    this._superBuyerStatic = superBuyer
    this._superBuyerEditable = superBuyer
    this.isCreatingNew = this.buyerService.checkIfCreatingNew()
    this.checkForChanges()
  }

  checkForChanges(): void {
    this.areChanges = !_isEqual(
      this._superBuyerStatic,
      this._superBuyerEditable
    )
  }

  discardChanges(): void {
    this.refreshBuyerData(this._superBuyerStatic)
  }

  async handleSave(): Promise<void> {
    if (this.isCreatingNew) {
      await this.createNewBuyer()
    } else {
      this.updateBuyer()
    }
  }

  async createNewBuyer(): Promise<void> {
    try {
      this.dataIsSaving = true
      const newSuperBuyer = await this.buyerTempService.create(
        this._superBuyerEditable
      )
      this.router.navigateByUrl(`/buyers/${newSuperBuyer.Buyer.ID}`)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async updateBuyer(): Promise<void> {
    try {
      this.dataIsSaving = true
      const updatedBuyer = await this.buyerTempService.save(
        this._superBuyerEditable.Buyer.ID,
        this._superBuyerEditable
      )
      this.refreshBuyerData(updatedBuyer)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  deleteBuyer(): void {
    this.resourceDelete.emit()
  }

  ngOnDestroy(): void {
    this.impersonationSubscription.unsubscribe()
  }
}
