import { Component, OnInit } from '@angular/core'
import { UntypedFormGroup, UntypedFormArray, AbstractControl } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'
import { specErrors } from '../errors'

@Component({
  selector: 'spec-form-number',
  template: `
    <div [formGroup]="group">
      <div class="form-input" [class.row]="compact">
        <label
          [for]="config.label"
          [class.col-3]="compact"
          class="form-label"
          >{{ config.label }}</label
        >
        <input
          [id]="config.label"
          type="number"
          [attr.min]="config.min"
          [attr.step]="config.step"
          class="form-control form-control-sm"
          [class.col-9]="compact"
          [attr.placeholder]="config.placeholder"
          [formControlName]="config.name"
        />
        <div
          *ngIf="
            byIndex(index).invalid &&
            (byIndex(index).dirty || byIndex(index).touched)
          "
          alert="alert alert-danger"
        >
          <div
            *ngIf="
              byIndex(index).errors['required'] ||
              byIndex(index).errors['min'] ||
              byIndex(index).errors['max']
            "
          >
            <div *ngIf="byIndex(index).errors['required']">
              {{ errorMsgs.required }}
            </div>
            <div *ngIf="byIndex(index).errors['min']">{{ errorMsgs.min }}</div>
            <div *ngIf="byIndex(index).errors['max']">{{ errorMsgs.max }}</div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./spec-form-number.component.scss'],
})
export class SpecFormNumberComponent implements Field, OnInit {
  config: FieldConfig
  group: UntypedFormGroup
  ctrls: UntypedFormArray
  index: number
  errorMsgs = specErrors

  ngOnInit(): void {
    this.ctrls = this.group.get('ctrls') as UntypedFormArray
  }

  byIndex(index: number): AbstractControl {
    return this.ctrls.at(index)
  }
}
