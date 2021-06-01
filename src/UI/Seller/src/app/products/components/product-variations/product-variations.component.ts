import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import {
  Variant,
  SpecOption,
  Spec,
  OcSpecService,
} from '@ordercloud/angular-sdk'
import {
  faExclamationCircle,
  faCog,
  faTrash,
  faTimesCircle,
  faCheckDouble,
  faPlusCircle,
  faCaretRight,
  faCaretDown,
  faCheckCircle,
} from '@fortawesome/free-solid-svg-icons'
import { ProductService } from '@app-seller/products/product.service'
import { ToastrService } from 'ngx-toastr'
import {
  SuperHSProduct,
  HSVariant,
  ImageAsset,
} from '@ordercloud/headstart-sdk'
import { BehaviorSubject } from 'rxjs'
import { Products } from 'ordercloud-javascript-sdk'
import { SupportedRates } from '@app-seller/shared'

@Component({
  selector: 'product-variations-component',
  templateUrl: './product-variations.component.html',
  styleUrls: ['./product-variations.component.scss'],
})
export class ProductVariations implements OnChanges {
  @Input()
  set superHSProductEditable(superProductEditable: SuperHSProduct) {
    this.superProductEditable = superProductEditable
    this.variants.next(superProductEditable?.Variants)
    this.variantInSelection = {}
    this.canConfigureVariations = !!superProductEditable?.Product?.ID
    this.addVariableTextSpecs = this.addVariableTextSpecs || superProductEditable?.Specs?.some(
      (s) => s.AllowOpenText
    )
    this.editSpecs = this.editSpecs || superProductEditable?.Specs?.some((s) => !s.AllowOpenText)
  }
  @Input()
  set superHSProductStatic(superProductStatic: SuperHSProduct) {
    this.superProductStatic = superProductStatic
  }
  @Input() areChanges: boolean
  @Input() readonly = false
  @Input() myCurrency: SupportedRates
  @Input() checkForChanges
  @Input() copyProductResource
  @Input() isCreatingNew = false
  get specsWithVariations(): Spec[] {
    return this.superProductEditable?.Specs?.filter(
      (s) => s.DefinesVariant && !s.AllowOpenText
    ) as Spec[]
  }
  get specsWithoutVariations(): Spec[] {
    return this.superProductEditable?.Specs?.filter(
      (s) => !s.DefinesVariant && !s.AllowOpenText
    )
  }
  get nonVariableTextSpecs(): Spec[] {
    return this.superProductEditable?.Specs?.filter((s) => !s.AllowOpenText)
  }

  get variableTextSpecs(): Spec[] {
    return this.superProductEditable?.Specs?.filter((s) => s.AllowOpenText)
  }
  @Output()
  productVariationsChanged = new EventEmitter<SuperHSProduct>()
  @Output() skuUpdated = new EventEmitter<SuperHSProduct>()
  @Output() variantsValidated = new EventEmitter<boolean>()
  @Output() specsValidated = new EventEmitter<boolean>()
  superProductEditable: SuperHSProduct
  superProductStatic: SuperHSProduct
  variants: BehaviorSubject<HSVariant[]>
  specOptAdded = new EventEmitter<SpecOption>()
  canConfigureVariations = false
  areSpecChanges = false
  definesVariant = false
  variantsValid = true
  specOptionsExistOnSpec = true
  editSpecs = false
  faTrash = faTrash
  faCog = faCog
  faPlusCircle = faPlusCircle
  faTimesCircle = faTimesCircle
  faCheckDouble = faCheckDouble
  faCaretRight = faCaretRight
  faCaretDown = faCaretDown
  faCheckCircle = faCheckCircle
  faExclamationCircle = faExclamationCircle
  assignVariantImages = false
  viewVariantDetails = false
  variantInSelection: Variant
  imageInSelection: ImageAsset
  addVariableTextSpecs = false
  customizationRequired = true

