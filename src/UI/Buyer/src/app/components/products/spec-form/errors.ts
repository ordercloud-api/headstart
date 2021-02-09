export const specErrors = {
  required: '*required',
  email: '* valid email format required',
  password: `* passwords requires a minimum of 8 characters, two or more of the following:
    alphabetic, numeric, punctuation or other characters (e.g., !@#$%^&*()_+|~-=\`{}[]:";'<>?,./)`,
  id:
    '* IDs must have at least 8 characters and no more than 100, are required and can only contain characters Aa-Zz, 0-9, -, and _',
  maxLength160: 'Restricted to 160 characters',
  maxLength12: 'Restricted to 12',
  mismatch: 'Passwords do not match',
  min: 'Value less than minimum allowed',
  max: 'Value more than maximum allowed',
  maxLength: 'Maximum character limit exceeded',
}
