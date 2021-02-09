import { Directive, ElementRef, HostListener } from '@angular/core'
import { PhoneFormatPipe } from '@app-seller/shared/pipes/phone-format.pipe'

@Directive({ selector: '[appPhoneInput]' })
// Directive class
export class PhoneInputDirective {
  constructor(private el: ElementRef, private phoneFormat: PhoneFormatPipe) {}

  @HostListener('keyup')
  keyUp() {
    this.format()
  }

  // Cap the length of the field at 14 characters - 10 numbers plus 4 characters
  @HostListener('keydown', ['$event'])
  keyDown(event: KeyboardEvent) {
    const key = event.keyCode
    if (!this.allowedKeys(key) && this.el.nativeElement.value.length === 14) {
      event.preventDefault()
    }
  }

  format() {
    this.el.nativeElement.value = this.phoneFormat.transform(
      this.el.nativeElement.value
    )
  }

  allowedKeys(key) {
    if (!key) {
      return true
    }
    const isCtrlAltShift = 15 < key && key < 19
    const isArrowKeys = 37 <= key && key <= 40
    const isMetaKeys = key === 91
    const isDelete = key === 46 || key === 8
    const isTab = key === 9
    return isCtrlAltShift || isArrowKeys || isMetaKeys || isDelete || isTab
  }
}
