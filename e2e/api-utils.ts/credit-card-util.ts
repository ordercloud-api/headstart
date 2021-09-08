import {
	HeadStartSDK,
	OrderCloudIntegrationsCreditCardPayment,
	OrderCloudIntegrationsCreditCardToken,
} from '@ordercloud/headstart-sdk'
import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { TEST_CREDITCARD_NAME } from '../test-constants'

export async function createCreditCard(authToken: string) {
	let creditCard: OrderCloudSDK.BuyerCreditCard = {
		CardType: 'Mastercard',
		CardholderName: TEST_CREDITCARD_NAME,
		ExpirationDate: '2022-01-19T20:19:19.514Z',
		PartialAccountNumber: '5454',
		Token: '9545666483645454',
		xp: {
			CCBillingAddress: {
				Street1: '700 American Ave #200',
				City: 'King of Prussia',
				State: 'PA',
				Zip: '19406',
				Country: 'US',
			},
		},
	}

	const buyerCreditCard = await OrderCloudSDK.Me.CreateCreditCard(creditCard, {
		accessToken: authToken,
	})
	return buyerCreditCard.ID
}
