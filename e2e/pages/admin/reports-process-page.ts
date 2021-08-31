import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import loadingHelper from '../../helpers/loading-helper'
import randomString from '../../helpers/random-string'
import { refreshPage } from '../../helpers/page-helper'
import mainResourcePage from './main-resource-page'
import adminHeaderPage from './admin-header-page'
import { date } from 'faker'


class reportsProcessPage {
    reportTypeDropdown: Selector
    reportOptionFromDropdown: Selector

    reportTemplateDropdown: Selector
    specificReport: Selector
    dateLowCalendar: Selector
    dateHighCalendar: Selector
    previewReportButton: Selector
    downloadReportButton: Selector
    iDOfTable: Selector


    constructor() {
        this.reportTypeDropdown = Selector('#ReportType')
        this.reportOptionFromDropdown = Selector('option')

        this.reportTemplateDropdown = Selector('#ReportTemplate')
        this.specificReport = Selector('option')
        this.dateLowCalendar = Selector('#DateLow')
        this.dateHighCalendar = Selector('#DateHigh')
        this.previewReportButton = Selector('button').withText('Preview Report')
        this.downloadReportButton = Selector('button').withText('Download Report')
        this.iDOfTable = Selector('th').withText('ID')
    }


    async selectReport(reportType: string, reportName: string) {
        await adminHeaderPage.selectProcessReports()
        await t.click(this.reportTypeDropdown)
        await t.click(this.reportOptionFromDropdown.withExactText(reportType))
        await t.click(this.reportTemplateDropdown)
        await t.click(this.specificReport.withText(reportName))
        if (await this.dateLowCalendar.exists) {
            await t.typeText(this.dateLowCalendar, lowDate)
            await t.typeText(this.dateHighCalendar, highDate)
            await t.click(this.previewReportButton)
        } else {
            await t.click(this.previewReportButton)
        }
    }

}

const lowDate = '2021-01-01'
const highDate = '2022-01-01'

export default new reportsProcessPage()
