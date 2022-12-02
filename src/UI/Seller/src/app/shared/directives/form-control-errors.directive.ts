/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import {
  Directive,
  Self,
  ElementRef,
  OnInit,
  Renderer2,
  Input,
} from '@angular/core'
import { NgControl, FormGroup } from '@angular/forms'
import { ErrorDictionary } from '../../../app/validators/validators'

@Directive({
  selector: '[showErrors]',
})
export class FormControlErrorDirective implements OnInit {
  constructor(
    @Self() private control: NgControl,
    private el: ElementRef,
    private renderer: Renderer2
  ) {}

  @Input()
  formControlName: string

  // resourceForm needs to be passed in to remove error messages when resetting the form
  // without changing the inputs, could be a better way
  @Input()
  set resourceForm(value: FormGroup) {
    // need this to remove the error when the selected resource is changed
    if (this.errorSpan) {
      this.errorSpan.innerHTML = ''
    }
  }

  errorSpan: HTMLElement

  ngOnInit(): void {
    this.initializeSubscriptions()
  }

  initializeSubscriptions(): void {
    this.errorSpan = this.renderer.createElement('span') as HTMLElement
    this.renderer.appendChild(
      (this.el.nativeElement as HTMLElement).parentNode,
      this.errorSpan
    )
    this.renderer.setAttribute(this.errorSpan, 'class', 'error-message')
    // eslint-disable-next-line @typescript-eslint/no-unsafe-call
    ;(this.control as any).update.subscribe(this.displayErrorMsg)
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
