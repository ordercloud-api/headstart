/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import { Directive, Self, ElementRef, OnInit, Renderer2 } from '@angular/core'
import { NgControl } from '@angular/forms'
import { fromEvent } from 'rxjs'
import { ErrorDictionary } from '../validators/validators'

@Directive({
  selector: '[showErrors]',
})
export class FormControlErrorDirective implements OnInit {
  errorSpan: HTMLElement

  constructor(
    @Self() private control: NgControl,
    private el: ElementRef,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    this.errorSpan = this.renderer.createElement('span')
    this.renderer.appendChild(this.el.nativeElement.parentNode, this.errorSpan)
    this.renderer.setAttribute(this.errorSpan, 'class', 'error-message')
    ;(this.control as any).update.subscribe(this.displayErrorMsg)
    fromEvent(this.el.nativeElement.form, 'submit').subscribe(
      this.displayErrorMsg
    )
  }

  displayErrorMsg = (): void => {
    this.errorSpan.innerHTML = this.getErrorMsg(this.control)
  }

  getErrorMsg(control: NgControl): string {
    if (!control.errors) return ''
    if (control.pristine) return ''
    let controlErrors = Object.keys(control.errors)
    if (control.value)
      controlErrors = controlErrors.filter((x) => x !== 'required')
    if (controlErrors.length === 0) return ''
    const firstError = controlErrors[0]
    let message: string
    message = ErrorDictionary[firstError as keyof typeof ErrorDictionary]
    if (message) {
      // enhance string message with additional validation information via string replacement
      message = message.replace(
        /\$minlength/g,
        control?.errors?.minlength?.requiredLength
      )
      message = message.replace(
        /\$maxlength/g,
        control?.errors?.maxlength?.requiredLength
      )
      message = message.replace(/\$min/g, control?.errors?.min?.min)
      message = message.replace(/\$max/g, control?.errors?.max?.max)
    }
    return message || ''
  }
}
