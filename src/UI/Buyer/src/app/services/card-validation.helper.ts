export const isValidPerLuhnAlgorithm = (cardNumber: string): boolean => {
  const map: number[] = [
    0,
    1,
    2,
    3,
    4,
    5,
    6,
    7,
    8,
    9,
    0,
    2,
    4,
    6,
    8,
    1,
    3,
    5,
    7,
    9,
  ]
  let sum = 0
  const cardNumberArrayString: string[] = cardNumber.split('')
  const cardNumberArray: number[] = cardNumberArrayString.map((num) =>
    parseInt(num, 10)
  )
  const numberLength: number = cardNumber.split('').length - 1
  for (let i = 0; i <= numberLength; i++) {
    sum += map[cardNumberArray[numberLength - i] + (i & 1) * 10]
  }
  if (sum % 10 !== 0) {
    return false
  }
  // If we made it this far the credit card number is in a valid format
  return true
}

export const isValidLength = (cardNumber: string): boolean => {
  // ensures that the card is of valid length for Visa, Discover, or MasterCard
  return cardNumber.length === 16
}
