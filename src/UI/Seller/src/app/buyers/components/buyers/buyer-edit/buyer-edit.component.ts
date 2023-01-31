import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnDestroy,
} from '@angular/core'
import {
  UntypedFormGroup,
  UntypedFormControl,
  Validators,
} from '@angular/forms'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { HeadStartSDK, HSBuyer, SuperHSBuyer } from '@ordercloud/headstart-sdk'
import { BuyerService } from '../buyer.service'
import { Router } from '@angular/router'
import { isEqual as _isEqual } from 'lodash'
import { Subscription } from 'rxjs'
import { ResourceUpdate } from '@app-seller/shared'
import { TranslateService } from '@ngx-translate/core'
import { Addresses } from 'ordercloud-javascript-sdk'
@Component({
  selector: 'app-buyer-edit',
  templateUrl: './buyer-edit.component.html',
  styleUrls: ['./buyer-edit.component.scss'],
})
export class BuyerEditComponent implements OnDestroy {
  resourceForm: UntypedFormGroup
  showImpersonation: boolean
  isCreatingNew = false
  areChanges = false
  dataIsSaving = false
  impersonationSubscription: Subscription
  helperMessage: string
  helperAction: string
  helperLink: string

  _superBuyerStatic: SuperHSBuyer
  _superBuyerEditable: SuperHSBuyer

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
    private router: Router,
    private translate: TranslateService,
    private buyerTempService: BuyerTempService
  ) {}

  async getBuyerData(buyerID: string): Promise<void> {
    const [catalogs, addresses] = await Promise.all([
      HeadStartSDK.Catalogs.List(buyerID),
      Addresses.List(buyerID),
    ])
    if (!catalogs?.Items || catalogs.Items?.length === 0) {
      this.helperMessage = this.translate.instant(
        'BUYER.HELPERMESSAGES.CATALOG.MESSAGE'
      )
      this.helperAction = this.translate.instant(
        'BUYER.HELPERMESSAGES.CATALOG.ACTION'
      )
      this.helperLink = `/buyers/${buyerID}/catalogs/new`
    } else if (!addresses || addresses.Items?.length === 0) {
      this.helperMessage = this.translate.instant(
        'BUYER.HELPERMESSAGES.BUYERGROUP.MESSAGE'
      )
      this.helperAction = this.translate.instant(
        'BUYER.HELPERMESSAGES.BUYERGROUP.ACTION'
      )
      this.helperLink = `/buyers/${buyerID}/locations/new`
    }
  }

  updateResourceFromEvent(event: any, field: string): void {
    let resourceUpdate: ResourceUpdate
    if (field === 'ImpersonatingEnabled') {
      resourceUpdate = {
        field: 'ImpersonationConfig',
        value: this.showImpersonation
          ? null
          : this._superBuyerStatic?.ImpersonationConfig,
        form: this.resourceForm,
      }
      this.showImpersonation = !this.showImpersonation
    } else {
      const value =
        field === 'Buyer.Active' ? event.target.checked : event.target.value
      resourceUpdate = {
        field,
        value,
        form: this.resourceForm,
      }
    }
    this._superBuyerEditable =
      this.buyerService.getUpdatedEditableResource<SuperHSBuyer>(
        resourceUpdate,
        this._superBuyerEditable
      )
    this.checkForChanges()
  }

  createBuyerForm(superBuyer: any): void {
    const { Buyer, ImpersonationConfig } = superBuyer
    this.showImpersonation = ImpersonationConfig && ImpersonationConfig !== null

    this.resourceForm = new UntypedFormGroup({
      Name: new UntypedFormControl(Buyer.Name, Validators.required),
      Active: new UntypedFormControl(Buyer.Active),
      Markup: new UntypedFormControl(Buyer.xp.MarkupPercent),
      ImpersonatingEnabled: new UntypedFormControl(this.showImpersonation),
      URL: new UntypedFormControl(Buyer.xp.URL),
      ClientID: new UntypedFormControl(ImpersonationConfig?.ClientID),
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
          ClientID.setValidators([
            Validators.required,
            Validators.minLength(36),
          ])
        }
      })
  }

  async handleSelectedBuyerChange(buyer: HSBuyer): Promise<void> {
    const superHSBuyer = await this.buyerTempService.get(buyer.ID)
    this.refreshBuyerData(superHSBuyer)
  }

  refreshBuyerData(superBuyer: SuperHSBuyer): void {
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
