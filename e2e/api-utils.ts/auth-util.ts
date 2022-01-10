import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { ApiRole, Order } from 'ordercloud-javascript-sdk'
import testConfig from '../testConfig'
import { ClientFunction, t } from 'testcafe'

export async function adminClientAuth(
	clientID: string,
	clientSecret: string,
	scope: ApiRole[]
) {
	const response = await OrderCloudSDK.Auth.ClientCredentials(
		clientSecret,
		clientID,
		scope
	)

	const token = response['access_token']

	return token
}

export async function userClientAuth(
	clientID: string,
	username: string,
	password: string,
	scope: ApiRole[]
) {
	const response = await OrderCloudSDK.Auth.Login(
		username,
		password,
		clientID,
		scope
	)

	const token = response['access_token']

	return token
}

export async function authAdminBrowser(user: Partial<OrderCloudSDK.User>) {
	const userToken = await userClientAuth(
		testConfig.adminAppClientID,
		user.Username,
		user.Password,
		adminUserRoles
	)

	await setBrowserAuthCookie(userToken, 'headstart_demo_admin.token')
	await setBrowserAuthCookie(userToken, 'headstart_demo_admin.access-token')
	//Below cookie is set on the browser when logging in, but does not seem to be needed
	await setBrowserAuthCookie(userToken, 'ordercloud.access-token')

	t.ctx.userAuth = userToken
}

export async function authSupplierBrowser(user: Partial<OrderCloudSDK.User>) {
	const userToken = await userClientAuth(
		testConfig.adminAppClientID,
		user.Username,
		user.Password,
		supplierUserRoles
	)

	await setBrowserAuthCookie(userToken, 'headstart_demo_admin.token')
	await setBrowserAuthCookie(userToken, 'headstart_demo_admin.access-token')
	//Below cookie is set on the browser when logging in, but does not seem to be needed
	await setBrowserAuthCookie(userToken, 'ordercloud.access-token')

	t.ctx.userAuth = userToken
}

export async function authBuyerBrowser(user: Partial<OrderCloudSDK.User>) {
	const userToken = await userClientAuth(
		testConfig.buyerAppClientID,
		user.Username,
		user.Password,
		buyerUserRoles
	)

	await setBrowserAuthCookie(
		userToken,
		'headstartdemo-testbuyer.access-token'
	)
	await setBrowserAuthCookie(userToken, 'ordercloud.access-token')

	t.ctx.userAuth = userToken
}

export async function setBrowserAuthCookie(token: string, tokenName: string) {
	const setCookieFunction = ClientFunction((tokenName, token) => {
		document.cookie = `${tokenName}=${token}`
	})

	await setCookieFunction(tokenName, token)
}

export const adminUserRoles: ApiRole[] = [
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

export const supplierUserRoles: ApiRole[] = [
	"AddressReader",
	"MeAdmin",
	"MeXpAdmin",
	"ProductAdmin",
	"PriceScheduleAdmin",
	"SupplierReader",
	"SupplierAddressReader",
	"OrderAdmin",
	"SupplierAdmin",
	"SupplierUserAdmin",
	"SupplierUserGroupAdmin",
	"SupplierAddressAdmin",
	"ProductFacetReader",
	"ShipmentAdmin",
	//@ts-ignore
	"AssetAdmin",
	//@ts-ignore
	"HSMeProductAdmin",
	//@ts-ignore
	"HSOrderAdmin",
	//@ts-ignore
	"HSShipmentAdmin",
	//@ts-ignore
	"HSReportReader",
	//@ts-ignore
	"HSMeSupplierAdmin",
	//@ts-ignore
	"HSMeSupplierAddressAdmin",
	//@ts-ignore
	"HSMeSupplierUserAdmin",
	//@ts-ignore
	"HSSupplierUserGroupAdmin"
]

export const buyerUserRoles: ApiRole[] = [
	"MeAddressAdmin",
	"MeAdmin",
	"MeCreditCardAdmin",
	"MeXpAdmin",
	"Shopper",
	"SupplierReader",
	"SupplierAddressReader"
]
