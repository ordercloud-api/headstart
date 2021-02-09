import * as OrderCloudSDK from 'ordercloud-javascript-sdk'

export async function getApiClient(clientID: string, clientAuth: string) {
	const client = await OrderCloudSDK.ApiClients.Get(clientID, {
		accessToken: clientAuth,
		requestType: 'Cleanup',
	})

	if (client.AppName.includes('AutomationVendor_')) return client
}

export async function deleteApiClient(clientID: string, clientAuth: string) {
	const client = await getApiClient(clientID, clientAuth)
	if (client.AppName.includes('AutomationVendor_')) {
		await OrderCloudSDK.ApiClients.Delete(clientID, {
			accessToken: clientAuth,
			requestType: 'Cleanup',
		})
	}
}
