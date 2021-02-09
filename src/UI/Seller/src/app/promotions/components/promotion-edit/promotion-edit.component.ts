import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ChangeDetectorRef,
  OnChanges,
} from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import {
  faTimesCircle,
  faCalendar,
  faExclamationCircle,
  faQuestionCircle,
} from '@fortawesome/free-solid-svg-icons'
import {
  Promotion,
  OcPromotionService,
  OcSupplierService,
  Product,
  ListPage,
} from '@ordercloud/angular-sdk'
import { PromotionService } from '@app-seller/promotions/promotion.service'
import {
  PromotionXp,
  HSPromoType,
  HSPromoEligibility,
  MinRequirementType,
} from '@app-seller/models/promo-types'
import * as moment from 'moment'
import { Router } from '@angular/router'
import { ListArgs, HSSupplier } from '@ordercloud/headstart-sdk'
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap'
import { TranslateService } from '@ngx-translate/core'
import { Products, Meta } from 'ordercloud-javascript-sdk'
import { ToastrService } from 'ngx-toastr'
import { BehaviorSubject } from 'rxjs'
@Component({
  selector: 'app-promotion-edit',
  templateUrl: './promotion-edit.component.html',
  styleUrls: ['./promotion-edit.component.scss'],
})
export class PromotionEditComponent implements OnInit, OnChanges {
  @ViewChild('popover', { static: false })
  public popover: NgbPopover
  @Input()
  filterConfig
  @Input()
  set resourceInSelection(promotion: Promotion<PromotionXp>) {
    if (promotion.ID) {
      this.setUpSuppliers(promotion.xp?.Supplier)
      this.refreshPromoData(promotion)
    } else {
      this.setUpSuppliers()
      this.refreshPromoData(this.promotionService.emptyResource)
    }
  }
  @Input()
  updatedResource
  @Output()
  updateResource = new EventEmitter<any>()
  suppliers: HSSupplier[]
  products = new BehaviorSubject<Product[]>([])
  productMeta: Meta
  selectedSupplier: HSSupplier
  resourceForm: FormGroup
  _promotionEditable: Promotion<PromotionXp>
  _promotionStatic: Promotion<PromotionXp>
  hasRedemptionLimit = false
  limitPerUser = false
  hasExpiration = false
  capShipCost = false
  areChanges = false
  isExpired = false
  hasNotBegun = false
  dataIsSaving = false
  isCreatingNew: boolean
  searchTerm = ''
  faTimesCircle = faTimesCircle
  faExclamationCircle = faExclamationCircle
  faQuestionCircle = faQuestionCircle
  faCalendar = faCalendar
  productsCollapsed = true
  currentDateTime: string
  constructor(
    public promotionService: PromotionService,
    private ocPromotionService: OcPromotionService,
    private ocSupplierService: OcSupplierService,
    private router: Router,
    private translate: TranslateService,
    private toastrService: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isCreatingNew = this.promotionService.checkIfCreatingNew()
    this.listResources()
  }

  ngOnChanges(): void {
    this.productsCollapsed =
      this._promotionEditable?.xp?.AppliesTo !==
      HSPromoEligibility.SpecificSKUs
    this.currentDateTime = moment().format('YYYY-MM-DD[T]hh:mm')
  }

