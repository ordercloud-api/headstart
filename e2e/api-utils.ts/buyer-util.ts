import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import randomString from '../helpers/random-string'
import { HeadStartSDK, SuperHSBuyer } from '@ordercloud/headstart-sdk'
import testConfig from '../testConfig'

export async function getBuyerID(buyerName: string, clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Buyers.List(
		{ search: buyerName, searchOn: 'Name' },
		{ accessToken: clientAuth }
	)

	const buyer = searchResponse.Items.find(x => x.Name === buyerName)

	if (buyer.Name.includes('Automationbuyer_')) return buyer.ID
}

export async function getAutomationbuyers(clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Buyers.List(
		{
			search: 'Automationbuyer_',
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
	const buyer: SuperHSBuyer = {
		Buyer: {
			Name: `Automationbuyer_${randomString(5)}`,
			Active: true,
			xp: {
				ChiliPublishFolder: '',
				URL: testConfig.buyerAppUrl,
			}
		},
		ImpersonationConfig: {
			ClientID: testConfig.buyerAppClientID,
			SecurityProfileID: "HSBaseBuyer"
		},
		Markup: {
			Percent: 10,
		},
	}

	const createdBuyer = await HeadStartSDK.Buyers.Create(buyer, clientAuth)

	return createdBuyer.Buyer.ID
}
