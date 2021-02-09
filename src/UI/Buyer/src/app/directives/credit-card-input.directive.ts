import { Directive, ElementRef, HostListener } from '@angular/core'
import { CreditCardFormatPipe } from '../pipes/credit-card-format.pipe'

@Directive({ selector: '[appCreditCardInput]' })
export class CreditCardInputDirective {
  constructor(
    private el: ElementRef,
    private creditCardFormat: CreditCardFormatPipe
  ) {}

  @HostListener('keyup')
  keyUp(): void {
    this.format()
  }

  // Cap the length of the field at 14 characters - 10 numbers plus 4 characters
  @HostListener('keydown', ['$event'])
  keyDown(event: KeyboardEvent): void {
    const key = event.keyCode
    if (
      !this.allowedKeys(key) &&
      this.el.nativeElement.value.length >= 23 &&
      key !== 65
    ) {
      event.preventDefault()
    }
  }

  format(): void {
    this.el.nativeElement.value = this.creditCardFormat.transform(
      this.el.nativeElement.value
    )
  }

  allowedKeys(key: number): boolean {
    if (!key) {
      return true
    }
    const isCtrlAltShift = 15 < key && key < 19
    const isArrowKeys = 37 <= key && key <= 40
    const isMetaKeys = key === 91
    const isDelete = key === 46 || key === 8
    const isTab = key === 9
    const isA = key === 65 // to allow select all
    return (
      isCtrlAltShift || isArrowKeys || isMetaKeys || isDelete || isTab || isA
    )
  }
}
