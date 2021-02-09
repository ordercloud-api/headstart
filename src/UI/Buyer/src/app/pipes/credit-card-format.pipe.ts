import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'card',
})
export class CreditCardFormatPipe implements PipeTransform {
  transform(card: string): string {
    if (!card) {
      return ''
    }

    const value = card
      .toString() // ensure cardnumber is a string,
      .trim() // remove whitespace from ends, and replace
      .replace(/[^0-9]/g, '') // replace non number characters

    // no adjustment needed if less than 5 characters
    if (value.length < 5) {
      return value
    }

    const valueArray = value.split('')
    const newValueArray = []

    // add space after every fourth character
    valueArray.forEach((val, index) => {
      newValueArray.push(val)
      if ((index + 1) % 4 === 0) {
        newValueArray.push(' ')
      }
    })

    // remove space that could be added to the end per above
    // max length for Discover, MasterCard, and Visa is 19 character, surprisingly not 16
    return newValueArray.join('').trim().substring(0, 23)
  }
}
