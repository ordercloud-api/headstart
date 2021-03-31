function handlebarsExt(Handlebars) {
  // converts /buyers/{buyerID} to /buyers/${buyerID} so template literal take in parameters
  Handlebars.registerHelper('parameterizePath', (path?: string) => {
    if (!path) {
      return ''
    }
    return path.replace(/{/g, '${')
  })

  Handlebars.registerHelper('commaSeparate', (fields: string[]) => {
    return fields.join(', ')
  })

  Handlebars.registerHelper(
    'commaSeparateWithDefaultAny',
    (types: string[]) => {
      return types.map(t => `${t} = any`).join(', ')
    }
  )
}

module.exports = handlebarsExt
