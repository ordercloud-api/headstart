/* eslint-disable prettier/prettier */
import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class AddressBookPage {
	addAddressButton: Selector
	address: Selector
	smartyStreetsSuggestionHeader: Selector
	confirmAddressDeleteButton: Selector
	addressCard: Selector

	constructor() {
		this.addAddressButton = Selector('#add-address-button')
		this.address = Selector('ocm-address-card')
		this.smartyStreetsSuggestionHeader = Selector('h5').withText('Did you mean...')
		this.confirmAddressDeleteButton = Selector('#confirm-address-delete')
		this.addressCard = Selector('.address-detail.mb-0')
	}

	async clickAddAddressButton() {
		await t.click(this.addAddressButton)
	}

	async clickDeleteAddressButton() {
		const slottedAddressDeleteButton = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[0]
				.querySelector('.address-delete')
		)
		await t.click(slottedAddressDeleteButton)
	}

	async clickConfirmDeleteAddressButton() {
		await t.click(this.confirmAddressDeleteButton)
	}

	async clickEditAddressButton() {
		const slottedAddressEditButton = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[0]
				.querySelector('.address-edit')
		)
		await t.click(slottedAddressEditButton)
	}

	async addedOrEditedAddressExists(address: string) {
		await this.address()
		const shadowAddressDataExists = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[0]
				.shadowRoot.querySelector('.address-detail')
		).withText(createRegExp(address)).exists
		return shadowAddressDataExists
	}

	async suggestedAddressExists() {
		const shadowAddressDataExists = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[0]
				.shadowRoot.querySelector('.address-detail')
		).exists
		return shadowAddressDataExists
	}

	async deletedAddressExists() {
		return Selector(() => document.querySelectorAll('ocm-address-card')[0])
			.exists
	}
}

export default new AddressBookPage()
