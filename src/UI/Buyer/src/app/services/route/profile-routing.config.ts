import { RouteConfig } from "src/app/models/shared.types";

export const ProfileRoutes: RouteConfig[] = [
  {
    routerCall: 'toMyProfile',
    displayText: 'My Profile',
    url: '/profile/details',
    showInDropdown: true,
  },
  {
    routerCall: 'toMyAddresses',
    displayText: 'My Addresses',
    url: '/profile/addresses',
    showInDropdown: true,
  },
  {
    routerCall: 'toMyLocations',
    displayText: 'My Locations',
    url: '/profile/locations',
    showInDropdown: true,
  },
  {
    routerCall: 'toMyPaymentMethods',
    displayText: 'My Credit Cards',
    url: '/profile/payment-methods',
    showInDropdown: true,
  },
  {
    routerCall: 'toChangePassword',
    displayText: 'Change Password',
    url: '/profile/details',
    showInDropdown: false,
  },
]
