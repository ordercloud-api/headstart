import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import {
	HeadStartSDK,
	Configuration,
	SdkConfiguration,
} from '@ordercloud/headstart-sdk'
import axios from 'axios'
import { getAxiosHeaders } from '../helpers/axios-helper'
import randomString from '../helpers/random-string'
import { t } from 'testcafe'


export async function getWarehouseID(
	warehouseName: string,
	supplierID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.SupplierAddresses.List(
		supplierID,
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
	supplierID: string,
	clientAuth: string
) {
	await OrderCloudSDK.SupplierAddresses.Delete(supplierID, warehouseID, {
		accessToken: clientAuth,
	})
}

export async function createDefaultSupplierAddress(
	supplierID: string,
	clientAuth: string
) {
	const name = `AutomationAddress_${randomString(5)}`
	const warehouse: OrderCloudSDK.Address = {
		ID: name,
		AddressName: name,
		Country: 'US',
		City: 'King of Prussia',
		CompanyName: name,
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
		supplierID,
		warehouse,
		clientAuth
	)

	return warehouse.ID
}
