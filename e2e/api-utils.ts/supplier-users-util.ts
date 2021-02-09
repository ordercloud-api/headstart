import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import faker = require('faker')
import { saveUserAssignment } from './usergroups-helper'
import { t } from 'testcafe'

export async function deleteSupplierUser(
	userID: string,
	vendorID: string,
	clientAuth: string
) {
	await OrderCloudSDK.SupplierUsers.Delete(vendorID, userID, {
		accessToken: clientAuth,
		requestType: 'Cleanup',
	})
}

export async function createDefaultSupplierUser(
	supplierID: string,
	clientAuth: string
) {
	const firstName = faker.name.firstName()
	const lastName = faker.name.lastName()
	const firstNameReplaced = firstName.replace(/'/g, '')
	const lastNameReplaced = lastName.replace(/'/g, '')
	const email = `${firstNameReplaced}${lastNameReplaced}.hpmqx9la@mailosaur.io`

	const user: OrderCloudSDK.User = {
		Email: email,
		Username: email,
		FirstName: firstNameReplaced,
		LastName: lastNameReplaced,
		Active: true,
		Password: 'Test123!',
	}

	const createdUser = await OrderCloudSDK.SupplierUsers.Create(
		supplierID,
		user,
		{
			accessToken: clientAuth,
		}
	)

	//assign roles
	await saveSupplierUserAssignment(
		createdUser.ID,
		`${supplierID}ProductAdmin`,
		supplierID,
		clientAuth
	)
	await saveSupplierUserAssignment(
		createdUser.ID,
		`${supplierID}OrderAdmin`,
		supplierID,
		clientAuth
	)
	await saveSupplierUserAssignment(
		createdUser.ID,
		`${supplierID}AccountAdmin`,
		supplierID,
		clientAuth
	)

	return createdUser.ID
}

export async function createDefaultSupplierUserWithoutRoles(
	supplierID: string,
	clientAuth: string
) {
	const firstName = faker.name.firstName()
	const lastName = faker.name.lastName()
	const firstNameReplaced = firstName.replace(/'/g, '')
	const lastNameReplaced = lastName.replace(/'/g, '')
	const email = `${firstNameReplaced}${lastNameReplaced}.hpmqx9la@mailosaur.io`

	const user: OrderCloudSDK.User = {
		Email: email,
		Username: email,
		FirstName: firstNameReplaced,
		LastName: lastNameReplaced,
		Active: true,
		Password: 'Test123!',
	}

	const createdUser = await OrderCloudSDK.SupplierUsers.Create(
		supplierID,
		user,
		{
			accessToken: clientAuth,
		}
	)

	return createdUser.ID
}

export async function getSupplierUserID(
	username: string,
	vendorID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.SupplierUsers.List(
		vendorID,
		{
			search: username,
			searchOn: 'Username',
		},
		{ accessToken: clientAuth }
	)

	const user = searchResponse.Items.find(x => x.Username === username)

	if (user.Username.includes('.hpmqx9la@mailosaur.io')) return user.ID
}

export async function setupGetSupplierUserID(
	username: string,
	vendorID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.SupplierUsers.List(
		vendorID,
		{
			search: username,
			searchOn: 'Username',
		},
		{ accessToken: clientAuth }
	)

	const user = searchResponse.Items.find(x => x.Username === username)

	return user.ID
}

export async function updateSupplierUser(
	supplierID: string,
	userID: string,
	update: Partial<OrderCloudSDK.User>,
	clientAuth: string
) {
	await OrderCloudSDK.SupplierUsers.Patch(supplierID, userID, update, {
		accessToken: clientAuth,
	})
}

export async function getSupplierUser(
	userID: string,
	vendorID: string,
	clientAuth: string
) {
	const getUser = await OrderCloudSDK.SupplierUsers.Get(vendorID, userID, {
		accessToken: clientAuth,
	})

	return getUser
}

export async function getSupplierUsers(supplierID: string, clientAuth: string) {
	const users = await OrderCloudSDK.SupplierUsers.List(
		supplierID,
		{},
		{ accessToken: clientAuth }
	)

	//filter out integration user
	return users.Items.filter(user => !user.Username.includes('dev_'))
}

//delete supplier integration user that is created when creating a new supplier (AKA vendor)
export async function deleteSupplierIntegrationUser(
	supplierID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.SupplierUsers.List(
		supplierID,
		{
			search: 'dev_*',
			searchOn: 'Username',
		},
		{ accessToken: clientAuth, requestType: 'Cleanup' }
	)

	const integrationUser = searchResponse.Items.find(x =>
		x.Username.includes('dev_')
	)

	if (
		integrationUser.FirstName === 'Integration' &&
		integrationUser.LastName === 'Developer'
	) {
		await deleteSupplierUser(integrationUser.ID, supplierID, clientAuth)
	}
}

export async function saveSupplierUserAssignment(
	userID: string,
	userGroupID: string,
	supplierID: string,
	authToken: string
) {
	const assignment: OrderCloudSDK.UserGroupAssignment = {
		UserID: userID,
		UserGroupID: userGroupID,
	}

	await OrderCloudSDK.SupplierUserGroups.SaveUserAssignment(
		supplierID,
		assignment,
		{
			accessToken: authToken,
		}
	)
}
