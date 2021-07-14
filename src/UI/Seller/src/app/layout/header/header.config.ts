import { HSRoles } from '@app-seller/config/mp-security-profiles'
import { HSRoute } from '@app-seller/models/shared.types'
import { SELLER, SUPPLIER } from '@app-seller/models/user.types'

// ! included to ensure no overlap with ordercloud ids as this in invalid in ids
export const REDIRECT_TO_FIRST_PARENT = '!'

// Products
const AllProducts: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSProductAdmin,
    HSRoles.HSProductReader,
    HSRoles.HSMeProductAdmin,
  ],
  title: 'ADMIN.NAV.ALL_PRODUCTS',
  route: '/products',
}
// TODO: Reimplement once UI is added to address these xp values
// const LiveProducts: HSRoute = {
//   rolesWithAccess: [HSRoles.HSProductAdmin, HSRoles.HSProductReader, HSRoles.HSMeProductAdmin],
//   title: 'ADMIN.NAV.LIVE_PRODUCTS',
//   route: '/products',
//   queryParams: { 'xp.Status': 'Published' },
// };

// const PendingProducts: HSRoute = {
//   rolesWithAccess: [HSRoles.HSProductAdmin, HSRoles.HSProductReader, HSRoles.HSMeProductAdmin],
//   title: 'ADMIN.NAV.PENDING_PRODUCTS',
//   route: '/products',
//   queryParams: { 'xp.Status': 'Draft' },
// };

const Promotions: HSRoute = {
  rolesWithAccess: [HSRoles.HSPromotionAdmin, HSRoles.HSPromotionReader],
  title: 'ADMIN.NAV.PROMOTIONS',
  route: '/promotions',
}

const ProductFacets: HSRoute = {
  rolesWithAccess: [HSRoles.HSStorefrontAdmin],
  title: 'ADMIN.NAV.FACETS',
  route: '/facets',
}

const ProductNavGrouping: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSProductAdmin,
    HSRoles.HSProductReader,
    HSRoles.HSMeProductAdmin,
  ],
  title: 'ADMIN.NAV.PRODUCTS',
  route: '/products',
  subRoutes: [AllProducts, Promotions, ProductFacets],
}

// Orders
const BuyerOrders: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.SALES_ORDERS',
  route: '/orders',
  queryParams: { OrderDirection: 'Incoming' },
}

const SupplierPurchaseOrders: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.PURCHASE_ORDERS',
  route: '/orders',
  queryParams: { OrderDirection: 'Outgoing' },
}

const RequiringAttentionOrders: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.NEEDING_ATTENTION',
  route: '/orders',
  queryParams: { OrderDirection: 'Incoming', 'xp.NeedsAttention': 'true' },
}

const Orders: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.ORDERS',
  route: '/orders',
}

const SellerOrderNavGrouping: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.ORDERS',
  route: '/orders',
  orderCloudUserTypesWithAccess: [SELLER],
  subRoutes: [BuyerOrders, SupplierPurchaseOrders, RequiringAttentionOrders],
}

const SupplierOrderBatchUpload: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.ORDERS_BATCH',
  route: '/orders/uploadshipments',
}

const SupplierOrderNavGrouping: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.ORDERS',
  route: '/orders',
  orderCloudUserTypesWithAccess: [SUPPLIER],
  subRoutes: [Orders, SupplierOrderBatchUpload],
}

const RMAs: HSRoute = {
  rolesWithAccess: [
    HSRoles.HSOrderAdmin,
    HSRoles.HSOrderReader,
    HSRoles.HSShipmentAdmin,
  ],
  title: 'ADMIN.NAV.RMAS',
  route: '/rmas',
}

// Buyers
const AllBuyers: HSRoute = {
  rolesWithAccess: [HSRoles.HSBuyerAdmin, HSRoles.HSBuyerReader],
  title: 'ALIAS.ALL_BUYERS',
  route: '/buyers',
}

const BuyerUsers: HSRoute = {
  rolesWithAccess: [HSRoles.HSBuyerAdmin, HSRoles.HSBuyerReader],
  title: 'ADMIN.NAV.USERS',
  route: `/buyers/${REDIRECT_TO_FIRST_PARENT}/users`,
}

const BuyerPurchasingLocations: HSRoute = {
  rolesWithAccess: [HSRoles.HSBuyerAdmin, HSRoles.HSBuyerReader],
  title: 'ALIAS.BUYER_LOCATIONS',
  route: `/buyers/${REDIRECT_TO_FIRST_PARENT}/locations`,
}

const BuyerCatalogs: HSRoute = {
  rolesWithAccess: [HSRoles.HSBuyerAdmin, HSRoles.HSBuyerReader],
  title: 'ADMIN.NAV.CATALOGS',
  route: `/buyers/${REDIRECT_TO_FIRST_PARENT}/catalogs`,
}

const BuyerCategories: HSRoute = {
  rolesWithAccess: [HSRoles.HSBuyerAdmin, HSRoles.HSBuyerReader],
  title: 'ADMIN.NAV.CATEGORIES',
  route: `/buyers/${REDIRECT_TO_FIRST_PARENT}/categories`,
}

const BuyerNavGrouping = {
  rolesWithAccess: [HSRoles.HSBuyerAdmin, HSRoles.HSBuyerReader],
  title: 'ALIAS.BUYERS',
  route: '/buyers',
  subRoutes: [
    AllBuyers,
    BuyerUsers,
    BuyerPurchasingLocations,
    BuyerCatalogs,
    BuyerCategories,
  ],
}

