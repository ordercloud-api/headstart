import { t } from 'testcafe'
import loadingHelper from './loading-helper'

export async function refreshPage() {
	await t.eval(() => location.reload(true))
	await loadingHelper.waitForLoadingBar()
}
