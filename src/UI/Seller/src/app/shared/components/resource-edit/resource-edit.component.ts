import {
  Component,
  Input,
  ChangeDetectorRef,
  Output,
  EventEmitter,
} from '@angular/core'
import {
  ResourceUpdate,
  SwaggerSpecProperty,
} from '@app-seller/models/shared.types'
import { schemas } from './swagger-spec'
import { FormControl, FormGroup } from '@angular/forms'

@Component({
  selector: 'resource-edit-component',
  templateUrl: './resource-edit.component.html',
  styleUrls: ['./resource-edit.component.scss'],
})
export class ResourceEditComponent {
  _resource: any
  _resourceFields: SwaggerSpecProperty[]
  _resourceType: string
  resourceForm: FormGroup

  constructor(private changeDetectorRef: ChangeDetectorRef) {}

  @Input()
  set resource(value: any) {
    this._resource = value
    this.changeDetectorRef.detectChanges()
    this.resourceForm = this.buildForm(value)
  }
  @Input()
  set resourceType(value: string) {
    this._resourceType = value
    this._resourceFields = this.buildResourceFields(value)
  }

  @Output()
  updateResource = new EventEmitter<ResourceUpdate>()

  handleUpdateResource(event: any, fieldType: string) {
    const resourceupdate = {
      field: event.target.id,
      value:
        fieldType === 'boolean' ? event.target.checked : event.target.value,
      form: this.resourceForm,
    }
    this.updateResource.emit(resourceupdate)
  }

  buildForm(resource: any): FormGroup {
    const formGroup = new FormGroup({})
    Object.entries(schemas[this._resourceType]?.properties).forEach(
      ([key, value]) => {
        if (key !== 'xp') {
          const control = new FormControl(resource[key], value['validators'])
          formGroup.addControl(key, control)
        }
      }
    )
    return formGroup
  }

  buildResourceFields(resourceType: string): SwaggerSpecProperty[] {
    return Object.entries(schemas[resourceType]?.properties)
      .map(([key, value]) => {
        return {
          field: key,
          type: value['type'],
          maxLength: value['maxLength'] || 1000,
        }
      })
      .filter((r) => r.field !== 'xp')
  }
}
