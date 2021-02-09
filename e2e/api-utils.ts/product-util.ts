import {
	HeadStartSDK,
	SuperMarketplaceProduct,
} from '@ordercloud/headstart-sdk'
import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { t } from 'testcafe'
import randomString from '../helpers/random-string'

export async function deleteProduct(productID: string, clientAuth: string) {
	//put in try/catch because seeing some random 500 errors when deleting product
	//with middleware. The product gets deleted, but still throws error
	try {
		await HeadStartSDK.Products.Delete(productID, clientAuth)
	} catch (e) {
		console.log('Error deleting product')
	}
}

export async function deleteAutomationProducts(
	products: OrderCloudSDK.Product[],
	clientAuth: string
) {
	for await (const product of products) {
		if (product.Name.includes('AutomationProduct_')) {
			await deleteProduct(product.ID, clientAuth)
		}
	}
}

export async function getAutomationProducts(clientAuth: string) {
	const searchResponse = await OrderCloudSDK.Products.List(
		{
			search: 'AutomationProduct_',
			searchOn: ['Name'],
		},
		{ accessToken: clientAuth }
	)

	return searchResponse.Items
}

export async function getProductID(productName: string, clientAuth: string) {
	let searchResponse
	for (let i = 0; i < 5; i++) {
		searchResponse = await OrderCloudSDK.Products.List(
			{
				search: productName,
				searchOn: ['Name'],
			},
			{ accessToken: clientAuth }
		)
		if (searchResponse.Items.length > 0) {
			break
		} else {
			await t.wait(5000)
		}
	}

	const product = searchResponse.Items.find(x => x.Name === productName)

	if (product.Name.includes('AutomationProduct_')) return product.ID
}

export async function createDefaultProduct(
	warehouseID: string,
	clientAuth: string
) {
	const productName = `AutomationProduct_${randomString(5)}`
	//commented out values were sent from the UI, but do not exist on the product object
	const product: SuperMarketplaceProduct = {
		Product: {
			OwnerID: '',
			DefaultPriceScheduleID: '',
			AutoForward: false,
			Active: false,
			ID: null,
			Name: productName,
			Description: null,
			QuantityMultiplier: 1,
			ShipWeight: 5,
			ShipHeight: null,
			ShipWidth: null,
			ShipLength: null,
			ShipFromAddressID: warehouseID,
			Inventory: null,
			DefaultSupplierID: null,
			xp: {
				//@ts-ignore
				IntegrationData: null,
				IsResale: false,
				//@ts-ignore
				Facets: {},
				//@ts-ignore
				Images: [],
				Status: 'Draft',
				HasVariants: false,
				Note: '',
				Tax: {
					Category: 'FR000000',
					Code: 'FR010000',
					Description: 'Delivery By Company Vehicle',
				},
				UnitOfMeasure: {
					Unit: 'Unit',
					//@ts-ignore
					Qty: '1',
				},
				ProductType: 'Standard',
				//@ts-ignore
				StaticContent: null,
				Currency: 'USD',
				//@ts-ignore
				SizeTier: 'A',
			},
		},
		PriceSchedule: {
			ID: null,
			Name: `Default_Marketplace_Buyer${productName}`,
			ApplyTax: false,
			ApplyShipping: false,
			MinQuantity: 1,
			MaxQuantity: null,
			UseCumulativeQuantity: false,
			RestrictedQuantity: false,
			PriceBreaks: [
				{
					Quantity: 1,
					//@ts-ignore
					Price: '5',
				},
			],
			xp: {},
		},
		Specs: [],
		Variants: [],
	}
	const createdProduct = await HeadStartSDK.Products.Post(product, clientAuth)

	return createdProduct.Product.ID
}
