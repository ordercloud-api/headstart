import { Component, OnInit } from '@angular/core'
import { UntypedFormGroup, UntypedFormArray, AbstractControl } from '@angular/forms'
import { Field, FieldConfig } from 'src/app/models/product.types'
import { specErrors } from '../errors'

@Component({
  selector: 'spec-form-textarea',
  template: `
    <div [formGroup]="group">
      <div class="form-input" [class.row]="compact">
        <label [for]="config.name" class="form-label" [class.col-3]="compact">{{
          config.label
        }}</label>
        <textarea
          id="config.name"
          type="text"
          [formControlName]="config.name"
          class="form-control form-control-sm text-area"
          [class.col-9]="compact"
          [attr.rows]="config.rows"
          [attr.maxlength]="config.max"
        ></textarea>
        <div
          *ngIf="
            byIndex(index).invalid &&
            (byIndex(index).dirty || byIndex(index).touched)
          "
          alert="alert alert-danger"
        >
          <div *ngIf="byIndex(index).errors['required']">
            {{ errorMsgs.required }}
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./spec-form-textarea.component.scss'],
})
export class SpecFormTextAreaComponent implements Field, OnInit {
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
