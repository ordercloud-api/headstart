import { Component, Input, Output, EventEmitter } from '@angular/core'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { HSBuyer } from '@ordercloud/headstart-sdk'
import { BuyerService } from '../buyer.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { Router } from '@angular/router'
import { isEqual as _isEqual } from 'lodash'
import { HSBuyerPriceMarkup } from '@app-seller/models/buyer-markups.types'
import { OcImpersonationConfigService } from '@ordercloud/angular-sdk'
import { ImpersonationConfigs } from 'ordercloud-javascript-sdk'
@Component({
  selector: 'app-buyer-edit',
  templateUrl: './buyer-edit.component.html',
  styleUrls: ['./buyer-edit.component.scss'],
})
export class BuyerEditComponent {
  resourceForm: FormGroup
  showImpersonation: boolean
  isCreatingNew = false
  areChanges = false
  dataIsSaving = false

  _superBuyerStatic: HSBuyerPriceMarkup
  _superBuyerEditable: HSBuyerPriceMarkup

  @Input()
  labelSingular: string
  @Input()
  set orderCloudBuyer(buyer: HSBuyer) {
    if (buyer.ID) {
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
    private appAuthService: AppAuthService
  ) {}

  updateResourceFromEvent(event: any, field: string): void {
    let resourceUpdate;
    if(field==="ImpersonatingEnabled") {
      resourceUpdate= {
        field: 'ImpersonationConfig',
        value: this.showImpersonation ? null : this._superBuyerStatic?.ImpersonationConfig
      }
      this.showImpersonation = !this.showImpersonation
    } else {
      const value =
      field === 'Buyer.Active' ? event.target.checked : event.target.value
      resourceUpdate = { field, value }
    }
    this._superBuyerEditable = this.buyerService.getUpdatedEditableResource<HSBuyerPriceMarkup>(
      resourceUpdate,
      this._superBuyerEditable
    )
    this.checkForChanges()
  }

  createBuyerForm(superBuyer: any): void {
    const {Buyer, Markup, ImpersonationConfig} = superBuyer;
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
  }

  async handleSelectedBuyerChange(buyer: HSBuyer): Promise<void> {
    //const buyerReq = this.buyerTempService.get(buyer.ID)
    //var impReq = this.ocImpersonationService.List({filters: {"BuyerID": buyer.ID}}).toPromise()
    //var test = await Promise.all([buyerReq, impReq]);
    //console.log(test)
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
}
