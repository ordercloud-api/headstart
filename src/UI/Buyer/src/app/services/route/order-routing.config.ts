import { OrderViewContext } from "src/app/models/order.types";
import { RouteConfig } from "src/app/models/shared.types";


export const OrderRoutes: RouteConfig[] = [
  {
    routerCall: 'toMyOrders',
    displayText: 'LAYOUT.APP_HEADER.ORDERS_PLACED_BY_ME',
    url: '/orders',
    showInDropdown: true,
    context: OrderViewContext.MyOrders,
  },
  {
    routerCall: 'toMyQuotes',
    displayText: 'LAYOUT.APP_HEADER.ORDERS_QUOTES',
    url: '/orders/quotes',
    showInDropdown: true,
    context: OrderViewContext.Quote,
  },
  {
    routerCall: 'toOrdersByLocation',
    displayText: 'LAYOUT.APP_HEADER.ORDERS_PLACED_IN_MY_LOCATIONS',
    url: '/orders/location',
    rolesWithAccess: ['HSLocationViewAllOrders'],
    showInDropdown: true,
    context: OrderViewContext.Location,
  },
  {
    routerCall: 'toOrdersToApprove',
    displayText: 'LAYOUT.APP_HEADER.ORDERS_AWAITING_MY_APPROVAL',
    url: '/orders/approve',
    rolesWithAccess: ['HSLocationOrderApprover'],
    showInDropdown: true,
    context: OrderViewContext.Approve,
  },
]
