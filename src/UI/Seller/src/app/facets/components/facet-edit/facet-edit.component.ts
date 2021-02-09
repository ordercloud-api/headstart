import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { FacetService } from '@app-seller/facets/facet.service'
import { faTimesCircle } from '@fortawesome/free-solid-svg-icons'
import { ActiveToast, ToastrService } from 'ngx-toastr'
import { ProductFacets, ProductFacet } from 'ordercloud-javascript-sdk'
import { Router } from '@angular/router'
@Component({
  selector: 'app-facet-edit',
  templateUrl: './facet-edit.component.html',
  styleUrls: ['./facet-edit.component.scss'],
})
export class FacetEditComponent implements OnInit {
  @Input()
  filterConfig
  @Input() set resourceInSelection(facet: ProductFacet) {
    this.refreshFacetData(facet)
    this.createProductFacetForm(facet)
  }
  @Input()
  updatedResource
  @Output()
  updateResource = new EventEmitter<any>()
  _facetEditable: ProductFacet
  _facetStatic: ProductFacet
  resourceForm: FormGroup
  areChanges = false
  isCreatingNew: boolean
  dataIsSaving = false
  faTimesCircle = faTimesCircle
  constructor(
    public facetService: FacetService,
    private toaster: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isCreatingNew = this.facetService.checkIfCreatingNew()
  }

  refreshFacetData(facet: ProductFacet): void {
    this._facetEditable = facet
    this._facetStatic = facet
    this.areChanges = this.facetService.checkForChanges(
      this._facetEditable,
      this._facetStatic
    )
    this.createProductFacetForm(facet)
  }

  createProductFacetForm(facet: ProductFacet): void {
    this.resourceForm = new FormGroup({
      ID: new FormControl(facet.ID, Validators.required),
      Name: new FormControl(facet.Name, Validators.required),
      XpPath: new FormControl(facet.XpPath),
      Options: new FormControl(facet?.xp?.Options),
    })
  }

  async saveResource(): Promise<void> {
    // dataIsSaving indicator is used in the resource table to conditionally tell the
    // submit button to disable
    this._facetEditable.ID = this._facetEditable?.ID?.split(' ')
      .join('_')
      .trim()
      .replace(/[^a-zA-Z0-9-_ ]/g, '')
    this._facetEditable.XpPath = `Facets.${this._facetEditable?.ID}`
    try {
      this.dataIsSaving = true
      const newResource = await ProductFacets.Save(
        this._facetEditable.ID,
        this._facetEditable
      )
      this.router.navigateByUrl(`/facets/${newResource.ID}`)
      this.refreshFacetData(newResource)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async handleDelete(): Promise<void> {
    await ProductFacets.Delete(this._facetEditable?.ID)
    this.router.navigateByUrl('/facets')
  }

  handleDiscardChanges(): void {
    this._facetEditable = this._facetStatic
    this.refreshFacetData(this._facetStatic)
  }

  updateResourceFromEvent(event: any, field: string): void {
    if (field === 'ID') event.target.value = event.target.value.toLowerCase() // Facet IDs must be in lowercase
    field === 'Active'
      ? this.updateFacetResource({ value: event.target.checked, field })
      : this.updateFacetResource({ value: event.target.value, field })
    this.areChanges = this.facetService.checkForChanges(
      this._facetEditable,
      this._facetStatic
    )
  }

  updateFacetResource(facetUpdate: any): void {
    const resourceToUpdate =
      this._facetEditable || this.facetService.emptyResource
    this._facetEditable = this.facetService.getUpdatedEditableResource(
      facetUpdate,
      resourceToUpdate
    )
    this.areChanges = this.facetService.checkForChanges(
      this._facetEditable,
      this._facetStatic
    )
  }

  removeFacetOption(option: string): void {
    const copiedResource = this.facetService.copyResource(this._facetEditable)
    copiedResource.xp.Options = copiedResource.xp.Options.filter(
      (o) => o !== option
    )
    const event = {
      target: {
        value: copiedResource.xp.Options,
      },
    }
    this.updateResourceFromEvent(event, 'xp.Options')
  }

  addFacetOption(): ActiveToast<any> | void {
    const newFacetOptionInput = document.getElementById('newFacetOption') as any
    const copiedResource = this.facetService.copyResource(this._facetEditable)
    // If facet options are null or undefined, initialize as an empty array
    if (copiedResource.xp.Options === null) copiedResource.xp.Options = []
    // If proposed facet option already exists in the array, return warning
    if (copiedResource.xp.Options.includes(newFacetOptionInput.value)) {
      return this.toaster.warning(
        `The option "${newFacetOptionInput.value}" already exists.`
      )
    }
    copiedResource.xp.Options = copiedResource.xp.Options.concat(
      newFacetOptionInput.value
    )
    newFacetOptionInput.value = ''
    this.updateResourceFromEvent(
      { target: { value: copiedResource.xp.Options } },
      'xp.Options'
    )
  }

  facetOptionLimitReached(): boolean {
    return this._facetEditable?.xp?.Options?.length === 25
  }

  getSaveBtnText(): string {
    return this.facetService.getSaveBtnText(
      this.dataIsSaving,
      this.isCreatingNew
    )
  }
}
