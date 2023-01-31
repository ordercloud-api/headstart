import {
  Component,
  Input,
  EventEmitter,
  Output,
  ViewEncapsulation,
} from '@angular/core'
import { FormGroup, UntypedFormControl } from '@angular/forms'
import { get as _get } from 'lodash'

@Component({
  selector: 'reactive-quill-editor-component',
  templateUrl: './reactive-quill-editor.component.html',
  styleUrls: ['./reactive-quill-editor.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class ReactiveQuillComponent {
  _updatedResource: any
  _formControlForText: UntypedFormControl
  quillChangeSubscription: any

  @Input()
  pathOnResource: string
  @Output()
  resourceUpdated = new EventEmitter()
  @Input()
  set formControlForText(value: UntypedFormControl) {
    this._formControlForText = value
    this.setQuillChangeEvent()
  }

  @Input()
  set resourceInSelection(resource: any) {
    this.setQuillChangeEvent()
  }

  @Input()
  readonly: boolean

  setQuillChangeEvent() {
    if (this._formControlForText) {
      this.quillChangeSubscription = this._formControlForText.valueChanges.subscribe(
        (newFormValue) => {
          this.resourceUpdated.emit({
            field: this.pathOnResource,
            value: newFormValue,
          })
        }
      )
    }
  }
}
