import { Directive, ElementRef, HostListener } from '@angular/core'

@Directive({ selector: '[appLocationIDInput]' })
// Directive class
export class LocationIDInputDirective {
  constructor(private el: ElementRef) {}

  // Cap the length of the field at 14 characters - 10 numbers plus 4 characters
  @HostListener('keydown', ['$event'])
  keyDown(event: KeyboardEvent): void {
    const key = event.keyCode
    if (!this.isAllowedKey(key)) {
      event.preventDefault()
    }
  }

  isAllowedKey(key): boolean {
    if (!key) {
      return true
    }
    const isNum = (key >= 48 && key <= 57) || (key >= 96 && key <= 105)
    const isLetter = key >= 65 && key <= 90
    const isCtrlAltShift = 15 < key && key < 19
    const isDelete = key === 46 || key === 8
    const isArrowKeys = 37 <= key && key <= 40
    const isTab = key === 9
    const isMetaKeys = key === 91

    return (
      isMetaKeys ||
      isNum ||
      isLetter ||
      isCtrlAltShift ||
      isDelete ||
      isArrowKeys ||
      isTab
    )
  }
}
