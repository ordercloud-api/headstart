/* eslint-disable @typescript-eslint/quotes */
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms'
import {
  isValidLength,
  isValidPerLuhnAlgorithm,
} from '../services/card-validation.helper'

export const ErrorDictionary = {
  name: "Name can only contain characters Aa-Zz 0-9 - ' .",
  phone: 'Phone can only contain 20 numbers or "-" chars (no spaces)',
  zip: 'Zip Code is invalid',
  DateError: 'Enter date of the form YYYY-MM-DD',
  date: 'Enter date of the form mm-dd-yyyy',
  required: 'This field is required',
  email: 'Please enter a valid email',
  ocMatchFields: "Passwords don't match",
  strongPassword: `Password must be at least ten characters long and include at least 
    one uppercase letter, one lowercase letter, one number, and one special character.`,
  creditCardNumber: 'Card number is invalid',
  creditCardLength: 'Card length is invalid',
  min: 'Please enter a higher value',
  max: 'Please enter a lower value',
  pattern:
    'Zip Code is invalid' /* Maybe change if another regex is utilized elsewhere? */,
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
  const isValid = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/.test(control.value)
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

export function ValidateCAZip(
  control: AbstractControl
): ValidationErrors | null {
  const isValid = /^[A-Za-z]\\d[A-Za-z][ -]?\\d[A-Za-z]\\d$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { zip: true }
}

export function ValidateUSZip(
  control: AbstractControl
): ValidationErrors | null {
  const isValid = /^[0-9]{5}(?:-[0-9]{4})?$/.test(control.value)
  if (!control.value || isValid) {
    return null
  }
  return { zip: true }
}

// password must include one number, one uppercase letter, one lowercase letter, one special character and have min length of 10
export function ValidateStrongPassword(
  control: AbstractControl
): ValidationErrors | null {
  const hasMinLength = control.value && control.value.length >= 10
  const pattern = new RegExp(
    '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[-+_!@#$%^&*.,?]).+$'
  )
  const meetsPasswordRequirements = pattern.test(control.value) && hasMinLength
  if (!control.value) {
    return null
  }
  if (meetsPasswordRequirements) {
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

// Credit Card Validation

export function ValidateCreditCard(
  control: AbstractControl
): ValidationErrors | null {
  const cardToken = control.value
  if (!control.value) {
    return null
  }

  if (!isValidLength(cardToken)) return { creditCardLength: true }
  return null
}
