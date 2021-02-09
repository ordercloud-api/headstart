import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import randomString from '../helpers/random-string'
import { HeadStartSDK, SuperMarketplaceBuyer } from '@ordercloud/headstart-sdk'

export async function getBuyerID(buyerName: string, clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Buyers.List(
		{ search: buyerName, searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	const buyer = searchResponse.Items.find(x => x.Name === buyerName)

	if (buyer.Name.includes('AutomationBrand_')) return buyer.ID
}

export async function getAutomationBuyers(clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Buyers.List(
		{
			search: 'AutomationBuyer_',
			searchOn: 'Name',
		},
		{ accessToken: clientAuth }
	)

	return searchResponse.Items
}

export async function deleteBuyer(buyerID: string, clientAuth: string) {
	await OrderCloudSDK.Buyers.Delete(buyerID, { accessToken: clientAuth })
}

export async function deleteBuyerWithName(
	buyerName: string,
	clientAuth: string
) {
	const buyerID = await getBuyerID(buyerName, clientAuth)
	await deleteBuyer(buyerID, clientAuth)
}

export async function createDefaultBuyer(clientAuth: string) {
	const buyer: SuperMarketplaceBuyer = {
		Buyer: {
			Name: `AutomationBrand_${randomString(5)}`,
			Active: true,
		},
		Markup: {
			Percent: 0,
		},
	}

	const createdBuyer = await HeadStartSDK.Buyers.Create(buyer, clientAuth)

	return createdBuyer.Buyer.ID
}
