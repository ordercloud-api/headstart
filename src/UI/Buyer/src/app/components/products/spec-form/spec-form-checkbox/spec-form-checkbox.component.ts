import { Component, OnInit } from '@angular/core'
import { FormGroup, FormArray, AbstractControl } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'
@Component({
  selector: 'spec-form-checkbox',
  template: `
    <div class="form-check mb-3" [formGroup]="group">
      <input
        [id]="config.label"
        type="checkbox"
        class="form-check-input form-control form-control-sm"
        [formControlName]="config.name"
      />
      <label [for]="config.label" class="form-check-label">{{
        config.label
      }}</label>
    </div>
  `,
  styleUrls: ['./spec-form-checkbox.component.scss'],
})
export class SpecFormCheckboxComponent implements Field, OnInit {
  config: FieldConfig
  group: FormGroup
  index: number
  ctrls: FormArray

  ngOnInit(): void {
    this.ctrls = this.group.get('ctrls') as FormArray
  }

  byIndex(index: number): AbstractControl {
    return this.ctrls.at(index)
  }
}
