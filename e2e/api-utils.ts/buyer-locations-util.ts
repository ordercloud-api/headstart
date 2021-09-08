import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import randomString from '../helpers/random-string'
import testConfig from '../testConfig'
import {
	HeadStartSDK,
	HSLocationUserGroup,
	HSBuyerLocation,
} from '@ordercloud/headstart-sdk'
import { t } from 'testcafe'
import { CatalogAssignment } from 'ordercloud-javascript-sdk'

export async function getBuyerLocationID(
	locationName: string,
	buyerID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.Addresses.List(
		buyerID,
		{ search: locationName, searchOn: 'AddressName' },
		{ accessToken: clientAuth }
	)

	const location = searchResponse.Items.find(
		x => x.AddressName === locationName
	)

	if (location.AddressName.includes('AutomationLocation_')) return location.ID
}

export async function getBuyerLocations(buyerID: string, clientAuth: string) {
	const buyerLocations = await OrderCloudSDK.Addresses.List(
		buyerID,
		{},
		{ accessToken: clientAuth }
	)

	return buyerLocations.Items
}

export async function deleteBuyerLocationWithName(
	locationName: string,
	buyerID: string,
	clientAuth: string
) {
	const locationID = await getBuyerLocationID(
		locationName,
		buyerID,
		clientAuth
	)
	await deleteBuyerLocation(buyerID, locationID, clientAuth)
}

export async function createDefaultBuyerLocation(
	buyerID: string,
	clientAuth: string,
) {
	const addressName = `AutomationLocation_${randomString(5)}`

	const location: OrderCloudSDK.Address = {
		// ID: `${buyerID}-{${buyerID}-LocationIncrementor}`,
		ID: '',
		AddressName: addressName,
		City: 'King of Prussia',
		CompanyName: addressName,
		Country: 'US',
		State: 'PA',
		Street1: '700 American Ave #200',
		Zip: '19406',
		Phone: '1231231234',
		//not sure if these xp values are needed, but this is returned from the API for a created location
		xp: {
			Accessorials: null,
			AvalaraCertificateExpiration: null,
			AvalaraCertificateID: null,
			BillingNumber: null,
			Coordinates: null,
			LegalEntity: null,
			LocationID: null,
			OpeningDate: null,
			PrimaryContactName: null,
			Status: null,
			Email: `${addressName}.hpmqx9la@mailosaur.io`,
		},
	}

	const locationUserGroup: HSLocationUserGroup = {
		Name: addressName,
		// ID: `${buyerID}-{${buyerID}-LocationIncrementor}`,
		ID: '',
		xp: {
			Country: "US",
			Currency: "USD",
			Role: null,
			Type: "BuyerLocation"
		},
	}

	const marketplaceBuyerLocation: HSBuyerLocation = {
		UserGroup: locationUserGroup,
		Address: location,

	}

	const createdLocation = await HeadStartSDK.BuyerLocations.Create(
		buyerID,
		marketplaceBuyerLocation,
		clientAuth
	)

	return createdLocation
}
export async function createDefaultCanadianBuyerLocation(
	buyerID: string,
	clientAuth: string
) {
	const addressName = `AutomationLocation_${randomString(5)}`

	const location: OrderCloudSDK.Address = {
		// ID: `${buyerID}-{${buyerID}-LocationIncrementor}`,
		ID: '',
		AddressName: addressName,
		City: 'Mississauga',
		CompanyName: addressName,
		Country: 'CA',
		State: 'ON',
		Street1: '1150 Lorne Park Rd',
		Zip: 'L5H 3A7',
		Phone: '1231231234',
		//not sure if these xp values are needed, but this is returned from the API for a created location
		xp: {
			Accessorials: null,
			AvalaraCertificateExpiration: null,
			AvalaraCertificateID: null,
			BillingNumber: null,
			Coordinates: null,
			LegalEntity: null,
			LocationID: null,
			OpeningDate: null,
			PrimaryContactName: null,
			Status: null,
			Email: `${addressName}.hpmqx9la@mailosaur.io`,
		},
	}

	const locationUserGroup: HSLocationUserGroup = {
		Name: addressName,
		// ID: `${buyerID}-{${buyerID}-LocationIncrementor}`,
		ID: '',
		xp: {
			CatalogAssignments: null,
			Country: "CA",
			Currency: "CAD",
			Role: null,
			Type: "BuyerLocation"
		},
	}

	const marketplaceBuyerLocation: HSBuyerLocation = {
		UserGroup: locationUserGroup,
		Address: location,

	}

	const createdLocation = await HeadStartSDK.BuyerLocations.Create(
		buyerID,
		marketplaceBuyerLocation,
		clientAuth
	)

	return createdLocation
}
export async function deleteBuyerLocation(
	buyerID: string,
	locationID: string,
	clientAuth: string
) {
	await HeadStartSDK.BuyerLocations.Delete(buyerID, locationID, clientAuth)
}
