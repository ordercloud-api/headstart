import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import {
	HeadStartSDK,
	Configuration,
	SdkConfiguration,
} from '@ordercloud/headstart-sdk'
import axios from 'axios'
import { getAxiosHeaders } from '../helpers/axios-helper'
import randomString from '../helpers/random-string'

export async function getWarehouseID(
	warehouseName: string,
	vendorID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.SupplierAddresses.List(
		vendorID,
		{ search: warehouseName, searchOn: 'AddressName' },
		{ accessToken: clientAuth }
	)

	const warehouse = searchResponse.Items.find(
		x => x.AddressName === warehouseName
	)

	if (warehouse.AddressName.includes('AutomationAddress_')) return warehouse.ID
}

export async function getSupplierAddresses(
	supplierID: string,
	clientAuth: string
) {
	const addresses = await OrderCloudSDK.SupplierAddresses.List(
		supplierID,
		{},
		{ accessToken: clientAuth }
	)

	return addresses.Items
}

export async function deleteSupplierAddress(
	warehouseID: string,
	vendorID: string,
	clientAuth: string
) {
	await OrderCloudSDK.SupplierAddresses.Delete(vendorID, warehouseID, {
		accessToken: clientAuth,
	})
}

export async function createDefaultSupplierAddress(
	vendorID: string,
	clientAuth: string
) {
	const name = `AutomationAddress_${randomString(5)}`
	const warehouse: OrderCloudSDK.Address = {
		ID: name,
		AddressName: name,
		City: 'King of Prussia',
		CompanyName: name,
		Country: 'US',
		State: 'PA',
		Street1: '700 American Ave Ste 200',
		Zip: '19406-4031',
		FirstName: '',
		LastName: '',
		Phone: '',
		Street2: '',
		xp: null,
	}

	await HeadStartSDK.ValidatedAddresses.CreateSupplierAddress(
		vendorID,
		warehouse,
		clientAuth
	)

	return warehouse.ID
}