  refreshPromoData(promo: Promotion<PromotionXp>): void {
    this.productsCollapsed =
      promo?.xp?.AppliesTo !== HSPromoEligibility.SpecificSKUs
    const now = moment(Date.now()).format('YYYY-MM-DD[T]hh:mm')
    this.isExpired = promo.ExpirationDate
      ? Date.parse(promo.ExpirationDate) < Date.parse(now)
      : false
    this.hasNotBegun = Date.parse(promo.StartDate) > Date.parse(now)
    // Modify the datetime to work with the UI
    if (promo.StartDate)
      promo.StartDate = moment(promo.StartDate).format('YYYY-MM-DD[T]hh:mm')
    if (promo.ExpirationDate) {
      this.hasExpiration = true
      promo.ExpirationDate = promo.ExpirationDate = moment(
        promo.ExpirationDate
      ).format('YYYY-MM-DD[T]hh:mm')
    } else {
      this.hasExpiration = false
    }
    this.hasRedemptionLimit = promo.RedemptionLimit ? true : false
    this.limitPerUser = promo.RedemptionLimitPerUser ? true : false
    this.capShipCost = promo.xp?.MaxShipCost ? true : false
    this._promotionEditable = JSON.parse(JSON.stringify(promo))
    this._promotionStatic = JSON.parse(JSON.stringify(promo))
    this.createPromotionForm(promo)
  }

  async setUpSuppliers(existingSupplierID?: string): Promise<void> {
    const supplierResponse = await this.ocSupplierService
      .List({ pageSize: 100 })
      .toPromise()
    this.suppliers = supplierResponse.Items
    await this.selectSupplier(existingSupplierID || this.suppliers[0].ID)
  }

  async selectSupplier(supplierID: string): Promise<void> {
    const s = await this.ocSupplierService.Get(supplierID).toPromise()
    this.selectedSupplier = s
    if (
      this._promotionEditable?.xp?.AppliesTo ===
      HSPromoEligibility.SpecificSupplier
    ) {
      this.handleUpdatePromo({ target: { value: s.ID } }, 'xp.Supplier')
    }
  }

  searchedResources(searchText: any): void {
    this.listResources(1, searchText)
    this.searchTerm = searchText
  }

  async listResources(pageNumber = 1, searchText = ''): Promise<void> {
    const options: ListArgs = {
      page: pageNumber,
      search: searchText,
      sortBy: ['Name'],
      pageSize: 25,
      filters: {},
    }
    const resourceResponse = await Products.List(options)
    if (pageNumber === 1) {
      this.setNewResources(resourceResponse)
    } else {
      this.addResources(resourceResponse)
    }
  }

  setNewResources(resourceResponse: ListPage<Product>): void {
    this.productMeta = resourceResponse?.Meta
    this.products.next(resourceResponse?.Items)
  }

  addResources(resourceResponse: ListPage<Product>): void {
    this.products.next([...this.products.value, ...resourceResponse?.Items])
    this.productMeta = resourceResponse?.Meta
  }

  handleScrollEnd(event: any): void {
    // This event check prevents the scroll-end event from firing when dropdown is closed
    // It limits the action within the if block to only fire when you truly hit the scroll-end
    if (event.target.classList.value.includes('active')) {
      const totalPages = this.productMeta?.TotalPages
      const nextPageNumber = this.productMeta?.Page + 1
      if (totalPages >= nextPageNumber) {
        this.listResources(nextPageNumber, this.searchTerm).then(() =>
          this.cdr.detectChanges()
        )
      }
    }
  }

  addSKU(sku: string): void {
    if (this._promotionEditable?.xp?.SKUs.includes(sku)) {
      this.toastrService.warning('You have already selected this product')
    } else {
      const newSKUs = [...this._promotionEditable?.xp?.SKUs, sku]
      this.handleUpdatePromo({ target: { value: newSKUs } }, 'xp.SKUs')
    }
  }

  removeSku(sku: string): void {
    const modifiedSkus = this._promotionEditable?.xp?.SKUs?.filter(
      (s) => s !== sku
    )
    this.handleUpdatePromo({ target: { value: modifiedSkus } }, 'xp.SKUs')
  }

  alreadySelected(sku: string): boolean {
    return this._promotionEditable?.xp?.SKUs?.includes(sku)
  }

  getSKUsBtnPlaceholderValue(): string {
    let placeholder = ''
    const path = this._promotionEditable?.xp?.SKUs
    if (path?.length === 1) {
      placeholder = path?.[0]
    } else if (path?.length > 1) {
      placeholder = `${path?.[0]} +${path?.length - 1} more`
    } else {
      placeholder = 'Choose a product'
    }
    return placeholder
  }

