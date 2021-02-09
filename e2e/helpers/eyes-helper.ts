import Eyes from '@applitools/eyes-testcafe'

export function getBrowsers() {
	return [
		{ width: 1920, height: 1080, name: 'chrome' },
		{ width: 1920, height: 1080, name: 'firefox' },
	]
}

export async function checkWindow(eyes: Eyes, testName: string) {
	await eyes.checkWindow({
		//@ts-ignore
		useDom: true,
		ignoreDisplacements: true,
		tag: testName,
		fully: true,
	})
}
