import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import randomString from '../helpers/random-string'
import testConfig from '../testConfig'

export async function getCatalogID(
	catalogName: string,
	buyerID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.UserGroups.List(
		buyerID,
		{ search: catalogName, searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	const catalog = searchResponse.Items.find(x => x.Name === catalogName)

	if (catalog.Name.includes('AutomationCatalog_')) return catalog.ID
}

export async function getCatalogs(buyerID: string, clientAuth: string) {
	const catalogs = await OrderCloudSDK.UserGroups.List(
		buyerID,
		{ search: 'AutomationCatalog_', searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	return catalogs.Items
}

export async function deleteCatalog(
	catalogID: string,
	buyerID: string,
	clientAuth: string
) {
	await OrderCloudSDK.UserGroups.Delete(buyerID, catalogID, {
		accessToken: clientAuth,
	})
}

export async function deleteCatalogWithName(
	catalogName: string,
	buyerID: string,
	clientAuth: string
) {
	const catalogID = await getCatalogID(catalogName, buyerID, clientAuth)
	await deleteCatalog(catalogID, buyerID, clientAuth)
}

export async function createDefaultCatalog(
	buyerID: string,
	clientAuth: string
) {
	const catalog: OrderCloudSDK.Buyer = {
		Name: `AutomationCatalog_${randomString(5)}`,
		xp: {
			Type: 'Catalog',
		},
	}

	const createdCatalog = await OrderCloudSDK.UserGroups.Create(
		buyerID,
		catalog,
		{
			accessToken: clientAuth,
		}
	)

	return createdCatalog
}