// Suppliers
const AllSuppliers: HSRoute = {
  rolesWithAccess: [HSRoles.HSSupplierAdmin],
  title: 'ALIAS.ALL_SUPPLIERS',
  route: '/suppliers',
}

const SupplierUsers: HSRoute = {
  rolesWithAccess: [HSRoles.HSSupplierAdmin],
  title: 'ADMIN.NAV.USERS',
  route: `/suppliers/${REDIRECT_TO_FIRST_PARENT}/users`,
}

const SupplierLocations: HSRoute = {
  rolesWithAccess: [HSRoles.HSSupplierAdmin, HSRoles.HSMeSupplierAdmin],
  title: 'ALIAS.SUPPLIER_LOCATIONS',
  route: `/suppliers/${REDIRECT_TO_FIRST_PARENT}/locations`,
}

const SupplierNavGrouping: HSRoute = {
  rolesWithAccess: [HSRoles.HSSupplierAdmin],
  title: 'ALIAS.SUPPLIERS',
  route: '/suppliers',
  subRoutes: [AllSuppliers, SupplierUsers, SupplierLocations],
}

/** https://four51.atlassian.net/browse/HDS-319 Reimplement once feature is stable */
// const ProcessReports = {
//   rolesWithAccess: [HSRoles.HSReportReader],
//   title: 'ADMIN.NAV.PROCESS_REPORTS',
//   route: 'reports/reports',
// }

// const ReportTemplates = {
//   rolesWithAccess: [HSRoles.HSReportAdmin],
//   title: 'ADMIN.NAV.REPORT_TEMPLATES',
//   route: `reports/${REDIRECT_TO_FIRST_PARENT}/templates`,
// }

// const ReportsNavGrouping = {
//   rolesWithAccess: [HSRoles.HSReportAdmin, HSRoles.HSReportReader],
//   title: 'ADMIN.NAV.REPORTS',
//   route: '/reports',
//   subRoutes: [ProcessReports, ReportTemplates],
// }

//Seller Admin
const SellerUsers = {
  rolesWithAccess: [HSRoles.HSSellerAdmin],
  title: 'ADMIN.NAV.SELLER_USERS',
  route: '/seller-admin/users',
}

const SellerLocations = {
  rolesWithAccess: [HSRoles.HSSellerAdmin],
  title: 'ALIAS.SELLER_LOCATIONS',
  route: '/seller-admin/locations',
}

const SellerNavGrouping: HSRoute = {
  rolesWithAccess: [HSRoles.HSSellerAdmin],
  title: 'ALIAS.SELLER_ADMIN',
  route: '/seller',
  subRoutes: [SellerUsers, SellerLocations],
}

const AllStorefronts = {
  rolesWithAccess: [HSRoles.HSStorefrontAdmin],
  title: 'All Storefronts',
  route: '/storefronts',
}

const StorefrontNavGrouping = {
  rolesWithAccess: [HSRoles.HSStorefrontAdmin],
  title: 'Storefronts',
  route: '/storefronts',
  subRoutes: [AllStorefronts],
}

const MySupplierProfile = {
  rolesWithAccess: [HSRoles.HSMeSupplierAdmin],
  title: 'ALIAS.SUPPLIER_PROFILE',
  route: '/my-supplier',
}

const MySupplierLocations = {
  rolesWithAccess: [HSRoles.HSMeSupplierAddressAdmin],
  title: 'ALIAS.SUPPLIER_LOCATIONS',
  route: '/my-supplier/locations',
}

const MySupplerUsers = {
  rolesWithAccess: [HSRoles.HSMeSupplierUserAdmin],
  title: 'ADMIN.NAV.USERS',
  route: '/my-supplier/users',
}

const Support = {
  rolesWithAccess: [],
  orderCloudUserTypesWithAccess: [SUPPLIER, SELLER],
  title: 'Submit a Case',
  route: '/support',
}

const AllNavGroupings: HSRoute[] = [
  ProductNavGrouping,
  SupplierOrderNavGrouping,
  SellerOrderNavGrouping,
  RMAs,
  BuyerNavGrouping,
  SupplierNavGrouping,
  // ReportsNavGrouping,
  SellerNavGrouping,
  StorefrontNavGrouping,
  MySupplierProfile,
  MySupplierLocations,
  MySupplerUsers,
  Support,
]

export const getHeaderConfig = (
  userRoles: string[],
  orderCloudUserType: string
): HSRoute[] => {
  const navGroupingsApplicableToUser = filterOutNavGroupings(
    AllNavGroupings,
    userRoles,
    orderCloudUserType
  )
  return navGroupingsApplicableToUser.map((navGrouping) => {
    if (!navGrouping.subRoutes) {
      return navGrouping
    } else {
      const routesApplicableToUser = filterOutNavGroupings(
        navGrouping.subRoutes,
        userRoles,
        orderCloudUserType
      )
      navGrouping.subRoutes = routesApplicableToUser
      return navGrouping
    }
  })
}

const filterOutNavGroupings = (
  navGroupings: HSRoute[],
  userRoles: string[] = [],
  orderCloudUserType: string
): HSRoute[] => {
  return navGroupings.filter((navGrouping) => {
    return (
      (navGrouping.rolesWithAccess.some((role) => userRoles?.includes(role)) ||
        !navGrouping.rolesWithAccess.length) &&
      (!navGrouping.orderCloudUserTypesWithAccess ||
        navGrouping.orderCloudUserTypesWithAccess.includes(orderCloudUserType))
    )
  })
}
