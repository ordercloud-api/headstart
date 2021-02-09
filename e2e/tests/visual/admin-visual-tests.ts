/* eslint-disable @typescript-eslint/ban-ts-ignore */
import Eyes from '@applitools/eyes-testcafe'
import { checkWindow, getBrowsers } from '../../helpers/eyes-helper'
import { adminClientSetup, adminTestSetup } from '../../helpers/test-setup'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import mainResourcePage from '../../pages/admin/main-resource-page'
import minorResourcePage from '../../pages/admin/minor-resource-page'
import testConfig from '../../testConfig'

const eyes = new Eyes()
const appName = 'Headstart Admin'

fixture`Headstart Admin Visual Tests`
	.meta('TestRun', 'Visual')
	.before(async ctx => {
		ctx.adminClientAuth = await adminClientSetup()
	})
	.afterEach(async () => {
		await eyes.close()
	})
	.after(async () => {
		await eyes.waitForResults()
	})
	.page(testConfig.adminAppUrl)

test('Admin Login Page', async t => {
	await t.maximizeWindow()
	await eyes.open({
		appName: appName,
		// @ts-ignore
		testName: t.testRun.test.name,
		// @ts-ignore
		browser: getBrowsers(),
		t,
		accessibilityValidation: { level: 'AA', guidelinesVersion: 'WCAG_2_0' },
	})
	// @ts-ignore
	await checkWindow(eyes, t.testRun.test.name)
})

test.before(async t => {
	await adminTestSetup()
})('Admin Homepage', async t => {
	await eyes.open({
		appName: appName,
		// @ts-ignore
		testName: t.testRun.test.name,
		// @ts-ignore
		browser: getBrowsers(),
		t,
		accessibilityValidation: { level: 'AA', guidelinesVersion: 'WCAG_2_0' },
	})
	// @ts-ignore
	await checkWindow(eyes, t.testRun.test.name)
})

test.before(async t => {
	await adminTestSetup()
})('Admin Products Pages', async t => {
	await eyes.open({
		appName: appName,
		// @ts-ignore
		testName: t.testRun.test.name,
		// @ts-ignore
		browser: getBrowsers(),
		t,
		accessibilityValidation: { level: 'AA', guidelinesVersion: 'WCAG_2_0' },
	})
	await adminHeaderPage.selectAllProducts()
	// @ts-ignore
	await t.expect(mainResourcePage.resourceList.exists).ok()
	await checkWindow(eyes, 'All Products List')
	await mainResourcePage.selectResourceByIndex(0)
	await checkWindow(eyes, 'Product Detail Page')
})

test.before(async t => {
	await adminTestSetup()
})('Admin Promotions Pages', async t => {
	await eyes.open({
		appName: appName,
		// @ts-ignore
		testName: t.testRun.test.name,
		// @ts-ignore
		browser: getBrowsers(),
		t,
		accessibilityValidation: { level: 'AA', guidelinesVersion: 'WCAG_2_0' },
	})
	await adminHeaderPage.selectPromotionsLink()
	// @ts-ignore
	await t.expect(mainResourcePage.resourceList.exists).ok()
	await checkWindow(eyes, 'Promotions List')
	await minorResourcePage.selectResourceByIndex(0)
	await checkWindow(eyes, 'Promotion Detail Page')
})

test.before(async t => {
	await adminTestSetup()
})('Admin Orders Pages', async t => {
	await eyes.open({
		appName: appName,
		// @ts-ignore
		testName: t.testRun.test.name,
		// @ts-ignore
		browser: getBrowsers(),
		t,
		accessibilityValidation: { level: 'AA', guidelinesVersion: 'WCAG_2_0' },
	})
	await adminHeaderPage.selectSalesOrdersLink()
	// @ts-ignore
	await t.expect(mainResourcePage.resourceList.exists).ok()
	await checkWindow(eyes, 'Sales Orders List')
	await minorResourcePage.selectResourceByIndex(0)
	await checkWindow(eyes, 'Sales Order Detail Page')
})
