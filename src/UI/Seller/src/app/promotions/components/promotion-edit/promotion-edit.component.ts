import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ChangeDetectorRef,
  OnChanges,
  ComponentFactoryResolver,
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
  OcProductService,
  OcBuyerService,
} from '@ordercloud/angular-sdk'
import { PromotionService } from '@app-seller/promotions/promotion.service'
import {
  PromotionXp,
  HSPromoType,
  HSPromoEligibility,
  MinRequirementType,
  HSBogoType,
} from '@app-seller/models/promo-types'
import * as moment from 'moment'
import { Router } from '@angular/router'
import {
  ListArgs,
  HSSupplier,
  HSProduct,
  HSBuyer,
} from '@ordercloud/headstart-sdk'
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap'
import { TranslateService } from '@ngx-translate/core'
import {
  Products,
  Meta,
  Suppliers,
  Supplier,
  Buyers,
} from 'ordercloud-javascript-sdk'
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
  set resourceInSelection(promotion: Promotion<PromotionXp>) {
    if (promotion?.xp?.Type === HSPromoType.BOGO) {
      this.setUpBOGO(
        promotion?.xp?.BOGO?.BuySKU?.SKU,
        promotion?.xp?.BOGO?.GetSKU?.SKU
      )
      this.refreshPromoData(promotion)
    }
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
  suppliers = new BehaviorSubject<HSSupplier[]>([])
  supplierMeta: Meta
  products = new BehaviorSubject<Product[]>([])
  productMeta: Meta
  buyers = new BehaviorSubject<HSBuyer[]>([])
  buyerMeta: Meta
  selectedSupplier: HSSupplier
  selectedBuySKU: HSProduct
  selectedGetSKU: HSProduct
  selectedBuyer: HSBuyer
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
  isCloning = false
  searchTerm = ''
  buyerSearchTerm = ''
  faTimesCircle = faTimesCircle
  faExclamationCircle = faExclamationCircle
  faQuestionCircle = faQuestionCircle
  faCalendar = faCalendar
  productsCollapsed = true
  buyProductsCollapsed = false
  getProductsCollapsed = true
  currentDateTime: string
  constructor(
    public promotionService: PromotionService,
    private ocPromotionService: OcPromotionService,
    private ocSupplierService: OcSupplierService,
    private ocProductService: OcProductService,
    private ocBuyerService: OcBuyerService,
    private router: Router,
    private translate: TranslateService,
    private toastrService: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const splitUrl = this.router.routerState.snapshot.url.split('/')
    if (splitUrl[splitUrl.length - 2] === 'clone') {
      this.isCreatingNew = true
      this.isCloning = true
    } else {
      this.isCreatingNew = this.promotionService.checkIfCreatingNew()
    }
    void this.listResources().then(() => this.cdr.detectChanges())
  }

  ngOnChanges(): void {
    this.productsCollapsed =
      this._promotionEditable?.xp?.AppliesTo !== HSPromoEligibility.SpecificSKUs
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
    this._promotionEditable = JSON.parse(
      JSON.stringify(promo)
    ) as Promotion<PromotionXp>
    this._promotionStatic = JSON.parse(
      JSON.stringify(promo)
    ) as Promotion<PromotionXp>
    this.buyProductsCollapsed = this.selectedBuySKU ? true : false
    this.getProductsCollapsed = this.selectedGetSKU ? true : false
    this.createPromotionForm(promo)
  }

  async setUpSuppliers(existingSupplierID?: string): Promise<void> {
    const supplierResponse = await this.ocSupplierService
      .List({ pageSize: 25, sortBy: ['Name'] })
      .toPromise()
    this.suppliers.next(supplierResponse.Items)
    this.supplierMeta = supplierResponse.Meta
    await this.selectSupplier(existingSupplierID || this.suppliers.value[0].ID)
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

  async setUpBOGO(
    existingBuyProductID: string,
    existingGetProductID: string
  ): Promise<void> {
    const productResponse = await this.ocProductService
      .List({ pageSize: 25, sortBy: ['Name'] })
      .toPromise()
    this.products.next(productResponse.Items)
    this.productMeta = productResponse.Meta
    await this.selectBuySKU(existingBuyProductID)
    await this.selectGetSKU(existingGetProductID)
  }

  toggleBOGOProductsCollapse(buyOrGet: string) {
    if (buyOrGet === 'buy') {
      this.getProductsCollapsed = true
      this.buyProductsCollapsed = !this.buyProductsCollapsed
    } else {
      this.buyProductsCollapsed = true
      this.getProductsCollapsed = !this.getProductsCollapsed
    }
    this.cdr.detectChanges()
  }

  selectBOGOPromoType(event: any): void {
    this.buyProductsCollapsed = false
    this.getProductsCollapsed = true
    if (this.selectedBuySKU && !this.selectedGetSKU) {
      this.getProductsCollapsed = false
    }
    if (this.selectedBuySKU && this.selectedGetSKU) {
      this.buyProductsCollapsed = true
    }
    this.handleUpdatePromo(event, 'xp.Type')
    this.cdr.detectChanges()
  }

  async selectBuySKU(productID: string): Promise<void> {
    this.buyProductsCollapsed = true
    if (!this.selectedGetSKU) {
      this.getProductsCollapsed = false
    }
    const productInObservable = this.products?.value?.find(
      (p) => p.ID === productID
    )
    let p
    if (productInObservable) {
      p = productInObservable
      this.selectedBuySKU = productInObservable
    } else {
      p = await this.ocProductService.Get(productID).toPromise()
      this.selectedBuySKU = p
    }
    this.handleUpdatePromo({ target: { value: p?.ID } }, 'xp.BOGO.BuySKU.SKU')
    this.cdr.detectChanges()
  }

  async selectGetSKU(productID: string): Promise<void> {
    this.getProductsCollapsed = true
    const productInObservable = this.products?.value?.find(
      (p) => p.ID === productID
    )
    let p
    if (productInObservable) {
      p = productInObservable
      this.selectedGetSKU = productInObservable
    } else {
      p = await this.ocProductService.Get(productID).toPromise()
      this.selectedGetSKU = p
    }
    this.handleUpdatePromo({ target: { value: p?.ID } }, 'xp.BOGO.GetSKU.SKU')
    this.cdr.detectChanges()
  }

  searchedResources(searchText: string): void {
    void this.listResources(1, searchText).then(() => this.cdr.detectChanges())
    this.searchTerm = searchText
  }

  buyerSearchedResources(buyerSearchText: string): void {
    void this.listResources(1, buyerSearchText).then(() =>
      this.cdr.detectChanges()
    )
    this.buyerSearchTerm = buyerSearchText
  }

  async listResources(pageNumber = 1, searchText = ''): Promise<void> {
    const options: ListArgs<any> = {
      page: pageNumber,
      search: searchText,
      sortBy: ['Name'],
      pageSize: 25,
      filters: {},
    }
    let resourceResponse
    if (
      this._promotionEditable?.xp?.AppliesTo ===
      HSPromoEligibility.SpecificSupplier
    ) {
      resourceResponse = await Suppliers.List(options as any) // Issue with the SDK
    } else {
      resourceResponse = await Products.List(options)
    }
    const buyerResourceResponse = await Buyers.List(options as any)
    if (pageNumber === 1) {
      this.setNewResources(resourceResponse, buyerResourceResponse)
    } else {
      this.addResources(resourceResponse, buyerResourceResponse)
    }
  }

  setNewResources(
    resourceResponse: ListPage<any>,
    buyerResourceResponse: ListPage<HSBuyer>
  ): void {
    if (
      this._promotionEditable?.xp?.AppliesTo ===
      HSPromoEligibility.SpecificSupplier
    ) {
      this.supplierMeta = resourceResponse?.Meta
      this.suppliers.next(resourceResponse?.Items)
    } else {
      this.productMeta = resourceResponse?.Meta
      this.products.next(resourceResponse?.Items)
    }
    this.buyerMeta = buyerResourceResponse?.Meta
    this.buyers.next(buyerResourceResponse?.Items)
  }

  addResources(
    resourceResponse: ListPage<Product>,
    buyerResourceResponse: ListPage<HSBuyer>
  ): void {
    if (
      this._promotionEditable?.xp?.AppliesTo ===
      HSPromoEligibility.SpecificSupplier
    ) {
      this.suppliers.next([...this.suppliers.value, ...resourceResponse?.Items])
      this.supplierMeta = resourceResponse?.Meta
    } else {
      this.products.next([...this.products.value, ...resourceResponse?.Items])
      this.productMeta = resourceResponse?.Meta
    }
    this.buyers.next([...this.buyers.value, ...buyerResourceResponse?.Items])
    this.buyerMeta = buyerResourceResponse?.Meta
  }

  handleScrollEnd(event: any): void {
    // This event check prevents the scroll-end event from firing when dropdown is closed
    // It limits the action within the if block to only fire when you truly hit the scroll-end
    if (event.target.classList.value.includes('active')) {
      let totalPages, nextPageNumber
      if (
        this._promotionEditable?.xp?.AppliesTo ===
        HSPromoEligibility.SpecificSupplier
      ) {
        totalPages = this.supplierMeta?.TotalPages
        nextPageNumber = this.supplierMeta?.Page + 1
      } else {
        totalPages = this.productMeta?.TotalPages
        nextPageNumber = this.productMeta?.Page + 1
      }
      if (totalPages >= nextPageNumber) {
        void this.listResources(nextPageNumber, this.searchTerm).then(() =>
          this.cdr.detectChanges()
        )
      }
    }
    this.cdr.detectChanges()
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

  addBuyer(buyerID: string): void {
    if (this._promotionEditable?.xp?.Buyers?.includes(buyerID)) {
      this.toastrService.warning('You have already selected this buyer')
    } else {
      const newBuyerIDs = [...this._promotionEditable?.xp?.Buyers, buyerID]
      this.handleUpdatePromo({ target: { value: newBuyerIDs } }, 'xp.Buyers')
    }
  }

  removeBuyer(buyerID: string): void {
    const modifiedBuyerIDs = this._promotionEditable?.xp?.Buyers?.filter(
      (b) => b !== buyerID
    )
    this.handleUpdatePromo({ target: { value: modifiedBuyerIDs } }, 'xp.Buyers')
  }

  alreadySelected(sku: string): boolean {
    return this._promotionEditable?.xp?.SKUs?.includes(sku)
  }

  buyerAlreadySelected(buyerID: string): boolean {
    return this._promotionEditable?.xp?.Buyers?.includes(buyerID)
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
    if (this.isCloning) {
      this._promotionEditable.Code = promotion.Code = null
      promotion.Name = 'Cloned Promotion'
    }
    this.resourceForm = new FormGroup({
      Code: new FormControl(promotion.Code, Validators.required),
      Type: new FormControl(_get(promotion, 'xp.Type')),
      BOGOType: new FormControl(_get(promotion, 'xp.BOGO.Type')),
      BOGOValue: new FormControl(_get(promotion, 'xp.BOGO.Value')),
      BOGOBuyQty: new FormControl(_get(promotion, 'xp.BOGO.BuySKU.Qty')),
      BOGOGetQty: new FormControl(_get(promotion, 'xp.BOGO.GetSKU.Qty')),
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
    if (field === 'AllowAllBuyers') {
      this.updatePromoResource({
        field: 'AllowAllBuyers',
        value: event,
      })
      if (event) {
        this._promotionEditable.xp.Buyers = []
      }
    } else {
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
    this._promotionEditable.ValueExpression =
      this.promotionService.buildValueExpression(
        this._promotionEditable?.xp,
        this.selectedSupplier
      )
    this._promotionEditable.EligibleExpression =
      this.promotionService.buildEligibleExpression(
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
    if (safeXp?.Type === HSPromoType.BOGO) {
      let bogoValueString
      if (
        this.selectedBuySKU &&
        this.selectedGetSKU &&
        safeXp?.BOGO?.BuySKU?.Qty &&
        safeXp?.BOGO?.GetSKU &&
        safeXp?.BOGO?.Value &&
        safeXp?.BOGO?.Type
      ) {
        let valueStr =
          safeXp?.BOGO?.Type === HSBogoType.FixedAmount
            ? `$${safeXp?.BOGO?.Value} off`
            : `${safeXp?.BOGO?.Value}% off`
        if (
          safeXp?.BOGO?.Type === HSBogoType.Percentage &&
          safeXp?.BOGO?.Value === 100
        ) {
          valueStr = `Free`
        }
        bogoValueString = `Buy ${safeXp?.BOGO?.BuySKU?.Qty} ${this.selectedBuySKU?.Name}, Get ${safeXp?.BOGO?.GetSKU?.Qty} ${this.selectedGetSKU?.Name} ${valueStr}`
      } else {
        bogoValueString = `Choosing options ...`
      }
      this.handleUpdatePromo(
        { target: { value: bogoValueString.trim() } },
        'Description'
      )
      return bogoValueString
    }
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
    if (this._promotionEditable.AllowAllBuyers) {
      eligibilityString = `${eligibilityString} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.ELIGIBILITY.ALL_BUYERS'
      )}`
    } else {
      eligibilityString = `${eligibilityString} ${this.translate.instant(
        'ADMIN.PROMOTIONS.DISPLAY.ELIGIBILITY.SELECTED_BUYERS'
      )}`
    }
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
      this._promotionEditable.ID = null
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
      if (!promo.AllowAllBuyers) {
        await this.handlePromotionAssignments(newPromo)
      }
      this.refreshPromoData(newPromo)
      this.router.navigateByUrl(`/promotions/${newPromo.ID}`)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async handlePromotionAssignments(
    promo: Promotion<PromotionXp>
  ): Promise<void> {
    // Assemble assignment data
    const promoBuyers = promo.xp?.Buyers
    const listOptions = {
      page: 1,
      pageSize: 100,
      promotionID: promo.ID,
    }
    const currentPromotionAssignments = await this.ocPromotionService
      .ListAssignments(listOptions)
      .toPromise()
    const assignmentsToDelete: string[] = []
    const assignmentsToAdd: string[] = []

    // Identify assignments to add
    if (promoBuyers.length > 0) {
      for (const buyerIDToAssign of promoBuyers) {
        const matchingAssignment = currentPromotionAssignments.Items.find(
          (assignment) => assignment?.BuyerID == buyerIDToAssign
        )
        if (!matchingAssignment) {
          assignmentsToAdd.push(buyerIDToAssign)
        }
      }
    }

    // Identify assignments to delete
    if (currentPromotionAssignments.Items.length > 0) {
      for (const currentAssignment of currentPromotionAssignments.Items) {
        const matchingAssignment = promoBuyers.find(
          (assignment) => assignment == currentAssignment.BuyerID
        )
        if (!matchingAssignment) {
          assignmentsToDelete.push(currentAssignment.BuyerID)
        }
      }
    }

    const assignmentsToAddRequests: Promise<void>[] = []
    const assignmentsToDeleteRequests: Promise<void>[] = []

    // Add assignments
    if (assignmentsToAdd.length > 0) {
      for (const assignment of assignmentsToAdd) {
        assignmentsToAddRequests.push(
          this.ocPromotionService
            .SaveAssignment({
              PromotionID: promo.ID,
              BuyerID: assignment,
            })
            .toPromise()
        )
      }
    }
    // Delete assignments
    if (assignmentsToDelete.length > 0) {
      for (const assignment of assignmentsToDelete) {
        assignmentsToDeleteRequests.push(
          this.ocPromotionService
            .DeleteAssignment(promo.ID, {
              buyerID: assignment,
            })
            .toPromise()
        )
      }
    }

    await Promise.all([
      ...assignmentsToAddRequests,
      ...assignmentsToDeleteRequests,
    ])
  }

  // TODO: Find diff'd object and only 'PATCH' what changed?
  async updatePromotion(promo: Promotion<PromotionXp>): Promise<void> {
    try {
      this.dataIsSaving = true
      const updatedPromo = await this.ocPromotionService
        .Save(promo.ID, promo)
        .toPromise()
      await this.handlePromotionAssignments(updatedPromo)
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

  isSaveDisabled(): boolean {
    const promoXpBogo = this._promotionEditable?.xp?.BOGO
    if (this._promotionEditable?.xp?.Type === HSPromoType.BOGO) {
      return (
        !promoXpBogo?.Value ||
        !promoXpBogo?.BuySKU?.SKU ||
        !promoXpBogo?.BuySKU?.Qty ||
        !promoXpBogo?.GetSKU?.SKU ||
        !promoXpBogo?.GetSKU?.Qty ||
        this.resourceForm?.status === 'INVALID' ||
        this.dataIsSaving
      )
    }
    return this.resourceForm?.status === 'INVALID' || this.dataIsSaving
  }
}
