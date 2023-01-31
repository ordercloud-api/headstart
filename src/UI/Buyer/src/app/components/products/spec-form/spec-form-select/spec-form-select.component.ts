import { Component, OnInit } from '@angular/core'
import { UntypedFormGroup, UntypedFormArray, AbstractControl } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'

@Component({
  selector: 'spec-form-select',
  template: `
    <div class="container px-0">
      <div
        class="mb-3 justify-content-center"
        [formGroup]="group"
        [class.row]="compact"
      >
        <label
          class="form-label text-uppercase fw-bolder small text-muted mb-0 d-flex align-items-center"
          [class.col-3]="compact"
          for="{{ config.name }}"
          >{{ config.label }}
        </label>
        <select
          [formControlName]="config.name"
          id="{{ config.name }}"
          class="form-select"
          [class.col-7]="compact"
          value="{{ config.value }}"
        >
          <option *ngIf="!config.value" value=""></option>
          <option
            *ngFor="let option of config.options"
            value="{{ option.Value }}"
          >
            {{ option.Value }}
            <span *ngIf="option.PriceMarkup">
              (+ {{ option.PriceMarkup | currency : config.currency }})</span
            >
          </option>
        </select>
      </div>
    </div>
  `,
  styleUrls: ['./spec-form-select.component.scss'],
})
export class SpecFormSelectComponent implements Field, OnInit {
  config: FieldConfig
  group: UntypedFormGroup
  index: number
  compact?: boolean
  ctrls: UntypedFormArray

  ngOnInit(): void {
    this.ctrls = this.group.get('ctrls') as UntypedFormArray
  }

  byIndex(index: number): AbstractControl {
    return this.ctrls.at(index)
  }
}
