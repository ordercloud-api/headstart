import { HSVariant } from '@ordercloud/headstart-sdk'
import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import { FormControl, FormBuilder } from '@angular/forms'
import { FormGroup, Validators } from '@angular/forms'
import { map as _map, find as _find } from 'lodash'
import { SpecOption, Spec } from 'ordercloud-javascript-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { FieldConfig, SpecFormEvent } from 'src/app/models/product.types'

@Component({
  template: `
    <form *ngIf="form" [formGroup]="form">
      <ng-container
        *ngFor="let field of config; let i = index"
        formArrayName="ctrls"
        [config]="field"
        [group]="form"
        [index]="i"
        [compact]="compact"
        ocSpecField
      ></ng-container>
    </form>
  `,
  styleUrls: ['./spec-form.component.scss'],
})
export class OCMSpecForm implements OnChanges {
  @Output()
  specFormChange: EventEmitter<SpecFormEvent> = new EventEmitter<SpecFormEvent>()
  @Output()
  isSelectionInactive: EventEmitter<boolean> = new EventEmitter<boolean>()
  config: FieldConfig[] = []
  form: FormGroup
  isValidAvailability: boolean
  controlInactive: boolean

  @Input() currency: string
  @Input() disabledVariants: HSVariant[]
  @Input() compact?: boolean = false // displays inputs in a compact way by setting them on a single line
  @Input() specs: Spec[]

  constructor(
    private fb: FormBuilder,
    private context: ShopperContextService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.specs) {
      this.init()
    }
  }

  init(): void {
    this.config = this.createFieldConfig()
    this.form = this.createGroup()
    this.form.valueChanges.subscribe(() => {
      this.handleChange()
    })
    // trigger change to update referencing components form validity
    this.handleChange()
  }

  createGroup(): FormGroup {
    const group = this.fb.group({
      ctrls: this.fb.array([]),
    })
    this.config.forEach((control) => {
      const ctrl = this.createControl(control)
      group.addControl(control.name, ctrl)
      // tslint:disable-next-line:no-string-literal
      ;(group.controls.ctrls as any).push(ctrl)
    })
    return group
  }

  createFieldConfig(): FieldConfig[] {
    const c: FieldConfig[] = []
    if (!this.specs) return c
    for (const spec of this.specs) {
      if (spec?.xp?.control === 'checkbox') {
        c.push(this.createCheckboxField(spec))
      } else if (spec?.xp?.control === 'range') {
        c.push(this.createRangeField(spec))
      } else if (spec?.Options.length === 1) {
        c.push(this.createSelectField(spec))
        // c.unshift(this.createLabelField(spec));
        // TODO: Possibly reinstate if needed, but for single option specs,
        // it does not have a value selected, so for required specs, this does not work.
      } else if (spec?.Options.length > 1) {
        c.push(this.createSelectField(spec))
      } else if (spec.AllowOpenText) {
        c.push(this.createInputField(spec))
      }
    }
    return c
  }

  createControl(config: FieldConfig): FormControl {
    const { disabled, validation, value } = config
    return new FormControl({ disabled, value }, validation)
  }

  handleChange(): void {
    this.validateChangeAvailability(this.form, this.disabledVariants)
    this.specFormChange.emit({
      form: this.form,
    })
  }

  validateChangeAvailability(
    form: FormGroup,
    disabledVariants: HSVariant[]
  ): void {
    let controlInactive = false
    if (!disabledVariants) {
      return
    }
    if (disabledVariants?.length < 1) {
      return
    }
    for (const disabledVariant of disabledVariants) {
      if (this.isControlInactive(form.value.ctrls, disabledVariant)) {
        controlInactive = true
        this.isSelectionInactive.emit(controlInactive)
        return
      }
    }

    if (!controlInactive) {
      this.isSelectionInactive.emit(controlInactive)
    }
  }

  isControlInactive(
    ctrls: string[],
    disabledVariant: HSVariant
  ): boolean {
    let controlCount = 0
    for (const variant of disabledVariant.Specs) {
      ctrlLoop: for (const controlValue of ctrls) {
        if (variant.Value === controlValue) {
          controlCount = controlCount + 1
          if (controlCount === ctrls.length) {
            return true
          }
          break ctrlLoop
        }
      }
    }
    return false
  }

  private createCheckboxField(spec: Spec): FieldConfig {
    return {
      type: 'checkbox',
      label: spec.Name,
      name: spec.Name.replace(/ /g, ''),
      value: spec.DefaultOptionID,
      options: _map(spec.Options, 'Value'),
      validation: [Validators.nullValidator],
    }
  }

  private createRangeField(spec: Spec): FieldConfig {
    return {
      type: 'range',
      label: spec.Name,
      name: spec.Name.replace(/ /g, ''),
      value: spec.DefaultValue,
      min: Math.min(
        ..._map(spec.Options, (option: SpecOption) => +option.Value)
      ),
      max: Math.max(
        ..._map(spec.Options, (option: SpecOption) => +option.Value)
      ),
      validation: [
        spec.Required ? Validators.required : Validators.nullValidator,
        Validators.min(
          Math.min(..._map(spec.Options, (option: SpecOption) => +option.Value))
        ),
        Validators.max(
          Math.max(..._map(spec.Options, (option: SpecOption) => +option.Value))
        ),
      ],
    }
  }

  private createSelectField(spec: Spec): FieldConfig {
    return {
      type: 'select',
      label: spec.Name,
      name: spec.Name.replace(/ /g, ''),
      value: spec.DefaultOptionID
        ? _find(spec.Options, (option) => {
            return option.ID === spec.DefaultOptionID
          }).Value
        : null,
      options: _map(spec.Options),
      disabledVariants: this.disabledVariants,
      validation: [
        spec.Required ? Validators.required : Validators.nullValidator,
      ],
      currency: this.context.currentUser.get().Currency,
    }
  }

  private createLabelField(spec: Spec): FieldConfig {
    return {
      type: 'label',
      label: spec.Name,
      name: spec.Name.replace(/ /g, ''),
      options: _map(spec.Options, 'Value'),
    }
  }

  private createInputField(spec: Spec): FieldConfig {
    return {
      type: 'input',
      label: spec.Name,
      name: spec.Name.replace(/ /g, ''),
      value: spec.DefaultValue,
      validation: [
        spec.Required ? Validators.required : Validators.nullValidator,
        spec.xp?.CharacterLimit > 0
          ? Validators.maxLength(spec.xp.CharacterLimit)
          : Validators.nullValidator,
      ],
    }
  }
}
