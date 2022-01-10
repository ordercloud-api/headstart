import OrderCloudSDK = require('ordercloud-javascript-sdk')
import faker = require('faker')
import { saveUserAssignment, getLocationID } from './usergroups-helper'
import { TEST_PASSWORD } from '../test-constants'

export async function createUser(
	clientAuth: string,
	buyerID: string,
	country?: string
) {
	const firstName = faker.name.firstName()
	const lastName = faker.name.lastName()
	const firstNameReplaced = firstName.replace(/'/g, '')
	const lastNameReplaced = lastName.replace(/'/g, '')
	const email = `${firstNameReplaced}${lastNameReplaced}.hpmqx9la@mailosaur.io`

	const testUser: OrderCloudSDK.User = {
		Active: true,
		Email: email,
		FirstName: firstNameReplaced,
		LastName: lastNameReplaced,
		Username: email,
		ID: `${buyerID}-{${buyerID}-UserIncrementor}`,
		xp: {
			AutomationUser: true,
			Country: 'US'
		},
		Password: TEST_PASSWORD,


	}

	const createdTestUser = await OrderCloudSDK.Users.Create(buyerID, testUser, {
		accessToken: clientAuth,
	})

	testUser.ID = createdTestUser.ID

	//make user assignments so user is able to see products
	//JSYILMZLsU-q-meiGTsIBg is All Location Products
	//iJhQ4uM-1UaFruXemXNZaw is Canada Only Products
	const locationID = getLocationID(country)
	// await saveUserAssignment(testUser.ID, locationID, '0001', clientAuth)
	await saveUserAssignment(
		testUser.ID,
		`${buyerID}-0001`,
		buyerID,
		clientAuth
	)


	return testUser
}

export async function deleteUser(
	userID: string,
	buyerID: string,
	clientAuth: string
) {
	await OrderCloudSDK.Users.Delete(buyerID, userID, {
		accessToken: clientAuth,
	})
}

export async function getUserID(
	username: string,
	buyerID: string,
	clientAuth: string
) {
	const searchResponse = await OrderCloudSDK.Users.List(
		buyerID,
		{
			search: username,
			searchOn: 'Username',
		},
		{ accessToken: clientAuth }
	)

	const user = searchResponse.Items.find(x => x.Username === username)

	if (user.Username.includes('.hpmqx9la@mailosaur.io')) return user.ID
}

export async function getUsers(buyerID: string, clientAuth: string) {
	const users = await OrderCloudSDK.Users.List(
		buyerID,
		{},
		{ accessToken: clientAuth }
	)

	return users.Items
}
