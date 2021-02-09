import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { t } from 'testcafe'

export async function deleteOrder(
	clientAuth: string,
	direction: OrderCloudSDK.OrderDirection,
	orderID: string
) {
	await OrderCloudSDK.Orders.Delete(direction, orderID, {
		accessToken: clientAuth,
	})
}

//Deletes unsubmitted and open orders for a user, also delete outgoing seller orders
export async function deleteOrdersForUser(
	clientAuth: string,
	buyerID: string,
	userID: string
) {
	const incomingOrderOptions = {
		buyerID: buyerID,
		filters: {
			'FromUserID': userID,
			'Status': 'Unsubmitted|Open',
		},
	}

	const incomingOrders = await OrderCloudSDK.Orders.List(
		'Incoming',
		incomingOrderOptions,
		{
			accessToken: clientAuth,
		}
	)

	//if outgoing orders exist, delete them. These are orders that start with SEB
	if (
		incomingOrders.Items.filter(order => order.ID.includes('SEB')).length > 0
	) {
		const sebOrderID = incomingOrders.Items.find(order =>
			order.ID.includes('SEB')
		).ID

		const outgoingOrderOptions = {
			filters: {
				'ID': `${sebOrderID}*`,
			},
		}

		const outgoingOrders = await OrderCloudSDK.Orders.List(
			'Outgoing',
			outgoingOrderOptions,
			{
				accessToken: clientAuth,
			}
		)

		for await (const order of outgoingOrders.Items) {
			await deleteOrder(clientAuth, 'Outgoing', order.ID)
		}
	}

	for await (const order of incomingOrders.Items) {
		await deleteOrder(clientAuth, 'Incoming', order.ID)
	}
}
