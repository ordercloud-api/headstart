import { Injectable } from '@angular/core'
import { UntypedFormControl, UntypedFormGroup, AbstractControl } from '@angular/forms'

@Injectable({
  providedIn: 'root',
})
export class AppFormErrorService {
  displayFormErrors(form: UntypedFormGroup): void {
    Object.keys(form.controls).forEach((key) => {
      form.get(key).markAsDirty()
    })
  }

  hasValidEmailError(input: UntypedFormControl | AbstractControl): boolean {
    return (
      (input.hasError('required') || input.hasError('email')) && input.dirty
    )
  }

  hasPasswordMismatchError(form: UntypedFormGroup | AbstractControl): boolean {
    return form.hasError('ocMatchFields')
  }

  hasInvalidIdError(input: UntypedFormControl | AbstractControl): boolean {
    return input.hasError('invalidIdError') && input.dirty
  }

  hasRequiredError(controlName: string, form: UntypedFormGroup): boolean {
    const control = form.get(controlName)
    return control && control.hasError('required') && control.dirty
  }

  hasPatternError(controlName: string, form: UntypedFormGroup): boolean {
    return (
      form.get(controlName).hasError('pattern') && form.get(controlName).dirty
    )
  }
}