  createPromotionForm(promotion: Promotion): void {
    this.resourceForm = new FormGroup({
      Code: new FormControl(promotion.Code, Validators.required),
      Type: new FormControl(_get(promotion, 'xp.Type')),
      Value: new FormControl(_get(promotion, 'xp.Value'), Validators.min(0)),
      AppliesTo: new FormControl(_get(promotion, 'xp.AppliesTo')),
      Supplier: new FormControl(_get(promotion, 'xp.Supplier')),
      RedemptionLimit: new FormControl(
        promotion.RedemptionLimit,
        Validators.min(0)
      ),
      RedemptionLimitPerUser: new FormControl(
        promotion.RedemptionLimitPerUser,
        Validators.min(0)
      ),
      Description: new FormControl(promotion.Description),
      FinePrint: new FormControl(promotion.FinePrint),
      StartDate: new FormControl(promotion.StartDate, Validators.required),
      ExpirationDate: new FormControl(promotion.ExpirationDate),
      Automatic: new FormControl(_get(promotion, 'xp.Automatic')),
      AllowAllBuyers: new FormControl(promotion.AllowAllBuyers),
      MinReqType: new FormControl(_get(promotion, 'xp.MinReq.Type')),
      MinReqInt: new FormControl(
        _get(promotion, 'xp.MinReq.Int'),
        Validators.min(0)
      ),
      MaxShipCost: new FormControl(
        _get(promotion, 'xp.MaxShipCost'),
        Validators.min(0)
      ),
    })
  }

  generateRandomCode(): void {
    const randomCode = Math.random()
      .toString(36)
      .slice(2)
      .substr(0, 5)
      .toUpperCase()
    this.handleUpdatePromo({ target: { value: randomCode } }, 'Code')
    this._promotionEditable.Code = randomCode
    this.resourceForm.controls.Code.setValue(randomCode)
  }

  handleUpdatePromo(event: any, field: string, typeOfValue?: string): void {
    if (
      field === 'xp.AppliesTo' &&
      event?.target?.value === HSPromoEligibility.SpecificSupplier
    ) {
      this.updatePromoResource({ field: 'LineItemLevel', value: true })
    } else if (
      field === 'xp.AppliesTo' &&
      event?.target?.value === HSPromoEligibility.SpecificSKUs
    ) {
      this.updatePromoResource({ field: 'LineItemLevel', value: true })
      this.productsCollapsed = false
    }
    const promoUpdate = {
      field,
      value: ['Type', 'CanCombine', 'xp.Automatic'].includes(field)
        ? event.target.checked
        : typeOfValue === 'number'
        ? Number(event.target.value)
        : event.target.value,
    }
    this.updatePromoResource(promoUpdate)
  }

  updatePromoResource(promoUpdate: any): void {
    const resourceToUpdate =
      this._promotionEditable || this.promotionService.emptyResource
    this._promotionEditable = this.promotionService.getUpdatedEditableResource(
      promoUpdate,
      resourceToUpdate
    )
    this.areChanges = this.promotionService.checkForChanges(
      this._promotionEditable,
      this._promotionStatic
    )
    this._promotionEditable.ValueExpression = this.promotionService.buildValueExpression(
      this._promotionEditable?.xp,
      this.selectedSupplier
    )
    this._promotionEditable.EligibleExpression = this.promotionService.buildEligibleExpression(
      this._promotionEditable?.xp,
      this.selectedSupplier
    )
  }

  promoTypeCheck(type: HSPromoType): boolean {
    return type === this._promotionEditable?.xp?.Type
  }

  promoEligibilityCheck(eligibility: HSPromoEligibility): boolean {
    return eligibility === this._promotionEditable?.xp?.AppliesTo
  }

