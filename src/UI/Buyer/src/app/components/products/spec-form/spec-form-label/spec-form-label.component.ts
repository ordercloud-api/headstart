import { Component } from '@angular/core'
import { FormGroup } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'
import { specErrors } from '../errors'

@Component({
  selector: 'spec-form-label',
  template: `
    <div
      class="mb-3 mb-0 justify-content-center"
      [formGroup]="group"
      [class.row]="compact"
    >
      <label
        class="form-label text-uppercase fw-bolder small text-muted d-flex align-items-center"
        [class.col-3]="compact"
        for="{{ config.name }}-readonly"
        >{{ config.label }}
      </label>
      <input
        class="form-control"
        [class.col-7]="compact"
        type="text"
        readonly
        placeholder="{{ config.options[0] }}"
        id="{{ config.name }}-readonly"
      />
    </div>
  `,
  styleUrls: ['./spec-form-label.component.scss'],
})
export class SpecFormLabelComponent implements Field {
  config: FieldConfig
  group: FormGroup
  index: number
  compact?: boolean
  errorMsgs = specErrors
}