  constructor(
    private productService: ProductService,
    private toasterService: ToastrService,
    private ocSpecService: OcSpecService,
  ) {
    this.variants = new BehaviorSubject<HSVariant[]>([])
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes?.superHSProductEditable) {
      this.variants.next(
        changes?.superHSProductEditable?.currentValue?.Variants
      )
    }
  }

  getTotalMarkup = (specOptions: SpecOption[]): number => {
    let totalMarkup = 0
    if (specOptions) {
      specOptions.forEach((opt) =>
        opt.PriceMarkup ? (totalMarkup = +totalMarkup + +opt.PriceMarkup) : 0
      )
    }
    return totalMarkup
  }

  toggleEditSpecs(): void {
    if (this.editSpecs) {
      const updateProductResourceCopy = this.productService.copyResource(
        this.superProductEditable || this.productService.emptyResource
      )
      // Remove all specs that are *not* variable text specs
      updateProductResourceCopy.Specs = updateProductResourceCopy.Specs.filter((s) => s.AllowOpenText)
      updateProductResourceCopy.Variants = []
      this.superProductEditable = updateProductResourceCopy
      this.productVariationsChanged.emit(this.superProductEditable)
    } else {
      this.superProductEditable.Variants = this.superProductStatic.Variants
      this.superProductEditable.Specs = [
        ...(this.superProductStatic?.Specs || []),
        ...(this.superProductEditable?.Specs?.filter((s) => s.AllowOpenText) || []),
      ]
    }
    this.checkForSpecChanges()
    this.editSpecs = !this.editSpecs
  }

  handleDiscardSpecChanges(): void {
    this.editSpecs = !this.editSpecs
    this.superProductEditable.Specs = this.superProductEditable?.Specs
    this.checkForSpecChanges()
  }

  checkForSpecChanges(): void {
    this.areSpecChanges =
      JSON.stringify(this.superProductEditable?.Specs) !==
      JSON.stringify(this.superProductStatic?.Specs)
    if (this.areSpecChanges) {
      this.specOptionsExistOnSpec = this.hasSpecOptions()
    }
    this.specsValidated.emit(this.specOptionsExistOnSpec)
  }

  hasSpecOptions(): boolean {
    return this.superProductEditable?.Specs?.every(
      (spec) => spec?.Options?.length || spec.AllowOpenText
    )
  }

  shouldDefinesVariantBeChecked(): boolean {
    if (this.definesVariant) return true
    if (this.variants?.getValue()?.length >= 100) return false
  }

  shouldDisableAddSpecOptBtn(spec: Spec): boolean {
    if (this.readonly) {
      return true
    }
    if (!this.variantsValid) {
      return true
    } else {
      return this.variants?.getValue().length === 100 && spec.DefinesVariant
    }
  }

  toggleActive(variant: Variant): void {
    variant.Active = !variant.Active

    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Variants.find((x) => x.ID === variant.ID).Active =
      variant.Active
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
  }

  getVariantStatusDisplay(variant: Variant): string {
    if (variant.Active) {
      return 'Active'
    } else {
      return 'Inactive'
    }
  }

  updateSku($event: any, i: number): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Variants[
      i
    ].xp.NewID = $event.target.value.replace(/[^a-zA-Z0-9_-]/g, '')
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
  }

  updateVariantInventory($event: any, i: number): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    if (
      updateProductResourceCopy.Variants[i].Inventory === null ||
      updateProductResourceCopy.Variants[i].Inventory === undefined
    ) {
      updateProductResourceCopy.Variants[i].Inventory = { QuantityAvailable: 0 }
    }
    updateProductResourceCopy.Variants[i].Inventory.QuantityAvailable = Number(
      $event.target.value
    )
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
  }

  toggleAddVariableTextSpecs(event: any): void {
    if (this.addVariableTextSpecs) {
      const updateProductResourceCopy = this.productService.copyResource(
        this.superProductEditable || this.productService.emptyResource
      )
      // Remove all specs that are variable text specs
      updateProductResourceCopy.Specs = updateProductResourceCopy.Specs.filter(
        (s) => !s.AllowOpenText
      )
      this.superProductEditable = updateProductResourceCopy
      this.checkForSpecChanges()
      this.productVariationsChanged.emit(this.superProductEditable)
    }
    this.addVariableTextSpecs = event?.target?.checked
  }

  getSpecIndex(specID: string): number {
    return this.superProductEditable?.Specs?.findIndex((s) => s.ID === specID)
  }

  addVariableTextSpec(): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    const input = document.getElementById('AddVariableTextSpec') as any
    const charLimitInput = document.getElementById('CharacterLimit') as any
    if (input.value === '') {
      this.toasterService.warning(
        'Please provide a description of what the buyer will be customizing.',
        'Warning'
      )
      return
    }
    if (Number(charLimitInput?.value) < 0) {
      this.toasterService.warning(
        'Character limit must be a positive number.',
        'Warning'
      )
      return
    }
    const newSpec: Spec[] | any = [
      {
        ID: `${updateProductResourceCopy.Product.ID}${input.value
          .split(' ')
          .join('-')
          .replace(/[^a-zA-Z0-9 ]/g, '')}`,
        Name: input.value,
        // If AllowOpenText is trye - DefinesVariant _MUST_ be false (platform requirement)
        AllowOpenText: true,
        Required: this.customizationRequired,
        DefinesVariant: false,
        ListOrder: (updateProductResourceCopy.Specs?.length || 0) + 1,
        Options: [],
        xp: {
          CharacterLimit:
            Number(charLimitInput?.value) === 0
              ? null
              : Number(charLimitInput?.value),
        },
      },
    ]
    input.value = ''
    charLimitInput.value = ''
    updateProductResourceCopy.Specs = updateProductResourceCopy.Specs.concat(
      newSpec
    )
    this.superProductEditable = updateProductResourceCopy
    this.customizationRequired = true
    this.checkForSpecChanges()
    this.productVariationsChanged.emit(this.superProductEditable)
  }

  addSpec(): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    const input = document.getElementById('AddVariation') as any
    if (input.value === '') {
      this.toasterService.warning('Please name your variation')
      return
    }
    const newSpec: Spec[] | any = [
      {
        ID: `${updateProductResourceCopy.Product.ID}${input.value
          .split(' ')
          .join('-')
          .replace(/[^a-zA-Z0-9 ]/g, '')}`,
        Name: input.value,
        // If this.definesVariant - AllowOptenText _MUST_ be false (platform requirement)
        AllowOpenText: false,
        Required: this.definesVariant,
        DefinesVariant: this.definesVariant,
        ListOrder: (updateProductResourceCopy.Specs?.length || 0) + 1,
        Options: [],
      },
    ]
    input.value = ''
    updateProductResourceCopy.Specs = updateProductResourceCopy.Specs.concat(
      newSpec
    )
    this.superProductEditable = updateProductResourceCopy
    this.definesVariant = false
    this.checkForSpecChanges()
    this.productVariationsChanged.emit(this.superProductEditable)
  }
  addSpecOption(spec: Spec): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    // TODO: Browser compatability for .findIndex?
    const specIndex = this.getSpecIndex(spec.ID)
    const input = document.getElementById(`${spec.ID}`) as any
    const markup = (document.getElementById(`${spec.ID}Markup`) as any).value
    if (input.value === '') {
      this.toasterService.warning('Please name your option')
      return
    }
    const newOption = [
      {
        ID: input.value
          .split(' ')
          .join('-')
          .trim()
          .replace(/[^a-zA-Z0-9 ]/g, ''),
        Value: input.value,
        ListOrder: spec.Options.length + 1,
        IsOpenText: false,
        PriceMarkupType: markup ? 1 : 'NoMarkup',
        PriceMarkup: markup,
        xp: null,
      },
    ]
    if (!updateProductResourceCopy.Specs[specIndex].Options.length)
      updateProductResourceCopy.Specs[specIndex].DefaultOptionID =
        newOption[0].ID
    if (!updateProductResourceCopy.Specs[specIndex].DefaultOptionID)
      updateProductResourceCopy.Specs[specIndex].DefaultOptionID =
        updateProductResourceCopy.Specs[specIndex].Options[0].ID
    updateProductResourceCopy.Specs[
      specIndex
    ].Options = updateProductResourceCopy.Specs[specIndex].Options.concat(
      newOption
    )
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
    this.mockVariants()
    this.checkForSpecChanges()
  }

  removeSpecOption(spec: Spec, optionIndex: number): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    const specIndex = this.getSpecIndex(spec.ID)
    if (
      updateProductResourceCopy.Specs[specIndex].DefaultOptionID ===
      updateProductResourceCopy.Specs[specIndex].Options[optionIndex].ID
    )
      updateProductResourceCopy.Specs[specIndex].DefaultOptionID = null
    updateProductResourceCopy.Specs[specIndex].Options.splice(optionIndex, 1)
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
    this.mockVariants()
    this.checkForSpecChanges()
  }

  removeSpec(spec: Spec): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Specs = updateProductResourceCopy.Specs.filter(
      (s) => s.ID !== spec.ID
    )
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
    this.mockVariants()
    this.checkForSpecChanges()
  }

  mockVariants(): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Variants = this.generateVariantsFromCurrentSpecs()
    this.superProductEditable = updateProductResourceCopy
    this.productVariationsChanged.emit(this.superProductEditable)
    this.validateVariants()
    this.variantsValidated.emit(this.variantsValid)
    this.checkForChanges()
  }

  validateVariants(): void {
    this.variantsValid = this.superProductEditable.Variants.length <= 100
    this.variantsValid = this.superProductEditable?.Variants?.length <= 100
  }

  generateVariantsFromCurrentSpecs(): Variant[] {
    let specsDefiningVariants = this.specsWithVariations
    specsDefiningVariants = specsDefiningVariants.sort(
      (a, b) => a.ListOrder - b.ListOrder
    )
    const firstSpec = specsDefiningVariants[0]
    let variants = this.createVariantsForFirstSpec(firstSpec)
    for (let i = 1; i < specsDefiningVariants.length; i++) {
      variants = this.combineSpecOptions(variants, specsDefiningVariants[i])
    }
    return variants
  }

  createVariantsForFirstSpec(spec: Spec): Variant[] {
    if (!spec) return []
    return spec.Options.map((opt) => {
      return {
        ID: `${this.superProductEditable.Product.ID}-${opt.ID}`,
        Name: `${this.superProductEditable.Product.ID} ${opt.Value}`,
        Active: true,
        Inventory: {
          QuantityAvailable: 0,
        },
        xp: {
          SpecCombo: `${opt.ID}`,
          SpecValues: [
            {
              SpecName: spec.Name,
              SpecOptionValue: opt.Value,
              PriceMarkup: opt.PriceMarkup,
            },
          ],
        },
      }
    })
  }

  combineSpecOptions(workingVariantList: Variant[], spec: Spec): Variant[] {
    const newVariantList = []
    workingVariantList.forEach((variant) => {
      spec.Options.forEach((opt) => {
        newVariantList.push({
          ID: `${variant.ID}-${opt.ID}`,
          Name: `${variant.Name} ${opt.Value}`,
          Active: true,
          Inventory: {
            QuantityAvailable: 0,
          },
          xp: {
            SpecCombo: `${variant.xp.SpecCombo}-${opt.ID}`,
            SpecValues: [
              ...variant.xp.SpecValues,
              {
                SpecName: spec.Name,
                SpecOptionValue: opt.Value,
                PriceMarkup: opt.PriceMarkup,
              },
            ],
          },
        })
      })
    })
    return newVariantList
  }

  getPriceMarkup = (specOption: SpecOption): number =>
    !specOption.PriceMarkup ? 0 : specOption.PriceMarkup

  isDefaultSpecOption = (specID: string, optionID: string): boolean => {
    const specIndex = this.getSpecIndex(specID)
    return (
      this.superProductEditable?.Specs[specIndex]?.DefaultOptionID === optionID
    )
  }
  disableSpecOption = (specID: string, option: SpecOption): boolean => {
    if (this.readonly) {
      return true
    }
    const specIndex = this.getSpecIndex(specID)
    return this.isCreatingNew
      ? false
      : !JSON.stringify(
        this.superProductStatic?.Specs[specIndex]?.Options
      )?.includes(JSON.stringify(option))
  }

  stageDefaultSpecOption(specID: string, optionID: string): void {
    const specIndex = this.getSpecIndex(specID)
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Specs[specIndex].DefaultOptionID = optionID
    this.superProductEditable = updateProductResourceCopy
  }

  async setDefaultSpecOption(specID: string, optionID: string): Promise<void> {
    const specIndex = this.getSpecIndex(specID)
    const updateProductResourceCopy = this.productService.copyResource(
      this.superProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Specs[specIndex].DefaultOptionID = optionID
    this.superProductEditable.Specs = updateProductResourceCopy.Specs
    await this.ocSpecService
      .Patch(specID, { DefaultOptionID: optionID })
      .toPromise()
  }

  isImageSelected(img: ImageAsset): boolean {
    if (!img.Tags) img.Tags = []
    return img.Tags.includes(this.variantInSelection?.xp?.SpecCombo)
  }

  openVariantDetails(variant: Variant): void {
    const variantBehaviorSubjectValue = this.variants?.getValue()
    this.viewVariantDetails = true
    this.variantInSelection = variant
    if (variantBehaviorSubjectValue !== null) {
      this.variantInSelection =
        variantBehaviorSubjectValue[
        variantBehaviorSubjectValue?.indexOf(variant)
        ]
    }
  }

  closeVariantDetails(): void {
    this.viewVariantDetails = false
    this.variantInSelection = null
  }

  toggleAssignImage(img: ImageAsset, specCombo: string): void {
    this.imageInSelection = img
    if (!this.imageInSelection.Tags) this.imageInSelection.Tags = []
    this.imageInSelection.Tags.includes(specCombo)
      ? this.imageInSelection.Tags.splice(this.imageInSelection.Tags.indexOf(specCombo), 1)
      : this.imageInSelection.Tags.push(specCombo)
  }

  async updateProductImageTags(): Promise<void> {
    const images = this.superProductEditable.Product?.xp?.Images
    const patchObj = {
      xp: {
        Images: images
      }
    }
    await Products.Patch(this.superProductEditable.Product.ID, patchObj)
    Object.assign(this.superProductStatic.Product, this.superProductEditable.Product)
    this.imageInSelection = {}
    this.assignVariantImages = false
  }

  getVariantImages(variant: Variant): ImageAsset[] {
    this.superProductEditable?.Product?.xp?.Images?.forEach((i) =>
      !i.Tags ? (i.Tags = []) : null
    )
    const imgs = this.superProductEditable?.Product?.xp?.Images?.filter((i) =>
      i.Tags.includes(variant?.xp?.SpecCombo)
    )
    return imgs
  }

  getVariantDetailColSpan(): number {
    const colSpan = 4 + this.superProductEditable?.Specs?.length
    if (this.superProductEditable?.Product?.Inventory?.VariantLevelTracking)
      colSpan + 1
    return colSpan
  }

  async patchVariant(variantID: string, partial: Partial<Variant>) {
    const patchedVariant = await Products.PatchVariant<HSVariant>(
      this.superProductEditable.Product?.ID,
      this.variantInSelection.ID,
      partial
    )
    const variants = this.variants?.getValue();
    if (variants !== null) {
      const index = variants.findIndex(variant => variant.ID === variantID)
      variants[index] = JSON.parse(JSON.stringify(patchedVariant))
      this.variants.next(variants)
      this.variantInSelection = this.variants?.getValue()[index]
    }
  }

  async variantShippingDimensionUpdate(
    event: any,
    field: string,
    index: number
  ): Promise<void> {
    let partialVariant: Variant = {}
    // If there's no value, or the value didn't change, don't send request.
    if (event.target.value === '') return
    if (Number(event.target.value) === this.variantInSelection[field]) return
    const value = Number(event.target.value)
    switch (field) {
      case 'ShipWeight':
        partialVariant = { ShipWeight: value }
        break
      case 'ShipHeight':
        partialVariant = { ShipHeight: value }
        break
      case 'ShipWidth':
        partialVariant = { ShipWidth: value }
        break
      case 'ShipLength':
        partialVariant = { ShipLength: value }
        break
    }
    try {
      await this.patchVariant(this.variantInSelection.ID, partialVariant)
      this.toasterService.success('Shipping dimensions updated', 'OK')
    } catch (err) {
      console.log(err)
      this.toasterService.error('Something went wrong', 'Error')
    }
  }
}
