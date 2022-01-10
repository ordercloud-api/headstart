import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import {
	adminClientAuth,
	authAdminBrowser,
	authBuyerBrowser,
	authSupplierBrowser,
} from '../api-utils.ts/auth-util'
import testConfig from '../testConfig'
import {
	ApiRole,
	Configuration,
	SdkConfiguration,
} from 'ordercloud-javascript-sdk'
import { axiosSetup } from './axios-helper'
import { createUser, deleteUser } from '../api-utils.ts/users-util'
import { saveUserAssignment } from '../api-utils.ts/usergroups-helper'
import { t } from 'testcafe'
import { setHeadstartSDKUrl } from './headstart-sdk-helper'
import { createCreditCard } from '../api-utils.ts/credit-card-util'
import { deleteOrdersForUser } from '../api-utils.ts/order-util'
import loadingHelper from './loading-helper'

export async function adminClientSetup() {
	await axiosSetup()

	setStagingUrl()

	setHeadstartSDKUrl()

	const adminClientToken = await adminClientAuth(
		testConfig.automationClientID,
		testConfig.automationClientSecret,
		adminRoles
	)


	return adminClientToken
}

const adminRoles: ApiRole[] = [
	"AddressAdmin",
	"AddressReader",
	"AdminAddressAdmin",
	"AdminAddressReader",
	"AdminUserAdmin",
	"ApiClientAdmin",
	"ApprovalRuleAdmin",
	// @ts-ignore
	"AssetAdmin",
	"BuyerAdmin",
	"BuyerImpersonation",
	"BuyerUserAdmin",
	"CatalogAdmin",
	"CategoryAdmin",
	"CreditCardAdmin",
	// @ts-ignore
	"DocumentAdmin",
	// @ts-ignore
	"HSBuyerAdmin",
	// @ts-ignore
	"HSBuyerImpersonator",
	// @ts-ignore
	"HSCategoryAdmin",
	// @ts-ignore
	"HSMeAdmin",
	// @ts-ignore
	"HSOrderAdmin",
	// @ts-ignore
	"HSProductAdmin",
	// @ts-ignore
	"HSPromotionAdmin",
	// @ts-ignore
	"HSReportAdmin",
	// @ts-ignore
	"HSReportReader",
	// @ts-ignore
	"HSSellerAdmin",
	// @ts-ignore
	"HSShipmentAdmin",
	// @ts-ignore
	"HSStorefrontAdmin",
	// @ts-ignore
	"HSSupplierAdmin",
	// @ts-ignore
	"HSSupplierUserGroupAdmin",
	"MeAdmin",
	"MeXpAdmin",
	"OrderAdmin",
	"OrderReader",
	"PriceScheduleAdmin",
	"ProductAdmin",
	"ProductAssignmentAdmin",
	"ProductFacetAdmin",
	"ProductFacetReader",
	"PromotionAdmin",
	// @ts-ignore
	"SchemaAdmin",
	"ShipmentAdmin",
	"ShipmentReader",
	"SupplierAddressAdmin",
	"SupplierAddressReader",
	"SupplierAdmin",
	"SupplierReader",
	"SupplierUserAdmin",
	"SupplierUserGroupAdmin",
	"UserGroupAdmin"
]

export function setStagingUrl() {
	const config: SdkConfiguration = {
		baseApiUrl: 'https://sandboxapi.ordercloud.io',
	}
	Configuration.Set(config)
}

export async function loginTestSetup(authToken: string, buyerID: string) {
	await t.maximizeWindow()
	const user: OrderCloudSDK.User = await createUser(authToken, buyerID)
	await saveUserAssignment(user.ID, `${buyerID}-0001`, buyerID, authToken)
	return user
}

export async function loginTestCleanup(
	userID: string,
	buyerID: string,
	authToken: string
) {
	await deleteUser(userID, buyerID, authToken)
}

export async function baseTestCleanup(
	userID: string,
	buyerID: string,
	authToken: string
) {
	//delete orders for user
	await deleteOrdersForUser(authToken, buyerID, userID)
	await deleteUser(userID, buyerID, authToken)
}

export async function buyerTestSetup(authToken: string, buyerID?: string, country?: string) {
	await t.maximizeWindow()
	const user: OrderCloudSDK.User = await createUser(authToken, buyerID, country)

	await authBuyerBrowser(user)

	await createCreditCard(t.ctx.userAuth)

	await t.navigateTo(`${testConfig.buyerAppUrl}home`)

	await loadingHelper.waitForLoadingBar()

	return user
}

export async function adminTestSetup() {
	await t.maximizeWindow()

	const user: Partial<OrderCloudSDK.User> = {
		Username: testConfig.adminSellerUsername,
		Password: testConfig.adminSellerPassword,
	}

	await authAdminBrowser(user)

	await t.navigateTo(`${testConfig.adminAppUrl}home`)
}



export async function supplierTestSetup(username: string, password: string) {
	await t.maximizeWindow()

	const user: Partial<OrderCloudSDK.User> = {
		Username: username,
		Password: password,
	}
	await authSupplierBrowser(user)

	await t.navigateTo(`${testConfig.adminAppUrl}home`)
}

export async function existingBuyerTestSetup(username, password) {
	await t.maximizeWindow()

	const user: Partial<OrderCloudSDK.User> = {
		Username: username,
		Password: password
	}

	await authBuyerBrowser(user)

	await t.navigateTo(`${testConfig.buyerAppUrl}home`)
}
