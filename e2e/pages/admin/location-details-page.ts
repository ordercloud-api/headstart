import { Selector, t, ClientFunction } from 'testcafe'
import faker = require('faker')
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'
import { scrollIntoView } from '../../helpers/element-helper'

class LocationDetailsPage {
	createButton: Selector
	locationNameField: Selector
	companyNameField: Selector
	street1Field: Selector
	cityField: Selector
	stateField: Selector
	zipField: Selector
	countryCodeDropdown: Selector
	countryCodeOptions: Selector
	phoneField: Selector
	emailField: Selector
	locationIDField: Selector
	catalogAssignments: Selector
	saveChangesButton: Selector

	constructor() {
		this.createButton = Selector('button')
			.withText(createRegExp('create'))
			.withAttribute('type', 'submit')
		this.locationNameField = Selector('#LocationName')
		this.companyNameField = Selector('input').withAttribute('formcontrolname', "CompanyName")
		this.street1Field = Selector('input').withAttribute(
			'formcontrolname',
			'Street1'
		)
		this.cityField = Selector('#City')
		this.stateField = Selector('#State')
		this.zipField = Selector('#Zip')
		this.countryCodeDropdown = Selector('#Country')
		this.countryCodeOptions = this.countryCodeDropdown.find('option')
		this.phoneField = Selector('#Phone')
		this.emailField = Selector('#Email')
		this.locationIDField = Selector('#LocationID')
		this.catalogAssignments = Selector('app-buyer-location-catalogs')
			.find('tbody')
			.find('tr')
		this.saveChangesButton = Selector('button').withText(
			createRegExp('save changes')
		)
	}

	async createDefaultLocation() {
		const locationName = `AutomationLocation_${randomString(5)}`
		await t.typeText(this.locationNameField, locationName)
		await t.typeText(this.companyNameField, locationName)
		await t.typeText(this.street1Field, '700 American Ave #200')
		await t.typeText(this.cityField, 'King of Prussia')
		await t.typeText(this.stateField, 'PA')
		await t.typeText(this.zipField, '19406')
		await t.click(this.countryCodeDropdown)
		await t.click(
			this.countryCodeOptions.withText(
				createRegExp('united states of america')
			)
		)
		await t.typeText(this.phoneField, '1231231234')
		await t.typeText(this.emailField, `${locationName}.hpmqx9la@mailosaur.io`)
		await t.typeText(this.locationIDField, locationName)
		await scrollIntoView('.row.pt-3.mt-3.bg-white.shadow-sm.card-highlight-border')
		await t.click(this.catalogAssignments)
		await scrollIntoView(`button[type="submit"]`)
		await t.click(this.createButton)

		await loadingHelper.waitForLoadingBar()

		return locationName
	}

	async assignCatalogToLocation(catalogName: string) {
		await scrollIntoView('app-buyer-location-catalogs')

		const selectedAssignment = this.catalogAssignments
			.withText(createRegExp(catalogName))
			.find('label')

		await t.click(selectedAssignment)
		await t.click(this.saveChangesButton)

		await loadingHelper.waitForLoadingBar()
	}
}

export default new LocationDetailsPage()
