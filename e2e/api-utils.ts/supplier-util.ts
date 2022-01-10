import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import randomString from '../helpers/random-string'
import testConfig from '../testConfig'
import { HeadStartSDK, HSSupplier } from '@ordercloud/headstart-sdk'
import { t } from 'testcafe'

export async function getSupplierID(supplierName: string, clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Suppliers.List(
		{ search: supplierName, searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	const supplier = searchResponse.Items.find(x => x.Name === supplierName)

	if (supplier.Name.includes('AutomationSupplier_')) return supplier.ID
}

export async function setupGetSupplierID(
	supplierName: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.Suppliers.List(
		{ search: supplierName, searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	const supplier = searchResponse.Items.find(x => x.Name === supplierName)
	return supplier.ID
}

export async function getSupplier(supplierName: string, clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Suppliers.List(
		{ search: supplierName, searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	const supplier = searchResponse.Items.find(x => x.Name === supplierName)

	if (supplier.Name.includes('AutomationSupplier_')) return supplier
}

export async function getAutomationSuppliers(clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Suppliers.List(
		{
			search: 'AutomationSupplier_',
			searchOn: 'Name',
		},
		{ accessToken: clientAuth }
	)

	return searchResponse.Items
}

export async function deleteAutomationSuppliers(
	suppliers: OrderCloudSDK.Supplier[],
	clientAuth: string
) {
	for await (const supplier of suppliers) {
		if (supplier.Name.includes('AutomationSupplier_')) {
			await deleteSupplier(supplier.ID, clientAuth)
		}
	}
}

export async function getSupplierWithID(
	supplierID: string,
	clientAuth: string
) {
	const supplier = await OrderCloudSDK.Suppliers.Get(supplierID, {
		accessToken: clientAuth,
	})

	if (supplier.Name.includes('AutomationSupplier_')) return supplier
}

export async function deleteSupplier(supplierID: string, clientAuth: string) {
	await OrderCloudSDK.Suppliers.Delete(supplierID, {
		accessToken: clientAuth,
		requestType: 'Cleanup',
	})
}

export async function createSupplier(clientAuth: string) {
	const supplierName = `AutomationSupplier_${randomString(5)}`
	const supplier: OrderCloudSDK.Supplier = {
		Name: supplierName,
		ID: supplierName,
		Active: true,
		xp: {
			// CountriesServicing: ['US'], this was removed from the UI
			Currency: 'USD',
			ID: "{supplierIncrementor}",
			Description: '',
			Images: [],
			CountriesServicing: [],
			ProductTypes: ['Standard', 'Quote'],
			SupportContact: {
				Name: '',
				Email: '',
				Phone: '',
			},
		},
	}
	const createdSupplier = await HeadStartSDK.Suppliers.Create(supplier, clientAuth)

	return createdSupplier.ID
}
