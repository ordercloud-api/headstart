/**
 * configuration for prettier - an opinonated code formatter
 * used to auto-format our typescript, javascript and html files
 *
 * unless otherwise specified, the below options are default
 * https://prettier.io/docs/en/options.html
 */
module.exports = {
  printWidth: 80,
  tabWidth: 2,
  useTabs: false,
  semi: false,
  singleQuote: true, // default: false
  trailingComma: 'es5', // default: 'none'
  bracketSpacing: true,
  endOfLine: 'auto',
}
