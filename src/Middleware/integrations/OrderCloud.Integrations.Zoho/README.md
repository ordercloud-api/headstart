Steps to create Access Token for Zoho

1. Log into your Zoho account
2. Create API Client in Api Console: https://accounts.zoho.com/developerconsole
	a. Create "Server-based Applications" type
		1. Client Name = whatever you wish to identify it as (ex: OrderCloud)
		2. Homepage URL = whatever you wish to use (ex: https://ordercloud.io)
		3. Authorized Redirect URL = https://ordercloud.io
	b. After creation copy the Client ID and Client Secret provided
3. Create qualified URL and open in browser window
	a. https://accounts.zoho.com/oauth/v2/auth?response_type=code&client_id={client_id}&scope=ZohoBooks.fullaccess.all&redirect_uri=https://ordercloud.io&prompt=consent&access_type=offline
	b. Accept authorization
		1. Creates a redirect to the provided redirect URL
		2. Take the "code" query param from the URL in the browser window. This is your short lived access_token
4. Request oauth for long lived refresh token
	a. POST: https://accounts.zoho.com/oauth/v2/token?client_id={client_id}&grant_type=authorization_code&client_secret={client_secret}&redirect_uri=https://ordercloud.io&code={code from step 3.b.2}&access_type=offline&response_type=code
	b. Copy response "refresh_token". This is your long lived token that will be used to retrieve future access tokens in the Zoho SDK
