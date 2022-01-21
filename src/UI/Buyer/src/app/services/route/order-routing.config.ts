import { OrderViewContext } from "src/app/models/order.types";
import { RouteConfig } from "src/app/models/shared.types";


export const OrderRoutes: RouteConfig[] = [
  {
    routerCall: 'toMyOrders',
    displayText: 'Placed by Me',
    url: '/orders',
    showInDropdown: true,
    context: OrderViewContext.MyOrders,
  },
  {
    routerCall: 'toMyQuotes',
    displayText: 'Quotes',
    url: '/orders/quotes',
    showInDropdown: true,
    context: OrderViewContext.Quote,
  },
  {
    routerCall: 'toOrdersByLocation',
    displayText: 'Placed in My Locations',
    url: '/orders/location',
    rolesWithAccess: ['HSLocationViewAllOrders'],
    showInDropdown: true,
    context: OrderViewContext.Location,
  },
  {
    routerCall: 'toOrdersToApprove',
    displayText: 'Awaiting My Approval',
    url: '/orders/approve',
    rolesWithAccess: ['HSLocationOrderApprover'],
    showInDropdown: true,
    context: OrderViewContext.Approve,
  },
]
