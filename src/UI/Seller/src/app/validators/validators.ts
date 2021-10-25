import {
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
  FormGroup,
} from '@angular/forms'

export const ErrorDictionary = {
  name: "Name can only contain characters Aa-Zz 0-9 - ' .",
  phone: 'Phone can only contain 20 numbers or "-" chars (no spaces)',
  zip: 'Zip Code is invalid',
  DateError: 'Enter date of the form mm-dd-yyyy',
  date: 'Enter date of the form mm-dd-yyyy',
  required: 'This field is required',
  min: 'Please enter a higher value',
  max: 'Please enter a lower value',
  email: 'Please enter a valid email',
  ocMatchFields: "Passwords don't match",
  minGreaterThanMax: 'Minimum value cannot be greater than maximum',
  maxLessThanMin: 'Maximum value cannot be less than minimum',
  strongPassword: `Password must be at least eight characters long and include at least 
    one letter and one number. Password can also include special characters.`,
  richTextFormatError:
    'Descriptions can only be 1000 characters. Remember the character count includes HTML formatting text.',
  supplierCategoryError: 'Supplier category selections are invalid',
  supplierProductTypeError: 'Please select at least one product type',
  noSpecialCharactersAndSpaces:
    'Please enter a value with no special characters ($, #, %, !, ?, *, etc.) or spaces.',
}

// only alphanumic and space . '
export function ValidateName(
  control: AbstractControl
): ValidationErrors | null {
  const isValid = /^[a-zA-Z0-9-.'\\s]*$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { name: true }
}

export function ValidateNoSpecialCharactersAndSpaces(
  control: AbstractControl
): ValidationErrors | null {
  const isValid = /^[a-zA-Z0-9_-]*$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { noSpecialCharactersAndSpaces: true }
}

// max 20 chars, numbers and -
export function ValidatePhone(
  control: AbstractControl
): ValidationErrors | null {
  const isValid = /^[0-9-]{0,20}$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { phone: true }
}

// contains @ and . with text surrounding
export function ValidateEmail(
  control: AbstractControl
): ValidationErrors | null {
  // longest TLD currently in existence is 24 characters
  const isValid = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,24}$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { email: true }
}

// mm-dd-yyyy, all numbers
export function ValidateDate(
  control: AbstractControl
): ValidationErrors | null {
  const isValid = /^[0-9]{2}-[0-9]{2}-[0-9]{4}$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { date: true }
}

export function ValidateZip(countryCode: String): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    let isValid;
    switch (countryCode) {
      case 'AU':
        isValid = /^((0[289][0-9]{2})|([1345689][0-9]{3})|(2[0-8][0-9]{2})|(290[0-9])|(291[0-4])|(7[0-4][0-9]{2})|(7[8-9][0-9]{2}))$/.test(control.value)
        break
      case 'CA':
        isValid = /^[A-Za-z]\\d[A-Za-z][ -]?\\d[A-Za-z]\\d$/.test(control.value)
        break
      case 'US':
      default:
        isValid = /^[0-9]{5}(?:-[0-9]{4})?$/.test(control.value)
        break
    }
    if (!control.value || isValid) {
      return null
    }
    return { zip: true }
  }
}

// password must include one number, one letter and have min length of 8
export function ValidateStrongPassword(
  control: AbstractControl
): ValidationErrors | null {
  const hasNumber = /[0-9]/.test(control.value) // TODO - boil these 3 checks into one regex
  const hasLetter = /[a-zA-Z]/.test(control.value)
  const hasMinLength = control.value && control.value.length >= 8
  if (!control.value) {
    return null
  }
  if (hasNumber && hasLetter && hasMinLength) {
    return null
  }
  return { strongPassword: true }
}

export function ValidateFieldMatches(fieldToMatch: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.parent) return null
    const passwordControl = control.parent.controls[fieldToMatch]
    // only validate if both fields have been touched
    if (passwordControl.pristine || control.pristine) {
      return null
    }
    if (passwordControl.value === control.value) {
      return null
    }
    return { ocMatchFields: true }
  }
}

export const ValidateMinMax: ValidatorFn = (
  control: FormGroup
): ValidationErrors | null => {
  const min = control.get('MinQuantity')
  const max = control.get('MaxQuantity')
  if (min.value <= max.value || max.value === null) {
    if (min.errors) {
      min.setErrors(null)
      min.setValue(min.value)
    }
    if (max.errors) {
      max.setErrors(null)
      max.setValue(max.value)
    }
    return null
  } else if (min.value > max.value && max.value !== null) {
    min.setErrors({ minGreaterThanMax: true })
    max.setErrors({ maxLessThanMin: true })
    return null
  }
  return null
}

/**
 * Our date inputs use ngbDatepicker but also allow freeform entry.
 * We need to validate the free form entry strings which are converted to date objects
 */

export function DateValidator(
  control: AbstractControl
): ValidationErrors | null {
  // only validate if both fields have been touched
  if (control.value == null || control.value === '') {
    return null
  }

  if (
    // the user's text input is converted to Date() if days and months check out
    control.value instanceof Date &&
    // validate that the year is also within reasonable range
    control.value.getFullYear().toString().length === 4
  ) {
    return null
  }

  return { DateError: true }
}

export function ValidateRichTextDescription(
  control: AbstractControl
): ValidationErrors | null {
  return control.value && control.value.length >= 2000
    ? { richTextFormatError: true }
    : null
}

export function RequireCheckboxesToBeChecked(minRequired = 1): ValidatorFn {
  return function validate(formGroup: FormGroup): ValidationErrors | null {
    let checked = 0

    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.controls[key]

      if (control.value === true) {
        checked++
      }
    })
    return checked < minRequired ? { supplierProductTypeError: true } : null
  }
}
