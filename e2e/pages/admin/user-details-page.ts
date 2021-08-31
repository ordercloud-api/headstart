import { Selector, t } from 'testcafe'
import faker = require('faker')
import { createRegExp } from '../../helpers/regExp-helper'
import loadingHelper from '../../helpers/loading-helper'

class UserDetailsPage {
	activeToggle: Selector
	usernameField: Selector
	emailField: Selector
	firstNameField: Selector
	lastNameField: Selector
	createButton: Selector
	userGroupAssignments: Selector
	saveChangesButton: Selector
	confirmButton: Selector
	countryDropdown: Selector
	locationAssignments: Selector

	constructor() {
		this.activeToggle = Selector('#Active').parent()
		this.createButton = Selector('button')
			.withText(createRegExp('create'))
			.withAttribute('type', 'submit')
		this.usernameField = Selector('#Username')
		this.emailField = Selector('#Email')
		this.firstNameField = Selector('#FirstName')
		this.lastNameField = Selector('#LastName')
		this.userGroupAssignments = Selector('user-group-assignments')
		this.saveChangesButton = Selector('button').withText(
			createRegExp('save changes')
		)
		this.confirmButton = Selector('button').withText(createRegExp('confirm'))
		this.countryDropdown = Selector('#Country')
		this.locationAssignments = Selector('user-group-assignments').find('tr')
	}

	async createDefaultSupplierUser() {
		const firstName = faker.name.firstName()
		const lastName = faker.name.lastName()
		const firstNameReplaced = firstName.replace(/'/g, '')
		const lastNameReplaced = lastName.replace(/'/g, '')
		const email = `${firstNameReplaced}${lastNameReplaced}.hpmqx9la@mailosaur.io`

		await t.click(this.activeToggle)
		await t.typeText(this.usernameField, email)
		await t.typeText(this.emailField, email)
		await t.typeText(this.firstNameField, firstNameReplaced)
		await t.typeText(this.lastNameField, lastNameReplaced)

		await t.click(this.createButton)

		await loadingHelper.thisWait()

		return email
	}

	async createSupplierUser(
		email: string,
		userName: string,
		firstName: string,
		lastName: string
	) {
		await t.click(this.activeToggle)
		await t.typeText(this.usernameField, userName)
		await t.typeText(this.emailField, email)
		await t.typeText(this.firstNameField, firstName)
		await t.typeText(this.lastNameField, lastName)
		await this.addUserPermissions()

		await t.click(this.createButton)

		await loadingHelper.thisWait()
	}

	async createDefaultbuyerUser() {
		const firstName = faker.name.firstName()
		const lastName = faker.name.lastName()
		const firstNameReplaced = firstName.replace(/'/g, '')
		const lastNameReplaced = lastName.replace(/'/g, '')
		const email = `${firstNameReplaced}${lastNameReplaced}.hpmqx9la@mailosaur.io`

		await t.click(this.activeToggle)
		await t.typeText(this.usernameField, email)
		await t.typeText(this.emailField, email)
		await t.typeText(this.firstNameField, firstNameReplaced)
		await t.typeText(this.lastNameField, lastNameReplaced)

		await t.click(this.countryDropdown)
		await t.click(
			this.countryDropdown
				.find('option')
				.withText(createRegExp('united states of america'))
		)

		await loadingHelper.waitForLoadingBar()

		await t.click(this.createButton)

		await loadingHelper.thisWait()

		return email
	}

	async createDefaultbuyerUserWithLocation(location: string) {
		const firstName = faker.name.firstName()
		const lastName = faker.name.lastName()
		const firstNameReplaced = firstName.replace(/'/g, '')
		const lastNameReplaced = lastName.replace(/'/g, '')
		const email = `${firstNameReplaced}${lastNameReplaced}.hpmqx9la@mailosaur.io`

		await t.click(this.activeToggle)
		await t.typeText(this.usernameField, email)
		await t.typeText(this.emailField, email)
		await t.typeText(this.firstNameField, firstNameReplaced)
		await t.typeText(this.lastNameField, lastNameReplaced)

		await t.click(this.countryDropdown)
		await t.click(
			this.countryDropdown
				.find('option')
				.withText(createRegExp('united states of america'))
		)

		await loadingHelper.waitForLoadingBar()

		await this.assignUserToLocation(location)

		await t.click(this.createButton)

		await loadingHelper.waitForLoadingBar()

		return email
	}

	async assignUserToLocation(locationName: string) {
		const selectedLocation = this.locationAssignments
			.withText(createRegExp(locationName))
			.find('label')

		await t.click(selectedLocation)
	}

	async updateUserPermissions() {
		const permissionToggles = this.userGroupAssignments
			.find('input')
			.withAttribute('type', 'checkbox')

		await t.expect(await permissionToggles.count).gt(0)

		for (let i = 0; i < (await permissionToggles.count); i++) {
			await t.click(permissionToggles.nth(i).parent())
		}

		await t.click(this.saveChangesButton)
		await t.click(this.confirmButton)

		await loadingHelper.waitForLoadingBar()
	}

	async addUserPermissions() {
		const permissionToggles = this.userGroupAssignments
			.find('input')
			.withAttribute('type', 'checkbox')

		await t.expect(await permissionToggles.count).gt(0)

		for (let i = 0; i < (await permissionToggles.count); i++) {
			await t.click(permissionToggles.nth(i).parent())
		}
	}
}

export default new UserDetailsPage()
