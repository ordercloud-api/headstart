import {
	deleteBuyerLocation,
	getBuyerLocations,
} from '../../api-utils.ts/buyer-locations-util'
import { deleteBuyer, getAutomationBuyers } from '../../api-utils.ts/buyer-util'
import { deleteCatalog, getCatalogs } from '../../api-utils.ts/catalog-util'
import {
	deleteAutomationProducts,
	getAutomationProducts,
} from '../../api-utils.ts/product-util'
import {
	deleteSupplierUser,
	getSupplierUsers,
} from '../../api-utils.ts/supplier-users-util'
import {
	deleteAutomationSuppliers,
	getAutomationSuppliers,
} from '../../api-utils.ts/supplier-util'
import { deleteUser, getUsers } from '../../api-utils.ts/users-util'
import {
	deleteSupplierAddress,
	getSupplierAddresses,
} from '../../api-utils.ts/warehouse-util'
import { cleanupVendorWithID } from '../../helpers/test-cleanup'
//brand = buyer
//vendor = supplier

import { adminClientSetup } from '../../helpers/test-setup'

fixture`Cleanup Tests`.meta('TestRun', 'Cleanup').before(async ctx => {
	ctx.clientAuth = await adminClientSetup()
})

test('Delete Automation Products', async t => {
	const automationProducts = await getAutomationProducts(
		t.fixtureCtx.clientAuth
	)
	console.log(
		`Number of automation products found: ${automationProducts.length}`
	)
	if (automationProducts.length > 0) {
		await deleteAutomationProducts(
			automationProducts,
			t.fixtureCtx.clientAuth
		)
	}
})

//supplier user
//supplier
//supplier address (warehouse)
test('Delete Automation Suppliers', async t => {
	//get suppliers, starting with AutomationVendor_
	const automationSuppliers = await getAutomationSuppliers(
		t.fixtureCtx.clientAuth
	)
	console.log(
		`Number of automation suppliers found: ${automationSuppliers.length}`
	)
	//delete supplier users
	if (automationSuppliers.length > 0) {
		for await (const supplier of automationSuppliers) {
			const supplierUsers = await getSupplierUsers(
				supplier.ID,
				t.fixtureCtx.clientAuth
			)
			console.log(
				`Number of users found for ${supplier.Name}: ${supplierUsers.length}`
			)
			if (supplierUsers.length > 0) {
				for await (const user of supplierUsers) {
					await deleteSupplierUser(
						user.ID,
						supplier.ID,
						t.fixtureCtx.clientAuth
					)
				}
			}
		}
	}
	//delete supplier addresses
	if (automationSuppliers.length > 0) {
		for await (const supplier of automationSuppliers) {
			const supplierAddresses = await getSupplierAddresses(
				supplier.ID,
				t.fixtureCtx.clientAuth
			)
			console.log(
				`Number of addresses found for ${supplier.Name}: ${supplierAddresses.length}`
			)
			if (supplierAddresses.length > 0) {
				for await (const address of supplierAddresses) {
					await deleteSupplierAddress(
						address.ID,
						supplier.ID,
						t.fixtureCtx.clientAuth
					)
				}
			}
		}
	}
	//delete suppliers
	if (automationSuppliers.length > 0) {
		for await (const supplier of automationSuppliers) {
			await cleanupVendorWithID(supplier.ID, t.fixtureCtx.clientAuth)
		}
	}
})

//buyer location
//catalog
//buyer
//buyer user
test('Delete Automation Buyers', async t => {
	//get buyers, starting with AutomationBrand_
	const automationBuyers = await getAutomationBuyers(t.fixtureCtx.clientAuth)
	console.log(`Number of automation buyers found: ${automationBuyers.length}`)
	//delete buyer locations
	if (automationBuyers.length > 0) {
		for await (const buyer of automationBuyers) {
			const buyerLocations = await getBuyerLocations(
				buyer.ID,
				t.fixtureCtx.clientAuth
			)
			console.log(
				`Number of locations found for ${buyer.Name}: ${buyerLocations.length}`
			)
			if (buyerLocations.length > 0) {
				for await (const location of buyerLocations) {
					await deleteBuyerLocation(
						buyer.ID,
						location.ID,
						t.fixtureCtx.clientAuth
					)
				}
			}
		}
	}
	//delete buyer catalogs
	if (automationBuyers.length > 0) {
		for await (const buyer of automationBuyers) {
			const buyerCatalogs = await getCatalogs(
				buyer.ID,
				t.fixtureCtx.clientAuth
			)
			console.log(
				`Number of catalogs found for ${buyer.Name}: ${buyerCatalogs.length}`
			)
			if (buyerCatalogs.length > 0) {
				for await (const catalog of buyerCatalogs) {
					await deleteCatalog(
						catalog.ID,
						buyer.ID,
						t.fixtureCtx.clientAuth
					)
				}
			}
		}
	}
	//delete buyer users
	if (automationBuyers.length > 0) {
		for await (const buyer of automationBuyers) {
			const buyerUsers = await getUsers(buyer.ID, t.fixtureCtx.clientAuth)
			console.log(
				`Number of users found for ${buyer.Name}: ${buyerUsers.length}`
			)
			if (buyerUsers.length > 0) {
				for await (const user of buyerUsers) {
					await deleteUser(user.ID, buyer.ID, t.fixtureCtx.clientAuth)
				}
			}
		}
	}
	//delete buyers
	if (automationBuyers.length > 0) {
		for await (const buyer of automationBuyers) {
			await deleteBuyer(buyer.ID, t.fixtureCtx.clientAuth)
		}
	}
})
