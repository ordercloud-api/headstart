import { Component, OnInit } from '@angular/core'
import { FormGroup, FormArray, AbstractControl } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'
@Component({
  selector: 'spec-form-checkbox',
  template: `
    <div class="form-group" [formGroup]="group">
      <input
        type="checkbox"
        class="form-control form-control-sm"
        [formControlName]="config.name"
      />{{ config.label }}
    </div>
  `,
  styleUrls: ['./spec-form-checkbox.component.scss'],
})
export class SpecFormCheckboxComponent implements Field, OnInit {
  config: FieldConfig
  group: FormGroup
  index: number
  ctrls: FormArray

  constructor() {}

  ngOnInit(): void {
    this.ctrls = this.group.get('ctrls') as FormArray
  }

  byIndex(index: number): AbstractControl {
    return this.ctrls.at(index)
  }
}
