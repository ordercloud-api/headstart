import {
	getSupplier,
	deleteSupplier,
	getSupplierWithID,
} from '../api-utils.ts/supplier-util'
import { deleteApiClient } from '../api-utils.ts/api-client-util'
import { deleteSupplierIntegrationUser } from '../api-utils.ts/supplier-users-util'

//will delete supplier, supplier API client, and supplier integration user
export async function cleanupSupplierWithName(
	supplierName: string,
	clientAuth: string
) {
	//instead of passing in just clientAuth to these, pass in the whole object and include requestType = Cleanup for all API calls
	const supplier = await getSupplier(supplierName, clientAuth)
	const apiClientID = supplier.xp.ApiClientID
	await deleteApiClient(apiClientID, clientAuth)
	await deleteSupplierIntegrationUser(supplier.ID, clientAuth)
	await deleteSupplier(supplier.ID, clientAuth)
}

export async function cleanupSupplierWithID(
	supplierID: string,
	clientAuth: string
) {
	const supplier = await getSupplierWithID(supplierID, clientAuth)
	const apiClientID = supplier.xp.ApiClientID
	await deleteApiClient(apiClientID, clientAuth)
	await deleteSupplierIntegrationUser(supplier.ID, clientAuth)
	await deleteSupplier(supplier.ID, clientAuth)
}
