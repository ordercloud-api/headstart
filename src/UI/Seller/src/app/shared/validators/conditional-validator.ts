import { FormControl, ValidationErrors, ValidatorFn } from '@angular/forms'

// Use this to conditionally apply a validator if the predicate evaluates to true
// ex: conditionalValidator(() => this.myForm.get('myTextField').value.includes('Illuminati'), Validators.required)
export function conditionalValidator(
  predicate: Predicate,
  validator: ValidatorFn
) {
  return (formControl: FormControl): ValidationErrors | null => {
    if (!formControl.parent) {
      return null
    }
    if (predicate()) {
      return validator(formControl)
    }
    return null
  }
}

type Predicate = () => boolean
