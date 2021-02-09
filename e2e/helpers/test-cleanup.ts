import {
	getSupplier,
	deleteSupplier,
	getSupplierWithID,
} from '../api-utils.ts/supplier-util'
import { deleteApiClient } from '../api-utils.ts/api-client-util'
import { deleteSupplierIntegrationUser } from '../api-utils.ts/supplier-users-util'

//will delete vendor, vendor API client, and vendor integration user
export async function cleanupVendorWithName(
	vendorName: string,
	clientAuth: string
) {
	//instead of passing in just clientAuth to these, pass in the whole object and include requestType = Cleanup for all API calls
	const vendor = await getSupplier(vendorName, clientAuth)
	const apiClientID = vendor.xp.ApiClientID
	await deleteApiClient(apiClientID, clientAuth)
	await deleteSupplierIntegrationUser(vendor.ID, clientAuth)
	await deleteSupplier(vendor.ID, clientAuth)
}

export async function cleanupVendorWithID(
	vendorID: string,
	clientAuth: string
) {
	const vendor = await getSupplierWithID(vendorID, clientAuth)
	const apiClientID = vendor.xp.ApiClientID
	await deleteApiClient(apiClientID, clientAuth)
	await deleteSupplierIntegrationUser(vendor.ID, clientAuth)
	await deleteSupplier(vendor.ID, clientAuth)
}
