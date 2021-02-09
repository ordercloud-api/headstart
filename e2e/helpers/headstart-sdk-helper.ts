import { Configuration, SdkConfiguration } from '@ordercloud/headstart-sdk'

export function setHeadstartSDKUrl() {
	const config: SdkConfiguration = {
		baseApiUrl: 'https://middleware-api-test.sebvendorportal.com',
	}

	Configuration.Set(config)
}
