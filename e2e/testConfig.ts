import * as dotenv from 'dotenv'

dotenv.config()

interface testConfig {
	buyerAppUrl: string
	adminAppUrl: string
	automationClientID: string
	automationClientSecret: string
	adminSellerUsername: string
	adminSellerPassword: string
	adminAppClientID: string
	buyerAppClientID: string
}

const testConfig: testConfig = {
	buyerAppUrl: process.env.TEST_BUYER_UI_URL,
	adminAppUrl: process.env.TEST_ADMIN_UI_URL,
	automationClientID: process.env.AUTOMATION_CLIENT_ID,
	automationClientSecret: process.env.AUTOMATION_CLIENT_SECRET,
	adminSellerUsername: process.env.ADMIN_SELLER_USER_USERNAME,
	adminSellerPassword: process.env.ADMIN_SELLER_USER_PASSWORD,
	adminAppClientID: process.env.ADMIN_APP_CLIENT_ID,
	buyerAppClientID: process.env.BUYER_APP_CLIENT_ID,
}

export default testConfig
