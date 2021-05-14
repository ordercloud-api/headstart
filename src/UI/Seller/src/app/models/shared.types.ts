import { FormGroup } from "@angular/forms";

export interface Route {
  title: string
  route: string
}

export interface HeaderNav {
  title: string
  routes: Route[]
}

export interface HSRoute {
  rolesWithAccess: string[]
  // this allows the routes to be narrowed based upon OC user type
  orderCloudUserTypesWithAccess?: string[]
  title: string
  route: string
  queryParams?: Record<string, any>
  // if subroutes are included, itesms will display in a dropdown
  subRoutes?: HSRoute[]
}

export interface ResourceUpdate<T = any> {
  field: string
  value: T
  form: FormGroup
}

export interface SwaggerSpecProperty {
  field: string
  type: SwaggerSpecPropertyType
}

export enum SwaggerSpecPropertyType {
  String = 'string',
  Integer = 'integer',
  Object = 'object',
  Boolean = 'boolean',
  Number = 'number',
  Array = 'array',
}

export interface SummaryResourceInfoPaths {
  toPrimaryHeader: string
  toSecondaryHeader: string
  toImage: string
  toExpandable: boolean
}

export interface SummaryResourceInfoPathsDictionary {
  [resourceType: string]: SummaryResourceInfoPaths
}
