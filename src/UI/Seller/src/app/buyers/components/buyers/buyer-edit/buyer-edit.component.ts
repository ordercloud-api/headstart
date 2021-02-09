import { Component, Input, Output, EventEmitter } from '@angular/core'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { HSBuyer } from '@ordercloud/headstart-sdk'
import { BuyerService } from '../buyer.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { Router } from '@angular/router'
import { isEqual as _isEqual } from 'lodash'
import { HSBuyerPriceMarkup } from '@app-seller/models/buyer-markups.types'
@Component({
  selector: 'app-buyer-edit',
  templateUrl: './buyer-edit.component.html',
  styleUrls: ['./buyer-edit.component.scss'],
})
export class BuyerEditComponent {
  resourceForm: FormGroup
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
    private router: Router,
    private buyerTempService: BuyerTempService,
    private appAuthService: AppAuthService
  ) {}

  updateResourceFromEvent(event: any, field: string): void {
    const value =
      field === 'Buyer.Active' ? event.target.checked : event.target.value
    const resourceUpdate = { field, value }
    this._superBuyerEditable = this.buyerService.getUpdatedEditableResource<HSBuyerPriceMarkup>(
      resourceUpdate,
      this._superBuyerEditable
    )
    this.checkForChanges()
  }

  createBuyerForm(superBuyer: HSBuyerPriceMarkup): void {
    const buyer = superBuyer.Buyer
    const markup = superBuyer.Markup
    this.resourceForm = new FormGroup({
      Name: new FormControl(buyer.Name, Validators.required),
      Active: new FormControl(buyer.Active),
      Markup: new FormControl(markup.Percent),
      ChiliPublishFolder: new FormControl(buyer.xp.ChiliPublishFolder),
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
}
