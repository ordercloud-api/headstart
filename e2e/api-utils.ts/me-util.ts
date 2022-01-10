import * as OrderCloudSDK from 'ordercloud-javascript-sdk'

export async function meListAddresses(auth: string) {
	const response = await OrderCloudSDK.Me.ListAddresses(
		{
			page: 1,
			pageSize: 100,
		},
		{ accessToken: auth }
	)
	return response
}

export async function meGetFirstAddressID(auth: string) {
	const listAddresses = await meListAddresses(auth)
	return listAddresses.Items[0].ID
}
