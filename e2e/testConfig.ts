import * as dotenv from 'dotenv'

dotenv.config()

interface testConfig {
	buyerAppUrl: string
	adminAppUrl: string
	automationClientID: string
	automationClientSecret: string
	adminSellerUsername: string
	adminSellerPassword: string
	adminSupplierUserID: string
	adminSupplierUsername: string
	adminSupplierPassword: string
	adminSupplierID: string
	adminAppClientID: string
	buyerAppClientID: string
	buyerUsername: string
	BuyerPassword: string
}

const testConfig: testConfig = {
	buyerAppUrl: process.env.TEST_BUYER_UI_URL,
	adminAppUrl: process.env.TEST_ADMIN_UI_URL,
	automationClientID: process.env.AUTOMATION_CLIENT_ID,
	automationClientSecret: process.env.AUTOMATION_CLIENT_SECRET,
	adminSellerUsername: process.env.ADMIN_SELLER_USER_USERNAME,
	adminSellerPassword: process.env.ADMIN_SELLER_USER_PASSWORD,
	adminSupplierUserID: process.env.ADMIN_SUPPLIER_USER_ID,
	adminSupplierUsername: process.env.ADMIN_SUPPLIER_USER_USERNAME,
	adminSupplierPassword: process.env.ADMIN_SUPPLIER_USER_PASSWORD,
	buyerUsername: process.env.BUYER_USER_USERNAME,
	BuyerPassword: process.env.BUYER_USER_PASSWORD,
	adminSupplierID: process.env.DEFAULT_SUPPLIER_ID,
	adminAppClientID: process.env.ADMIN_APP_CLIENT_ID,
	buyerAppClientID: process.env.BUYER_APP_CLIENT_ID,
}

export default testConfig
