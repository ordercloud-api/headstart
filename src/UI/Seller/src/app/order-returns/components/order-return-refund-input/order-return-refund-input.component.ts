import {
  Component,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  EventEmitter,
  OnDestroy,
} from '@angular/core'
import { FormBuilder, ValidatorFn, Validators } from '@angular/forms'
import {
  debounceTime,
  distinctUntilChanged,
  takeWhile,
  filter,
} from 'rxjs/operators'
import { TypedFormGroup } from 'ngx-forms-typed'

export interface RefundInputChange {
  lineItemId: string
  refundAmount: number
}

type FormValue = { refundAmount: number }
type RefundInputForm = TypedFormGroup<FormValue>
@Component({
  selector: 'order-return-refund-input',
  templateUrl: './order-return-refund-input.component.html',
  styleUrls: ['./order-return-refund-input.component.scss'],
})
export class OrderReturnRefundInputComponent
  implements OnInit, OnChanges, OnDestroy
{
  @Input() lineItemRefundAmount: number
  @Input() orderTotal: number
  @Input() totalRefundAmount: number
  @Input() lineItemId: string
  @Input() disabled: boolean
  @Output() refundAmountChange = new EventEmitter<RefundInputChange>()
  alive = true
  form: RefundInputForm
  constructor(private formBuilder: FormBuilder) {}

  ngOnInit(): void {
    this.buildForm()
    this.onFormChanges()
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.didInputChange('disabled', changes)) {
      this.disabled
        ? this.form.controls.refundAmount.disable()
        : this.form.controls.refundAmount.enable()
    }
  }

  onFormChanges(): void {
    this.form.controls.refundAmount.valueChanges
      .pipe(
        takeWhile(() => this.alive),

        distinctUntilChanged(),
        debounceTime(500),
        filter((value) => value > 0 && value < this.getMaxRefundAmount())
      )
      .subscribe((refundAmount) => {
        this.refundAmountChange.emit({
          lineItemId: this.lineItemId,
          refundAmount,
        })
      })
  }

  buildForm(): void {
    this.form = this.formBuilder.group({
      refundAmount: [
        { value: this.lineItemRefundAmount, disabled: this.disabled },
        [Validators.min(0), this.maxValidator],
      ],
    }) as RefundInputForm
  }

  maxValidator: ValidatorFn = (control) => {
    const max = this.getMaxRefundAmount()
    const value = parseFloat(control.value)
    // Controls with NaN values after parsing should be treated as not having a
    // maximum, per the HTML forms spec: https://www.w3.org/TR/html5/forms.html#attr-input-max
    return !isNaN(value) && value > max
      ? { max: { max: max, actual: control.value as number } }
      : null
  }

  getMaxRefundAmount(): number {
    // multiply by 100 and then divide by 100 to avoid javascript decimal issues
    return (
      (this.orderTotal * 100 -
        this.totalRefundAmount * 100 +
        this.lineItemRefundAmount * 100) /
      100
    )
  }

  didInputChange(inputName: string, changes: SimpleChanges): boolean {
    if (changes[inputName]?.isFirstChange()) {
      return false
    }
    return (
      changes[inputName] &&
      changes[inputName].currentValue !== changes[inputName].previousValue
    )
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
