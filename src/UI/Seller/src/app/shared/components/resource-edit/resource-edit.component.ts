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

@Component({
  selector: 'resource-edit-component',
  templateUrl: './resource-edit.component.html',
  styleUrls: ['./resource-edit.component.scss'],
})
export class ResourceEditComponent {
  _resource: any
  _resourceFields: SwaggerSpecProperty[]

  constructor(private changeDetectorRef: ChangeDetectorRef) {}

  @Input()
  set resource(value: any) {
    this._resource = value
    this.changeDetectorRef.detectChanges()
  }
  @Input()
  set resourceType(value: string) {
    this._resourceFields = this.buildResourceFields(value)
  }
  @Output()
  updateResource = new EventEmitter<ResourceUpdate>()

  handleUpdateResource(event: any, fieldType: string) {
    const resourceUpdate = {
      field: event.target.id,
      value:
        fieldType === 'boolean' ? event.target.checked : event.target.value,
    }
    this.updateResource.emit(resourceUpdate)
  }

  buildResourceFields(resourceType: string): SwaggerSpecProperty[] {
    return Object.entries(schemas[resourceType].properties)
      .map(([key, value]) => {
        return {
          field: key,
          type: value['type'],
        }
      })
      .filter((r) => r.field !== 'xp')
  }
}
