import { Configuration, SdkConfiguration } from '@ordercloud/headstart-sdk'

export function setHeadstartSDKUrl() {
	const config: SdkConfiguration = {
		baseApiUrl: 'https://headstartdemo-middleware-test.azurewebsites.net',
	}

	Configuration.Set(config)
}
