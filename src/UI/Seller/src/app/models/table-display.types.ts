export interface ResourceColumnConfiguration {
  path: string
  header: string
  type: string
  sortable: boolean
  queryRestriction?: string
}

export interface ResourceConfiguration {
  fields: ResourceColumnConfiguration[]
  imgPath?: string
}

export interface ResourceCell {
  type: string
  value: any
}

export interface ResourceRow {
  resource: any
  cells: ResourceCell[]
  imgPath?: string
}

export interface ResourceConfigurationDictionary {
  [resourceType: string]: ResourceConfiguration
}
