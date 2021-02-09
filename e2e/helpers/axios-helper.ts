import axios from 'axios'

export async function axiosSetup() {
	//if there is already an interceptor setup, dont create another one
	const isInitialized = axios.interceptors.response['handlers'].length
	if (isInitialized) {
		return
	}

	axios.interceptors.response.use(
		function(response) {
			return response
		},
		function(error) {
			if (error.config) {
				if (error.config.requestType === 'Cleanup') {
					logApiErrorInfo(error)
					return { data: null }
				} else {
					logApiErrorInfo(error)
					return Promise.reject(error)
				}
			} else {
				logApiErrorInfo(error)
				return Promise.reject(error)
			}
		}
	)
}

export async function logApiErrorInfo(error: any) {
	console.log(`New Error:\n`)
	if (error.response) {
		console.log(`error.response.data: ${error.response.data}`)
		console.log(`error.response.status: ${error.response.status}`)
		console.log(
			`error.response.config.method: ${error.response.config.method}`
		)
		console.log(
			`error.response.config.params: ${error.response.config.params}`
		)
		console.log(`error.response.config.url: ${error.response.config.url}`)
		console.log(`error.response.config.data: ${error.response.config.data}`)
		console.log(
			`error.response.config.headers: ${error.response.config.headers}`
		)
	} else {
		console.log(error)
	}
	console.log('\n')
}

export function getAxiosHeaders(clientAuth: string) {
	return {
		headers: {
			Authorization: `Bearer ${clientAuth}`,
			'Content-Type': 'application/json',
		},
	}
}
