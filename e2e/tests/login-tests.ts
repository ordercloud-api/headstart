import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { ClientFunction } from 'testcafe'
import loginPage from '../pages/login-page'
import testConfig from '../testConfig'
import buyerHeaderPage from '../pages/buyer/buyer-header-page'
import {
	adminClientSetup,
	loginTestSetup,
	loginTestCleanup,
} from '../helpers/test-setup'
import adminHeaderPage from '../pages/admin/admin-header-page'
import { refreshPage } from '../helpers/page-helper'
import homepage from '../pages/buyer/homepage'
import { createDefaultBuyer } from '../api-utils.ts/buyer-util'
import { createDefaultCatalog } from '../api-utils.ts/catalog-util'
import { createDefaultBuyerLocation } from '../api-utils.ts/buyer-locations-util'

const getLocation = ClientFunction(() => document.location.href)

fixture`Log In Tests`
	.meta('TestRun', 'HS')
	.before(async ctx => {
		ctx.adminClientAuth = await adminClientSetup()
		ctx.buyerID = await createDefaultBuyer(ctx.adminClientAuth)
		const catalog = await createDefaultCatalog(ctx.buyerID, ctx.adminClientAuth)
		ctx.catalogID = catalog.ID
		const location = await createDefaultBuyerLocation(
			ctx.buyerID,
			ctx.adminClientAuth
		)
		ctx.locationID = location.Address.ID
	})
	.page(testConfig.buyerAppUrl)

test
	.before(async t => {
		t.ctx.testUser = await loginTestSetup(t.fixtureCtx.adminClientAuth, t.fixtureCtx.buyerID)
	})
	.after(async t => {
		await loginTestCleanup(
			t.ctx.testUser.ID,
			t.fixtureCtx.buyerID,
			t.fixtureCtx.adminClientAuth
		)
	})('Buyer Log In And Out | 19700', async t => {
		const testUser: OrderCloudSDK.User = t.ctx.testUser
		await loginPage.login(`${testConfig.buyerUsername}2`, testConfig.BuyerPassword)
		await t.expect(getLocation()).contains('home')
		await buyerHeaderPage.logout()
	})

test('Admin Log In And Out | 19966', async t => {
	await t.maximizeWindow()
	await loginPage.login(
		testConfig.adminSellerUsername,
		testConfig.adminSellerPassword
	)
	//need to refresh page because getting header too large error from logging in
	//this is because of three large tokens being set in the cookies when logging in
	await t.wait(3000)
	await t.expect(getLocation()).contains('home')
	await adminHeaderPage.logout()
	await t.expect(getLocation()).contains('login')
	await t.expect(loginPage.submitButton.exists).ok()
}).page(testConfig.adminAppUrl)
