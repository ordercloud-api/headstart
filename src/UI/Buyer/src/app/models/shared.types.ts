export class ModalState {
    static Open = { isOpen: true }
    static Closed = { isOpen: false }
  }

  export interface CountryDefinition {
    label: string
    abbreviation: string
}

export interface StateDefinition {
    label: string
    abbreviation: string
    country: string
}



export interface RouteConfig {
    routerCall: string
    displayText: string
    url: string
    showInDropdown: boolean
    // no roles with access means all users will see
    rolesWithAccess?: string[]
    context?: string
  }

