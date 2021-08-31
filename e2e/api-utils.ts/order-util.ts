import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { t } from 'testcafe'
import {
	HeadStartSDK,
	OrderCloudIntegrationsCreditCardPayment,
} from '@ordercloud/headstart-sdk'
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

export async function createOrder(
	Auth: string,
	direction: OrderCloudSDK.OrderDirection
) {
	const order = await OrderCloudSDK.Orders.Create(
		direction,
		{
			xp: {
				AvalaraTaxTransactionCode: '',
				OrderType: 'Standard',
				QuoteOrderInfo: null,
				Currency: 'USD',
				Returns: {
					HasClaims: false,
					HasUnresolvedClaims: false,
					Resolutions: [],
				},
				ClaimStatus: 'NoClaim',
				ShippingStatus: 'Processing',
			},
		},
		{
			accessToken: Auth,
		}
	)
	return order.ID
}
export async function addLineItem(
	orderID: string,
	productID: string,
	auth: string,
	quantity: number
) {
	await HeadStartSDK.Orders.UpsertLineItem(
		orderID,
		{
			ProductID: productID,
			Quantity: quantity,
			Specs: [],
			xp: {},
		},
		auth
	)
}

export async function addAddresses(
	orderID: string,
	addressID: string,
	auth: string,
	orderDirection: OrderCloudSDK.OrderDirection
) {
	await OrderCloudSDK.Orders.Patch(
		orderDirection,
		orderID,
		{
			BillingAddressID: addressID,
			xp: { ApprovalNeeded: '' },
		},
		{ accessToken: auth }
	)
	await OrderCloudSDK.Orders.Patch(
		orderDirection,
		orderID,
		{
			ShippingAddressID: addressID,
		},
		{ accessToken: auth }
	)
}

// return to this later to find the proper route
export async function postEstimateShipping(
	orderID: string,
	auth: string,
	orderDirection: OrderCloudSDK.OrderDirection
) {
	const orderEstimate = await OrderCloudSDK.IntegrationEvents.EstimateShipping(
		orderDirection,
		orderID,
		{
			accessToken: auth,
		}
	)
	return orderEstimate
}

export async function shipMethods(
	shipmentID: string,
	shipMethodID: string,
	directionOFOrder: 'Outgoing',
	orderID: string,
	auth: string
) {
	var shipMethodSelection: OrderCloudSDK.OrderShipMethodSelection = {
		'ShipMethodSelections': [
			{ 'ShipEstimateID': shipmentID, 'ShipMethodID': shipMethodID },
		],
	}
	await OrderCloudSDK.IntegrationEvents.SelectShipmethods(
		directionOFOrder,
		orderID,
		shipMethodSelection,
		{ accessToken: auth }
	)
}

export async function calculateCost(
	orderID: string,
	auth: string,
	orderDirection: OrderCloudSDK.OrderDirection
) {
	await OrderCloudSDK.IntegrationEvents.Calculate(orderDirection, orderID, {
		accessToken: auth,
	})
}

export async function orderSubmit(
	orderID: string,
	paymentID: string,
	creditCardID: string,
	currency: string,
	cvv: string,
	auth: string
) {
	const creditCardPayment: OrderCloudIntegrationsCreditCardPayment = {
		CVV: cvv,
		CreditCardID: creditCardID,
		Currency: currency,
		OrderID: orderID,
		PaymentID: paymentID,
	}

	const newOrder = await HeadStartSDK.Orders.Submit(
		'Outgoing',
		orderID,
		creditCardPayment,
		auth
	)
	return newOrder.ID
}
