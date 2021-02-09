import {
	HeadStartSDK,
	OrderCloudIntegrationsCreditCardPayment,
	OrderCloudIntegrationsCreditCardToken,
} from '@ordercloud/headstart-sdk'

export async function createCreditCard(
	authToken: string,
	firstName: string,
	lastName: string
) {
	let creditCard: OrderCloudIntegrationsCreditCardToken = {
		AccountNumber: '4111111111111111',
		CardType: 'Visa',
		CardholderName: `${firstName} ${lastName}`,
		ExpirationDate: '0323',
		CCBillingAddress: {
			City: 'Stillwater',
			Country: 'US',
			State: 'MN',
			Street1: '210 Wildwood Court',
			Zip: '19406',
		},
	}

	await HeadStartSDK.MeCreditCardAuthorizations.MePost(creditCard, authToken)
}