  getValueDisplay(): string {
    const safeXp = this._promotionEditable?.xp
    let valueString = ''
    switch (safeXp?.AppliesTo) {
      case HSPromoEligibility.SpecificSupplier:
        valueString = `${this.translate.instant(
          'ADMIN.PROMOTIONS.DISPLAY.VALUE.OFF_ENTIRE'
        )} ${this.selectedSupplier?.Name} ${this.translate.instant(
          'ADMIN.PROMOTIONS.DISPLAY.VALUE.PRODUCTS_ORDER'
        )}`
        break
      case HSPromoEligibility.SpecificSKUs:
        valueString = this.translate.instant(
          'ADMIN.PROMOTIONS.DISPLAY.VALUE.OFF_SELECT_PRODUCTS'
        )
        break
      default:
        valueString = this.translate.instant(
          'ADMIN.PROMOTIONS.DISPLAY.VALUE.OFF_ENTIRE_ORDER'
        )
    }
    valueString = this.arrangeValueString(safeXp, valueString)
    // Update `promotion.Description` with this value string
    this.handleUpdatePromo(
      { target: { value: valueString.trim() } },
      'Description'
    )
    return valueString.trim()
  }

  arrangeValueString(safeXp: PromotionXp, valueString: string): string {
    if (safeXp?.Type === HSPromoType.FixedAmount) {
      valueString = `$${safeXp?.Value} ${valueString}`
    }
    if (safeXp?.Type === HSPromoType.Percentage) {
      valueString = `${safeXp?.Value}% ${valueString}`
    }
    if (safeXp?.Type === HSPromoType.FreeShipping) {
      valueString = this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.VALUE.FREE_SHIPPING_ENTIRE_ORDER'
      )
    }
    if (
      safeXp?.MinReq?.Type === MinRequirementType.MinPurchase &&
      safeXp?.MinReq?.Int
    ) {
      valueString = `${valueString} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.VALUE.OVER'
      )} $${safeXp?.MinReq?.Int}`
    }
    if (
      safeXp?.MinReq?.Type === MinRequirementType.MinItemQty &&
      safeXp?.MinReq?.Int
    ) {
      ;`${valueString} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.VALUE.OVER'
      )} ${this._promotionEditable?.xp?.MinReq?.Int} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.VALUE.ITEMS'
      )}`
    }
    return valueString
  }

  getDateRangeDisplay(): string {
    let dateRangeString = this.translate.instant(
      'ADMIN.PROMOTIONS.DISPLAY.DATE.VALID_FROM'
    )
    const formattedStart =
      this._promotionEditable.StartDate.substr(0, 4) === moment().format('YYYY')
        ? moment(this._promotionEditable.StartDate).format('MMM Do')
        : moment(this._promotionEditable.StartDate).format('MMM Do, YYYY')
    const formattedExpiry =
      this._promotionEditable.ExpirationDate.substr(0, 4) ===
      moment().format('YYYY')
        ? moment(this._promotionEditable.ExpirationDate).format('MMM Do')
        : moment(this._promotionEditable.ExpirationDate).format('MMM Do, YYYY')
    moment(this._promotionEditable.StartDate).format('MM-DD-YYYY') ===
    moment().format('MM-DD-YYYY')
      ? (dateRangeString = `${dateRangeString} ${this.translate.instant(
          'ADMIN.PROMOTIONS.DISPLAY.DATE.TODAY_TO'
        )} ${formattedExpiry}`)
      : (dateRangeString = `${dateRangeString} ${formattedStart} ${this.translate.instant(
          'ADMIN.PROMOTIONS.DISPLAY.DATE.TO'
        )} ${formattedExpiry}`)
    return dateRangeString
  }

  getEligibilityDisplay(): string {
    let eligibilityString = this.translate.instant(
      'ADMIN.PROMOTIONS.DISPLAY.ELIGIBILITY.FOR'
    )
    if (this._promotionEditable.AllowAllBuyers)
      eligibilityString = `${eligibilityString} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.ELIGIBILITY.ALL_BUYERS'
      )}`
    // In the future, there will be other considerations for finer grained eligibility
    return eligibilityString
  }

  getUsageLimitDisplay(): string {
    let usageLimitString = this.translate.instant(
      'ADMIN.PROMOTIONS.DISPLAY.USAGE.LIMIT_OF'
    )
    if (this._promotionEditable.RedemptionLimit)
      usageLimitString = `${usageLimitString} ${
        this._promotionEditable.RedemptionLimit
      } ${
        this._promotionEditable.RedemptionLimit > 1
          ? this.translate.instant('ADMIN.PROMOTIONS.DISPLAY.USAGE.USES')
          : this.translate.instant('ADMIN.PROMOTIONS.DISPLAY.USAGE.USE')
      }`
    if (this._promotionEditable.RedemptionLimitPerUser)
      usageLimitString = `${usageLimitString} ${
        this._promotionEditable.RedemptionLimitPerUser
      } ${this.translate.instant('ADMIN.PROMOTIONS.DISPLAY.USAGE.PER_USER')}`
    if (
      this._promotionEditable.RedemptionLimit &&
      this._promotionEditable.RedemptionLimitPerUser
    )
      usageLimitString = `${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.USAGE.LIMIT_OF'
      )} ${this._promotionEditable.RedemptionLimit} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.USAGE.USES'
      )}, ${
        this._promotionEditable.RedemptionLimitPerUser
      } ${this.translate.instant('ADMIN.PROMOTIONS.DISPLAY.USAGE.PER_USER')}`
    return usageLimitString
  }

  getMinDate(): string {
    return this.currentDateTime
  }

  toggleHasRedemptionLimit(): void {
    this.hasRedemptionLimit = !this.hasRedemptionLimit
    if (!this.hasRedemptionLimit) this._promotionEditable.RedemptionLimit = null
  }

  toggleLimitPerUser(): void {
    this.limitPerUser = !this.limitPerUser
    if (!this.limitPerUser)
      this._promotionEditable.RedemptionLimitPerUser = null
  }

  toggleHasExpiration(): void {
    this.hasExpiration = !this.hasExpiration
    if (!this.hasExpiration) this._promotionEditable.ExpirationDate = null
  }

  toggleCapShipCost(): void {
    this.capShipCost = !this.capShipCost
    if (!this.capShipCost) this._promotionEditable.xp.MaxShipCost = null
  }

  getSaveBtnText(): string {
    return this.promotionService.getSaveBtnText(
      this.dataIsSaving,
      this.isCreatingNew
    )
  }

  handleClearMinReq(): void {
    this._promotionEditable.EligibleExpression = 'true'
    this.handleUpdatePromo(
      { target: { value: { Type: null, Int: null } } },
      'xp.MinReq'
    )
  }

  handleDiscardChanges(): void {
    this.refreshPromoData(this._promotionStatic)
  }

  async handleSave(): Promise<void> {
    if (this.isCreatingNew) {
      await this.createPromotion(this._promotionEditable)
    } else {
      await this.updatePromotion(this._promotionEditable)
    }
  }

  async createPromotion(promo: Promotion<PromotionXp>): Promise<void> {
    try {
      this.dataIsSaving = true
      // Set promotion.Name to promotion.Code automatically
      promo.Name = promo.Code
      const newPromo = await this.ocPromotionService.Create(promo).toPromise()
      this.refreshPromoData(newPromo)
      this.router.navigateByUrl(`/promotions/${newPromo.ID}`)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  // TODO: Find diff'd object and only 'PATCH' what changed?
  async updatePromotion(promo: Promotion<PromotionXp>): Promise<void> {
    try {
      this.dataIsSaving = true
      const updatedPromo = await this.ocPromotionService
        .Save(promo.ID, promo)
        .toPromise()
      this.refreshPromoData(updatedPromo)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async handleDelete(): Promise<void> {
    await this.ocPromotionService.Delete(this._promotionStatic.ID).toPromise()
    this.router.navigateByUrl('/promotions')
  }
}
