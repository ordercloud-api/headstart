import {
  Component,
  Input,
  ChangeDetectorRef,
  Output,
  EventEmitter,
} from '@angular/core'
import {
  SwaggerSpecProperty,
} from '@app-seller/models/shared.types'
import { schemas } from './swagger-spec'
import { FormControl, FormGroup } from '@angular/forms'
import OrderCloudError from 'ordercloud-javascript-sdk/dist/utils/OrderCloudError'

@Component({
  selector: 'resource-edit-component',
  templateUrl: './resource-edit.component.html',
  styleUrls: ['./resource-edit.component.scss'],
})
export class ResourceEditComponent {
  _resource: any
  _resourceFields: SwaggerSpecProperty[]
  resourceForm: FormGroup

  constructor(private changeDetectorRef: ChangeDetectorRef,) {}

  @Input()
  set resource(value: any) {
    this._resource = value
    this.changeDetectorRef.detectChanges()
  }
  @Input()
  set resourceType(value: string) {
    this._resourceFields = this.buildResourceFields(value)
    this.resourceForm = this.buildForm(value);
  }

  @Output()
  updateResource = new EventEmitter<FormGroup>()

  handleUpdateResource() {
    this.updateResource.emit(this.resourceForm)
  }

 buildForm(resourceType: string): FormGroup {
   var formGroup = new FormGroup({});
   Object.entries(schemas[resourceType].properties)
   .forEach(([key, value]) => {
     if(key !== 'xp') {
       var control = new FormControl('', value['validators'])
       formGroup.addControl(key, control)
     }
   })
   return formGroup
 }

  buildResourceFields(resourceType: string): SwaggerSpecProperty[] {
    return Object.entries(schemas[resourceType].properties)
      .map(([key, value]) => {
        return {
          field: key,
          type: value['type'],
          maxLength: (value['maxLength'] || 1000 )
        }
      })
      .filter((r) => r.field !== 'xp')
  }
}
