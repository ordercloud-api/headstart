import { HeadStartSDK, PaymentUpdateRequest } from '@ordercloud/headstart-sdk'
import * as OrderCloudSDK from 'ordercloud-javascript-sdk'

export async function updatePayment(
	auth: string,
	creditCardID: string,
	orderID: string
) {
	const paymentUpdate: PaymentUpdateRequest = {
		Payments: [
			{
				DateCreated: 'Tue Jan 26 2025',
				Accepted: false,
				Type: 'CreditCard',
				CreditCardID: creditCardID,
				xp: { partialAccountNumber: '5454', cardType: 'Mastercard' },
			},
		],
	}
	const response = await HeadStartSDK.Payments.SavePayments(
		orderID,
		paymentUpdate,
		auth
	)
	return response[0].ID
}
